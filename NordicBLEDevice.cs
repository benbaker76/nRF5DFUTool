// * ----------------------------------------------------------------------------
// * Author: Ben Baker
// * Website: baker76.com
// * E-Mail: ben@baker76.com
// * Copyright (C) 2020 Ben Baker. All Rights Reserved.
// * ----------------------------------------------------------------------------

#define NRF_DFU_BLE_BUTTONLESS_SUPPORTS_BONDS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
using System.Threading;

using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;

using Newtonsoft.Json.Linq;

using ProtoBuf;

namespace nRF5DFUTool
{
    class NordicBLEDevice : BLEDevice
    {
        private readonly Guid NordicUARTServiceUuid = Guid.Parse("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        private readonly Guid NordicTXCharacteristicUuid = Guid.Parse("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
        private readonly Guid NordicRXCharacteristicUUid = Guid.Parse("6e400002-b5a3-f393-e0a9-e50e24dcca9e");

        private readonly Guid NordicSecureDFUService = BluetoothUuidHelper.FromShortId(0xfe59);

        private readonly Guid NordicDFUControlPointCharacteristicUuid = Guid.Parse("8ec90001-f315-4f60-9fb8-838830daea50"); // Write, Notify
        private readonly Guid NordicDFUPacketCharacteristicUuid = Guid.Parse("8ec90002-f315-4f60-9fb8-838830daea50");       // WriteWithoutResponse, Notify

#if NRF_DFU_BLE_BUTTONLESS_SUPPORTS_BONDS
        private readonly Guid NordicButtonlessDFUCharacteristicUuid = Guid.Parse("8ec90004-f315-4f60-9fb8-838830daea50");   // Write, Indicate
#else
        private readonly Guid NordicButtonlessDFUCharacteristicUuid = Guid.Parse("8ec90003-f315-4f60-9fb8-838830daea50");   // Write, Indicate
#endif
        private readonly string AdvertisementName = "Nordic_Blinky";
        private readonly string DFUAdvertisementName = "DfuTarg";

        public static byte[] PublicKey;

        private BLEDeviceWatcher m_bleDeviceWatcher = null;

        private GattCharacteristic m_serviceChangedCharacteristic = null;
        private GattCharacteristic m_batteryLevelCharacteristic = null;
        private GattCharacteristic m_nordicButtonlessDFUCharacteristic = null;
        private GattCharacteristic m_nordicDFUControlPointCharacteristic = null;
        private GattCharacteristic m_nordicDFUPacketCharacteristic = null;

        private List<FirmwareModeNode> m_firmwareModeList = null;

        private System.Timers.Timer m_timer = null;
        //public FirmwareNode m_firmware = null;
        public FirmwareType m_firmwareType;

        private List<FirmwareNode> m_firmwareList = null;
        private int m_firmwareIndex = 0;
        //private FirmwareMode m_firmwareMode = FirmwareMode.SetPRN;
        //private FirmwareType m_firmwareType = FirmwareType.InitPacket;
        private FirmwareTypes m_firmwareTypes = FirmwareTypes.None;

        // Packet Receipt Notification
        private ushort m_prnCount = 0;
        private ushort m_prnIndex = 0;

        private int m_maxPacketLength = DfuReq.MAX_DFU_PKT_LEN;

        private byte[] m_data = new byte[] { 0 };
        private int m_dataOffset = 0;
        private int m_blockSize = 0;
        private Crc32 m_crc = new Crc32();

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        private bool m_disposed = false;

        public NordicBLEDevice()
            : base()
        {
            Initialize();
        }

        public NordicBLEDevice(string advertismentName, string dfuAdvertismentName)
            : base()
        {
            AdvertisementName = advertismentName;
            DFUAdvertisementName = dfuAdvertismentName;

            Initialize();
        }

        private void Initialize()
        {
            m_firmwareModeList = new List<FirmwareModeNode>();

            m_timer = new System.Timers.Timer();
            m_timer.Elapsed += OnElapsed;
            m_timer.Interval = 10;

            m_bleDeviceWatcher = new BLEDeviceWatcher();
            m_bleDeviceWatcher.StartBleDeviceWatcher();
            m_bleDeviceWatcher.DevicesUpdated += OnDevicesUpdated;

            //byte[] responseBytes = new byte[] { 0x01, 0x02, 0x00, 0x10, 0x00, 0x00 };
            //byte[] responseBytes = new byte[] { 0x01, 0x02, 0xC0, 0x48, 0x02, 0x00 };
            byte[] responseBytes = new byte[] { 0x01, 0x02, 0xC0, 0x08, 0x00, 0x00 };
            UInt32 value = BitConverter.ToUInt32(responseBytes, 2);

            Type responseType = typeof(DfuReq.nrf_dfu_response_t);
            byte[] buffer = new byte[Marshal.SizeOf(responseType)];
            Array.Copy(responseBytes, 0, buffer, 0, responseBytes.Length);
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            DfuReq.nrf_dfu_response_t response = (DfuReq.nrf_dfu_response_t)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), responseType);
            handle.Free();

