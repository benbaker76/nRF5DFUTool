// * ----------------------------------------------------------------------------
// * Author: Ben Baker
// * Website: baker76.com
// * E-Mail: ben@baker76.com
// * Copyright (C) 2020 Ben Baker. All Rights Reserved.
// * ----------------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace nRF5DFUTool
{
    public class DfuReq
    {
        public const int GATT_HEADER_LEN = 3;
        public const int NRF_SDH_BLE_GATT_MAX_MTU_SIZE = 23;
        public const int MAX_DFU_PKT_LEN = (NRF_SDH_BLE_GATT_MAX_MTU_SIZE - GATT_HEADER_LEN);

        public enum nrf_dfu_obj_type_t : byte
        {
            NRF_DFU_OBJ_TYPE_INVALID,
            NRF_DFU_OBJ_TYPE_COMMAND,
            NRF_DFU_OBJ_TYPE_DATA,
        }

        public enum nrf_dfu_op_t : byte
        {
            NRF_DFU_OP_PROTOCOL_VERSION = 0,
            NRF_DFU_OP_OBJECT_CREATE = 1,
            NRF_DFU_OP_RECEIPT_NOTIF_SET = 2,
            NRF_DFU_OP_CRC_GET = 3,
            NRF_DFU_OP_OBJECT_EXECUTE = 4,
            NRF_DFU_OP_OBJECT_SELECT = 6,
            NRF_DFU_OP_MTU_GET = 7,
            NRF_DFU_OP_OBJECT_WRITE = 8,
            NRF_DFU_OP_PING = 9,
            NRF_DFU_OP_HARDWARE_VERSION = 10,
            NRF_DFU_OP_FIRMWARE_VERSION = 11,
            NRF_DFU_OP_ABORT = 12,
            NRF_DFU_OP_RESPONSE = 96,
            NRF_DFU_OP_INVALID = 255,
        }

        public enum nrf_dfu_result_t : byte
        {
            NRF_DFU_RES_CODE_INVALID = 0,
            NRF_DFU_RES_CODE_SUCCESS = 1,
            NRF_DFU_RES_CODE_OP_CODE_NOT_SUPPORTED = 2,
            NRF_DFU_RES_CODE_INVALID_PARAMETER = 3,
            NRF_DFU_RES_CODE_INSUFFICIENT_RESOURCES = 4,
            NRF_DFU_RES_CODE_INVALID_OBJECT = 5,
            NRF_DFU_RES_CODE_UNSUPPORTED_TYPE = 7,
            NRF_DFU_RES_CODE_OPERATION_NOT_PERMITTED = 8,
            NRF_DFU_RES_CODE_OPERATION_FAILED = 10,
            NRF_DFU_RES_CODE_EXT_ERROR = 11,
        }

        public enum nrf_dfu_ext_error_code_t : byte
        {
            NRF_DFU_EXT_ERROR_NO_ERROR = 0,
            NRF_DFU_EXT_ERROR_INVALID_ERROR_CODE = 1,
            NRF_DFU_EXT_ERROR_WRONG_COMMAND_FORMAT = 2,
            NRF_DFU_EXT_ERROR_UNKNOWN_COMMAND = 3,
            NRF_DFU_EXT_ERROR_INIT_COMMAND_INVALID = 4,
            NRF_DFU_EXT_ERROR_FW_VERSION_FAILURE = 5,
            NRF_DFU_EXT_ERROR_HW_VERSION_FAILURE = 6,
            NRF_DFU_EXT_ERROR_SD_VERSION_FAILURE = 7,
            NRF_DFU_EXT_ERROR_SIGNATURE_MISSING = 8,
            NRF_DFU_EXT_ERROR_WRONG_HASH_TYPE = 9,
            NRF_DFU_EXT_ERROR_HASH_FAILED = 10,
            NRF_DFU_EXT_ERROR_WRONG_SIGNATURE_TYPE = 11,
            NRF_DFU_EXT_ERROR_VERIFICATION_FAILED = 12,
            NRF_DFU_EXT_ERROR_INSUFFICIENT_SPACE = 13,
        }

        public enum nrf_dfu_firmware_type_t : byte
        {
            NRF_DFU_FIRMWARE_TYPE_SOFTDEVICE = 0,
            NRF_DFU_FIRMWARE_TYPE_APPLICATION = 1,
            NRF_DFU_FIRMWARE_TYPE_BOOTLOADER = 2,
            NRF_DFU_FIRMWARE_TYPE_UNKNOWN = 255,
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_protocol_t
        {
            public byte version;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_memory_t
        {
            public uint rom_size;
            public uint ram_size;
            public uint rom_page_size;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_hardware_t
        {
            public uint part;
            public uint variant;
            public nrf_dfu_memory_t memory;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_firmware_t
        {
            public nrf_dfu_firmware_type_t type;
            public uint version;
            public uint addr;
            public uint len;
        }

        /* [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_select_t
        {
            public uint offset;
            public uint crc;
            public uint max_size;
        } */

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_select_t
        {
            public uint max_size;
            public uint offset;
            public uint crc;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_create_t
        {
            public uint offset;
            public uint crc;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_write_t
        {
            public uint offset;
            public uint crc;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_crc_t
        {
            public uint offset;
            public uint crc;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_ping_t
        {
            public byte id;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_response_mtu_t
        {
            public ushort size;
        }

        [StructLayoutAttribute(LayoutKind.Explicit)]
        public struct nrf_dfu_response_type_t
        {
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_protocol_t protocol;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_hardware_t hardware;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_firmware_t firmware;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_select_t select;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_create_t create;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_write_t write;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_crc_t crc;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_ping_t ping;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_response_mtu_t mtu;
        }

        [StructLayoutAttribute(LayoutKind.Explicit)]
        public struct nrf_dfu_response_t
        {
            [FieldOffsetAttribute(0)]
            public nrf_dfu_op_t response;
            [FieldOffsetAttribute(1)]
            public nrf_dfu_op_t request;
            [FieldOffsetAttribute(2)]
            public nrf_dfu_result_t result;
            [FieldOffsetAttribute(3)]
            public nrf_dfu_response_type_t type;
            [FieldOffsetAttribute(3)]
            public nrf_dfu_ext_error_code_t ext_error;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_request_firmware_t
        {
            public byte image_number;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_request_select_t
        {
            public uint object_type;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_request_create_t
        {
            public uint object_type;
            public uint object_size;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_request_write_t
        {
            [MarshalAsAttribute(UnmanagedType.LPStr)]
            public string p_data;
            public ushort len;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_request_ping_t
        {
            public byte id;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_request_mtu_t
        {
            public ushort size;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_request_prn_t
        {
            public uint target;
        }

        [StructLayoutAttribute(LayoutKind.Explicit)]
        public struct nrf_dfu_request_type_t
        {
            [FieldOffsetAttribute(0)]
            public nrf_dfu_request_firmware_t firmware;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_request_select_t select;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_request_create_t create;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_request_write_t write;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_request_ping_t ping;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_request_mtu_t mtu;
            [FieldOffsetAttribute(0)]
            public nrf_dfu_request_prn_t prn;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct nrf_dfu_request_t
        {
            public nrf_dfu_op_t request;
            public nrf_dfu_request_type_t type;
        }
    }
}
