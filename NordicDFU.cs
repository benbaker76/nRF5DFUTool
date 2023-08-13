// Copyright (c) 2020, Ben Baker
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using ProtoBuf;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace nRF5DFUTool
{
    public enum WritePacketStatus
    {
        None = 0,
        ReachedPRN = 1,
        ReachedBlock = 2,
        Finished = 3
    }

    [Flags]
    public enum FirmwareTypes
    {
        None = 0,
        Application = (1 << 0),
        Softdevice = (1 << 1),
        Bootloader = (1 << 2),
        Softdevice_Bootloader = (1 << 3),
        External_Application = (1 << 4),
    }

    public enum FirmwareMode
    {
        SetPRN,
        GetMTU,
        GetCRC32,
        SelectCommand,
        CreateCommand,
        TransferData,
        ExecuteCommand
    }

    public enum FirmwareStatus
    {
        Stopped,
        //Waiting,
        Running,
        //Finished
    }

    public enum FirmwareType
    {
        InitPacket,
        FirmwareImage
    }

    public class NordicDFU
    {
        public static async Task<bool> WriteSetAdvertismentName(GattCharacteristic nordicButtonlessDFUCharacteristic, string name)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteByte((byte)nRFDfuInit.ble_dfu_buttonless_op_code_t.DFU_OP_SET_ADV_NAME);
                    var writeBuffer = CryptographicBuffer.ConvertStringToBinary(name, BinaryStringEncoding.Utf8);
                    dataWriter.WriteByte((byte)writeBuffer.Length);
                    dataWriter.WriteBuffer(writeBuffer);

                    IBuffer buffer = dataWriter.DetachBuffer();

                    LogFile.WriteLine("WriteSetAdvertismentName: {0}", BitConverter.ToString(WindowsRuntimeBufferExtensions.ToArray(buffer)));

                    return (await nordicButtonlessDFUCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteSetAdvertismentName Error {0}", ex.Message);
            }

            return false;
        }

        public static async Task<bool> WriteEnterDFUMode(GattCharacteristic nordicButtonlessDFUCharacteristic)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteByte((byte)nRFDfuInit.ble_dfu_buttonless_op_code_t.DFU_OP_ENTER_BOOTLOADER);

                    IBuffer buffer = dataWriter.DetachBuffer();

                    LogFile.WriteLine("WriteEnterDFUMode: {0}", BitConverter.ToString(WindowsRuntimeBufferExtensions.ToArray(buffer)));

                    return (await nordicButtonlessDFUCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteEnterDFUMode Error {0}", ex.Message);
            }

            return false;
        }

        public static async Task<bool> WriteSetPRN(GattCharacteristic nordicDFUControlPointCharacteristic, ushort prn)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteByte((byte)nRFDfuReq.nrf_dfu_op_t.NRF_DFU_OP_RECEIPT_NOTIF_SET);
                    dataWriter.WriteUInt16(prn);

                    IBuffer buffer = dataWriter.DetachBuffer();

                    LogFile.WriteLine("WriteSetPRN: {0}", BitConverter.ToString(WindowsRuntimeBufferExtensions.ToArray(buffer)));

                    return (await nordicDFUControlPointCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteSetPRN Error {0}", ex.Message);
            }

            return false;
        }

        public static async Task<bool> WriteGetMTU(GattCharacteristic nordicDFUControlPointCharacteristic)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteByte((byte)nRFDfuReq.nrf_dfu_op_t.NRF_DFU_OP_MTU_GET);

                    IBuffer buffer = dataWriter.DetachBuffer();

                    LogFile.WriteLine("WriteGetMTU: {0}", BitConverter.ToString(WindowsRuntimeBufferExtensions.ToArray(buffer)));

                    return (await nordicDFUControlPointCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteGetMTU Error {0}", ex.Message);
            }

            return false;
        }

        public static async Task<bool> WriteSelectCommand(GattCharacteristic nordicDFUControlPointCharacteristic, FirmwareType firmwareType)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteByte((byte)nRFDfuReq.nrf_dfu_op_t.NRF_DFU_OP_OBJECT_SELECT);
                    dataWriter.WriteByte((byte)(firmwareType == FirmwareType.InitPacket ? nRFDfuReq.nrf_dfu_obj_type_t.NRF_DFU_OBJ_TYPE_COMMAND : nRFDfuReq.nrf_dfu_obj_type_t.NRF_DFU_OBJ_TYPE_DATA));

                    IBuffer buffer = dataWriter.DetachBuffer();

                    LogFile.WriteLine("WriteSelectCommand: {0}", BitConverter.ToString(WindowsRuntimeBufferExtensions.ToArray(buffer)));

                    return (await nordicDFUControlPointCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteSelectCommand Error {0}", ex.Message);
            }

            return false;
        }

        public static async Task<bool> WriteCreateCommand(GattCharacteristic nordicDFUControlPointCharacteristic, byte[] data, FirmwareType firmwareType, int maxSize)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteByte((byte)nRFDfuReq.nrf_dfu_op_t.NRF_DFU_OP_OBJECT_CREATE);
                    dataWriter.WriteByte((byte)(firmwareType == FirmwareType.InitPacket ? nRFDfuReq.nrf_dfu_obj_type_t.NRF_DFU_OBJ_TYPE_COMMAND : nRFDfuReq.nrf_dfu_obj_type_t.NRF_DFU_OBJ_TYPE_DATA));
                    dataWriter.WriteUInt32((uint)Math.Min(maxSize, data.Length));

                    IBuffer buffer = dataWriter.DetachBuffer();

                    LogFile.WriteLine("WriteCreateCommand: {0}", BitConverter.ToString(WindowsRuntimeBufferExtensions.ToArray(buffer)));

                    return (await nordicDFUControlPointCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteCreateCommand Error {0}", ex.Message);
            }

            return false;
        }

        public static async Task<bool> WriteExecuteCommand(GattCharacteristic nordicDFUControlPointCharacteristic)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteByte((byte)nRFDfuReq.nrf_dfu_op_t.NRF_DFU_OP_OBJECT_EXECUTE);

                    IBuffer buffer = dataWriter.DetachBuffer();

                    LogFile.WriteLine("WriteExecuteCommand: {0}", BitConverter.ToString(WindowsRuntimeBufferExtensions.ToArray(buffer)));

                    return (await nordicDFUControlPointCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteExecuteCommand Error {0}", ex.Message);
            }

            return false;
        }

        public static async Task<bool> WriteGetCRC32(GattCharacteristic nordicDFUControlPointCharacteristic)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteByte((byte)nRFDfuReq.nrf_dfu_op_t.NRF_DFU_OP_CRC_GET);

                    IBuffer buffer = dataWriter.DetachBuffer();

                    LogFile.WriteLine("WriteGetCRC32: {0}", BitConverter.ToString(WindowsRuntimeBufferExtensions.ToArray(buffer)));

                    return (await nordicDFUControlPointCharacteristic.WriteValueAsync(buffer, GattWriteOption.WriteWithResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteGetCRC32 Error {0}", ex.Message);
            }

            return false;
        }

        public static async Task<bool> WriteDataPacket(GattCharacteristic nordicDFUPacketCharacteristic, byte[] data)
        {
            try
            {
                using (var dataWriter = new DataWriter())
                {
                    dataWriter.ByteOrder = ByteOrder.LittleEndian;
                    dataWriter.WriteBytes(data);

                    return (await nordicDFUPacketCharacteristic.WriteValueAsync(dataWriter.DetachBuffer(), GattWriteOption.WriteWithoutResponse) == GattCommunicationStatus.Success);
                }
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteDataPacket Error {0}", ex.Message);
            }

            return false;
        }

        public static string GetResponseErrorString(nRFDfuReq.nrf_dfu_response_t response)
        {
            return (response.result == nRFDfuReq.nrf_dfu_result_t.NRF_DFU_RES_CODE_EXT_ERROR ? response.ext_error.ToString() : response.result.ToString());
        }

        public static bool TryOpenAppDfuPackage(string appDFUPackagePath, out List<FirmwareNode> firmwareList)
        {
            firmwareList = new List<FirmwareNode>();

            try
            {
                string tempPath = Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
                string manifestFileName = Path.Combine(tempPath, "manifest.json");

                Directory.CreateDirectory(tempPath);

                ZipFile.ExtractToDirectory(appDFUPackagePath, tempPath);

                string manifestString = File.ReadAllText(manifestFileName);
                JObject jObject = JObject.Parse(manifestString);
                JToken manifestJToken = jObject.SelectToken("manifest");
                JEnumerable<JToken> list = manifestJToken.Children();
                JToken[] jtokenArray = list.ToArray();

                for (int i = 0; i < jtokenArray.Length; i++)
                {
                    JProperty item = (JProperty)jtokenArray[i];
                    string entryName = item.Name; // bootloader, application, softdevice
                    JToken entryJToken = item.Value;
                    string datFile = entryJToken.SelectToken("dat_file").ToString();
                    string binFile = entryJToken.SelectToken("bin_file").ToString();
                    string datFileName = Path.Combine(tempPath, datFile);
                    string binFileName = Path.Combine(tempPath, binFile);
                    byte[] datArray = File.ReadAllBytes(datFileName);
                    byte[] binArray = File.ReadAllBytes(binFileName);

                    firmwareList.Add(new FirmwareNode(entryName, datFile, binFile, datArray, binArray));
                }

                Directory.Delete(tempPath, true);

                firmwareList.Sort();
            }
            catch (Exception ex)
            {
                LogFile.WriteLine("WriteDataPacket Error {0}", ex.Message);

                return false;
            }

            return true;
        }
    }

    public class FirmwareModeNode
    {
        public FirmwareMode Mode;
        public FirmwareStatus Status;
        public int Timeout;

        private Stopwatch m_stopWatch = null;

        public FirmwareModeNode(FirmwareMode mode)
        {
            Mode = mode;
            Status = FirmwareStatus.Stopped;
            Timeout = 5000;
   
            m_stopWatch = new Stopwatch();
        }

        public void Start()
        {
            m_stopWatch.Reset();
            m_stopWatch.Start();
            Status = FirmwareStatus.Running;
        }

        public void Stop()
        {
            m_stopWatch.Stop();
            Status = FirmwareStatus.Stopped;
        }

        public bool IsTimeout
        {
            get { return (Timeout != 0 && m_stopWatch.Elapsed.TotalMilliseconds >= Timeout); }
        }

        public bool IsStopped
        {
            get { return (Status == FirmwareStatus.Stopped); }
        }

        public bool IsRunning
        {
            get { return (Status == FirmwareStatus.Running); }
        }

        public TimeSpan Elapsed
        {
            get { return m_stopWatch.Elapsed; }
        }
    }

    public class FirmwareNode : IComparable<FirmwareNode>
    {
        public Packet Packet;
        public string Name;
        public string DatFile;
        public string BinFile;
        public byte[] InitData;
        public byte[] ImageData;
        public byte[] InitCommand;
        public byte[] Signature;
        public byte[] Sha256Hash;

        public FirmwareNode(string name, string datFile, string binFile, byte[] initData, byte[] imageData)
        {
            Name = name;
            DatFile = datFile;
            BinFile = binFile;
            InitData = initData;
            ImageData = imageData;

            using (MemoryStream packetStream = new MemoryStream(InitData))
            {
                Packet = Serializer.Deserialize<Packet>(packetStream);

                using (MemoryStream initCommandStream = new MemoryStream())
                {
                    Serializer.Serialize<InitCommand>(initCommandStream, Packet.SignedCommand.Command.Init);
                    InitCommand = initCommandStream.ToArray();
                }
            }

            Signature = NordicEncryption.DerEncodeSignature(Packet.SignedCommand.Signature);
            Sha256Hash = NordicEncryption.GetSHA256Hash(imageData);
        }

        public int CompareTo(FirmwareNode other)
        {
            return -this.Packet.SignedCommand.Command.Init.Type.CompareTo(other.Packet.SignedCommand.Command.Init.Type);
        }

        public static string GetUInt32ArrayString(uint[] value)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (uint v in value)
                stringBuilder.Append(String.Format("0x{0:X8} ", v));

            return stringBuilder.ToString().TrimEnd();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(String.Format("Name: {0}", Name));
            stringBuilder.AppendLine(String.Format("Init Packet File: {0}", DatFile));
            stringBuilder.AppendLine(String.Format("Image File: {0}", BinFile));
            stringBuilder.AppendLine(String.Format("Firmware Version: 0x{0:X8} ({1})", Packet.SignedCommand.Command.Init.FwVersion, Packet.SignedCommand.Command.Init.FwVersion));
            stringBuilder.AppendLine(String.Format("Hardware Version: 0x{0:X8} ({1})", Packet.SignedCommand.Command.Init.HwVersion, Packet.SignedCommand.Command.Init.HwVersion));
            stringBuilder.AppendLine(String.Format("Softdevice Required: {0}", GetUInt32ArrayString(Packet.SignedCommand.Command.Init.SdReqs)));
            stringBuilder.AppendLine(String.Format("Type: {0}", Packet.SignedCommand.Command.Init.Type));
            stringBuilder.AppendLine(String.Format("Softdevice Size: {0}", Packet.SignedCommand.Command.Init.SdSize));
            stringBuilder.AppendLine(String.Format("Bootloader Size: {0}", Packet.SignedCommand.Command.Init.BlSize));
            stringBuilder.AppendLine(String.Format("Application Size: {0}", Packet.SignedCommand.Command.Init.AppSize));
            stringBuilder.AppendLine(String.Format("Is Debug: {0}", Packet.SignedCommand.Command.Init.IsDebug));
            //stringBuilder.AppendLine(String.Format("{0}", BitConverter.ToString(Packet.SignedCommand.Command.Init.Hash.hash)));
            //stringBuilder.AppendLine(String.Format("{0}", BitConverter.ToString(Sha256Hash)));
            stringBuilder.AppendLine(String.Format("{0} {1}", (HasHashMatch ? "✔ Hash" : "✘ Hash"), (IsVerified ? "✔ Signature" : "✘ Signature")));

            return stringBuilder.ToString();
        }

        public bool IsVerified
        {
            get { return NordicEncryption.VerifySignature(NordicBLEDevice.PublicKey, Signature, InitCommand); }
        }

        public bool HasHashMatch
        {
            get { return (BitConverter.ToString(Packet.SignedCommand.Command.Init.Hash.hash) == BitConverter.ToString(Sha256Hash)); }
        }
    }
}