            Debug.WriteLine("{0} bytes", value);
            Debug.WriteLine("");
        }

        private void OnDevicesUpdated(object sender, EventArgs e)
        {
            InitializeBLEDevice();
        }

        private async void InitializeBLEDevice()
        {
            BLEDevice foundBLEDevice = null;

            foreach (BLEDevice bleDevice in m_bleDeviceWatcher.KnownDevices)
            {
				if (!bleDevice.IsPresent)
					continue;

                if (bleDevice.Name != AdvertisementName && bleDevice.Name != DFUAdvertisementName)
                    continue;

                if (this.Id == bleDevice.Id)
                    return;

                foundBLEDevice = bleDevice;

                break;
            }

            if (foundBLEDevice == null)
                return;

            this.DeviceInformation = foundBLEDevice.DeviceInformation;

			var bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(foundBLEDevice.Id);

            if (bluetoothLEDevice == null)
                return;

            bluetoothLEDevice.GattServicesChanged -= OnGattServicesChanged;
            bluetoothLEDevice.GattServicesChanged += OnGattServicesChanged;
            bluetoothLEDevice.ConnectionStatusChanged -= OnConnectionStatusChanged;
            bluetoothLEDevice.ConnectionStatusChanged += OnConnectionStatusChanged;
            bluetoothLEDevice.NameChanged -= OnNameChanged;
            bluetoothLEDevice.NameChanged += OnNameChanged;

            OnGattServicesChanged(bluetoothLEDevice, null);
        }

        private void OnNameChanged(BluetoothLEDevice sender, object args)
        {
            this.Name = sender.Name;

            LogFile.WriteLine("OnNameChanged: {0}", this.Name);
        }

