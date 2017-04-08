using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ChipWaherPCKit
{
    public class PICkitFunctions
    {
        private struct scriptRedirect
        {
            public byte redirectToScriptLocation;
            public int deviceFileScriptNumber;
        }
        public static string FirmwareVersion = "NA";
        public static string DeviceFileVersion = "NA";
        public static DeviceFile DevFile = new DeviceFile();
        public static DeviceData DeviceBuffers;
        public static byte[] Usb_write_array = new byte[65];
        public static byte[] Usb_read_array = new byte[65];
        public static int ActivePart = 0;
        public static uint LastDeviceID = 0u;
        public static int LastDeviceRev = 0;
        public static bool LearnMode = false;
        public static byte LastICSPSpeed = 0;
        private static IntPtr usbReadHandle = IntPtr.Zero;
        private static IntPtr usbWriteHandle = IntPtr.Zero;
        private static ushort lastPk2number = 255;
        private static int[] familySearchTable;
        private static bool vddOn = false;
        private static float vddLastSet = 3.3f;
        private static bool targetSelfPowered = false;
        private static bool fastProgramming = true;
        private static bool assertMCLR = false;
        private static bool vppFirstEnabled = false;
        private static bool lvpEnabled = false;
        private static uint scriptBufferChecksum = 0u;
        private static int lastFoundPart = 0;
        private static PICkitFunctions.scriptRedirect[] scriptRedirectTable = new PICkitFunctions.scriptRedirect[32];
        public static void TestingMethod()
        {
        }
        public static bool CheckComm()
        {
            bool result;
            if (PICkitFunctions.writeUSB(new byte[]
			{
				167,
				168,
				8,
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8,
				185,
				0,
				1,
				170,
				167,
				169
			}))
            {
                if (PICkitFunctions.readUSB())
                {
                    if (PICkitFunctions.Usb_read_array[1] == 63)
                    {
                        for (int i = 1; i < 9; i++)
                        {
                            if ((int)PICkitFunctions.Usb_read_array[1 + i] != i)
                            {
                                result = false;
                                return result;
                            }
                        }
                        result = true;
                        return result;
                    }
                }
            }
            result = false;
            return result;
        }
        public static bool EnterLearnMode(byte memsize)
        {
            bool result;
            if (PICkitFunctions.writeUSB(new byte[]
			{
				181,
				80,
				75,
				50,
				memsize
			}))
            {
                PICkitFunctions.LearnMode = true;
                float num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].Vpp;
                if (num < 1f || (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0))
                {
                    if (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0)
                    {
                        string text = PICkitFunctions.DevFile.Scripts[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript - 1)].ScriptName;
                        text = text.Substring(text.Length - 2);
                        if (text == "HV")
                        {
                            num = (float)PICkitFunctions.DevFile.Scripts[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript - 1)].Script[1] / 10f;
                            PICkitFunctions.SetVppVoltage(num, 0.7f);
                        }
                        else
                        {
                            PICkitFunctions.SetVppVoltage(PICkitFunctions.vddLastSet, 0.7f);
                        }
                    }
                    else
                    {
                        PICkitFunctions.SetVppVoltage(PICkitFunctions.vddLastSet, 0.7f);
                    }
                }
                else
                {
                    PICkitFunctions.SetVppVoltage(num, 0.7f);
                }
                PICkitFunctions.downloadPartScripts(PICkitFunctions.GetActiveFamily());
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
        public static bool ExitLearnMode()
        {
            PICkitFunctions.LearnMode = false;
            return PICkitFunctions.writeUSB(new byte[]
			{
				182
			});
        }
        public static bool EnablePK2GoMode(byte memsize)
        {
            PICkitFunctions.LearnMode = false;
            return PICkitFunctions.writeUSB(new byte[]
			{
				183,
				80,
				75,
				50,
				memsize
			});
        }
        public static bool MetaCmd_CHECK_DEVICE_ID()
        {
            int num = (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].DeviceIDMask;
            int num2 = (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DeviceID;
            if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift != 0)
            {
                num <<= 1;
                num2 <<= 1;
            }
            return PICkitFunctions.writeUSB(new byte[]
			{
				132,
				(byte)(num & 255),
				(byte)(num >> 8 & 255),
				(byte)(num2 & 255),
				(byte)(num2 >> 8 & 255)
			});
        }
        public static bool MetaCmd_READ_BANDGAP()
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				133
			});
        }
        public static bool MetaCmd_WRITE_CFG_BANDGAP()
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				134
			});
        }
        public static bool MetaCmd_READ_OSCCAL()
        {
            int num = (int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem - 1u);
            return PICkitFunctions.writeUSB(new byte[]
			{
				128,
				(byte)(num & 255),
				(byte)(num >> 8 & 255)
			});
        }
        public static bool MetaCmd_WRITE_OSCCAL()
        {
            int num = (int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem - 1u);
            return PICkitFunctions.writeUSB(new byte[]
			{
				129,
				(byte)(num & 255),
				(byte)(num >> 8 & 255)
			});
        }
        public static bool MetaCmd_START_CHECKSUM()
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				130,
				PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift,
				0
			});
        }
        public static bool MetaCmd_CHANGE_CHKSM_FRMT(byte format)
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				135,
				format,
				0
			});
        }
        public static bool MetaCmd_VERIFY_CHECKSUM(uint checksum)
        {
            checksum = ~checksum;
            return PICkitFunctions.writeUSB(new byte[]
			{
				131,
				(byte)(checksum & 255u),
				(byte)(checksum >> 8 & 255u)
			});
        }
        public static void ResetPk2Number()
        {
            PICkitFunctions.lastPk2number = 255;
        }
        public static float MeasurePGDPulse()
        {
            float result;
            if (PICkitFunctions.writeUSB(new byte[]
			{
				169,
				166,
				9,
				243,
				2,
				232,
				20,
				243,
				6,
				191,
				243,
				3,
				170
			}))
            {
                if (PICkitFunctions.readUSB())
                {
                    if (PICkitFunctions.Usb_read_array[1] == 2)
                    {
                        float num = (float)((int)PICkitFunctions.Usb_read_array[2] + (int)PICkitFunctions.Usb_read_array[3] * 256);
                        result = num * 0.021333f;
                        return result;
                    }
                }
            }
            result = 0f;
            return result;
        }
        public static bool EnterUARTMode(uint baudValue)
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				167,
				169,
				179,
				(byte)(baudValue & 255u),
				(byte)(baudValue >> 8 & 255u)
			});
        }
        public static bool ExitUARTMode()
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				180,
				167,
				169
			});
        }
        public static bool ValidateOSSCAL()
        {
            uint num = PICkitFunctions.DeviceBuffers.OSCCAL;
            num &= 65280u;
            return num != 0u && num == (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMasks[7];
        }
        public static bool isCalibrated()
        {
            bool result;
            if (PICkitFunctions.writeUSB(new byte[]
			{
				178,
				0,
				4
			}))
            {
                if (PICkitFunctions.readUSB())
                {
                    int num = (int)PICkitFunctions.Usb_read_array[1] + (int)PICkitFunctions.Usb_read_array[2] * 256;
                    if (num <= 320 && num >= 192)
                    {
                        result = (PICkitFunctions.Usb_read_array[1] != 0 || PICkitFunctions.Usb_read_array[2] != 1 || PICkitFunctions.Usb_read_array[3] != 0 || PICkitFunctions.Usb_read_array[4] != 128);
                        return result;
                    }
                }
            }
            result = false;
            return result;
        }
        public static string UnitIDRead()
        {
            string result = "";
            if (PICkitFunctions.writeUSB(new byte[]
			{
				178,
				240,
				16
			}))
            {
                if (PICkitFunctions.readUSB())
                {
                    if (PICkitFunctions.Usb_read_array[1] == 35)
                    {
                        int i;
                        for (i = 0; i < 15; i++)
                        {
                            if (PICkitFunctions.Usb_read_array[2 + i] == 0)
                            {
                                break;
                            }
                        }
                        byte[] array = new byte[i];
                        Array.Copy(PICkitFunctions.Usb_read_array, 2, array, 0, i);
                        char[] array2 = new char[Encoding.ASCII.GetCharCount(array, 0, array.Length)];
                        Encoding.ASCII.GetChars(array, 0, array.Length, array2, 0);
                        string text = new string(array2);
                        result = text;
                    }
                }
            }
            return result;
        }
        public static bool UnitIDWrite(string unitID)
        {
            int num = unitID.Length;
            if (num > 15)
            {
                num = 15;
            }
            byte[] array = new byte[19];
            array[0] = 177;
            array[1] = 240;
            array[2] = 16;
            byte[] bytes = Encoding.Unicode.GetBytes(unitID);
            byte[] array2 = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, bytes);
            if (num > 0)
            {
                array[3] = 35;
            }
            else
            {
                array[3] = 255;
            }
            for (int i = 0; i < 15; i++)
            {
                if (i < num)
                {
                    array[4 + i] = array2[i];
                }
                else
                {
                    array[4 + i] = 0;
                }
            }
            return PICkitFunctions.writeUSB(array);
        }
        public static bool SetVoltageCals(ushort adcCal, byte vddOffset, byte VddCal)
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				176,
				(byte)adcCal,
				(byte)(adcCal >> 8),
				vddOffset,
				VddCal
			});
        }
        public static bool HCS360_361_VppSpecial()
        {
            bool result;
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DeviceID != 4294967094u)
            {
                result = true;
            }
            else
            {
                byte[] array = new byte[12];
                array[0] = 166;
                array[1] = 10;
                if ((PICkitFunctions.DeviceBuffers.ProgramMemory[0] & 1u) == 0u)
                {
                    array[2] = 243;
                    array[3] = 4;
                    array[4] = 247;
                    array[5] = 250;
                    array[6] = 232;
                    array[7] = 5;
                    array[8] = 243;
                    array[9] = 4;
                    array[10] = 243;
                    array[11] = 0;
                }
                else
                {
                    array[2] = 243;
                    array[3] = 4;
                    array[4] = 246;
                    array[5] = 251;
                    array[6] = 232;
                    array[7] = 5;
                    array[8] = 243;
                    array[9] = 12;
                    array[10] = 243;
                    array[11] = 8;
                }
                result = PICkitFunctions.writeUSB(array);
            }
            return result;
        }
        public static bool FamilyIsEEPROM()
        {
            int num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Length;
            if (num > 6)
            {
                num = 6;
            }
            return PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Substring(0, num) == "EEPROM";
        }
        public static bool FamilyIsKeeloq()
        {
            return PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName == "KEELOQ® HCS";
        }
        public static bool FamilyIsMCP()
        {
            int num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Length;
            if (num > 3)
            {
                num = 3;
            }
            return PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Substring(0, num) == "MCP";
        }
        public static bool FamilyIsPIC32()
        {
            int num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Length;
            if (num > 5)
            {
                num = 5;
            }
            return PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Substring(0, num) == "PIC32";
        }
        public static bool FamilyIsdsPIC30()
        {
            int num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Length;
            if (num > 7)
            {
                num = 7;
            }
            return PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Substring(0, num) == "dsPIC30";
        }
        public static bool FamilyIsdsPIC30SMPS()
        {
            int num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Length;
            if (num > 9)
            {
                num = 9;
            }
            return PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Substring(0, num) == "dsPIC30 S";
        }
        public static bool FamilyIsPIC18J()
        {
            int num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Length;
            if (num > 9)
            {
                num = 9;
            }
            return PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName.Substring(0, num) == "PIC18F_J_";
        }
        public static bool FamilyIsPIC24FJ()
        {
            int num = PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].PartName.Length;
            if (num > 7)
            {
                num = 7;
            }
            return PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].PartName.Substring(0, num) == "PIC24FJ";
        }
        public static bool FamilyIsPIC24H()
        {
            int num = PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].PartName.Length;
            if (num > 6)
            {
                num = 6;
            }
            return PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].PartName.Substring(0, num) == "PIC24H";
        }
        public static bool FamilyIsdsPIC33F()
        {
            int num = PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].PartName.Length;
            if (num > 8)
            {
                num = 8;
            }
            return PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].PartName.Substring(0, num) == "dsPIC33F";
        }
        public static void SetVPPFirstProgramEntry()
        {
            PICkitFunctions.vppFirstEnabled = true;
            PICkitFunctions.scriptBufferChecksum = ~PICkitFunctions.scriptBufferChecksum;
        }
        public static void ClearVppFirstProgramEntry()
        {
            PICkitFunctions.vppFirstEnabled = false;
            PICkitFunctions.scriptBufferChecksum = ~PICkitFunctions.scriptBufferChecksum;
        }
        public static void SetLVPProgramEntry()
        {
            PICkitFunctions.lvpEnabled = true;
            PICkitFunctions.scriptBufferChecksum = ~PICkitFunctions.scriptBufferChecksum;
        }
        public static void ClearLVPProgramEntry()
        {
            PICkitFunctions.lvpEnabled = false;
            PICkitFunctions.scriptBufferChecksum = ~PICkitFunctions.scriptBufferChecksum;
        }
        public static void RowEraseDevice()
        {
            int num = (int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem / (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DebugRowEraseSize);
            PICkitFunctions.RunScript(0, 1);
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemWrPrepScript != 0)
            {
                PICkitFunctions.DownloadAddress3(0);
                PICkitFunctions.RunScript(6, 1);
            }
            do
            {
                if (num >= 256)
                {
                    PICkitFunctions.RunScript(26, 0);
                    num -= 256;
                }
                else
                {
                    PICkitFunctions.RunScript(26, num);
                    num = 0;
                }
            }
            while (num > 0);
            PICkitFunctions.RunScript(1, 1);
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERowEraseScript > 0)
            {
                int num2 = (int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEMem / PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERowEraseWords);
                PICkitFunctions.RunScript(0, 1);
                if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERdPrepScript != 0)
                {
                    PICkitFunctions.DownloadAddress3((int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEAddr / (uint)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].EEMemBytesPerWord));
                    PICkitFunctions.RunScript(8, 1);
                }
                do
                {
                    if (num2 >= 256)
                    {
                        PICkitFunctions.RunScript(28, 0);
                        num2 -= 256;
                    }
                    else
                    {
                        PICkitFunctions.RunScript(28, num2);
                        num2 = 0;
                    }
                }
                while (num2 > 0);
                PICkitFunctions.RunScript(1, 1);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMemEraseScript > 0)
            {
                PICkitFunctions.RunScript(0, 1);
                if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemWrPrepScript != 0)
                {
                    PICkitFunctions.DownloadAddress3((int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDAddr);
                    PICkitFunctions.RunScript(6, 1);
                }
                PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMemEraseScript);
                PICkitFunctions.RunScript(1, 1);
            }
        }
        public static bool ExecuteScript(int scriptArrayIndex)
        {
            bool result;
            if (scriptArrayIndex == 0)
            {
                result = false;
            }
            else
            {
                int scriptLength = (int)PICkitFunctions.DevFile.Scripts[--scriptArrayIndex].ScriptLength;
                byte[] array = new byte[3 + scriptLength];
                array[0] = 169;
                array[1] = 166;
                array[2] = (byte)scriptLength;
                for (int i = 0; i < scriptLength; i++)
                {
                    array[3 + i] = (byte)PICkitFunctions.DevFile.Scripts[scriptArrayIndex].Script[i];
                }
                result = PICkitFunctions.writeUSB(array);
            }
            return result;
        }
        public static bool GetVDDState()
        {
            return PICkitFunctions.vddOn;
        }
        public static bool SetMCLRTemp(bool nMCLR)
        {
            byte[] array = new byte[1];
            if (nMCLR)
            {
                array[0] = 247;
            }
            else
            {
                array[0] = 246;
            }
            return PICkitFunctions.SendScript(array);
        }
        public static bool HoldMCLR(bool nMCLR)
        {
            PICkitFunctions.assertMCLR = nMCLR;
            byte[] array = new byte[1];
            if (nMCLR)
            {
                array[0] = 247;
            }
            else
            {
                array[0] = 246;
            }
            return PICkitFunctions.SendScript(array);
        }
        public static void SetFastProgramming(bool fast)
        {
            PICkitFunctions.fastProgramming = fast;
            PICkitFunctions.scriptBufferChecksum = ~PICkitFunctions.scriptBufferChecksum;
        }
        public static void ForcePICkitPowered()
        {
            PICkitFunctions.targetSelfPowered = false;
        }
        public static void ForceTargetPowered()
        {
            PICkitFunctions.targetSelfPowered = true;
        }
        public static void ReadConfigOutsideProgMem()
        {
            PICkitFunctions.RunScript(0, 1);
            PICkitFunctions.RunScript(13, 1);
            PICkitFunctions.UploadData();
            PICkitFunctions.RunScript(1, 1);
            int configWords = (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords;
            int num = 2;
            for (int i = 0; i < configWords; i++)
            {
                uint num2 = (uint)PICkitFunctions.Usb_read_array[num++];
                num2 |= (uint)((uint)PICkitFunctions.Usb_read_array[num++] << 8);
                if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift > 0)
                {
                    num2 = (num2 >> 1 & PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue);
                }
                PICkitFunctions.DeviceBuffers.ConfigWords[i] = num2;
            }
        }
        public static void ReadBandGap()
        {
            PICkitFunctions.RunScript(0, 1);
            PICkitFunctions.RunScript(13, 1);
            PICkitFunctions.UploadData();
            PICkitFunctions.RunScript(1, 1);
            int configWords = (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords;
            uint num = (uint)PICkitFunctions.Usb_read_array[2];
            num |= (uint)((uint)PICkitFunctions.Usb_read_array[3] << 8);
            if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift > 0)
            {
                num = (num >> 1 & PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue);
            }
            PICkitFunctions.DeviceBuffers.BandGap = (num & PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].BandGapMask);
        }
        public static uint WriteConfigOutsideProgMem(bool codeProtect, bool dataProtect)
        {
            int configWords = (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords;
            uint num = 0u;
            byte[] array = new byte[configWords * 2];
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].BandGapMask > 0u)
            {
                PICkitFunctions.DeviceBuffers.ConfigWords[0] &= ~PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].BandGapMask;
                if (!PICkitFunctions.LearnMode)
                {
                    PICkitFunctions.DeviceBuffers.ConfigWords[0] |= PICkitFunctions.DeviceBuffers.BandGap;
                }
            }
            if (PICkitFunctions.FamilyIsMCP())
            {
                PICkitFunctions.DeviceBuffers.ConfigWords[0] |= 16376u;
            }
            PICkitFunctions.RunScript(0, 1);
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWrPrepScript > 0)
            {
                PICkitFunctions.DownloadAddress3(0);
                PICkitFunctions.RunScript(14, 1);
            }
            int i = 0;
            int num2 = 0;
            while (i < configWords)
            {
                uint num3 = PICkitFunctions.DeviceBuffers.ConfigWords[i] & (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMasks[i];
                if (i == (int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPConfig - 1))
                {
                    if (codeProtect)
                    {
                        num3 &= (uint)(~(uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPMask);
                    }
                    if (dataProtect)
                    {
                        num3 &= (uint)(~(uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DPMask);
                    }
                }
                if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift > 0)
                {
                    num3 |= ((uint)(~(uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMasks[i]) & ~PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].BandGapMask);
                    if (!PICkitFunctions.FamilyIsMCP())
                    {
                        num3 &= PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue;
                    }
                    num3 <<= 1;
                }
                array[num2++] = (byte)(num3 & 255u);
                array[num2++] = (byte)(num3 >> 8 & 255u);
                num += (uint)((byte)(num3 & 255u));
                num += (uint)((byte)(num3 >> 8 & 255u));
                i++;
            }
            PICkitFunctions.DataClrAndDownload(array, 0);
            if (PICkitFunctions.LearnMode && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].BandGapMask > 0u)
            {
                PICkitFunctions.MetaCmd_WRITE_CFG_BANDGAP();
            }
            else
            {
                PICkitFunctions.RunScript(15, 1);
            }
            PICkitFunctions.RunScript(1, 1);
            return num;
        }
        public static bool ReadOSSCAL()
        {
            bool result;
            if (PICkitFunctions.RunScript(0, 1))
            {
                if (PICkitFunctions.DownloadAddress3((int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem - 1u)))
                {
                    if (PICkitFunctions.RunScript(20, 1))
                    {
                        if (PICkitFunctions.UploadData())
                        {
                            if (PICkitFunctions.RunScript(1, 1))
                            {
                                PICkitFunctions.DeviceBuffers.OSCCAL = (uint)PICkitFunctions.Usb_read_array[2] + (uint)PICkitFunctions.Usb_read_array[3] * 256u;
                                if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift > 0)
                                {
                                    PICkitFunctions.DeviceBuffers.OSCCAL >>= 1;
                                }
                                PICkitFunctions.DeviceBuffers.OSCCAL &= PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue;
                                result = true;
                                return result;
                            }
                        }
                    }
                }
            }
            result = false;
            return result;
        }
        public static bool WriteOSSCAL()
        {
            bool result;
            if (PICkitFunctions.RunScript(0, 1))
            {
                uint num = PICkitFunctions.DeviceBuffers.OSCCAL;
                uint num2 = PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem - 1u;
                if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift > 0)
                {
                    num <<= 1;
                }
                PICkitFunctions.DataClrAndDownload(new byte[]
				{
					(byte)(num2 & 255u),
					(byte)(num2 >> 8 & 255u),
					(byte)(num2 >> 16 & 255u),
					(byte)(num & 255u),
					(byte)(num >> 8 & 255u)
				}, 0);
                if (PICkitFunctions.RunScript(21, 1))
                {
                    if (PICkitFunctions.RunScript(1, 1))
                    {
                        result = true;
                        return result;
                    }
                }
            }
            result = false;
            return result;
        }
        public static Constants.PICkit2PWR CheckTargetPower(ref float vdd, ref float vpp)
        {
            Constants.PICkit2PWR result;
            if (PICkitFunctions.vddOn)
            {
                result = Constants.PICkit2PWR.vdd_on;
            }
            else
            {
                if (PICkitFunctions.ReadPICkitVoltages(ref vdd, ref vpp))
                {
                    if (vdd > 2.3f)
                    {
                        PICkitFunctions.targetSelfPowered = true;
                        PICkitFunctions.SetVDDVoltage(vdd, 0.85f);
                        result = Constants.PICkit2PWR.selfpowered;
                    }
                    else
                    {
                        PICkitFunctions.targetSelfPowered = false;
                        result = Constants.PICkit2PWR.unpowered;
                    }
                }
                else
                {
                    PICkitFunctions.targetSelfPowered = false;
                    result = Constants.PICkit2PWR.no_response;
                }
            }
            return result;
        }
        public static int GetActiveFamily()
        {
            return (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].Family;
        }
        public static void SetActiveFamily(int family)
        {
            PICkitFunctions.ActivePart = 0;
            PICkitFunctions.lastFoundPart = 0;
            PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].Family = (ushort)family;
            PICkitFunctions.ResetBuffers();
        }
        public static bool SetVDDVoltage(float voltage, float threshold)
        {
            if (voltage < 2.5f)
            {
                voltage = 2.5f;
            }
            PICkitFunctions.vddLastSet = voltage;
            ushort num = PICkitFunctions.CalculateVddCPP(voltage);
            byte b = (byte)(threshold * voltage / 5f * 255f);
            if (b > 210)
            {
                b = 210;
            }
            return PICkitFunctions.writeUSB(new byte[]
			{
				160,
				(byte)(num & 255),
				(byte)(num / 256),
				b
			});
        }
        public static ushort CalculateVddCPP(float voltage)
        {
            ushort num = (ushort)(voltage * 32f + 10.5f);
            return (ushort)(num << 6);
        }
        public static bool VddOn()
        {
            byte[] array = new byte[4];
            array[0] = 166;
            array[1] = 2;
            array[2] = 252;
            if (PICkitFunctions.targetSelfPowered)
            {
                array[3] = 254;
            }
            else
            {
                array[3] = 255;
            }
            bool flag = PICkitFunctions.writeUSB(array);
            bool result;
            if (flag)
            {
                PICkitFunctions.vddOn = true;
                result = true;
            }
            else
            {
                result = flag;
            }
            return result;
        }
        public static bool VddOff()
        {
            byte[] array = new byte[4];
            array[0] = 166;
            array[1] = 2;
            array[2] = 254;
            if (PICkitFunctions.targetSelfPowered)
            {
                array[3] = 252;
            }
            else
            {
                array[3] = 253;
            }
            bool flag = PICkitFunctions.writeUSB(array);
            bool result;
            if (flag)
            {
                PICkitFunctions.vddOn = false;
                result = true;
            }
            else
            {
                result = flag;
            }
            return result;
        }
        public static bool SetProgrammingSpeed(byte speed)
        {
            PICkitFunctions.LastICSPSpeed = speed;
            return PICkitFunctions.writeUSB(new byte[]
			{
				166,
				2,
				234,
				speed
			});
        }
        public static bool ResetPICkit2()
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				174
			});
        }
        public static bool EnterBootloader()
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				66
			});
        }
        public static bool VerifyBootloaderMode()
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				118
			}) && PICkitFunctions.readUSB() && PICkitFunctions.Usb_read_array[1] == 118;
        }
        public static bool BL_EraseFlash()
        {
            byte[] array = new byte[]
			{
				3,
				192,
				0,
				32,
				0
			};
            bool result;
            if (PICkitFunctions.writeUSB(array))
            {
                array[3] = 80;
                result = PICkitFunctions.writeUSB(array);
            }
            else
            {
                result = false;
            }
            return result;
        }
        public static bool BL_WriteFlash(byte[] payload)
        {
            byte[] array = new byte[37];
            array[0] = 2;
            array[1] = 32;
            for (int i = 0; i < 35; i++)
            {
                array[2 + i] = payload[i];
            }
            return PICkitFunctions.writeUSB(array);
        }
        public static bool BL_WriteFWLoadedKey()
        {
            byte[] array = new byte[35];
            array[0] = 224;
            array[1] = 127;
            array[2] = 0;
            for (int i = 3; i < array.Length; i++)
            {
                array[i] = 255;
            }
            array[array.Length - 2] = 85;
            array[array.Length - 1] = 85;
            return PICkitFunctions.BL_WriteFlash(array);
        }
        public static bool BL_ReadFlash16(int address)
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				1,
				16,
				(byte)(address & 255),
				(byte)(address >> 8 & 255),
				0
			}) && PICkitFunctions.readUSB();
        }
        public static bool BL_Reset()
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				255
			});
        }
        public static bool ButtonPressed()
        {
            ushort num = PICkitFunctions.readPkStatus();
            return (num & 64) == 64;
        }
        public static bool BusErrorCheck()
        {
            ushort num = PICkitFunctions.readPkStatus();
            bool result;
            if ((num & 1024) == 1024)
            {
                result = true;
            }
            else
            {
                PICkitFunctions.writeUSB(new byte[]
				{
					166,
					1,
					245
				});
                result = false;
            }
            return result;
        }
        public static Constants.PICkit2PWR PowerStatus()
        {
            ushort num = PICkitFunctions.readPkStatus();
            Constants.PICkit2PWR result;
            if (num == 65535)
            {
                result = Constants.PICkit2PWR.no_response;
            }
            else
            {
                if ((num & 48) == 48)
                {
                    PICkitFunctions.vddOn = false;
                    result = Constants.PICkit2PWR.vddvpperrors;
                }
                else
                {
                    if ((num & 32) == 32)
                    {
                        PICkitFunctions.vddOn = false;
                        result = Constants.PICkit2PWR.vpperror;
                    }
                    else
                    {
                        if ((num & 16) == 16)
                        {
                            PICkitFunctions.vddOn = false;
                            result = Constants.PICkit2PWR.vdderror;
                        }
                        else
                        {
                            if ((num & 2) == 2)
                            {
                                PICkitFunctions.vddOn = true;
                                result = Constants.PICkit2PWR.vdd_on;
                            }
                            else
                            {
                                PICkitFunctions.vddOn = false;
                                result = Constants.PICkit2PWR.vdd_off;
                            }
                        }
                    }
                }
            }
            return result;
        }
        public static void DisconnectPICkit2Unit()
        {
            if (PICkitFunctions.usbWriteHandle != IntPtr.Zero)
            {
                USB.CloseHandle(PICkitFunctions.usbWriteHandle);
            }
            if (PICkitFunctions.usbReadHandle != IntPtr.Zero)
            {
                USB.CloseHandle(PICkitFunctions.usbReadHandle);
            }
            PICkitFunctions.usbReadHandle = IntPtr.Zero;
            PICkitFunctions.usbWriteHandle = IntPtr.Zero;
        }
        public static string GetSerialUnitID()
        {
            return USB.UnitID;
        }
        public static Constants.PICkit2USB DetectPICkit2Device(ushort pk2ID, bool readFW)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr zero2 = IntPtr.Zero;
            PICkitFunctions.DisconnectPICkit2Unit();
            bool flag = USB.Find_This_Device(1240, 51, pk2ID, ref zero, ref zero2);
            PICkitFunctions.lastPk2number = pk2ID;
            PICkitFunctions.usbReadHandle = zero;
            PICkitFunctions.usbWriteHandle = zero2;
            Constants.PICkit2USB result;
            if (flag && !readFW)
            {
                result = Constants.PICkit2USB.found;
            }
            else
            {
                if (flag)
                {
                    flag = PICkitFunctions.writeUSB(new byte[]
					{
						118
					});
                    if (flag)
                    {
                        if (PICkitFunctions.readUSB())
                        {
                            PICkitFunctions.FirmwareVersion = string.Format("{0:D1}.{1:D2}.{2:D2}", PICkitFunctions.Usb_read_array[1], PICkitFunctions.Usb_read_array[2], PICkitFunctions.Usb_read_array[3]);
                            if (PICkitFunctions.Usb_read_array[1] == 2)
                            {
                                if ((PICkitFunctions.Usb_read_array[2] == 32 && PICkitFunctions.Usb_read_array[3] >= 0) || PICkitFunctions.Usb_read_array[2] > 32)
                                {
                                    result = Constants.PICkit2USB.found;
                                    return result;
                                }
                            }
                            if (PICkitFunctions.Usb_read_array[1] == 118)
                            {
                                PICkitFunctions.FirmwareVersion = string.Format("BL {0:D1}.{1:D1}", PICkitFunctions.Usb_read_array[7], PICkitFunctions.Usb_read_array[8]);
                                result = Constants.PICkit2USB.bootloader;
                            }
                            else
                            {
                                result = Constants.PICkit2USB.firmwareInvalid;
                            }
                        }
                        else
                        {
                            result = Constants.PICkit2USB.readError;
                        }
                    }
                    else
                    {
                        result = Constants.PICkit2USB.writeError;
                    }
                }
                else
                {
                    result = Constants.PICkit2USB.notFound;
                }
            }
            return result;
        }
        public static bool ReadDeviceFile(string DeviceFileName)
        {
            bool flag = File.Exists(DeviceFileName);
            bool result;
            if (flag)
            {
                try
                {
                    FileStream fileStream = File.OpenRead(DeviceFileName);
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        PICkitFunctions.DevFile.Info.VersionMajor = binaryReader.ReadInt32();
                        PICkitFunctions.DevFile.Info.VersionMinor = binaryReader.ReadInt32();
                        PICkitFunctions.DevFile.Info.VersionDot = binaryReader.ReadInt32();
                        PICkitFunctions.DevFile.Info.VersionNotes = binaryReader.ReadString();
                        PICkitFunctions.DevFile.Info.NumberFamilies = binaryReader.ReadInt32();
                        PICkitFunctions.DevFile.Info.NumberParts = binaryReader.ReadInt32();
                        PICkitFunctions.DevFile.Info.NumberScripts = binaryReader.ReadInt32();
                        PICkitFunctions.DevFile.Info.Compatibility = binaryReader.ReadByte();
                        PICkitFunctions.DevFile.Info.UNUSED1A = binaryReader.ReadByte();
                        PICkitFunctions.DevFile.Info.UNUSED1B = binaryReader.ReadUInt16();
                        PICkitFunctions.DevFile.Info.UNUSED2 = binaryReader.ReadUInt32();
                        PICkitFunctions.DeviceFileVersion = string.Format("{0:D1}.{1:D2}.{2:D2}", PICkitFunctions.DevFile.Info.VersionMajor, PICkitFunctions.DevFile.Info.VersionMinor, PICkitFunctions.DevFile.Info.VersionDot);
                        PICkitFunctions.DevFile.Families = new DeviceFile.DeviceFamilyParams[PICkitFunctions.DevFile.Info.NumberFamilies];
                        PICkitFunctions.DevFile.PartsList = new DeviceFile.DevicePartParams[PICkitFunctions.DevFile.Info.NumberParts];
                        PICkitFunctions.DevFile.Scripts = new DeviceFile.DeviceScripts[PICkitFunctions.DevFile.Info.NumberScripts];
                        for (int i = 0; i < PICkitFunctions.DevFile.Info.NumberFamilies; i++)
                        {
                            PICkitFunctions.DevFile.Families[i].FamilyID = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].FamilyType = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].SearchPriority = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].FamilyName = binaryReader.ReadString();
                            PICkitFunctions.DevFile.Families[i].ProgEntryScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].ProgExitScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].ReadDevIDScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].DeviceIDMask = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.Families[i].BlankValue = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.Families[i].BytesPerLocation = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].AddressIncrement = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].PartDetect = binaryReader.ReadBoolean();
                            PICkitFunctions.DevFile.Families[i].ProgEntryVPPScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].UNUSED1 = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].EEMemBytesPerWord = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].EEMemAddressIncrement = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].UserIDHexBytes = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].UserIDBytes = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].ProgMemHexBytes = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].EEMemHexBytes = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].ProgMemShift = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.Families[i].TestMemoryStart = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.Families[i].TestMemoryLength = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Families[i].Vpp = binaryReader.ReadSingle();
                        }
                        PICkitFunctions.familySearchTable = new int[PICkitFunctions.DevFile.Info.NumberFamilies];
                        for (int j = 0; j < PICkitFunctions.DevFile.Info.NumberFamilies; j++)
                        {
                            PICkitFunctions.familySearchTable[(int)PICkitFunctions.DevFile.Families[j].SearchPriority] = j;
                        }
                        for (int i = 0; i < PICkitFunctions.DevFile.Info.NumberParts; i++)
                        {
                            PICkitFunctions.DevFile.PartsList[i].PartName = binaryReader.ReadString();
                            PICkitFunctions.DevFile.PartsList[i].Family = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DeviceID = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].ProgramMem = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].EEMem = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EEAddr = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].ConfigWords = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.PartsList[i].ConfigAddr = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].UserIDWords = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.PartsList[i].UserIDAddr = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].BandGapMask = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].ConfigMasks = new ushort[9];
                            PICkitFunctions.DevFile.PartsList[i].ConfigBlank = new ushort[9];
                            for (int k = 0; k < 8; k++)
                            {
                                PICkitFunctions.DevFile.PartsList[i].ConfigMasks[k] = binaryReader.ReadUInt16();
                            }
                            for (int k = 0; k < 8; k++)
                            {
                                PICkitFunctions.DevFile.PartsList[i].ConfigBlank[k] = binaryReader.ReadUInt16();
                            }
                            PICkitFunctions.DevFile.PartsList[i].CPMask = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].CPConfig = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.PartsList[i].OSSCALSave = binaryReader.ReadBoolean();
                            PICkitFunctions.DevFile.PartsList[i].IgnoreAddress = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].VddMin = binaryReader.ReadSingle();
                            PICkitFunctions.DevFile.PartsList[i].VddMax = binaryReader.ReadSingle();
                            PICkitFunctions.DevFile.PartsList[i].VddErase = binaryReader.ReadSingle();
                            PICkitFunctions.DevFile.PartsList[i].CalibrationWords = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.PartsList[i].ChipEraseScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemAddrSetScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemAddrBytes = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemRdScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemRdWords = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EERdPrepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EERdScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EERdLocations = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].UserIDRdPrepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].UserIDRdScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ConfigRdPrepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ConfigRdScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemWrPrepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemWrScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemWrWords = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemPanelBufs = binaryReader.ReadByte();
                            PICkitFunctions.DevFile.PartsList[i].ProgMemPanelOffset = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].EEWrPrepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EEWrScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EEWrLocations = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].UserIDWrPrepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].UserIDWrScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ConfigWrPrepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ConfigWrScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].OSCCALRdScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].OSCCALWrScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DPMask = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].WriteCfgOnErase = binaryReader.ReadBoolean();
                            PICkitFunctions.DevFile.PartsList[i].BlankCheckSkipUsrIDs = binaryReader.ReadBoolean();
                            PICkitFunctions.DevFile.PartsList[i].IgnoreBytes = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ChipErasePrepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].BootFlash = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.PartsList[i].Config9Mask = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ConfigMasks[8] = PICkitFunctions.DevFile.PartsList[i].Config9Mask;
                            PICkitFunctions.DevFile.PartsList[i].Config9Blank = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ConfigBlank[8] = PICkitFunctions.DevFile.PartsList[i].Config9Blank;
                            PICkitFunctions.DevFile.PartsList[i].ProgMemEraseScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EEMemEraseScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ConfigMemEraseScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].reserved1EraseScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].reserved2EraseScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].TestMemoryRdScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].TestMemoryRdWords = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EERowEraseScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].EERowEraseWords = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].ExportToMPLAB = binaryReader.ReadBoolean();
                            PICkitFunctions.DevFile.PartsList[i].DebugHaltScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugRunScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugStatusScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugReadExecVerScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugSingleStepScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugBulkWrDataScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugBulkRdDataScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugWriteVectorScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugReadVectorScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugRowEraseScript = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugRowEraseSize = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugReserved5Script = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugReserved6Script = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugReserved7Script = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].DebugReserved8Script = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.PartsList[i].LVPScript = binaryReader.ReadUInt16();
                        }
                        for (int i = 0; i < PICkitFunctions.DevFile.Info.NumberScripts; i++)
                        {
                            PICkitFunctions.DevFile.Scripts[i].ScriptNumber = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Scripts[i].ScriptName = binaryReader.ReadString();
                            PICkitFunctions.DevFile.Scripts[i].ScriptVersion = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Scripts[i].UNUSED1 = binaryReader.ReadUInt32();
                            PICkitFunctions.DevFile.Scripts[i].ScriptLength = binaryReader.ReadUInt16();
                            PICkitFunctions.DevFile.Scripts[i].Script = new ushort[(int)PICkitFunctions.DevFile.Scripts[i].ScriptLength];
                            for (int k = 0; k < (int)PICkitFunctions.DevFile.Scripts[i].ScriptLength; k++)
                            {
                                PICkitFunctions.DevFile.Scripts[i].Script[k] = binaryReader.ReadUInt16();
                            }
                            PICkitFunctions.DevFile.Scripts[i].Comment = binaryReader.ReadString();
                        }
                        binaryReader.Close();
                    }
                    fileStream.Close();
                }
                catch
                {
                    result = false;
                    return result;
                }
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
        public static bool DetectDevice(int familyIndex, bool resetOnNotFound, bool keepVddOn)
        {
            bool result;
            if (familyIndex == 16777215)
            {
                if (!PICkitFunctions.targetSelfPowered)
                {
                    PICkitFunctions.SetVDDVoltage(3.3f, 0.85f);
                }
                for (int i = 0; i < PICkitFunctions.DevFile.Families.Length; i++)
                {
                    if (PICkitFunctions.DevFile.Families[PICkitFunctions.familySearchTable[i]].PartDetect)
                    {
                        if (PICkitFunctions.searchDevice(PICkitFunctions.familySearchTable[i], true, keepVddOn))
                        {
                            result = true;
                            return result;
                        }
                    }
                }
                result = false;
            }
            else
            {
                PICkitFunctions.SetVDDVoltage(PICkitFunctions.vddLastSet, 0.85f);
                result = (!PICkitFunctions.DevFile.Families[familyIndex].PartDetect || PICkitFunctions.searchDevice(familyIndex, resetOnNotFound, keepVddOn));
            }
            return result;
        }
        public static int FindLastUsedInBuffer(uint[] bufferToSearch, uint blankValue, int startIndex)
        {
            int result;
            if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName != "KEELOQ® HCS")
            {
                for (int i = startIndex; i > 0; i--)
                {
                    if (bufferToSearch[i] != blankValue)
                    {
                        result = i;
                        return result;
                    }
                }
                result = 0;
            }
            else
            {
                result = bufferToSearch.Length - 1;
            }
            return result;
        }
        public static bool RunScriptUploadNoLen(int script, int repetitions)
        {
            bool flag = PICkitFunctions.writeUSB(new byte[]
			{
				169,
				165,
				PICkitFunctions.scriptRedirectTable[script].redirectToScriptLocation,
				(byte)repetitions,
				172
			});
            if (flag)
            {
                flag = PICkitFunctions.readUSB();
            }
            return flag;
        }
        public static bool GetUpload()
        {
            return PICkitFunctions.readUSB();
        }
        public static bool UploadData()
        {
            bool flag = PICkitFunctions.writeUSB(new byte[]
			{
				170
			});
            if (flag)
            {
                flag = PICkitFunctions.readUSB();
            }
            return flag;
        }
        public static bool UploadDataNoLen()
        {
            bool flag = PICkitFunctions.writeUSB(new byte[]
			{
				172
			});
            if (flag)
            {
                flag = PICkitFunctions.readUSB();
            }
            return flag;
        }
        public static bool RunScript(int script, int repetitions)
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				169,
				165,
				PICkitFunctions.scriptRedirectTable[script].redirectToScriptLocation,
				(byte)repetitions
			}) && (script != 1 || PICkitFunctions.assertMCLR || PICkitFunctions.HoldMCLR(false));
        }
        public static int DataClrAndDownload(byte[] dataArray, int startIndex)
        {
            int result;
            if (startIndex >= dataArray.Length)
            {
                result = 0;
            }
            else
            {
                int num = dataArray.Length - startIndex;
                if (num > 61)
                {
                    num = 61;
                }
                byte[] array = new byte[3 + num];
                array[0] = 167;
                array[1] = 168;
                array[2] = (byte)(num & 255);
                for (int i = 0; i < num; i++)
                {
                    array[3 + i] = dataArray[startIndex + i];
                }
                if (PICkitFunctions.writeUSB(array))
                {
                    result = startIndex + num;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static int DataDownload(byte[] dataArray, int startIndex, int lastIndex)
        {
            int result;
            if (startIndex >= lastIndex)
            {
                result = 0;
            }
            else
            {
                int num = lastIndex - startIndex;
                if (num > 62)
                {
                    num = 62;
                }
                byte[] array = new byte[2 + num];
                array[0] = 168;
                array[1] = (byte)(num & 255);
                for (int i = 0; i < num; i++)
                {
                    array[2 + i] = dataArray[startIndex + i];
                }
                if (PICkitFunctions.writeUSB(array))
                {
                    result = startIndex + num;
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }
        public static bool DownloadAddress3(int address)
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				167,
				168,
				3,
				(byte)(address & 255),
				(byte)(255 & address >> 8),
				(byte)(255 & address >> 16)
			});
        }
        public static bool DownloadAddress3MSBFirst(int address)
        {
            return PICkitFunctions.writeUSB(new byte[]
			{
				167,
				168,
				3,
				(byte)(255 & address >> 16),
				(byte)(255 & address >> 8),
				(byte)(address & 255)
			});
        }
        public static bool Download3Multiples(int downloadBytes, int multiples, int increment)
        {
            byte b = 167;
            while (true)
            {
                int num = multiples;
                if (multiples > 20)
                {
                    num = 20;
                    multiples -= 20;
                }
                else
                {
                    multiples = 0;
                }
                byte[] array = new byte[3 * num + 3];
                array[0] = b;
                array[1] = 168;
                array[2] = (byte)(3 * num);
                for (int i = 0; i < num; i++)
                {
                    array[3 + 3 * i] = (byte)(downloadBytes >> 16);
                    array[4 + 3 * i] = (byte)(downloadBytes >> 8);
                    array[5 + 3 * i] = (byte)downloadBytes;
                    downloadBytes += increment;
                }
                if (!PICkitFunctions.writeUSB(array))
                {
                    break;
                }
                b = 90;
                if (multiples <= 0)
                {
                    goto Block_4;
                }
            }
            bool result = false;
            return result;
        Block_4:
            result = true;
            return result;
        }
        public static uint ComputeChecksum(bool codeProtectOn, bool dataProtectOn)
        {
            uint num = 0u;
            uint result;
            if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue < 65535u)
            {
                int num2 = (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem;
                if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].OSSCALSave)
                {
                    num2--;
                }
                if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords > 0)
                {
                    if (((uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPMask & PICkitFunctions.DeviceBuffers.ConfigWords[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPConfig - 1)]) != (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPMask || codeProtectOn)
                    {
                        if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue < 16383u)
                        {
                            num2 = 64;
                        }
                        else
                        {
                            num2 = 0;
                        }
                    }
                }
                for (int i = 0; i < num2; i++)
                {
                    num += PICkitFunctions.DeviceBuffers.ProgramMemory[i];
                }
                if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords > 0)
                {
                    if (((uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPMask & PICkitFunctions.DeviceBuffers.ConfigWords[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPConfig - 1)]) != (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPMask || codeProtectOn)
                    {
                        for (int i = 0; i < (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWords; i++)
                        {
                            int num3 = 1;
                            for (int j = 0; j < i; j++)
                            {
                                num3 <<= 4;
                            }
                            num += (uint)((ulong)(15u & PICkitFunctions.DeviceBuffers.UserIDs[(int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWords - i - 1]) * (ulong)((long)num3));
                        }
                    }
                    for (int i = 0; i < (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords; i++)
                    {
                        if (i == (int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPConfig - 1))
                        {
                            uint num4 = PICkitFunctions.DeviceBuffers.ConfigWords[i] & (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMasks[i];
                            if (codeProtectOn)
                            {
                                num4 &= (uint)(~(uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPMask);
                            }
                            if (dataProtectOn)
                            {
                                num4 &= (uint)(~(uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DPMask);
                            }
                            num += num4;
                        }
                        else
                        {
                            num += (PICkitFunctions.DeviceBuffers.ConfigWords[i] & (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMasks[i]);
                        }
                    }
                }
                result = (num & 65535u);
            }
            else
            {
                int num2 = (int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigAddr / (uint)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemHexBytes);
                if ((long)num2 > (long)((ulong)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem))
                {
                    num2 = (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem;
                }
                for (int i = 0; i < num2; i++)
                {
                    uint num5 = PICkitFunctions.DeviceBuffers.ProgramMemory[i];
                    num += (num5 & 255u);
                    for (int k = 1; k < (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BytesPerLocation; k++)
                    {
                        num5 >>= 8;
                        num += (num5 & 255u);
                    }
                }
                if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords > 0)
                {
                    if (((uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPMask & PICkitFunctions.DeviceBuffers.ConfigWords[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPConfig - 1)]) != (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].CPMask || codeProtectOn)
                    {
                        num = 0u;
                        for (int i = 0; i < (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWords; i++)
                        {
                            uint num5 = PICkitFunctions.DeviceBuffers.UserIDs[i];
                            num += (num5 & 255u);
                            num += (num5 >> 8 & 255u);
                        }
                    }
                    for (int i = 0; i < (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords; i++)
                    {
                        uint num5 = PICkitFunctions.DeviceBuffers.ConfigWords[i] & (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMasks[i];
                        num += (num5 & 255u);
                        num += (num5 >> 8 & 255u);
                    }
                }
                result = (num & 65535u);
            }
            return result;
        }
        public static void ResetBuffers()
        {
            PICkitFunctions.DeviceBuffers = new DeviceData(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem, PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEMem, PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords, PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWords, PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue, (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].EEMemAddressIncrement, (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].UserIDBytes, PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigBlank, (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMasks[7]);
        }
        public static DeviceData CloneBuffers(DeviceData copyFrom)
        {
            DeviceData deviceData = new DeviceData(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgramMem, PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEMem, PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWords, PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWords, PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue, (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].EEMemAddressIncrement, (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].UserIDBytes, PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigBlank, (uint)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigMasks[7]);
            for (int i = 0; i < copyFrom.ProgramMemory.Length; i++)
            {
                deviceData.ProgramMemory[i] = copyFrom.ProgramMemory[i];
            }
            for (int i = 0; i < copyFrom.EEPromMemory.Length; i++)
            {
                deviceData.EEPromMemory[i] = copyFrom.EEPromMemory[i];
            }
            for (int i = 0; i < copyFrom.ConfigWords.Length; i++)
            {
                deviceData.ConfigWords[i] = copyFrom.ConfigWords[i];
            }
            for (int i = 0; i < copyFrom.UserIDs.Length; i++)
            {
                deviceData.UserIDs[i] = copyFrom.UserIDs[i];
            }
            deviceData.OSCCAL = copyFrom.OSCCAL;
            deviceData.OSCCAL = copyFrom.BandGap;
            return deviceData;
        }
        public static void PrepNewPart(bool resetBuffers)
        {
            if (resetBuffers)
            {
                PICkitFunctions.ResetBuffers();
            }
            float num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].Vpp;
            if (num < 1f || (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0))
            {
                if (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0)
                {
                    string text = PICkitFunctions.DevFile.Scripts[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript - 1)].ScriptName;
                    text = text.Substring(text.Length - 2);
                    if (text == "HV")
                    {
                        num = (float)PICkitFunctions.DevFile.Scripts[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript - 1)].Script[1] / 10f;
                        PICkitFunctions.SetVppVoltage(num, 0.7f);
                    }
                    else
                    {
                        PICkitFunctions.SetVppVoltage(PICkitFunctions.vddLastSet, 0.7f);
                    }
                }
                else
                {
                    PICkitFunctions.SetVppVoltage(PICkitFunctions.vddLastSet, 0.7f);
                }
            }
            else
            {
                PICkitFunctions.SetVppVoltage(num, 0.7f);
            }
            PICkitFunctions.downloadPartScripts(PICkitFunctions.GetActiveFamily());
        }
        public static uint ReadDebugVector()
        {
            PICkitFunctions.RunScript(0, 1);
            PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DebugReadVectorScript);
            PICkitFunctions.UploadData();
            PICkitFunctions.RunScript(1, 1);
            int num = 2;
            int num2 = 2;
            uint num3 = 0u;
            for (int i = 0; i < num; i++)
            {
                uint num4 = (uint)PICkitFunctions.Usb_read_array[num2++];
                num4 |= (uint)((uint)PICkitFunctions.Usb_read_array[num2++] << 8);
                if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift > 0)
                {
                    num4 = (num4 >> 1 & PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue);
                }
                if (i == 0)
                {
                    num3 = num4;
                }
                else
                {
                    num3 += num4 << 16;
                }
            }
            return num3;
        }
        public static void WriteDebugVector(uint debugWords)
        {
            int num = 2;
            byte[] array = new byte[4];
            PICkitFunctions.RunScript(0, 1);
            int i = 0;
            int num2 = 0;
            while (i < num)
            {
                uint num3;
                if (i == 0)
                {
                    num3 = (debugWords & 65535u);
                }
                else
                {
                    num3 = debugWords >> 16;
                }
                if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift > 0)
                {
                    num3 <<= 1;
                }
                array[num2++] = (byte)(num3 & 255u);
                array[num2++] = (byte)(num3 >> 8 & 255u);
                i++;
            }
            PICkitFunctions.DataClrAndDownload(array, 0);
            PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DebugWriteVectorScript);
            PICkitFunctions.RunScript(1, 1);
        }
        public static bool ReadPICkitVoltages(ref float vdd, ref float vpp)
        {
            bool result;
            if (PICkitFunctions.writeUSB(new byte[]
			{
				163
			}))
            {
                if (PICkitFunctions.readUSB())
                {
                    float num = (float)((int)PICkitFunctions.Usb_read_array[2] * 256 + (int)PICkitFunctions.Usb_read_array[1]);
                    vdd = num / 65536f * 5f;
                    num = (float)((int)PICkitFunctions.Usb_read_array[4] * 256 + (int)PICkitFunctions.Usb_read_array[3]);
                    vpp = num / 65536f * 13.7f;
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }
        public static bool SetVppVoltage(float voltage, float threshold)
        {
            byte b = 64;
            byte b2 = (byte)(voltage * 18.61f);
            byte b3 = (byte)(threshold * voltage * 18.61f);
            return PICkitFunctions.writeUSB(new byte[]
			{
				161,
				b,
				b2,
				b3
			});
        }
        public static bool SendScript(byte[] script)
        {
            int num = script.Length;
            byte[] array = new byte[2 + num];
            array[0] = 166;
            array[1] = (byte)num;
            for (int i = 0; i < num; i++)
            {
                array[2 + i] = script[i];
            }
            return PICkitFunctions.writeUSB(array);
        }
        private static ushort readPkStatus()
        {
            ushort result;
            if (PICkitFunctions.writeUSB(new byte[]
			{
				162
			}))
            {
                if (PICkitFunctions.readUSB())
                {
                    result = (ushort)((int)PICkitFunctions.Usb_read_array[2] * 256 + (int)PICkitFunctions.Usb_read_array[1]);
                }
                else
                {
                    result = 65535;
                }
            }
            else
            {
                result = 65535;
            }
            return result;
        }
        public static bool writeUSB(byte[] commandList)
        {
            int num = 0;
            PICkitFunctions.Usb_write_array[0] = 0;
            for (int i = 1; i < PICkitFunctions.Usb_write_array.Length; i++)
            {
                PICkitFunctions.Usb_write_array[i] = 173;
            }
            Array.Copy(commandList, 0, PICkitFunctions.Usb_write_array, 1, commandList.Length);
            bool flag = USB.WriteFile(PICkitFunctions.usbWriteHandle, PICkitFunctions.Usb_write_array, PICkitFunctions.Usb_write_array.Length, ref num, 0);
            return num == PICkitFunctions.Usb_write_array.Length && flag;
        }
        public static bool readUSB()
        {
            int num = 0;
            bool result;
            if (PICkitFunctions.LearnMode)
            {
                result = true;
            }
            else
            {
                bool flag = USB.ReadFile(PICkitFunctions.usbReadHandle, PICkitFunctions.Usb_read_array, PICkitFunctions.Usb_read_array.Length, ref num, 0);
                result = (num == PICkitFunctions.Usb_read_array.Length && flag);
            }
            return result;
        }
        public static bool VerifyDeviceID(bool resetOnNoDevice, bool keepVddOn)
        {
            float num = PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].Vpp;
            if (num < 1f || (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0))
            {
                if (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0)
                {
                    string text = PICkitFunctions.DevFile.Scripts[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript - 1)].ScriptName;
                    text = text.Substring(text.Length - 2);
                    if (text == "HV")
                    {
                        num = (float)PICkitFunctions.DevFile.Scripts[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript - 1)].Script[1] / 10f;
                        PICkitFunctions.SetVppVoltage(num, 0.7f);
                    }
                    else
                    {
                        PICkitFunctions.SetVppVoltage(PICkitFunctions.vddLastSet, 0.7f);
                    }
                }
                else
                {
                    PICkitFunctions.SetVppVoltage(PICkitFunctions.vddLastSet, 0.7f);
                }
            }
            else
            {
                PICkitFunctions.SetVppVoltage(num, 0.7f);
            }
            PICkitFunctions.SetMCLRTemp(true);
            PICkitFunctions.VddOn();
            if (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0)
            {
                PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript);
            }
            else
            {
                if (PICkitFunctions.vppFirstEnabled && PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgEntryVPPScript > 0)
                {
                    PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgEntryVPPScript);
                }
                else
                {
                    PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgEntryScript);
                }
            }
            PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ReadDevIDScript);
            PICkitFunctions.UploadData();
            PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgExitScript);
            if (!keepVddOn)
            {
                PICkitFunctions.VddOff();
            }
            if (!PICkitFunctions.assertMCLR)
            {
                PICkitFunctions.HoldMCLR(false);
            }
            uint num2 = (uint)PICkitFunctions.Usb_read_array[5] * 16777216u + (uint)PICkitFunctions.Usb_read_array[4] * 65536u + (uint)PICkitFunctions.Usb_read_array[3] * 256u + (uint)PICkitFunctions.Usb_read_array[2];
            for (int i = 0; i < (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift; i++)
            {
                num2 >>= 1;
            }
            if (PICkitFunctions.Usb_read_array[1] == 4)
            {
                PICkitFunctions.LastDeviceRev = (int)PICkitFunctions.Usb_read_array[5] * 256 + (int)PICkitFunctions.Usb_read_array[4];
                if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue == 4294967295u)
                {
                    PICkitFunctions.LastDeviceRev >>= 4;
                }
            }
            else
            {
                PICkitFunctions.LastDeviceRev = (int)(num2 & ~(int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].DeviceIDMask);
            }
            PICkitFunctions.LastDeviceRev &= 65535;
            PICkitFunctions.LastDeviceRev &= (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue;
            num2 &= PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].DeviceIDMask;
            PICkitFunctions.LastDeviceID = num2;
            bool result;
            if (num2 != PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DeviceID)
            {
                result = false;
            }
            else
            {
                if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].OSSCALSave)
                {
                    PICkitFunctions.VddOn();
                    PICkitFunctions.ReadOSSCAL();
                }
                if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].BandGapMask > 0u)
                {
                    PICkitFunctions.VddOn();
                    PICkitFunctions.ReadBandGap();
                }
                if (!keepVddOn)
                {
                    PICkitFunctions.VddOff();
                }
                result = true;
            }
            return result;
        }
        private static bool searchDevice(int familyIndex, bool resetOnNoDevice, bool keepVddOn)
        {
            int activePart = PICkitFunctions.ActivePart;
            if (PICkitFunctions.ActivePart != 0)
            {
                PICkitFunctions.lastFoundPart = PICkitFunctions.ActivePart;
            }
            float num = PICkitFunctions.DevFile.Families[familyIndex].Vpp;
            if (num < 1f || (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0))
            {
                if (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0)
                {
                    string text = PICkitFunctions.DevFile.Scripts[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript - 1)].ScriptName;
                    text = text.Substring(text.Length - 2);
                    if (text == "HV")
                    {
                        num = (float)PICkitFunctions.DevFile.Scripts[(int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript - 1)].Script[1] / 10f;
                        PICkitFunctions.SetVppVoltage(num, 0.7f);
                    }
                    else
                    {
                        PICkitFunctions.SetVppVoltage(PICkitFunctions.vddLastSet, 0.7f);
                    }
                }
                else
                {
                    PICkitFunctions.SetVppVoltage(PICkitFunctions.vddLastSet, 0.7f);
                }
            }
            else
            {
                PICkitFunctions.SetVppVoltage(num, 0.7f);
            }
            PICkitFunctions.SetMCLRTemp(true);
            PICkitFunctions.VddOn();
            if (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0)
            {
                PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript);
            }
            else
            {
                if (PICkitFunctions.vppFirstEnabled && PICkitFunctions.DevFile.Families[familyIndex].ProgEntryVPPScript > 0)
                {
                    PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.Families[familyIndex].ProgEntryVPPScript);
                }
                else
                {
                    PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.Families[familyIndex].ProgEntryScript);
                }
            }
            PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.Families[familyIndex].ReadDevIDScript);
            PICkitFunctions.UploadData();
            PICkitFunctions.ExecuteScript((int)PICkitFunctions.DevFile.Families[familyIndex].ProgExitScript);
            if (!keepVddOn)
            {
                PICkitFunctions.VddOff();
            }
            if (!PICkitFunctions.assertMCLR)
            {
                PICkitFunctions.HoldMCLR(false);
            }
            uint num2 = (uint)PICkitFunctions.Usb_read_array[5] * 16777216u + (uint)PICkitFunctions.Usb_read_array[4] * 65536u + (uint)PICkitFunctions.Usb_read_array[3] * 256u + (uint)PICkitFunctions.Usb_read_array[2];
            for (int i = 0; i < (int)PICkitFunctions.DevFile.Families[familyIndex].ProgMemShift; i++)
            {
                num2 >>= 1;
            }
            if (PICkitFunctions.Usb_read_array[1] == 4)
            {
                PICkitFunctions.LastDeviceRev = (int)PICkitFunctions.Usb_read_array[5] * 256 + (int)PICkitFunctions.Usb_read_array[4];
                if (PICkitFunctions.DevFile.Families[familyIndex].BlankValue == 4294967295u)
                {
                    PICkitFunctions.LastDeviceRev >>= 4;
                }
            }
            else
            {
                PICkitFunctions.LastDeviceRev = (int)(num2 & ~(int)PICkitFunctions.DevFile.Families[familyIndex].DeviceIDMask);
            }
            PICkitFunctions.LastDeviceRev &= 65535;
            PICkitFunctions.LastDeviceRev &= (int)PICkitFunctions.DevFile.Families[familyIndex].BlankValue;
            num2 &= PICkitFunctions.DevFile.Families[familyIndex].DeviceIDMask;
            PICkitFunctions.LastDeviceID = num2;
            PICkitFunctions.ActivePart = 0;
            for (int j = 0; j < PICkitFunctions.DevFile.PartsList.Length; j++)
            {
                if ((int)PICkitFunctions.DevFile.PartsList[j].Family == familyIndex)
                {
                    if (PICkitFunctions.DevFile.PartsList[j].DeviceID == num2)
                    {
                        PICkitFunctions.ActivePart = j;
                        break;
                    }
                }
            }
            bool result;
            if (PICkitFunctions.ActivePart == 0)
            {
                if (activePart != 0)
                {
                    PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart] = PICkitFunctions.DevFile.PartsList[activePart];
                    PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DeviceID = 0u;
                    PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].PartName = "Unsupported Part";
                }
                if (resetOnNoDevice)
                {
                    PICkitFunctions.ResetBuffers();
                }
                result = false;
            }
            else
            {
                if (PICkitFunctions.ActivePart == PICkitFunctions.lastFoundPart && PICkitFunctions.scriptBufferChecksum != 0u && PICkitFunctions.scriptBufferChecksum == PICkitFunctions.getScriptBufferChecksum())
                {
                    result = true;
                }
                else
                {
                    PICkitFunctions.downloadPartScripts(familyIndex);
                    if (PICkitFunctions.ActivePart != PICkitFunctions.lastFoundPart)
                    {
                        PICkitFunctions.ResetBuffers();
                    }
                    if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].OSSCALSave)
                    {
                        PICkitFunctions.VddOn();
                        PICkitFunctions.ReadOSSCAL();
                    }
                    if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].BandGapMask > 0u)
                    {
                        PICkitFunctions.VddOn();
                        PICkitFunctions.ReadBandGap();
                    }
                    if (!keepVddOn)
                    {
                        PICkitFunctions.VddOff();
                    }
                    result = true;
                }
            }
            return result;
        }
        private static void downloadPartScripts(int familyIndex)
        {
            bool flag = PICkitFunctions.writeUSB(new byte[]
			{
				171
			});
            for (int i = 0; i < PICkitFunctions.scriptRedirectTable.Length; i++)
            {
                PICkitFunctions.scriptRedirectTable[i].redirectToScriptLocation = 0;
                PICkitFunctions.scriptRedirectTable[i].deviceFileScriptNumber = 0;
            }
            if (PICkitFunctions.DevFile.Families[familyIndex].ProgEntryScript != 0)
            {
                if (PICkitFunctions.lvpEnabled && PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript > 0)
                {
                    PICkitFunctions.downloadScript(0, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].LVPScript);
                }
                else
                {
                    if (PICkitFunctions.vppFirstEnabled && PICkitFunctions.DevFile.Families[familyIndex].ProgEntryVPPScript != 0)
                    {
                        PICkitFunctions.downloadScript(0, (int)PICkitFunctions.DevFile.Families[familyIndex].ProgEntryVPPScript);
                    }
                    else
                    {
                        PICkitFunctions.downloadScript(0, (int)PICkitFunctions.DevFile.Families[familyIndex].ProgEntryScript);
                    }
                }
            }
            if (PICkitFunctions.DevFile.Families[familyIndex].ProgExitScript != 0)
            {
                PICkitFunctions.downloadScript(1, (int)PICkitFunctions.DevFile.Families[familyIndex].ProgExitScript);
            }
            if (PICkitFunctions.DevFile.Families[familyIndex].ReadDevIDScript != 0)
            {
                PICkitFunctions.downloadScript(2, (int)PICkitFunctions.DevFile.Families[familyIndex].ReadDevIDScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemRdScript != 0)
            {
                PICkitFunctions.downloadScript(3, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemRdScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ChipErasePrepScript != 0)
            {
                PICkitFunctions.downloadScript(4, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ChipErasePrepScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemAddrSetScript != 0)
            {
                PICkitFunctions.downloadScript(5, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemAddrSetScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemWrPrepScript != 0)
            {
                PICkitFunctions.downloadScript(6, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemWrPrepScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemWrScript != 0)
            {
                PICkitFunctions.downloadScript(7, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemWrScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERdPrepScript != 0)
            {
                PICkitFunctions.downloadScript(8, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERdPrepScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERdScript != 0)
            {
                PICkitFunctions.downloadScript(9, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERdScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEWrPrepScript != 0)
            {
                PICkitFunctions.downloadScript(10, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEWrPrepScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEWrScript != 0)
            {
                PICkitFunctions.downloadScript(11, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEWrScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigRdPrepScript != 0)
            {
                PICkitFunctions.downloadScript(12, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigRdPrepScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigRdScript != 0)
            {
                PICkitFunctions.downloadScript(13, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigRdScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWrPrepScript != 0)
            {
                PICkitFunctions.downloadScript(14, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWrPrepScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWrScript != 0)
            {
                PICkitFunctions.downloadScript(15, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ConfigWrScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDRdPrepScript != 0)
            {
                PICkitFunctions.downloadScript(16, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDRdPrepScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDRdScript != 0)
            {
                PICkitFunctions.downloadScript(17, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDRdScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWrPrepScript != 0)
            {
                PICkitFunctions.downloadScript(18, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWrPrepScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWrScript != 0)
            {
                PICkitFunctions.downloadScript(19, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].UserIDWrScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].OSCCALRdScript != 0)
            {
                PICkitFunctions.downloadScript(20, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].OSCCALRdScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].OSCCALWrScript != 0)
            {
                PICkitFunctions.downloadScript(21, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].OSCCALWrScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ChipEraseScript != 0)
            {
                PICkitFunctions.downloadScript(22, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ChipEraseScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemEraseScript != 0)
            {
                PICkitFunctions.downloadScript(23, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].ProgMemEraseScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEMemEraseScript != 0)
            {
                PICkitFunctions.downloadScript(24, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEMemEraseScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DebugRowEraseScript != 0)
            {
                PICkitFunctions.downloadScript(26, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].DebugRowEraseScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].TestMemoryRdScript != 0)
            {
                PICkitFunctions.downloadScript(27, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].TestMemoryRdScript);
            }
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERowEraseScript != 0)
            {
                PICkitFunctions.downloadScript(28, (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERowEraseScript);
            }
            PICkitFunctions.scriptBufferChecksum = PICkitFunctions.getScriptBufferChecksum();
        }
        private static uint getScriptBufferChecksum()
        {
            uint result;
            if (PICkitFunctions.LearnMode)
            {
                result = 0u;
            }
            else
            {
                if (PICkitFunctions.writeUSB(new byte[]
				{
					175
				}))
                {
                    if (PICkitFunctions.readUSB())
                    {
                        uint num = (uint)PICkitFunctions.Usb_read_array[4];
                        num += (uint)((uint)PICkitFunctions.Usb_read_array[3] << 8);
                        num += (uint)((uint)PICkitFunctions.Usb_read_array[2] << 16);
                        num += (uint)((uint)PICkitFunctions.Usb_read_array[1] << 24);
                        result = num;
                    }
                    else
                    {
                        result = 0u;
                    }
                }
                else
                {
                    result = 0u;
                }
            }
            return result;
        }
        private static bool downloadScript(byte scriptBufferLocation, int scriptArrayIndex)
        {
            byte b = scriptBufferLocation;
            byte b2 = 0;
            while ((int)b2 < PICkitFunctions.scriptRedirectTable.Length)
            {
                if (scriptArrayIndex == PICkitFunctions.scriptRedirectTable[(int)b2].deviceFileScriptNumber)
                {
                    b = b2;
                    break;
                }
                b2 += 1;
            }
            PICkitFunctions.scriptRedirectTable[(int)scriptBufferLocation].redirectToScriptLocation = b;
            PICkitFunctions.scriptRedirectTable[(int)scriptBufferLocation].deviceFileScriptNumber = scriptArrayIndex;
            bool result;
            if (scriptBufferLocation != b)
            {
                result = true;
            }
            else
            {
                int scriptLength = (int)PICkitFunctions.DevFile.Scripts[--scriptArrayIndex].ScriptLength;
                byte[] array = new byte[3 + scriptLength];
                array[0] = 164;
                array[1] = scriptBufferLocation;
                array[2] = (byte)scriptLength;
                for (int i = 0; i < scriptLength; i++)
                {
                    ushort num = PICkitFunctions.DevFile.Scripts[scriptArrayIndex].Script[i];
                    if (PICkitFunctions.fastProgramming)
                    {
                        array[3 + i] = (byte)num;
                    }
                    else
                    {
                        if (num == 43751)
                        {
                            ushort num2 = (ushort)(PICkitFunctions.DevFile.Scripts[scriptArrayIndex].Script[i + 1] & 255);
                            if (num2 < 170 && num2 != 0)
                            {
                                array[3 + i++] = (byte)num;
                                byte b3 = (byte)PICkitFunctions.DevFile.Scripts[scriptArrayIndex].Script[i];
                                array[3 + i] = (byte)(b3 + b3 / 2);
                            }
                            else
                            {
                                array[3 + i++] = 232;
                                array[3 + i] = 2;
                            }
                        }
                        else
                        {
                            if (num == 43752)
                            {
                                ushort num2 = (ushort)(PICkitFunctions.DevFile.Scripts[(byte)scriptArrayIndex].Script[i + 1] & 255);
                                if (num2 < 171 && num2 != 0)
                                {
                                    array[3 + i++] = (byte)num;
                                    byte b3 = (byte)PICkitFunctions.DevFile.Scripts[scriptArrayIndex].Script[i];
                                    array[3 + i] = (byte)(b3 + b3 / 2);
                                }
                                else
                                {
                                    array[3 + i++] = 232;
                                    array[3 + i] = 0;
                                }
                            }
                            else
                            {
                                array[3 + i] = (byte)num;
                            }
                        }
                    }
                }
                result = PICkitFunctions.writeUSB(array);
            }
            return result;
        }
    }
}
