
namespace ChipWaherPCKit
{
    public class DeviceData
    {
        public uint[] ProgramMemory;
        public uint[] EEPromMemory;
        public uint[] ConfigWords;
        public uint[] UserIDs;
        public uint OSCCAL;
        public uint BandGap;
        public DeviceData(uint progMemSize, ushort eeMemSize, byte numConfigs, byte numIDs, uint memBlankVal, int eeBytes, int idBytes, ushort[] configBlank, uint OSCCALInit)
        {
            this.ProgramMemory = new uint[progMemSize];
            this.EEPromMemory = new uint[(int)eeMemSize];
            this.ConfigWords = new uint[(int)numConfigs];
            this.UserIDs = new uint[(int)numIDs];
            this.ClearProgramMemory(memBlankVal);
            this.ClearEEPromMemory(eeBytes, memBlankVal);
            this.ClearConfigWords(configBlank);
            this.ClearUserIDs(idBytes, memBlankVal);
            this.OSCCAL = (OSCCALInit | 255u);
            this.BandGap = memBlankVal;
        }
        public void ClearProgramMemory(uint memBlankVal)
        {
            if (this.ProgramMemory.Length > 0)
            {
                for (int i = 0; i < this.ProgramMemory.Length; i++)
                {
                    this.ProgramMemory[i] = memBlankVal;
                }
            }
        }
        public void ClearConfigWords(ushort[] configBlank)
        {
            if (this.ConfigWords.Length > 0)
            {
                for (int i = 0; i < this.ConfigWords.Length; i++)
                {
                    this.ConfigWords[i] = (uint)configBlank[i];
                }
            }
        }
        public void ClearUserIDs(int idBytes, uint memBlankVal)
        {
            if (this.UserIDs.Length > 0)
            {
                uint num = memBlankVal;
                if (idBytes == 1)
                {
                    num = 255u;
                }
                for (int i = 0; i < this.UserIDs.Length; i++)
                {
                    this.UserIDs[i] = num;
                }
            }
        }
        public void ClearEEPromMemory(int eeBytes, uint memBlankVal)
        {
            if (this.EEPromMemory.Length > 0)
            {
                uint num = 255u;
                if (eeBytes == 2)
                {
                    num = 65535u;
                }
                if (memBlankVal == 4095u)
                {
                    num = 4095u;
                }
                for (int i = 0; i < this.EEPromMemory.Length; i++)
                {
                    this.EEPromMemory[i] = num;
                }
            }
        }
    }
}