        private async void OnGattServicesChanged(BluetoothLEDevice sender, object args)
        {
            LogFile.WriteLine("OnGattServicesChanged");

            try
            {
                GattDeviceServicesResult deviceServicesResult = await sender.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (deviceServicesResult.Status != GattCommunicationStatus.Success)
                    return;

                foreach (var service in deviceServicesResult.Services)
                {
                    // Generic Attribute Service
                    if (service.Uuid.Equals(GattServiceUuids.GenericAttribute))
                    {
                        // Service Changed Characteristic
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsForUuidAsync(GattCharacteristicUuids.GattServiceChanged);

                        if (characteristicResult.Status == GattCommunicationStatus.Success && m_serviceChangedCharacteristic == null)
                        {
                            m_serviceChangedCharacteristic = characteristicResult.Characteristics.FirstOrDefault();

                            if (m_serviceChangedCharacteristic != null)
                            {
                                LogFile.WriteLine("GattServiceChanged");

                                m_serviceChangedCharacteristic.ValueChanged += ServiceChangedCharacteristic_OnValueChanged;

                                var currentDescriptorValue = await m_serviceChangedCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                                if (currentDescriptorValue.Status == GattCommunicationStatus.Success && currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Indicate)
                                {
                                    var status = await m_serviceChangedCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);

                                    if (status == GattCommunicationStatus.Success)
                                    {
                                    }
                                }
                            }
                        }
                    }

                    // Device Information Service
                    if (service.Uuid.Equals(GattServiceUuids.DeviceInformation))
                    {
                        GetDeviceInformationString(service, "ModelNumber", GattCharacteristicUuids.ModelNumberString);
                        GetDeviceInformationString(service, "SerialNumber", GattCharacteristicUuids.SerialNumberString);
                        GetDeviceInformationString(service, "FirmwareNumber", GattCharacteristicUuids.FirmwareRevisionString);
                        GetDeviceInformationString(service, "HardwareNumber", GattCharacteristicUuids.HardwareRevisionString);
                        GetDeviceInformationString(service, "SoftwareNumber", GattCharacteristicUuids.SoftwareRevisionString);
                        GetDeviceInformationString(service, "ManufacturerName", GattCharacteristicUuids.ManufacturerNameString);
                    }

                    // Battery Service
                    if (service.Uuid.Equals(GattServiceUuids.Battery))
                    {
                        // Battery Level Characteristic
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsForUuidAsync(GattCharacteristicUuids.BatteryLevel);

                        if (characteristicResult.Status == GattCommunicationStatus.Success && m_batteryLevelCharacteristic == null)
                        {
                            m_batteryLevelCharacteristic = characteristicResult.Characteristics.FirstOrDefault();

                            if (m_batteryLevelCharacteristic != null)
                            {
                                LogFile.WriteLine("BatteryLevel");

                                m_batteryLevelCharacteristic.ValueChanged += BatteryLevelCharacteristic_OnValueChanged;

                                var currentDescriptorValue = await m_batteryLevelCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                                if (currentDescriptorValue.Status == GattCommunicationStatus.Success && currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify)
                                {
                                    var status = await m_batteryLevelCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                                    if (status == GattCommunicationStatus.Success)
                                    {
                                    }
                                }
                            }
                        }
                    }

                    // Nordic Secure DFU Service
                    if (service.Uuid.Equals(NordicSecureDFUService))
                    {
                        // Nordic Buttonless DFU Characteristic
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsForUuidAsync(NordicButtonlessDFUCharacteristicUuid);

                        if (characteristicResult.Status == GattCommunicationStatus.Success && m_nordicButtonlessDFUCharacteristic == null)
                        {
                            m_nordicButtonlessDFUCharacteristic = characteristicResult.Characteristics.FirstOrDefault();

                            if (m_nordicButtonlessDFUCharacteristic != null)
                            {
                                LogFile.WriteLine("NordicButtonlessDFUCharacteristicUuid");

                                m_nordicButtonlessDFUCharacteristic.ValueChanged += NordicButtonlessDFUCharacteristic_OnValueChanged;

                                var currentDescriptorValue = await m_nordicButtonlessDFUCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                                if (currentDescriptorValue.Status == GattCommunicationStatus.Success && currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Indicate)
                                {
                                    var status = await m_nordicButtonlessDFUCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);

                                    if (status == GattCommunicationStatus.Success)
                                    {
                                    }
                                }
                            }
                        }

                        // Nordic DFU Control Point Characteristic
                        characteristicResult = await service.GetCharacteristicsForUuidAsync(NordicDFUControlPointCharacteristicUuid);

                        if (characteristicResult.Status == GattCommunicationStatus.Success && m_nordicDFUControlPointCharacteristic == null)
                        {
                            m_nordicDFUControlPointCharacteristic = characteristicResult.Characteristics.FirstOrDefault();

                            if (m_nordicDFUControlPointCharacteristic != null)
                            {
                                LogFile.WriteLine("NordicDFUControlPointCharacteristicUuid");

                                m_nordicDFUControlPointCharacteristic.ValueChanged += NordicDFUControlPointCharacteristic_OnValueChanged;

                                var currentDescriptorValue = await m_nordicDFUControlPointCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                                if (currentDescriptorValue.Status == GattCommunicationStatus.Success && currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify)
                                {
                                    var status = await m_nordicDFUControlPointCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                                    if (status == GattCommunicationStatus.Success)
                                    {
                                    }
                                }
                            }
                        }

                        // Nordic DFU Packet Characteristic
                        characteristicResult = await service.GetCharacteristicsForUuidAsync(NordicDFUPacketCharacteristicUuid);

                        if (characteristicResult.Status == GattCommunicationStatus.Success && m_nordicDFUPacketCharacteristic == null)
                        {
                            m_nordicDFUPacketCharacteristic = characteristicResult.Characteristics.FirstOrDefault();

                            if (m_nordicDFUPacketCharacteristic != null)
                            {
                                LogFile.WriteLine("NordicDFUPacketCharacteristicUuid");

                                m_nordicDFUPacketCharacteristic.ValueChanged += NordicDFUPacketCharacteristic_OnValueChanged;

                                var currentDescriptorValue = await m_nordicDFUPacketCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();

                                if (currentDescriptorValue.Status == GattCommunicationStatus.Success && currentDescriptorValue.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify)
                                {
                                    var status = await m_nordicDFUPacketCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                                    if (status == GattCommunicationStatus.Success)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            switch(sender.ConnectionStatus)
            {
                case BluetoothConnectionStatus.Disconnected:
                    LogFile.WriteLine("OnConnectionStatusChanged: Disconnected");
                    IsConnected = false;
                    break;
                case BluetoothConnectionStatus.Connected:
                    LogFile.WriteLine("OnConnectionStatusChanged: Connected");
                    IsConnected = true;
                    break;
            }
        }

        private async void GetDeviceInformationString(GattDeviceService service, string propertyName, Guid characteristicUuid)
        {
            try
            {
                GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsForUuidAsync(characteristicUuid);

                if (characteristicResult.Status == GattCommunicationStatus.Success)
                {
                    GattCharacteristic gattCharacteristic = characteristicResult.Characteristics.FirstOrDefault();

                    if (gattCharacteristic != null)
                    {
                        GattReadResult readResult = await gattCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);

                        if (readResult.Status == GattCommunicationStatus.Success)
                        {
                            PropertyInfo propertyInfo = typeof(BLEDevice).GetProperty(propertyName);
                            propertyInfo.SetValue(this, m_bleDeviceWatcher.BufferToString(readResult.Value), null);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void ServiceChangedCharacteristic_OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs valueChangedArgs)
        {
            LogFile.WriteLine("ServiceChangedCharacteristic_OnValueChanged");

            InitializeBLEDevice();
        }

        private void BatteryLevelCharacteristic_OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs valueChangedArgs)
        {
            this.BatteryLevel = m_bleDeviceWatcher.BufferToByte(valueChangedArgs.CharacteristicValue);
        }

        private void NordicButtonlessDFUCharacteristic_OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs valueChangedArgs)
        {
            if (valueChangedArgs.CharacteristicValue.Length != 3)
            {
                return;
            }

            byte[] responseBytes = new byte[3];

            using (var dataReader = DataReader.FromBuffer(valueChangedArgs.CharacteristicValue))
                dataReader.ReadBytes(responseBytes);

            DfuInit.ble_dfu_buttonless_op_code_t opCode = (DfuInit.ble_dfu_buttonless_op_code_t)responseBytes[1];
            DfuInit.ble_dfu_buttonless_rsp_code_t rspCode = (DfuInit.ble_dfu_buttonless_rsp_code_t)responseBytes[2];

            if (responseBytes[0] != (byte)DfuInit.ble_dfu_buttonless_op_code_t.DFU_OP_RESPONSE_CODE)
            {
                return;
            }

            if (rspCode == DfuInit.ble_dfu_buttonless_rsp_code_t.DFU_RSP_SUCCESS)
            {
                LogFile.WriteLine("NordicButtonlessDFUCharacteristic_OnValueChanged Success!");
            }
            else
            {
                LogFile.WriteLine("NordicButtonlessDFUCharacteristic_OnValueChanged Failure! ({0})", rspCode);
            }
        }

        private void NordicDFUControlPointCharacteristic_OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs valueChangedArgs)
        {
            //GattReadResult gattReadResult = await m_nordicDFUControlPointCharacteristic.ReadValueAsync();

            //ProcessNordicDFUControlPointCharacteristicResponse(gattReadResult.Value);

            byte[] responseBytes = new byte[valueChangedArgs.CharacteristicValue.Length];

            using (DataReader dataReader = DataReader.FromBuffer(valueChangedArgs.CharacteristicValue))
                dataReader.ReadBytes(responseBytes);

            Type responseType = typeof(DfuReq.nrf_dfu_response_t);
            byte[] buffer = new byte[Marshal.SizeOf(responseType)];
            Array.Copy(responseBytes, 0, buffer, 0, responseBytes.Length);
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            DfuReq.nrf_dfu_response_t response = (DfuReq.nrf_dfu_response_t)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), responseType);
            handle.Free();

            LogFile.WriteLine("NordicDFUControlPointCharacteristic Response: {0}", BitConverter.ToString(responseBytes));

            lock (m_firmwareModeList)
            {
                ProcessFirmwareModeResponse(response);
                /* FirmwareModeNode firmwareMode = m_firmwareModeList.First();

                if (firmwareMode.Mode == FirmwareMode.TransferData)
                    ProcessFirmwareModeResponse(firmwareMode.FirmwareModeList, response);
                else
                    ProcessFirmwareModeResponse(m_firmwareModeList, response); */
            }
        }

        private void NordicDFUPacketCharacteristic_OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs valueChangedArgs)
        {
            LogFile.WriteLine("NordicDFUPacketCharacteristic_OnValueChanged");
        }

        public override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            //LogFile.WriteLine("OnPropertyChanged: {0}", name);
        }

