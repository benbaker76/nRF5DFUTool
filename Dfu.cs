﻿// * ----------------------------------------------------------------------------
// * Author: Ben Baker
// * Website: baker76.com
// * E-Mail: ben@baker76.com
// * Copyright (C) 2020 Ben Baker. All Rights Reserved.
// * ----------------------------------------------------------------------------

#pragma warning disable CS0612, CS1591, CS3021, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192

namespace nRF5DFUTool
{

    [global::ProtoBuf.ProtoContract()]
    public partial class Hash : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"hash_type", IsRequired = true)]
        public HashType HashType { get; set; }

        [global::ProtoBuf.ProtoMember(2, IsRequired = true)]
        public byte[] hash { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class BootValidation : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"type", IsRequired = true)]
        public ValidationType Type { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"bytes", IsRequired = true)]
        public byte[] Bytes { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class InitCommand : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"fw_version")]
        public uint FwVersion
        {
            get { return __pbn__FwVersion.GetValueOrDefault(); }
            set { __pbn__FwVersion = value; }
        }
        public bool ShouldSerializeFwVersion() => __pbn__FwVersion != null;
        public void ResetFwVersion() => __pbn__FwVersion = null;
        private uint? __pbn__FwVersion;

        [global::ProtoBuf.ProtoMember(2, Name = @"hw_version")]
        public uint HwVersion
        {
            get { return __pbn__HwVersion.GetValueOrDefault(); }
            set { __pbn__HwVersion = value; }
        }
        public bool ShouldSerializeHwVersion() => __pbn__HwVersion != null;
        public void ResetHwVersion() => __pbn__HwVersion = null;
        private uint? __pbn__HwVersion;

        [global::ProtoBuf.ProtoMember(3, Name = @"sd_req", IsPacked = true)]
        public uint[] SdReqs { get; set; }

        [global::ProtoBuf.ProtoMember(4, Name = @"type")]
        [global::System.ComponentModel.DefaultValue(FwType.Application)]
        public FwType Type
        {
            get { return __pbn__Type ?? FwType.Application; }
            set { __pbn__Type = value; }
        }
        public bool ShouldSerializeType() => __pbn__Type != null;
        public void ResetType() => __pbn__Type = null;
        private FwType? __pbn__Type;

        [global::ProtoBuf.ProtoMember(5, Name = @"sd_size")]
        public uint SdSize
        {
            get { return __pbn__SdSize.GetValueOrDefault(); }
            set { __pbn__SdSize = value; }
        }
        public bool ShouldSerializeSdSize() => __pbn__SdSize != null;
        public void ResetSdSize() => __pbn__SdSize = null;
        private uint? __pbn__SdSize;

        [global::ProtoBuf.ProtoMember(6, Name = @"bl_size")]
        public uint BlSize
        {
            get { return __pbn__BlSize.GetValueOrDefault(); }
            set { __pbn__BlSize = value; }
        }
        public bool ShouldSerializeBlSize() => __pbn__BlSize != null;
        public void ResetBlSize() => __pbn__BlSize = null;
        private uint? __pbn__BlSize;

        [global::ProtoBuf.ProtoMember(7, Name = @"app_size")]
        public uint AppSize
        {
            get { return __pbn__AppSize.GetValueOrDefault(); }
            set { __pbn__AppSize = value; }
        }
        public bool ShouldSerializeAppSize() => __pbn__AppSize != null;
        public void ResetAppSize() => __pbn__AppSize = null;
        private uint? __pbn__AppSize;

        [global::ProtoBuf.ProtoMember(8, Name = @"hash")]
        public Hash Hash { get; set; }

        [global::ProtoBuf.ProtoMember(9, Name = @"is_debug")]
        [global::System.ComponentModel.DefaultValue(false)]
        public bool IsDebug
        {
            get { return __pbn__IsDebug ?? false; }
            set { __pbn__IsDebug = value; }
        }
        public bool ShouldSerializeIsDebug() => __pbn__IsDebug != null;
        public void ResetIsDebug() => __pbn__IsDebug = null;
        private bool? __pbn__IsDebug;

        [global::ProtoBuf.ProtoMember(10, Name = @"boot_validation")]
        public global::System.Collections.Generic.List<BootValidation> BootValidations { get; } = new global::System.Collections.Generic.List<BootValidation>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class ResetCommand : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"timeout", IsRequired = true)]
        public uint Timeout { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class Command : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"op_code")]
        [global::System.ComponentModel.DefaultValue(OpCode.Reset)]
        public OpCode OpCode
        {
            get { return __pbn__OpCode ?? OpCode.Reset; }
            set { __pbn__OpCode = value; }
        }
        public bool ShouldSerializeOpCode() => __pbn__OpCode != null;
        public void ResetOpCode() => __pbn__OpCode = null;
        private OpCode? __pbn__OpCode;

        [global::ProtoBuf.ProtoMember(2, Name = @"init")]
        public InitCommand Init { get; set; }

        [global::ProtoBuf.ProtoMember(3, Name = @"reset")]
        public ResetCommand Reset { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class SignedCommand : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"command", IsRequired = true)]
        public Command Command { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"signature_type", IsRequired = true)]
        public SignatureType SignatureType { get; set; }

        [global::ProtoBuf.ProtoMember(3, Name = @"signature", IsRequired = true)]
        public byte[] Signature { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class Packet : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"command")]
        public Command Command { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"signed_command")]
        public SignedCommand SignedCommand { get; set; }

    }

    [global::ProtoBuf.ProtoContract()]
    public enum OpCode
    {
        [global::ProtoBuf.ProtoEnum(Name = @"RESET")]
        Reset = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"INIT")]
        Init = 1,
    }

    [global::ProtoBuf.ProtoContract()]
    public enum FwType
    {
        [global::ProtoBuf.ProtoEnum(Name = @"APPLICATION")]
        Application = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"SOFTDEVICE")]
        Softdevice = 1,
        [global::ProtoBuf.ProtoEnum(Name = @"BOOTLOADER")]
        Bootloader = 2,
        [global::ProtoBuf.ProtoEnum(Name = @"SOFTDEVICE_BOOTLOADER")]
        SoftdeviceBootloader = 3,
        [global::ProtoBuf.ProtoEnum(Name = @"EXTERNAL_APPLICATION")]
        ExternalApplication = 4,
    }

    [global::ProtoBuf.ProtoContract()]
    public enum HashType
    {
        [global::ProtoBuf.ProtoEnum(Name = @"NO_HASH")]
        NoHash = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"CRC")]
        Crc = 1,
        [global::ProtoBuf.ProtoEnum(Name = @"SHA128")]
        Sha128 = 2,
        [global::ProtoBuf.ProtoEnum(Name = @"SHA256")]
        Sha256 = 3,
        [global::ProtoBuf.ProtoEnum(Name = @"SHA512")]
        Sha512 = 4,
    }

    [global::ProtoBuf.ProtoContract()]
    public enum ValidationType
    {
        [global::ProtoBuf.ProtoEnum(Name = @"NO_VALIDATION")]
        NoValidation = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"VALIDATE_GENERATED_CRC")]
        ValidateGeneratedCrc = 1,
        [global::ProtoBuf.ProtoEnum(Name = @"VALIDATE_SHA256")]
        ValidateSha256 = 2,
        [global::ProtoBuf.ProtoEnum(Name = @"VALIDATE_ECDSA_P256_SHA256")]
        ValidateEcdsaP256Sha256 = 3,
    }

    [global::ProtoBuf.ProtoContract()]
    public enum SignatureType
    {
        [global::ProtoBuf.ProtoEnum(Name = @"ECDSA_P256_SHA256")]
        EcdsaP256Sha256 = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"ED25519")]
        Ed25519 = 1,
    }

}

#pragma warning restore CS0612, CS1591, CS3021, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
