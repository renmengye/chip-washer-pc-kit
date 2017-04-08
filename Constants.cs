
namespace ChipWaherPCKit
{
    public class Constants
    {
        public enum PICkit2USB
        {
            found,
            notFound,
            writeError,
            readError,
            firmwareInvalid,
            bootloader
        }
        public enum PICkit2PWR
        {
            no_response,
            vdd_on,
            vdd_off,
            vdderror,
            vpperror,
            vddvpperrors,
            selfpowered,
            unpowered
        }
        public enum FileRead
        {
            success,
            failed,
            noconfig,
            partialcfg,
            largemem
        }
        public enum StatusColor
        {
            normal,
            green,
            yellow,
            red
        }
        public enum VddTargetSelect
        {
            auto,
            pickit2,
            target
        }
        public const string AppVersion = "3.00.00";
        public const byte DevFileCompatLevel = 6;
        public const byte DevFileCompatLevelMin = 0;
        public const string UserGuideFileName = "\\User Guides\\DevBugger User Manual.pdf";
        public const byte FWVerMajorReq = 2;
        public const byte FWVerMinorReq = 32;
        public const byte FWVerDotReq = 0;
        public const string FWFileName = "PK2V023200.hex";
        public const uint PACKET_SIZE = 65u;
        public const uint USB_REPORTLENGTH = 64u;
        public const byte BIT_MASK_0 = 1;
        public const byte BIT_MASK_1 = 2;
        public const byte BIT_MASK_2 = 4;
        public const byte BIT_MASK_3 = 8;
        public const byte BIT_MASK_4 = 16;
        public const byte BIT_MASK_5 = 32;
        public const byte BIT_MASK_6 = 64;
        public const byte BIT_MASK_7 = 128;
        public const ushort MChipVendorID = 1240;
        public const ushort Pk2DeviceID = 51;
        public const ushort ConfigRows = 2;
        public const ushort ConfigColumns = 4;
        public const ushort MaxReadCfgMasks = 8;
        public const ushort NumConfigMasks = 9;
        public const float VddThresholdForSelfPoweredTarget = 2.3f;
        public const bool NoMessage = false;
        public const bool ShowMessage = true;
        public const bool UpdateMemoryDisplays = true;
        public const bool DontUpdateMemDisplays = false;
        public const bool EraseEE = true;
        public const bool WriteEE = false;
        public const int UploadBufferSize = 128;
        public const int DownLoadBufferSize = 256;
        public const byte READFWFLASH = 1;
        public const byte WRITEFWFLASH = 2;
        public const byte ERASEFWFLASH = 3;
        public const byte READFWEEDATA = 4;
        public const byte WRITEFWEEDATA = 5;
        public const byte RESETFWDEVICE = 255;
        public const byte ENTER_BOOTLOADER = 66;
        public const byte NO_OPERATION = 90;
        public const byte FIRMWARE_VERSION = 118;
        public const byte SETVDD = 160;
        public const byte SETVPP = 161;
        public const byte READ_STATUS = 162;
        public const byte READ_VOLTAGES = 163;
        public const byte DOWNLOAD_SCRIPT = 164;
        public const byte RUN_SCRIPT = 165;
        public const byte EXECUTE_SCRIPT = 166;
        public const byte CLR_DOWNLOAD_BUFFER = 167;
        public const byte DOWNLOAD_DATA = 168;
        public const byte CLR_UPLOAD_BUFFER = 169;
        public const byte UPLOAD_DATA = 170;
        public const byte CLR_SCRIPT_BUFFER = 171;
        public const byte UPLOAD_DATA_NOLEN = 172;
        public const byte END_OF_BUFFER = 173;
        public const byte RESET = 174;
        public const byte SCRIPT_BUFFER_CHKSUM = 175;
        public const byte SET_VOLTAGE_CALS = 176;
        public const byte WR_INTERNAL_EE = 177;
        public const byte RD_INTERNAL_EE = 178;
        public const byte ENTER_UART_MODE = 179;
        public const byte EXIT_UART_MODE = 180;
        public const byte ENTER_LEARN_MODE = 181;
        public const byte EXIT_LEARN_MODE = 182;
        public const byte ENABLE_PK2GO_MODE = 183;
        public const byte LOGIC_ANALYZER_GO = 184;
        public const byte COPY_RAM_UPLOAD = 185;
        public const byte MC_READ_OSCCAL = 128;
        public const byte MC_WRITE_OSCCAL = 129;
        public const byte MC_START_CHECKSUM = 130;
        public const byte MC_VERIFY_CHECKSUM = 131;
        public const byte MC_CHECK_DEVICE_ID = 132;
        public const byte MC_READ_BANDGAP = 133;
        public const byte MC_WRITE_CFG_BANDGAP = 134;
        public const byte MC_CHANGE_CHKSM_FRMT = 135;
        public const byte _VDD_ON = 255;
        public const byte _VDD_OFF = 254;
        public const byte _VDD_GND_ON = 253;
        public const byte _VDD_GND_OFF = 252;
        public const byte _VPP_ON = 251;
        public const byte _VPP_OFF = 250;
        public const byte _VPP_PWM_ON = 249;
        public const byte _VPP_PWM_OFF = 248;
        public const byte _MCLR_GND_ON = 247;
        public const byte _MCLR_GND_OFF = 246;
        public const byte _BUSY_LED_ON = 245;
        public const byte _BUSY_LED_OFF = 244;
        public const byte _SET_ICSP_PINS = 243;
        public const byte _WRITE_BYTE_LITERAL = 242;
        public const byte _WRITE_BYTE_BUFFER = 241;
        public const byte _READ_BYTE_BUFFER = 240;
        public const byte _READ_BYTE = 239;
        public const byte _WRITE_BITS_LITERAL = 238;
        public const byte _WRITE_BITS_BUFFER = 237;
        public const byte _READ_BITS_BUFFER = 236;
        public const byte _READ_BITS = 235;
        public const byte _SET_ICSP_SPEED = 234;
        public const byte _LOOP = 233;
        public const byte _DELAY_LONG = 232;
        public const byte _DELAY_SHORT = 231;
        public const byte _IF_EQ_GOTO = 230;
        public const byte _IF_GT_GOTO = 229;
        public const byte _GOTO_INDEX = 228;
        public const byte _EXIT_SCRIPT = 227;
        public const byte _PEEK_SFR = 226;
        public const byte _POKE_SFR = 225;
        public const byte _ICDSLAVE_RX = 224;
        public const byte _ICDSLAVE_TX_LIT = 223;
        public const byte _ICDSLAVE_TX_BUF = 222;
        public const byte _LOOPBUFFER = 221;
        public const byte _ICSP_STATES_BUFFER = 220;
        public const byte _POP_DOWNLOAD = 219;
        public const byte _COREINST18 = 218;
        public const byte _COREINST24 = 217;
        public const byte _NOP24 = 216;
        public const byte _VISI24 = 215;
        public const byte _RD2_BYTE_BUFFER = 214;
        public const byte _RD2_BITS_BUFFER = 213;
        public const byte _WRITE_BUFWORD_W = 212;
        public const byte _WRITE_BUFBYTE_W = 211;
        public const byte _CONST_WRITE_DL = 210;
        public const byte _WRITE_BITS_LIT_HLD = 209;
        public const byte _WRITE_BITS_BUF_HLD = 208;
        public const byte _SET_AUX = 207;
        public const byte _AUX_STATE_BUFFER = 206;
        public const byte _I2C_START = 205;
        public const byte _I2C_STOP = 204;
        public const byte _I2C_WR_BYTE_LIT = 203;
        public const byte _I2C_WR_BYTE_BUF = 202;
        public const byte _I2C_RD_BYTE_ACK = 201;
        public const byte _I2C_RD_BYTE_NACK = 200;
        public const byte _SPI_WR_BYTE_LIT = 199;
        public const byte _SPI_WR_BYTE_BUF = 198;
        public const byte _SPI_RD_BYTE_BUF = 197;
        public const byte _SPI_RDWR_BYTE_LIT = 196;
        public const byte _SPI_RDWR_BYTE_BUF = 195;
        public const byte _ICDSLAVE_RX_BL = 194;
        public const byte _ICDSLAVE_TX_LIT_BL = 193;
        public const byte _ICDSLAVE_TX_BUF_BL = 192;
        public const byte _MEASURE_PULSE = 191;
        public const byte _UNIO_TX = 190;
        public const byte _UNIO_TX_RX = 189;
        public const byte _JT2_SETMODE = 188;
        public const byte _JT2_SENDCMD = 187;
        public const byte _JT2_XFERDATA8_LIT = 186;
        public const byte _JT2_XFERDATA32_LIT = 185;
        public const byte _JT2_XFRFASTDAT_LIT = 184;
        public const byte _JT2_XFRFASTDAT_BUF = 183;
        public const byte _JT2_XFERINST_BUF = 182;
        public const byte _JT2_GET_PE_RESP = 181;
        public const byte _JT2_WAIT_PE_RESP = 180;
        public const int SEARCH_ALL_FAMILIES = 16777215;
        public const byte PROG_ENTRY = 0;
        public const byte PROG_EXIT = 1;
        public const byte RD_DEVID = 2;
        public const byte PROGMEM_RD = 3;
        public const byte ERASE_CHIP_PREP = 4;
        public const byte PROGMEM_ADDRSET = 5;
        public const byte PROGMEM_WR_PREP = 6;
        public const byte PROGMEM_WR = 7;
        public const byte EE_RD_PREP = 8;
        public const byte EE_RD = 9;
        public const byte EE_WR_PREP = 10;
        public const byte EE_WR = 11;
        public const byte CONFIG_RD_PREP = 12;
        public const byte CONFIG_RD = 13;
        public const byte CONFIG_WR_PREP = 14;
        public const byte CONFIG_WR = 15;
        public const byte USERID_RD_PREP = 16;
        public const byte USERID_RD = 17;
        public const byte USERID_WR_PREP = 18;
        public const byte USERID_WR = 19;
        public const byte OSSCAL_RD = 20;
        public const byte OSSCAL_WR = 21;
        public const byte ERASE_CHIP = 22;
        public const byte ERASE_PROGMEM = 23;
        public const byte ERASE_EE = 24;
        public const byte ROW_ERASE = 26;
        public const byte TESTMEM_RD = 27;
        public const byte EEROW_ERASE = 28;
        public const int OSCCAL_MASK = 7;
        public const int PROTOCOL_CFG = 0;
        public const int ADR_MASK_CFG = 1;
        public const int ADR_BITS_CFG = 2;
        public const int CS_PINS_CFG = 3;
        public const int I2C_BUS = 1;
        public const int SPI_BUS = 2;
        public const int MICROWIRE_BUS = 3;
        public const int UNIO_BUS = 4;
        public const bool READ_BIT = true;
        public const bool WRITE_BIT = false;
        public const uint FLASHW_STOP = 0u;
        public const uint FLASHW_CAPTION = 1u;
        public const uint FLASHW_TRAY = 2u;
        public const uint FLASHW_ALL = 3u;
        public const uint FLASHW_TIMER = 4u;
        public const uint FLASHW_TIMERNOFG = 12u;
        public const byte ADC_CAL_L = 0;
        public const byte ADC_CAL_H = 1;
        public const byte CPP_OFFSET = 2;
        public const byte CPP_CAL = 3;
        public const byte UNIT_ID = 240;
        public const uint P32_PROGRAM_FLASH_START_ADDR = 486539264u;
        public const uint P32_BOOT_FLASH_START_ADDR = 532676608u;
        public static uint[] BASELINE_CAL = new uint[]
		{
			3072u,
			37u,
			103u,
			104u,
			105u,
			102u,
			3326u,
			6u,
			1574u,
			2568u,
			1830u,
			2570u,
			112u,
			3202u,
			49u,
			752u,
			2575u,
			753u,
			2575u,
			3321u,
			48u,
			3272u,
			49u,
			1286u,
			0u,
			0u,
			0u,
			0u,
			0u,
			752u,
			2584u,
			0u,
			3321u,
			48u,
			0u,
			0u,
			0u,
			753u,
			2584u,
			1030u,
			2568u
		};
        public static uint[] MR16F676FAM_CAL = new uint[]
		{
			12288u,
			10245u,
			0u,
			0u,
			9u,
			5763u,
			144u,
			401u,
			415u,
			12542u,
			133u,
			4739u,
			12295u,
			153u,
			389u,
			6277u,
			10255u,
			7301u,
			10257u,
			416u,
			12418u,
			161u,
			2976u,
			10262u,
			2977u,
			10262u,
			12537u,
			160u,
			12488u,
			161u,
			5125u,
			0u,
			0u,
			0u,
			0u,
			0u,
			2976u,
			10271u,
			0u,
			12537u,
			160u,
			0u,
			0u,
			0u,
			2977u,
			10271u,
			4101u,
			10255u
		};
    }
}