        private void ProcessFirmwareModeRequest(FirmwareModeNode firmwareMode)
        {
            switch (firmwareMode.Mode)
            {
                case FirmwareMode.SetPRN:
                    {
                        if (firmwareMode.IsRunning)
                            return;

                        m_prnCount = (ushort)(m_firmwareType == FirmwareType.InitPacket ? 0 : 10);

                        if (NordicDFU.WriteSetPRN(m_nordicDFUControlPointCharacteristic, m_prnCount).Result)
                        {
                            LogFile.WriteLine("WriteSetPRN Success!");

                            firmwareMode.Start();
                        }
                        else
                        {
                            LogFile.WriteLine("WriteSetPRN Failure!");

                            StopDFU();
                        }
                    }
                    break;
                case FirmwareMode.GetMTU:
                    {
                        if (firmwareMode.IsRunning)
                            return;

                        if (NordicDFU.WriteGetMTU(m_nordicDFUControlPointCharacteristic).Result)
                        {
                            LogFile.WriteLine("WriteGetMTU Success!");

                            firmwareMode.Start();
                        }
                        else
                        {
                            LogFile.WriteLine("WriteGetMTU Failure!");

                            StopDFU();
                        }
                    }
                    break;
                case FirmwareMode.GetCRC32:
                    {
                        if (firmwareMode.IsRunning)
                            return;

                        if (NordicDFU.WriteGetCRC32(m_nordicDFUControlPointCharacteristic).Result)
                        {
                            LogFile.WriteLine("WriteGetCRC32 Success!");

                            firmwareMode.Start();
                        }
                        else
                        {
                            LogFile.WriteLine("WriteGetCRC32 Failure!");

                            StopDFU();
                        }
                    }
                    break;
                case FirmwareMode.SelectCommand:
                    {
                        if (firmwareMode.IsRunning)
                            return;

                        FirmwareNode firmware = m_firmwareList[m_firmwareIndex];
                        m_data = (m_firmwareType == FirmwareType.InitPacket ? firmware.InitData : firmware.ImageData);
                        m_dataOffset = 0;
                        m_crc = new Crc32();

                        if (NordicDFU.WriteSelectCommand(m_nordicDFUControlPointCharacteristic, m_firmwareType).Result)
                        {
                            LogFile.WriteLine("WriteSelectCommand Success!");

                            firmwareMode.Start();
                        }
                        else
                        {
                            LogFile.WriteLine("WriteSelectCommand Failure!");

                            StopDFU();
                        }
                    }
                    break;
                case FirmwareMode.CreateCommand:
                    {
                        if (firmwareMode.IsRunning)
                            return;

                        m_prnIndex = 0;

                        if (NordicDFU.WriteCreateCommand(m_nordicDFUControlPointCharacteristic, m_data, m_firmwareType, Math.Min(m_blockSize, m_data.Length - m_dataOffset)).Result)
                        {
                            LogFile.WriteLine("WriteCreateCommand Success!");

                            firmwareMode.Start();
                        }
                        else
                        {
                            LogFile.WriteLine("WriteCreateCommand Failure!");

                            StopDFU();
                        }
                    }
                    break;
                case FirmwareMode.TransferData:
                    {
                        int blockIndex = (m_dataOffset / m_blockSize) + 1;
                        int blockOffset = blockIndex * m_blockSize;
                        int packetLength = Math.Min(Math.Min(m_maxPacketLength, blockOffset - m_dataOffset), m_data.Length - m_dataOffset);
                        byte[] packetData = new byte[packetLength];
                        System.Buffer.BlockCopy(m_data, m_dataOffset, packetData, 0, packetData.Length);

                        if (NordicDFU.WriteDataPacket(m_nordicDFUPacketCharacteristic, packetData).Result)
                        {
                            LogFile.WriteLine("PRN {0:D2} [BLK {1} / {2}] {3:X6}: {4}", m_prnIndex, blockIndex, (int)Math.Ceiling((float)m_data.Length / m_blockSize), m_dataOffset, BitConverter.ToString(packetData));

                            m_crc.AddData(packetData);

                            m_dataOffset += packetLength;

                            if (m_dataOffset == m_data.Length)
                            {
                                LogFile.WriteLine("WriteDataPacket Finished Sending Data!");

                                m_firmwareModeList.RemoveAt(0);
                                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.GetCRC32));

                                return;
                            }

                            if ((m_dataOffset % m_blockSize) == 0)
                            {
                                LogFile.WriteLine("WriteDataPacket Reached Block!");

                                m_firmwareModeList.RemoveAt(0);

                                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.GetCRC32));

                                return;
                            }

