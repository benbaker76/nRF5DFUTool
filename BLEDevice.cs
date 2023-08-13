// * ----------------------------------------------------------------------------
// * Author: Ben Baker
// * Website: baker76.com
// * E-Mail: ben@baker76.com
// * Copyright (C) 2020 Ben Baker. All Rights Reserved.
// * ----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;

namespace nRF5DFUTool
{
    class BLEDevice : INotifyPropertyChanged, IDisposable
    {
        private string m_id = null;
        private string m_address = null;
        private string m_name = null;
        private bool m_canPair = false;
        private bool m_isPaired = false;
        private bool m_isPresent = false;
        private bool m_isConnected = false;
        private bool m_isConnectable = false;

        private string m_modelNumber = null;
        private string m_serialNumber = null;
        private string m_firmwareNumber = null;
        private string m_hardwareNumber = null;
        private string m_softwareNumber = null;
        private string m_manufacturerName = null;
        private byte m_batteryLevel = 0;

        private DeviceInformation m_deviceInfo;
        private SoftwareBitmap m_glyphBitmap;

        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_disposed = false;

        public BLEDevice()
        {
        }

        public BLEDevice(DeviceInformation deviceInfo)
        {
            DeviceInformation = deviceInfo;
        }

        ~BLEDevice()
        {
            Dispose(false);
        }

        private void Update(DeviceInformation deviceInfo)
        {
            this.Id = deviceInfo.Id;
            this.Name = deviceInfo.Name;
            this.CanPair = deviceInfo.Pairing.CanPair;
            this.IsPaired = deviceInfo.Pairing.IsPaired;

            object value;

            if (deviceInfo.Properties.TryGetValue("System.Devices.Aep.DeviceAddress", out value))
                this.Address = (string)value;

            if (deviceInfo.Properties.TryGetValue("System.Devices.Aep.IsPresent", out value))
                this.IsPresent = ((bool?)value) == true;

            if (deviceInfo.Properties.TryGetValue("System.Devices.Aep.IsConnected", out value))
                this.IsConnected = ((bool?)value) == true;

            if (deviceInfo.Properties.TryGetValue("System.Devices.Aep.Bluetooth.Le.IsConnectable", out value))
                this.IsConnectable = ((bool?)value) == true;

            UpdateGlyphBitmapImage(deviceInfo);
        }

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            m_deviceInfo.Update(deviceInfoUpdate);

            Update(m_deviceInfo);
        }

        private async void UpdateGlyphBitmapImage(DeviceInformation deviceInfo)
        {
            try
            {
                DeviceThumbnail deviceThumbnail = await deviceInfo.GetGlyphThumbnailAsync();
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(deviceThumbnail);
                GlyphBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            catch
            {
            }
        }

        public virtual void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string Id { get { return m_id; } set { m_id = value; OnPropertyChanged(nameof(Id)); } }
        public string Address { get { return m_address; } set { m_address = value; OnPropertyChanged(nameof(Address)); } }
        public string Name { get { return m_name; } set { m_name = value; OnPropertyChanged(nameof(Name)); } }
        public bool CanPair { get { return m_canPair; } set { m_canPair = value; OnPropertyChanged(nameof(CanPair)); } }
        public bool IsPaired { get { return m_isPaired; } set { m_isPaired = value; OnPropertyChanged(nameof(IsPaired)); } }
        public bool IsPresent { get { return m_isPresent; } set { m_isPresent = value; OnPropertyChanged(nameof(IsPresent)); } }
        public bool IsConnected { get { return m_isConnected; } set { m_isConnected = value; OnPropertyChanged(nameof(IsConnected)); } }
        public bool IsConnectable { get { return m_isConnectable; } set { m_isConnectable = value; OnPropertyChanged(nameof(IsConnectable)); } }
        public string ModelNumber { get { return m_modelNumber; } set { m_modelNumber = value; OnPropertyChanged(nameof(ModelNumber)); } }
        public string SerialNumber { get { return m_serialNumber; } set { m_serialNumber = value; OnPropertyChanged(nameof(SerialNumber)); } }
        public string FirmwareNumber { get { return m_firmwareNumber; } set { m_firmwareNumber = value; OnPropertyChanged(nameof(FirmwareNumber)); } }
        public string HardwareNumber { get { return m_hardwareNumber; } set { m_hardwareNumber = value; OnPropertyChanged(nameof(HardwareNumber)); } }
        public string SoftwareNumber { get { return m_softwareNumber; } set { m_softwareNumber = value; OnPropertyChanged(nameof(SoftwareNumber)); } }
        public string ManufacturerName { get { return m_manufacturerName; } set { m_manufacturerName = value; OnPropertyChanged(nameof(ManufacturerName)); } }
        public byte BatteryLevel { get { return m_batteryLevel; } set { m_batteryLevel = value; OnPropertyChanged(nameof(BatteryLevel)); } }
        public SoftwareBitmap GlyphBitmap { get { return m_glyphBitmap; } set { m_glyphBitmap = value; OnPropertyChanged(nameof(GlyphBitmap)); } }
        public DeviceInformation DeviceInformation { get { return m_deviceInfo; } set { m_deviceInfo = value; Update(m_deviceInfo); OnPropertyChanged(nameof(DeviceInformation)); } }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                }

                // m_disposed cleanup logic
                m_disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
