using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChipWaherPCKit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            byte[] array = new byte[128];
            base.Update();
            PICkitFunctions.RunScript(0, 1);
            if (PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERdPrepScript > 0)
            {
                if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].EEMemHexBytes == 4)
                {
                    PICkitFunctions.DownloadAddress3((int)(PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEAddr / 2u));
                }
                else
                {
                    PICkitFunctions.DownloadAddress3(0);
                }
                PICkitFunctions.RunScript(8, 1);
            }
            int eEMemBytesPerWord = (int)PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].EEMemBytesPerWord;
            int num = 128 / ((int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERdLocations * eEMemBytesPerWord);
            int num2 = num * (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EERdLocations;
            int num3 = 0;
            do
            {
                PICkitFunctions.RunScriptUploadNoLen(9, num);
                Array.Copy(PICkitFunctions.Usb_read_array, 1L, array, 0L, 64L);
                PICkitFunctions.UploadDataNoLen();
                Array.Copy(PICkitFunctions.Usb_read_array, 1L, array, 64L, 64L);
                int num4 = 0;
                for (int i = 0; i < num2; i++)
                {
                    int num5 = 0;
                    uint num6 = (uint)array[num4 + num5++];
                    if (num5 < eEMemBytesPerWord)
                    {
                        num6 |= (uint)((uint)array[num4 + num5++] << 8);
                    }
                    num4 += num5;
                    if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].ProgMemShift > 0)
                    {
                        //num6 = (num6 >> 1 & eEBlank);
                    }
                    PICkitFunctions.DeviceBuffers.EEPromMemory[num3++] = num6;
                    if (num3 >= (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEMem)
                    {
                        break;
                    }
                }
            }
            while (num3 < (int)PICkitFunctions.DevFile.PartsList[PICkitFunctions.ActivePart].EEMem);
            PICkitFunctions.RunScript(1, 1);

            foreach (var a in array)
            {
                this.textBox1.AppendText(a.ToString());
                this.textBox1.AppendText(" ");
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.InitializeComponent();
            float num = this.loadINI();
            this.initializeGUI();
            if (this.loadDeviceFile())
            {
                if (this.toolStripMenuItemManualSelect.Checked)
                {
                    this.ManualAutoSelectToggle(false);
                }
                this.buildDeviceMenu();
                if (!this.allowDataEdits)
                {
                    this.dataGridProgramMemory.ReadOnly = true;
                    this.dataGridViewEEPROM.ReadOnly = true;
                }
                PICkitFunctions.ResetBuffers();
                this.testConnected = this.checkForTest();
                if (this.testConnected)
                {
                    this.testConnected = this.testMenuBuild();
                }
                this.uartWindow.VddCallback = new DelegateVddCallback(this.SetVddState);
                if (!this.detectPICkit2(true, true))
                {
                    if (this.bootLoad)
                    {
                        return;
                    }
                    if (this.oldFirmware)
                    {
                        FormPICusb.TestMemoryOpen = false;
                        this.timerDLFW.Enabled = true;
                        return;
                    }
                    Thread.Sleep(3000);
                    if (!this.detectPICkit2(true, true))
                    {
                        return;
                    }
                }
                this.partialEnableGUIControls();
                PICkitFunctions.ExitUARTMode();
                PICkitFunctions.VddOff();
                PICkitFunctions.SetVDDVoltage(3.3f, 0.85f);
                if (this.autoDetectToolStripMenuItem.Checked)
                {
                    this.lookForPoweredTarget(false);
                }
                if (this.searchOnStartup && PICkitFunctions.DetectDevice(16777215, true, this.chkBoxVddOn.Checked))
                {
                    this.setGUIVoltageLimits(true);
                    PICkitFunctions.SetVDDVoltage((float)this.numUpDnVDD.Value, 0.85f);
                    this.displayStatusWindow.Text = this.displayStatusWindow.Text + "\nPIC Device Found.";
                    this.fullEnableGUIControls();
                }
                else
                {
                    for (int i = 0; i < PICkitFunctions.DevFile.Info.NumberFamilies; i++)
                    {
                        if (PICkitFunctions.DevFile.Families[i].FamilyName == this.lastFamily)
                        {
                            PICkitFunctions.SetActiveFamily(i);
                            if (!PICkitFunctions.DevFile.Families[i].PartDetect)
                            {
                                this.buildDeviceSelectDropDown(i);
                                this.comboBoxSelectPart.Visible = true;
                                this.comboBoxSelectPart.SelectedIndex = 0;
                                this.displayDevice.Visible = true;
                            }
                        }
                    }
                    for (int j = 1; j < PICkitFunctions.DevFile.Info.NumberParts; j++)
                    {
                        if ((int)PICkitFunctions.DevFile.PartsList[j].Family == PICkitFunctions.GetActiveFamily())
                        {
                            PICkitFunctions.DevFile.PartsList[0].VddMax = PICkitFunctions.DevFile.PartsList[j].VddMax;
                            PICkitFunctions.DevFile.PartsList[0].VddMin = PICkitFunctions.DevFile.PartsList[j].VddMin;
                            break;
                        }
                    }
                    this.setGUIVoltageLimits(true);
                }
                if (num != 0f && PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].FamilyName == this.lastFamily && !FormPICusb.selfPoweredTarget)
                {
                    if (num > (float)this.numUpDnVDD.Maximum)
                    {
                        num = (float)this.numUpDnVDD.Maximum;
                    }
                    if (num < (float)this.numUpDnVDD.Minimum)
                    {
                        num = (float)this.numUpDnVDD.Minimum;
                    }
                    this.numUpDnVDD.Value = (decimal)num;
                    PICkitFunctions.SetVDDVoltage((float)this.numUpDnVDD.Value, 0.85f);
                }
                this.checkForPowerErrors();
                if (FormPICusb.TestMemoryEnabled)
                {
                    this.toolStripMenuItemTestMemory.Visible = true;
                    if (FormPICusb.TestMemoryOpen)
                    {
                        this.openTestMemory();
                    }
                }
                if (!PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].PartDetect)
                {
                    this.disableGUIControls();
                }
                this.updateGUI(true);
            }
        }
        private uint getEEBlank()
        {
            uint result = 255u;
            if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].EEMemAddressIncrement > 1)
            {
                result = 65535u;
            }
            if (PICkitFunctions.DevFile.Families[PICkitFunctions.GetActiveFamily()].BlankValue == 4095u)
            {
                result = 4095u;
            }
            return result;
        }

        // PICusb.FormPICusb
        private bool detectPICkit2(bool showFound, bool detectMult)
        {
            Constants.PICkit2USB pICkit2USB;
            if (detectMult)
            {
                //FormPICusb.pk2number = 0;
                pICkit2USB = PICkitFunctions.DetectPICkit2Device(0, false);
                if (pICkit2USB != Constants.PICkit2USB.notFound)
                {
                    Constants.PICkit2USB pICkit2USB2 = PICkitFunctions.DetectPICkit2Device(1, false);
                    if (pICkit2USB2 != Constants.PICkit2USB.notFound)
                    {
                        //DialogUnitSelect dialogUnitSelect = new DialogUnitSelect();
                        //dialogUnitSelect.ShowDialog();
                    }
                }
            }
            pICkit2USB = PICkitFunctions.DetectPICkit2Device(0, true);
            bool result;
            if (pICkit2USB == Constants.PICkit2USB.found)
            {
                //this.downloadPICkit2FirmwareToolStripMenuItem.Enabled = true;
                //this.chkBoxVddOn.Enabled = true;
                //if (!FormPICusb.selfPoweredTarget)
                //{
                //    this.numUpDnVDD.Enabled = true;
                //}
                //this.deviceToolStripMenuItem.Enabled = true;
                if (showFound)
                {
                    string serialUnitID = PICkitFunctions.GetSerialUnitID();
                    if (serialUnitID[0] == '-')
                    {
                        //this.displayStatusWindow.Text = "DevBugger found and connected.";
                        this.Text = "PICusb";
                    }
                    else
                    {
                        if (serialUnitID == "DEVBUGV3")
                        {
                            //this.displayStatusWindow.Text = "DevBugger found and connected.";
                            //this.Text = "PICusb";
                        }
                        else
                        {
                            //this.displayStatusWindow.Text = "DevBugger connected.";
                            //this.Text = "PICusb";
                        }
                    }
                }
                result = true;
            }
            else
            {
                //this.downloadPICkit2FirmwareToolStripMenuItem.Enabled = false;
                //this.chkBoxVddOn.Enabled = false;
                //this.numUpDnVDD.Enabled = false;
                //this.deviceToolStripMenuItem.Enabled = false;
                //this.disableGUIControls();
                if (pICkit2USB == Constants.PICkit2USB.firmwareInvalid)
                {
                    //this.displayStatusWindow.BackColor = Color.Yellow;
                    //this.downloadPICkit2FirmwareToolStripMenuItem.Enabled = true;
                    //this.displayStatusWindow.Text = "The OS v" + PICkitFunctions.FirmwareVersion + " must be updated.\nUse the Tools menu to download a new OS.";
                    //this.oldFirmware = true;
                    result = false;
                }
                else
                {
                    if (pICkit2USB == Constants.PICkit2USB.bootloader)
                    {
                        //this.displayStatusWindow.BackColor = Color.Yellow;
                        //this.downloadPICkit2FirmwareToolStripMenuItem.Enabled = true;
                        //this.displayStatusWindow.Text = "The programmer has no Operating System.\nUse the Tools menu to download an OS.";
                        //this.bootLoad = true;
                        result = false;
                    }
                    else
                    {
                        //this.displayStatusWindow.BackColor = Color.Salmon;
                        //this.displayStatusWindow.Text = "DevBugger not found.  Check USB connections and \nuse Tools->Check Communication to retry.";
                        result = false;
                    }
                }
            }
            return result;
        }
    }
}