                            if (m_prnCount != 0)
                            {
                                if (++m_prnIndex == m_prnCount)
                                {
                                    LogFile.WriteLine("WriteDataPacket Reached PRN!");

                                    m_prnIndex = 0;

                                    firmwareMode.Stop();

                                    FirmwareModeNode getCRC32 = new FirmwareModeNode(FirmwareMode.GetCRC32);
                                    getCRC32.Start();

                                    m_firmwareModeList.Insert(0, getCRC32);

                                    return;
                                }
                            }

                            firmwareMode.Start();
                        }
                        else
                        {
                            LogFile.WriteLine("WriteDataPacket Failure!");

                            StopDFU();
                        }
                    }
                    break;
                case FirmwareMode.ExecuteCommand:
                    {
                        if (firmwareMode.IsRunning)
                            return;

                        if (NordicDFU.WriteExecuteCommand(m_nordicDFUControlPointCharacteristic).Result)
                        {
                            LogFile.WriteLine("WriteExecuteCommand Success!");

                            firmwareMode.Start();
                        }
                        else
                        {
                            LogFile.WriteLine("WriteExecuteCommand Failure!");

                            StopDFU();
                        }
                    }
                    break;
            }
        }

        private void ProcessFirmwareModeResponse(DfuReq.nrf_dfu_response_t response)
        {
            bool success = (response.response == DfuReq.nrf_dfu_op_t.NRF_DFU_OP_RESPONSE && response.result == DfuReq.nrf_dfu_result_t.NRF_DFU_RES_CODE_SUCCESS);

            FirmwareModeNode firmwareMode = m_firmwareModeList.First();

            switch (response.request)
            {
                case DfuReq.nrf_dfu_op_t.NRF_DFU_OP_OBJECT_CREATE:
                    {
                        if (firmwareMode.Mode == FirmwareMode.CreateCommand)
                            m_firmwareModeList.RemoveAt(0);

                        if (success)
                        {
                            LogFile.WriteLine("NRF_DFU_OP_OBJECT_CREATE Success!");

                            m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.TransferData));
                        }
                        else
                        {
                            LogFile.WriteLine("NRF_DFU_OP_OBJECT_CREATE Failure! ({0})", NordicDFU.GetResponseErrorString(response));

                            StopDFU();
                        }
                    }
                    break;
                case DfuReq.nrf_dfu_op_t.NRF_DFU_OP_RECEIPT_NOTIF_SET:
                    {
                        if (firmwareMode.Mode == FirmwareMode.SetPRN)
                            m_firmwareModeList.RemoveAt(0);

                        if (success)
                        {
                            LogFile.WriteLine("NRF_DFU_OP_RECEIPT_NOTIF_SET Success!");
                        }
                        else
                        {
                            LogFile.WriteLine("NRF_DFU_OP_RECEIPT_NOTIF_SET Failure! ({0})", NordicDFU.GetResponseErrorString(response));

                            StopDFU();
                        }
                    }
                    break;
                case DfuReq.nrf_dfu_op_t.NRF_DFU_OP_CRC_GET:
                    {
                        if (firmwareMode.Mode == FirmwareMode.GetCRC32)
                            m_firmwareModeList.RemoveAt(0);

                        if (success)
                        {
                            LogFile.WriteLine("NRF_DFU_OP_CRC_GET Success! offset: {0} ({1} / {2}) crc: 0x{3:X8} (0x{4:X8})", response.type.crc.offset, m_dataOffset, m_data.Length, response.type.crc.crc, m_crc.Crc32Value);

                            if (response.type.crc.crc != m_crc.Crc32Value)
                            {
                                LogFile.WriteLine("NRF_DFU_OP_CRC_GET CRC Error!");

                                ResetDFU();

                                return;
                            }

                            if (m_dataOffset == m_data.Length)
                            {
                                LogFile.WriteLine("NRF_DFU_OP_CRC_GET Data Finished! Offset and CRC Match!");

                                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.ExecuteCommand));
                            }
                            else if ((response.type.crc.offset % m_blockSize) == 0)
                            {
                                LogFile.WriteLine("NRF_DFU_OP_CRC_GET Block Finished! Offset and CRC Match!");

                                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.ExecuteCommand));
                                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.CreateCommand));
                            }
                        }
                        else
                        {
                            LogFile.WriteLine("NRF_DFU_OP_CRC_GET Falure! ({0})", NordicDFU.GetResponseErrorString(response));

                            StopDFU();
                        }
                    }
                    break;
                case DfuReq.nrf_dfu_op_t.NRF_DFU_OP_OBJECT_EXECUTE:
                    {
                        if (firmwareMode.Mode == FirmwareMode.ExecuteCommand)
                            m_firmwareModeList.RemoveAt(0);

                        if (success)
                        {
                            LogFile.WriteLine("NRF_DFU_OP_OBJECT_EXECUTE Success!");

                            if (m_dataOffset == m_data.Length)
                            {
                                LogFile.WriteLine("NRF_DFU_OP_OBJECT_EXECUTE Data Finished! Offset and CRC Match!");

                                if (m_firmwareType == FirmwareType.FirmwareImage)
                                {
                                    m_firmwareIndex++;

                                    if (m_firmwareIndex == m_firmwareList.Count)
                                    {
                                        LogFile.WriteLine("Done.");

                                        StopDFU();

                                        return;
                                    }

                                    m_firmwareType = FirmwareType.InitPacket;
                                }
                                else if (m_firmwareType == FirmwareType.InitPacket)
                                {
                                    m_firmwareType = FirmwareType.FirmwareImage;
                                }

                                ResetDFU();
                            }
                        }
                        else
                        {
                            LogFile.WriteLine("NRF_DFU_OP_OBJECT_EXECUTE Failure! ({0})", NordicDFU.GetResponseErrorString(response));

                            StopDFU();
                        }
                    }
                    break;
                case DfuReq.nrf_dfu_op_t.NRF_DFU_OP_OBJECT_SELECT:
                    {
                        if (firmwareMode.Mode == FirmwareMode.SelectCommand)
                            m_firmwareModeList.RemoveAt(0);

                        if (success)
                        {
                            LogFile.WriteLine("NRF_DFU_OP_OBJECT_SELECT Success! max_size: {0} offset: {1} crc: {2:X8}", response.type.select.max_size, response.type.select.offset, response.type.select.crc);

                            m_blockSize = (int)response.type.select.max_size;

                            // If an init packet with the same length and CRC is present, do not transfer anything and skip to the Execute command
                            if (response.type.select.offset == m_data.Length && response.type.select.crc == m_crc.Crc32Value)
                            {
                                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.ExecuteCommand));
                            }
                            // If there is no init packet or the init packet is invalid, create a new object
                            else if (response.type.select.offset == 0 || response.type.select.offset > m_data.Length || response.type.select.crc != m_crc.Crc32Value)
                            {
                                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.CreateCommand));
                            }
                        }
                        else
                        {
                            LogFile.WriteLine("NRF_DFU_OP_OBJECT_SELECT Failure! ({0})", NordicDFU.GetResponseErrorString(response));

                            StopDFU();
                        }
                    }
                    break;
                case DfuReq.nrf_dfu_op_t.NRF_DFU_OP_MTU_GET:
                    {
                        if (success)
                        {
                            LogFile.WriteLine("NRF_DFU_OP_MTU_GET Success! size: {0} maxPacketLength: {1}", response.type.mtu.size, m_maxPacketLength);

                            m_maxPacketLength = (response.type.mtu.size - DfuReq.GATT_HEADER_LEN);
                        }
                        else
                        {
                            LogFile.WriteLine("NRF_DFU_OP_MTU_GET Failure! ({0})", NordicDFU.GetResponseErrorString(response));
                        }

                        if (firmwareMode.Mode == FirmwareMode.GetMTU)
                            m_firmwareModeList.RemoveAt(0);
                    }
                    break;
            }
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (m_firmwareModeList)
            {
                if (IsInDFUMode && IsConnected)
                {
                    if (m_firmwareModeList.Count == 0)
                        return;

                    FirmwareModeNode firmwareMode = m_firmwareModeList.First();

                    if (firmwareMode.IsTimeout)
                    {
                        LogFile.WriteLine("{0} Timeout ({1})", firmwareMode.Mode, firmwareMode.Elapsed);

                        StopDFU();

                        return;
                    }
            
                    ProcessFirmwareModeRequest(firmwareMode);

                    if (ProgressChanged != null)
                        ProgressChanged(this, new ProgressChangedEventArgs((int)(((float)m_dataOffset / m_data.Length) * 100.0), null));

                    if (firmwareMode.Mode == FirmwareMode.ExecuteCommand &&
                        m_firmwareType == FirmwareType.FirmwareImage &&
                        m_dataOffset == m_data.Length)
                    {
                        m_serviceChangedCharacteristic = null;
                        m_batteryLevelCharacteristic = null;
                        m_nordicButtonlessDFUCharacteristic = null;
                        m_nordicDFUControlPointCharacteristic = null;
                        m_nordicDFUPacketCharacteristic = null;

                        LogFile.WriteLine("Waiting for 5 Seconds...");

                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }
            }
        }

        public async void StartDFU()
        {
            // https://devzone.nordicsemi.com/nordic/nordic-blog/b/blog/posts/getting-started-with-nordics-secure-dfu-bootloader
            // https://infocenter.nordicsemi.com/topic/sdk_nrf5_v16.0.0/lib_bootloader.html?cp=7_1_3_5_0_2#lib_bootloader_dfu_mode
            // https://infocenter.nordicsemi.com/topic/sdk_nrf5_v16.0.0/lib_secure_boot.html
            // https://infocenter.nordicsemi.com/topic/sdk_nrf5_v16.0.0/lib_dfu_transport_ble.html?cp=7_1_3_5_2_2
            //
            // The device should start and advertise as Nordic_Buttonless. Then you can test using nRF Connect app.
            // Observe that the app automatically bond when you enable indication, and then when you start DFU it switch to
            // bootloader mode without changing the address (+1 the address) when switching to bootloader. And then to see
            // that the DFU process is being done when the connection status is bonded. 

            // 1. Open Firmware
            // 2. Confirm two images (bootloader+app / softdevice)
            // 3. Set Advertisment Name (only without bond forwarding)
            // 4. Enter DFU Mode
            // 5. Write PRN
            // 6. Read MTU
            // 7. Set data / offset / crc to softdevice
            // 8. Transfer init packet
            //    - Select command
            //    - Read response (max_size, offset, CRC32)
            //    - Create command (size is data.Length)
            //    - Read response
            //    - Transfer data
            //    - Read response (PRN, offset, CRC32)
            //    - Calculate CRC
            //    - Read response
            //    - Execute command
            //    - Read response
            // 9. Transfer firmware image
            //    - Repeat until all objects are transferred
            // 10. Verify CRC

            if (m_nordicButtonlessDFUCharacteristic == null)
            {
                LogFile.WriteLine("m_nordicButtonlessDFUCharacteristic == null");

                return;
            }

            LogFile.WriteLine("Entering DFU Mode...");

            // In Default Mode
#if !NRF_DFU_BLE_BUTTONLESS_SUPPORTS_BONDS
            // We can only set advertisment name without bond forwarding

            if (await WriteSetAdvertismentName(DFUAdvertisementName))
            {
                LogFile.WriteLine("WriteSetAdvertismentName Success!");
            }
            else
            {
                LogFile.WriteLine("WriteSetAdvertismentName Failure!");
                return;
            }
#endif

            if (await NordicDFU.WriteEnterDFUMode(m_nordicButtonlessDFUCharacteristic))
            {
                LogFile.WriteLine("WriteEnterDFUMode Success!");
            }
            else
            {
                LogFile.WriteLine("WriteEnterDFUMode Failure!");
                return;
            }

            //await Task.Delay(TimeSpan.FromSeconds(5));
            m_firmwareIndex = 0;

            ResetDFU();
            m_timer.Start();
        }

        private void StopDFU()
        {
            m_timer.Stop();

            lock (m_firmwareModeList)
            {
                if (m_firmwareModeList.Count > 0)
                {
                    FirmwareModeNode firmwareMode = m_firmwareModeList.First();
                    firmwareMode.Stop();
                }
            }
        }

        private void ResetDFU()
        {
            lock (m_firmwareModeList)
            {
                m_firmwareModeList.Clear();
                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.GetMTU));
                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.SetPRN));
                m_firmwareModeList.Add(new FirmwareModeNode(FirmwareMode.SelectCommand));
            }
        }

        public bool TryOpenAppDfuPackage(string appDFUPackagePath)
        {
            bool result = true;

            m_firmwareList = new List<FirmwareNode>();
            m_firmwareTypes = FirmwareTypes.None;

            result = NordicDFU.TryOpenAppDfuPackage(appDFUPackagePath, out m_firmwareList);

            if (!result)
                return false;

            for (int i = 0; i < m_firmwareList.Count; i++)
            {
                FirmwareNode firmware = m_firmwareList[i];

                LogFile.WriteLine("Image Index: {0}", i);
                LogFile.WriteLine(firmware.ToString());

                if (!firmware.IsVerified || !firmware.HasHashMatch)
                {
                    LogFile.WriteLine("Firmware verification check failure");

                    result = false;
                    break;
                }

                if ((firmware.Packet.SignedCommand.Command.Init.SdSize % 4) != 0 ||
                    (firmware.Packet.SignedCommand.Command.Init.BlSize % 4) != 0 ||
                    (firmware.Packet.SignedCommand.Command.Init.AppSize % 4) != 0)
                {
                    LogFile.WriteLine("Firmware is not word-aligned");

                    result = false;
                    break;
                }

                m_firmwareTypes |= (FirmwareTypes)(1 << (int)m_firmwareList[i].Packet.SignedCommand.Command.Init.Type);
            }

            return result;
        }

        public bool IsInDFUMode
        {
            get { return (m_nordicDFUControlPointCharacteristic != null && m_nordicDFUPacketCharacteristic != null); }
        }

        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    m_bleDeviceWatcher.StopBleDeviceWatcher();
                }

                // new shared cleanup logic
                m_disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
