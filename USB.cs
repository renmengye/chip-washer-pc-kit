using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ChipWaherPCKit
{
    public class USB
    {
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public int Reserved;
        }
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            public string DevicePath;
        }
        public struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public int Reserved;
        }
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public int lpSecurityDescriptor;
            public int bInheritHandle;
        }
        public struct HIDP_CAPS
        {
            public short Usage;
            public short UsagePage;
            public short InputReportByteLength;
            public short OutputReportByteLength;
            public short FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public short[] Reserved;
            public short NumberLinkCollectionNodes;
            public short NumberInputButtonCaps;
            public short NumberInputValueCaps;
            public short NumberInputDataIndices;
            public short NumberOutputButtonCaps;
            public short NumberOutputValueCaps;
            public short NumberOutputDataIndices;
            public short NumberFeatureButtonCaps;
            public short NumberFeatureValueCaps;
            public short NumberFeatureDataIndices;
        }
        private const uint GENERIC_READ = 2147483648u;
        private const uint GENERIC_WRITE = 1073741824u;
        private const uint FILE_SHARE_READ = 1u;
        private const uint FILE_SHARE_WRITE = 2u;
        private const uint FILE_FLAG_OVERLAPPED = 1073741824u;
        private const int INVALID_HANDLE_VALUE = -1;
        private const short OPEN_EXISTING = 3;
        private const short DIGCF_PRESENT = 2;
        private const short DIGCF_DEVICEINTERFACE = 16;
        public static string UnitID = "";
        [DllImport("hid.dll")]
        public static extern void HidD_GetHidGuid(ref Guid HidGuid);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, string Enumerator, int hwndParent, int Flags);
        [DllImport("setupapi.dll")]
        public static extern int SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, int DeviceInfoData, ref Guid InterfaceClassGuid, int MemberIndex, ref USB.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref USB.SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, int DeviceInterfaceDetailDataSize, ref int RequiredSize, IntPtr DeviceInfoData);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, ref USB.SECURITY_ATTRIBUTES lpSecurityAttributes, int dwCreationDisposition, uint dwFlagsAndAttributes, int hTemplateFile);
        [DllImport("hid.dll")]
        public static extern int HidD_GetAttributes(IntPtr HidDeviceObject, ref USB.HIDD_ATTRIBUTES Attributes);
        [DllImport("hid.dll")]
        public static extern bool HidD_GetPreparsedData(IntPtr HidDeviceObject, ref IntPtr PreparsedData);
        [DllImport("hid.dll")]
        public static extern bool HidD_GetSerialNumberString(IntPtr HidDeviceObject, byte[] Buffer, ulong BufferLength);
        [DllImport("hid.dll")]
        public static extern int HidP_GetCaps(IntPtr PreparsedData, ref USB.HIDP_CAPS Capabilities);
        [DllImport("setupapi.dll")]
        public static extern int SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
        [DllImport("hid.dll")]
        public static extern bool HidD_FreePreparsedData(ref IntPtr PreparsedData);
        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(IntPtr hFile, byte[] Buffer, int numBytesToWrite, ref int numBytesWritten, int Overlapped);
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hFile, byte[] Buffer, int NumberOfBytesToRead, ref int pNumberOfBytesRead, int Overlapped);
        public static bool Find_This_Device(ushort p_VendorID, ushort p_PoductID, ushort p_index, ref IntPtr p_ReadHandle, ref IntPtr p_WriteHandle)
        {
            IntPtr deviceInfoSet = IntPtr.Zero;
            IntPtr zero = IntPtr.Zero;
            USB.HIDP_CAPS hIDP_CAPS = default(USB.HIDP_CAPS);
            ushort num = 0;
            IntPtr intPtr = IntPtr.Zero;
            int num2 = 0;
            USB.SECURITY_ATTRIBUTES sECURITY_ATTRIBUTES = default(USB.SECURITY_ATTRIBUTES);
            IntPtr value = new IntPtr(-1);
            byte[] array = new byte[64];
            sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
            sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
            sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
            Guid empty = Guid.Empty;
            USB.SP_DEVICE_INTERFACE_DATA sP_DEVICE_INTERFACE_DATA;
            sP_DEVICE_INTERFACE_DATA.cbSize = 0;
            sP_DEVICE_INTERFACE_DATA.Flags = 0;
            sP_DEVICE_INTERFACE_DATA.InterfaceClassGuid = Guid.Empty;
            sP_DEVICE_INTERFACE_DATA.Reserved = 0;
            USB.SP_DEVICE_INTERFACE_DETAIL_DATA sP_DEVICE_INTERFACE_DETAIL_DATA;
            sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = 0;
            sP_DEVICE_INTERFACE_DETAIL_DATA.DevicePath = "";
            USB.HIDD_ATTRIBUTES hIDD_ATTRIBUTES;
            hIDD_ATTRIBUTES.ProductID = 0;
            hIDD_ATTRIBUTES.Size = 0;
            hIDD_ATTRIBUTES.VendorID = 0;
            hIDD_ATTRIBUTES.VersionNumber = 0;
            bool result = false;
            sECURITY_ATTRIBUTES.lpSecurityDescriptor = 0;
            sECURITY_ATTRIBUTES.bInheritHandle = Convert.ToInt32(true);
            sECURITY_ATTRIBUTES.nLength = Marshal.SizeOf(sECURITY_ATTRIBUTES);
            USB.HidD_GetHidGuid(ref empty);
            deviceInfoSet = USB.SetupDiGetClassDevs(ref empty, null, 0, 18);
            sP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DATA);
            for (int i = 0; i < 20; i++)
            {
                int num3 = USB.SetupDiEnumDeviceInterfaces(deviceInfoSet, 0, ref empty, i, ref sP_DEVICE_INTERFACE_DATA);
                if (num3 != 0)
                {
                    USB.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, IntPtr.Zero, 0, ref num2, IntPtr.Zero);
                    sP_DEVICE_INTERFACE_DETAIL_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DETAIL_DATA);
                    IntPtr intPtr2 = Marshal.AllocHGlobal(num2);
                    Marshal.WriteInt32(intPtr2, 4 + Marshal.SystemDefaultCharSize);
                    USB.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, intPtr2, num2, ref num2, IntPtr.Zero);
                    IntPtr ptr = new IntPtr(intPtr2.ToInt32() + 4);
                    string lpFileName = Marshal.PtrToStringAuto(ptr);
                    intPtr = USB.CreateFile(lpFileName, 3221225472u, 3u, ref sECURITY_ATTRIBUTES, 3, 0u, 0);
                    if (intPtr != value)
                    {
                        hIDD_ATTRIBUTES.Size = Marshal.SizeOf(hIDD_ATTRIBUTES);
                        num3 = USB.HidD_GetAttributes(intPtr, ref hIDD_ATTRIBUTES);
                        if (num3 != 0)
                        {
                            if (hIDD_ATTRIBUTES.VendorID == p_VendorID && hIDD_ATTRIBUTES.ProductID == p_PoductID)
                            {
                                if (num == p_index)
                                {
                                    result = true;
                                    USB.HidD_GetSerialNumberString(intPtr, array, 64uL);
                                    if (array[0] == 9 || array[0] == 0)
                                    {
                                        USB.UnitID = "-";
                                    }
                                    else
                                    {
                                        int j;
                                        for (j = 2; j < 28; j += 2)
                                        {
                                            array[j / 2] = array[j];
                                            if (array[j] == 0 || array[j] == 224)
                                            {
                                                break;
                                            }
                                            array[j] = 0;
                                            array[j + 1] = 0;
                                        }
                                        j /= 2;
                                        char[] array2 = new char[Encoding.ASCII.GetCharCount(array, 0, j)];
                                        Encoding.ASCII.GetChars(array, 0, j, array2, 0);
                                        string unitID = new string(array2);
                                        USB.UnitID = unitID;
                                    }
                                    p_WriteHandle = intPtr;
                                    USB.HidD_GetPreparsedData(intPtr, ref zero);
                                    USB.HidP_GetCaps(zero, ref hIDP_CAPS);
                                    p_ReadHandle = USB.CreateFile(lpFileName, 3221225472u, 3u, ref sECURITY_ATTRIBUTES, 3, 0u, 0);
                                    USB.HidD_FreePreparsedData(ref zero);
                                    break;
                                }
                                USB.CloseHandle(intPtr);
                                num += 1;
                            }
                            else
                            {
                                result = false;
                                USB.CloseHandle(intPtr);
                            }
                        }
                        else
                        {
                            result = false;
                            USB.CloseHandle(intPtr);
                        }
                    }
                }
            }
            USB.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            return result;
        }
    }
}
