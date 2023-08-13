// * ----------------------------------------------------------------------------
// * Author: Ben Baker
// * Website: baker76.com
// * E-Mail: ben@baker76.com
// * Copyright (C) 2020 Ben Baker. All Rights Reserved.
// * ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace nRF5DFUTool
{
    class BLEDeviceWatcher
    {
        private DeviceWatcher m_deviceWatcher;
        private List<BLEDevice> m_knownDevices;
        private List<DeviceInformation> m_unknownDevices;

        public event EventHandler<EventArgs> DevicesUpdated;

        public BLEDeviceWatcher()
        {
            m_knownDevices = new List<BLEDevice>();
            m_unknownDevices = new List<DeviceInformation>();
        }

        public void StartBleDeviceWatcher()
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsPresent", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
            m_deviceWatcher = DeviceInformation.CreateWatcher(aqsAllBluetoothLEDevices, requestedProperties, DeviceInformationKind.AssociationEndpoint);
            m_deviceWatcher.Added += DeviceAdded;
            m_deviceWatcher.Updated += DeviceUpdated;
            m_deviceWatcher.Removed += DeviceRemoved;
            m_deviceWatcher.EnumerationCompleted += EnumerationCompleted;
            m_deviceWatcher.Stopped += DeviceStopped;

            m_knownDevices.Clear();

            m_deviceWatcher.Start();
        }

        public void StopBleDeviceWatcher()
        {
            if (m_deviceWatcher != null)
            {
                m_deviceWatcher.Added -= DeviceAdded;
                m_deviceWatcher.Updated -= DeviceUpdated;
                m_deviceWatcher.Removed -= DeviceRemoved;
                m_deviceWatcher.EnumerationCompleted -= EnumerationCompleted;
                m_deviceWatcher.Stopped -= DeviceStopped;

                m_deviceWatcher.Stop();
                m_deviceWatcher = null;
            }
        }

        private async void DeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            await Task.Run(() =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));

                    if (sender == m_deviceWatcher)
                    {
                        if (FindBluetoothLEDeviceDisplay(deviceInfo.Id) == null)
                        {
                            if (deviceInfo.Name != String.Empty)
                                m_knownDevices.Add(new BLEDevice(deviceInfo));
                            else
                                m_unknownDevices.Add(deviceInfo);

                            if (DevicesUpdated != null)
                                DevicesUpdated(this, EventArgs.Empty);
                        }
                    }
                }
            });

            /* await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                }
            }); */
        }

        private async void DeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            await Task.Run(() =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Updated {0}", deviceInfoUpdate.Id));

                    if (sender == m_deviceWatcher)
                    {
                        BLEDevice bleDevice = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);

                        if (bleDevice != null)
                        {
                            bleDevice.Update(deviceInfoUpdate);

                            if (DevicesUpdated != null)
                                DevicesUpdated(this, EventArgs.Empty);

                            return;
                        }

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);

                        if (deviceInfo != null)
                        {
                            deviceInfo.Update(deviceInfoUpdate);

                            if (deviceInfo.Name != String.Empty)
                            {
                                m_knownDevices.Add(new BLEDevice(deviceInfo));
                                m_unknownDevices.Remove(deviceInfo);

                                if (DevicesUpdated != null)
                                    DevicesUpdated(this, EventArgs.Empty);
                            }
                        }
                    }
                }
            });
        }

        private async void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            await Task.Run(() =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Removed {0}", deviceInfoUpdate.Id));

                    if (sender == m_deviceWatcher)
                    {
                        BLEDevice bleDevice = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);

                        if (bleDevice != null)
                            m_knownDevices.Remove(bleDevice);

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);

                        if (deviceInfo != null)
                            m_unknownDevices.Remove(deviceInfo);

                        if (DevicesUpdated != null)
                            DevicesUpdated(this, EventArgs.Empty);
                    }
                }
            });
        }

        private async void EnumerationCompleted(DeviceWatcher sender, object e)
        {
            await Task.Run(() =>
            {
                lock (this)
                {
                    if (sender == m_deviceWatcher)
                    {
                        Debug.WriteLine(String.Format("{0} devices found. Enumeration completed.", m_knownDevices.Count));
                    }
                }
            });
        }

        private async void DeviceStopped(DeviceWatcher sender, object e)
        {
            await Task.Run(() =>
            {
                lock (this)
                {
                    if (sender == m_deviceWatcher)
                    {
                        // sender.Status == DeviceWatcherStatus.Aborted

                        Debug.WriteLine("No longer watching for devices.");
                    }
                }
            });
        }

        public async Task<GattCharacteristic> GetCharacteristic(GattDeviceService deviceService, Guid characteristicUUID)
        {
            GattCharacteristicsResult characteristicResult = await deviceService.GetCharacteristicsForUuidAsync(characteristicUUID);          

            if (characteristicResult.Status == GattCommunicationStatus.Success && characteristicResult.Characteristics.Count > 0)
                return characteristicResult.Characteristics[0];

            return null;
        }

        private async Task<GattDeviceService> GetDeviceService(Guid serviceGuid)
        {
            var filter = GattDeviceService.GetDeviceSelectorFromUuid(serviceGuid);
            var deviceInfos = await DeviceInformation.FindAllAsync(filter);

            if (deviceInfos != null)
            {
                var deviceService = await GattDeviceService.FromIdAsync(deviceInfos[0].Id);

                return deviceService;
            }

            return null;
        }

        private BLEDevice FindBluetoothLEDeviceDisplay(string id)
        {
            lock (this)
            {
                foreach (BLEDevice bleDevice in m_knownDevices)
                {
                    if (bleDevice.Id == id)
                        return bleDevice;
                }
            }

            return null;
        }

        private DeviceInformation FindUnknownDevices(string id)
        {
            lock (this)
            {
                foreach (DeviceInformation bleDeviceInfo in m_unknownDevices)
                {
                    if (bleDeviceInfo.Id == id)
                        return bleDeviceInfo;
                }
            }

            return null;
        }

        public Byte BufferToByte(IBuffer buffer)
        {
            if (buffer.Length != 1)
                return (byte)0;

            using (var dataReader = DataReader.FromBuffer(buffer))
                return dataReader.ReadByte();
        }

        public UInt32 BufferToUInt16(IBuffer buffer)
        {
            if (buffer.Length != 2)
                return 0;

            using (var dataReader = DataReader.FromBuffer(buffer))
                return dataReader.ReadUInt16();
        }

        public UInt32 BufferToUInt32(IBuffer buffer)
        {
            if (buffer.Length != 4)
                return 0;

            using (var dataReader = DataReader.FromBuffer(buffer))
                return dataReader.ReadUInt32();
        }

        public string BufferToString(IBuffer buffer)
        {
            using (var dataReader = DataReader.FromBuffer(buffer))
            {
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                return dataReader.ReadString(buffer.Length);
            }
        }

        public List<BLEDevice> KnownDevices
        {
            get { return m_knownDevices; }
        }
    }
}
