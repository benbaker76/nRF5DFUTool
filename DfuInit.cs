// Copyright (c) 2020, Ben Baker
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree. 

using System.Runtime.InteropServices;

namespace nRF5DFUTool
{
    public class DfuInit
    {
        public enum dfu_action_t
        {
            ACTION_PAUSE = 0,
            ACTION_RESUME = 1,
            ACTION_ABORT = 2
        }

        public enum dfu_state_t
        {
            UNKNOWN_STATE = 0,
            DFU_ABORTED = 1,
            DFU_PROCESS_STARTING = 2,
            DFU_COMPLETED = 3,
            DFU_STATE_UPLOADING = 4,
            CONNECTING = 5,
            FIRMWARE_VALIDATING = 6,
            DEVICE_DISCONNECTING = 7,
            ENABLING_DFU_MODE = 8
        }

        public enum dfu_fw_type_t
        {
            DFU_FW_TYPE_APPLICATION = 0,
            DFU_FW_TYPE_SOFTDEVICE = 1,
            DFU_FW_TYPE_BOOTLOADER = 2,
            DFU_FW_TYPE_SOFTDEVICE_BOOTLOADER = 3,
            DFU_FW_TYPE_EXTERNAL_APPLICATION = 4,
        }

        public enum dfu_hash_type_t
        {
            DFU_HASH_TYPE_NO_HASH = 0,
            DFU_HASH_TYPE_CRC = 1,
            DFU_HASH_TYPE_SHA128 = 2,
            DFU_HASH_TYPE_SHA256 = 3,
            DFU_HASH_TYPE_SHA512 = 4,
        }

        public enum dfu_op_code_t
        {
            DFU_OP_CODE_INIT = 1,
        }

        public enum ble_dfu_buttonless_rsp_code_t
        {
            DFU_RSP_INVALID = 0,                // Invalid op code.
            DFU_RSP_SUCCESS = 1,                // Success.
            DFU_RSP_OP_CODE_NOT_SUPPORTED = 2,  // Op code not supported.
            DFU_RSP_OPERATION_FAILED = 4,       // Operation failed.
            DFU_RSP_ADV_NAME_INVALID = 5,       // Requested advertisement name is too short or too long.
            DFU_RSP_BUSY = 6,                   // Ongoing async operation.
            DFU_RSP_NOT_BONDED = 7,             // Buttonless unavailable due to device not bonded.
        }

        public enum ble_dfu_buttonless_op_code_t
        {
            DFU_OP_RESERVED = 0,            // Reserved for future use.
            DFU_OP_ENTER_BOOTLOADER = 1,    // Enter bootloader.
            DFU_OP_SET_ADV_NAME = 2,        // Set advertisement name to use in DFU mode. 
            DFU_OP_RESPONSE_CODE = 32,      // Response code.
        }

        public enum ble_dfu_buttonless_evt_type_t
        {
            BLE_DFU_EVT_BOOTLOADER_ENTER_PREPARE,
            BLE_DFU_EVT_BOOTLOADER_ENTER,
            BLE_DFU_EVT_BOOTLOADER_ENTER_FAILED,
            BLE_DFU_EVT_RESPONSE_SEND_ERROR
        }

        public enum dfu_validation_type_t
        {
            DFU_VALIDATION_TYPE_NO_VALIDATION = 0,
            DFU_VALIDATION_TYPE_VALIDATE_GENERATED_CRC = 1,
            DFU_VALIDATION_TYPE_VALIDATE_SHA256 = 2,
            DFU_VALIDATION_TYPE_VALIDATE_ECDSA_P256_SHA256 = 3,
        }

        public enum dfu_signature_type_t
        {
            DFU_SIGNATURE_TYPE_ECDSA_P256_SHA256 = 0,
            DFU_SIGNATURE_TYPE_ED25519 = 1,
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct dfu_boot_validation_bytes_t
        {
            public uint size;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string bytes;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct dfu_boot_validation_t
        {
            public dfu_validation_type_t type;
            public dfu_boot_validation_bytes_t bytes;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct dfu_hash_hash_t
        {
            public uint size;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string bytes;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct dfu_hash_t
        {
            public dfu_hash_type_t hash_type;
            public dfu_hash_hash_t hash;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct dfu_init_command_t
        {
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_fw_version;
            public uint fw_version;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_hw_version;
            public uint hw_version;
            public uint sd_req_count;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U4)]
            public uint[] sd_req;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_type;
            public dfu_fw_type_t type;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_sd_size;
            public uint sd_size;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_bl_size;
            public uint bl_size;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_app_size;
            public uint app_size;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_hash;
            public dfu_hash_t hash;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_is_debug;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool is_debug;
            public uint boot_validation_count;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.Struct)]
            public dfu_boot_validation_t[] boot_validation;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct dfu_command_t
        {
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_op_code;
            public dfu_op_code_t op_code;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_init;
            public dfu_init_command_t init;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct dfu_signed_command_signature_t
        {
            public uint size;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string bytes;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct dfu_signed_command_t
        {
            public dfu_command_t command;
            public dfu_signature_type_t signature_type;
            public dfu_signed_command_signature_t signature;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct dfu_packet_t
        {
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_command;
            public dfu_command_t command;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool has_signed_command;
            public dfu_signed_command_t signed_command;
        }


        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct ble_gatts_char_handles_t
        {
            public ushort value_handle;
            public ushort user_desc_handle;
            public ushort cccd_handle;
            public ushort sccd_handle;
        }

        public delegate void ble_dfu_buttonless_evt_handler_t(ble_dfu_buttonless_evt_type_t p_evt);

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct ble_dfu_buttonless_t
        {
            public byte uuid_type;
            public ushort service_handle;
            public ushort conn_handle;
            public ble_gatts_char_handles_t control_point_char;
            public uint peers_count;
            public ble_dfu_buttonless_evt_handler_t evt_handler;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool is_waiting_for_reset;
            [MarshalAsAttribute(UnmanagedType.I1)]
            public bool is_waiting_for_svci;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct ble_dfu_buttonless_init_t
        {
            public ble_dfu_buttonless_evt_handler_t evt_handler;
        }
    }
}
