
namespace ChipWaherPCKit
{

    public class DeviceFile
    {
        public struct DeviceFileParams
        {
            public int VersionMajor;
            public int VersionMinor;
            public int VersionDot;
            public string VersionNotes;
            public int NumberFamilies;
            public int NumberParts;
            public int NumberScripts;
            public byte Compatibility;
            public byte UNUSED1A;
            public ushort UNUSED1B;
            public uint UNUSED2;
        }
        public struct DeviceFamilyParams
        {
            public ushort FamilyID;
            public ushort FamilyType;
            public ushort SearchPriority;
            public string FamilyName;
            public ushort ProgEntryScript;
            public ushort ProgExitScript;
            public ushort ReadDevIDScript;
            public uint DeviceIDMask;
            public uint BlankValue;
            public byte BytesPerLocation;
            public byte AddressIncrement;
            public bool PartDetect;
            public ushort ProgEntryVPPScript;
            public ushort UNUSED1;
            public byte EEMemBytesPerWord;
            public byte EEMemAddressIncrement;
            public byte UserIDHexBytes;
            public byte UserIDBytes;
            public byte ProgMemHexBytes;
            public byte EEMemHexBytes;
            public byte ProgMemShift;
            public uint TestMemoryStart;
            public ushort TestMemoryLength;
            public float Vpp;
        }
        public struct DevicePartParams
        {
            public string PartName;
            public ushort Family;
            public uint DeviceID;
            public uint ProgramMem;
            public ushort EEMem;
            public uint EEAddr;
            public byte ConfigWords;
            public uint ConfigAddr;
            public byte UserIDWords;
            public uint UserIDAddr;
            public uint BandGapMask;
            public ushort[] ConfigMasks;
            public ushort[] ConfigBlank;
            public ushort CPMask;
            public byte CPConfig;
            public bool OSSCALSave;
            public uint IgnoreAddress;
            public float VddMin;
            public float VddMax;
            public float VddErase;
            public byte CalibrationWords;
            public ushort ChipEraseScript;
            public ushort ProgMemAddrSetScript;
            public byte ProgMemAddrBytes;
            public ushort ProgMemRdScript;
            public ushort ProgMemRdWords;
            public ushort EERdPrepScript;
            public ushort EERdScript;
            public ushort EERdLocations;
            public ushort UserIDRdPrepScript;
            public ushort UserIDRdScript;
            public ushort ConfigRdPrepScript;
            public ushort ConfigRdScript;
            public ushort ProgMemWrPrepScript;
            public ushort ProgMemWrScript;
            public ushort ProgMemWrWords;
            public byte ProgMemPanelBufs;
            public uint ProgMemPanelOffset;
            public ushort EEWrPrepScript;
            public ushort EEWrScript;
            public ushort EEWrLocations;
            public ushort UserIDWrPrepScript;
            public ushort UserIDWrScript;
            public ushort ConfigWrPrepScript;
            public ushort ConfigWrScript;
            public ushort OSCCALRdScript;
            public ushort OSCCALWrScript;
            public ushort DPMask;
            public bool WriteCfgOnErase;
            public bool BlankCheckSkipUsrIDs;
            public ushort IgnoreBytes;
            public ushort ChipErasePrepScript;
            public uint BootFlash;
            public ushort Config9Mask;
            public ushort Config9Blank;
            public ushort ProgMemEraseScript;
            public ushort EEMemEraseScript;
            public ushort ConfigMemEraseScript;
            public ushort reserved1EraseScript;
            public ushort reserved2EraseScript;
            public ushort TestMemoryRdScript;
            public ushort TestMemoryRdWords;
            public ushort EERowEraseScript;
            public ushort EERowEraseWords;
            public bool ExportToMPLAB;
            public ushort DebugHaltScript;
            public ushort DebugRunScript;
            public ushort DebugStatusScript;
            public ushort DebugReadExecVerScript;
            public ushort DebugSingleStepScript;
            public ushort DebugBulkWrDataScript;
            public ushort DebugBulkRdDataScript;
            public ushort DebugWriteVectorScript;
            public ushort DebugReadVectorScript;
            public ushort DebugRowEraseScript;
            public ushort DebugRowEraseSize;
            public ushort DebugReserved5Script;
            public ushort DebugReserved6Script;
            public ushort DebugReserved7Script;
            public ushort DebugReserved8Script;
            public ushort LVPScript;
        }
        public struct DeviceScripts
        {
            public ushort ScriptNumber;
            public string ScriptName;
            public ushort ScriptVersion;
            public uint UNUSED1;
            public ushort ScriptLength;
            public ushort[] Script;
            public string Comment;
        }
        public DeviceFile.DeviceFileParams Info = default(DeviceFile.DeviceFileParams);
        public DeviceFile.DeviceFamilyParams[] Families;
        public DeviceFile.DevicePartParams[] PartsList;
        public DeviceFile.DeviceScripts[] Scripts;
    }
}
