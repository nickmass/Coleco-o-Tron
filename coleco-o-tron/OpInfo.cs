using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coleco_o_tron
{
    class OpInfo
    {
        public const int InstrLD = 0;
        public const int InstrEXDEHL = 1;
        public const int InstrEXAFAF = 2;
        public const int InstrEXX = 3;
        public const int InstrEXSP = 4;
        public const int Instr8ADD = 5;
        public const int Instr8ADC = 6;
        public const int Instr8SUB = 7;
        public const int Instr8SBC = 8;
        public const int InstrAND = 9;
        public const int InstrOR = 10;
        public const int InstrXOR = 11;
        public const int InstrCP = 12;
        public const int Instr8INC = 13;
        public const int Instr8DEC = 14;
        public const int InstrDAA = 15;
        public const int InstrCPL = 16;
        public const int InstrNEG = 17;
        public const int InstrCCF = 18;
        public const int InstrSCF = 19;
        public const int InstrNOP = 20;
        public const int InstrHALT = 21;
        public const int InstrDI = 22;
        public const int InstrEI = 23;
        public const int InstrIM0 = 24;
        public const int InstrIM1 = 25;
        public const int InstrIM2 = 26;
        public const int InstrADD = 27;
        public const int InstrADC = 28;
        public const int InstrSBC = 29;
        public const int InstrINC = 30;
        public const int InstrDEC = 31;
        public const int InstrRLCA = 32;
        public const int InstrRLA = 33;
        public const int InstrRRCA = 34;
        public const int InstrRRA = 35;
        public const int InstrRLC = 36;
        public const int InstrRL = 37;
        public const int InstrRRC = 38;
        public const int InstrRR = 39;
        public const int InstrSLA = 40;
        public const int InstrSRA = 41;
        public const int InstrSRL = 42;
        public const int InstrRLD = 43;
        public const int InstrRRD = 44;
        public const int InstrBIT = 45;
        public const int InstrSET = 46;
        public const int InstrRES = 47;
        public const int InstrJP = 48;
        public const int InstrJPc = 49;
        public const int InstrJR = 50;
        public const int InstrJRC = 51;
        public const int InstrJRNC = 52;
        public const int InstrJRZ = 53;
        public const int InstrJRNZ = 54;
        public const int InstrDJNZ = 55;
        public const int InstrCALL = 56;
        public const int InstrCALLc = 57;
        public const int InstrRET = 58;
        public const int InstrRETc = 59;
        public const int InstrRETI = 60;
        public const int InstrRETN = 61;
        public const int InstrRST = 62;
        public const int InstrLDI = 63;
        public const int InstrLDIR = 64;
        public const int InstrLDD = 65;
        public const int InstrLDDR = 66;
        public const int InstrCPI = 67;
        public const int InstrCPIR = 68;
        public const int InstrCPD = 69;
        public const int InstrCPDR = 70;
        public const int InstrINA = 71;
        public const int InstrIN = 72;
        public const int InstrINI = 73;
        public const int InstrINIR = 74;
        public const int InstrIND = 75;
        public const int InstrINDR = 76;
        public const int InstrOUTA = 77;
        public const int InstrOUT = 78;
        public const int InstrOUTI = 79;
        public const int InstrOUTIR = 80;
        public const int InstrOUTD = 81;
        public const int InstrOUTDR = 82;
        public const int InstrSLL = 83;

        public const int PrefixED = 100;
        public const int PrefixCB = 101;
        public const int PrefixDD = 102;
        public const int PrefixFD = 103;


        public const int LocRegB = 0;
        public const int LocRegC = 1;
        public const int LocRegD = 2;
        public const int LocRegE = 3;
        public const int LocRegF = 6;
        public const int LocRegA = 7;
        public const int LocRegH = 4;
        public const int LocRegL = 5;
        public const int Loc8Immediate = 8;
        public const int LocAddrAF = 9;
        public const int LocAddrBC = 10;
        public const int LocAddrDE = 11;
        public const int LocAddrHL = 12;
        public const int LocRegAF = 13;
        public const int LocRegBC = 14;
        public const int LocRegDE = 15;
        public const int LocRegHL = 16;
        public const int LocAddrAbsoulute = 17;
        public const int Loc16Immediate = 18;
        public const int LocNone = 19;
        public const int Loc8Stack = 20;
        public const int Loc16Stack = 21;
        public const int LocRegSP = 22;
        public const int Loc16AddrAbsoulute = 23;
        public const int LocRegI = 24;
        public const int LocRegR = 25;


        public static int[] edOpListing;
        public static int[] GetEDOps()
        {
            if (edOpListing == null)
                LoadEDOps();
            return edOpListing;
        }

        public static int[] opListing;
        public static int[] GetOps()
        {
            if (opListing == null)
                LoadOps();
            return opListing;
        }

        private static void LoadEDOps()
        {
            edOpListing = new int[256];

            for (int i = 0; i < 256; i++ )
                SetEDOp(i, InstrNOP, LocNone, LocNone, 2);

            SetEDOp(0x40, InstrIN, LocRegB, LocNone, 3);
            SetEDOp(0x50, InstrIN, LocRegD, LocNone, 3);
            SetEDOp(0x60, InstrIN, LocRegH, LocNone, 3);
            SetEDOp(0x70, InstrIN, LocNone, LocNone, 3);
            SetEDOp(0x48, InstrIN, LocRegC, LocNone, 3);
            SetEDOp(0x58, InstrIN, LocRegE, LocNone, 3);
            SetEDOp(0x68, InstrIN, LocRegL, LocNone, 3);
            SetEDOp(0x78, InstrIN, LocRegA, LocNone, 3);


            SetEDOp(0x41, InstrOUT, LocNone, LocRegB, 3);
            SetEDOp(0x51, InstrOUT, LocNone, LocRegD, 3);
            SetEDOp(0x61, InstrOUT, LocNone, LocRegH, 3);
            SetEDOp(0x71, InstrOUT, LocNone, LocNone, 3);
            SetEDOp(0x49, InstrOUT, LocNone, LocRegC, 3);
            SetEDOp(0x59, InstrOUT, LocNone, LocRegE, 3);
            SetEDOp(0x69, InstrOUT, LocNone, LocRegL, 3);
            SetEDOp(0x79, InstrOUT, LocNone, LocRegA, 3);

            SetEDOp(0x42, InstrSBC, LocRegHL, LocRegBC, 4);
            SetEDOp(0x52, InstrSBC, LocRegHL, LocRegDE, 4);
            SetEDOp(0x62, InstrSBC, LocRegHL, LocRegHL, 4);
            SetEDOp(0x72, InstrSBC, LocRegHL, LocRegSP, 4);

            SetEDOp(0x4A, InstrADC, LocRegHL, LocRegBC, 4);
            SetEDOp(0x5A, InstrADC, LocRegHL, LocRegDE, 4);
            SetEDOp(0x6A, InstrADC, LocRegHL, LocRegHL, 4);
            SetEDOp(0x7A, InstrADC, LocRegHL, LocRegSP, 4);

            SetEDOp(0x43, InstrLD, Loc16AddrAbsoulute, LocRegBC, 6);
            SetEDOp(0x53, InstrLD, Loc16AddrAbsoulute, LocRegDE, 6);
            SetEDOp(0x63, InstrLD, Loc16AddrAbsoulute, LocRegHL, 6);
            SetEDOp(0x73, InstrLD, Loc16AddrAbsoulute, LocRegSP, 6);

            SetEDOp(0x4B, InstrLD, LocRegBC, Loc16AddrAbsoulute, 6);
            SetEDOp(0x5B, InstrLD, LocRegDE, Loc16AddrAbsoulute, 6);
            SetEDOp(0x6B, InstrLD, LocRegHL, Loc16AddrAbsoulute, 6);
            SetEDOp(0x7B, InstrLD, LocRegSP, Loc16AddrAbsoulute, 6);

            SetEDOp(0x44, InstrNEG, LocRegA, LocNone, 2);
            SetEDOp(0x54, InstrNEG, LocRegA, LocNone, 2);
            SetEDOp(0x64, InstrNEG, LocRegA, LocNone, 2);
            SetEDOp(0x74, InstrNEG, LocRegA, LocNone, 2);
            SetEDOp(0x4C, InstrNEG, LocRegA, LocNone, 2);
            SetEDOp(0x5C, InstrNEG, LocRegA, LocNone, 2);
            SetEDOp(0x6C, InstrNEG, LocRegA, LocNone, 2);
            SetEDOp(0x7C, InstrNEG, LocRegA, LocNone, 2);

            SetEDOp(0x45, InstrRETN, LocNone, LocNone, 4);
            SetEDOp(0x55, InstrRETN, LocNone, LocNone, 4);
            SetEDOp(0x65, InstrRETN, LocNone, LocNone, 4);
            SetEDOp(0x75, InstrRETN, LocNone, LocNone, 4);
            SetEDOp(0x5D, InstrRETN, LocNone, LocNone, 4);
            SetEDOp(0x6D, InstrRETN, LocNone, LocNone, 4);
            SetEDOp(0x7D, InstrRETN, LocNone, LocNone, 4);

            SetEDOp(0x4D, InstrRETI, LocNone, LocNone, 4);

            SetEDOp(0x46, InstrIM0, LocNone, LocNone, 2);
            SetEDOp(0x66, InstrIM0, LocNone, LocNone, 2);
            SetEDOp(0x4E, InstrIM0, LocNone, LocNone, 2); //Should be illegal im0/1
            SetEDOp(0x6E, InstrIM0, LocNone, LocNone, 2); //Should be illegal im0/1

            SetEDOp(0x56, InstrIM1, LocNone, LocNone, 2);
            SetEDOp(0x76, InstrIM1, LocNone, LocNone, 2);

            SetEDOp(0x5E, InstrIM2, LocNone, LocNone, 2);
            SetEDOp(0x7E, InstrIM2, LocNone, LocNone, 2);

            SetEDOp(0x47, InstrLD, LocRegI, LocRegA, 2); //TODO - This group should set flags
            SetEDOp(0x57, InstrLD, LocRegA, LocRegI, 2);
            SetEDOp(0x4F, InstrLD, LocRegR, LocRegA, 2);
            SetEDOp(0x5F, InstrLD, LocRegA, LocRegR, 2);

            SetEDOp(0x67, InstrRRD, LocAddrHL, LocAddrHL, 5);

            SetEDOp(0x6F, InstrRLD, LocAddrHL, LocAddrHL, 5);

            SetEDOp(0xA0, InstrLDI, LocNone, LocNone, 4);
            SetEDOp(0xB0, InstrLDIR, LocNone, LocNone, 4);
            SetEDOp(0xA8, InstrLDD, LocNone, LocNone, 4);
            SetEDOp(0xB8, InstrLDDR, LocNone, LocNone, 4);

            SetEDOp(0xA1, InstrCPI, LocNone, LocNone, 4);
            SetEDOp(0xB1, InstrCPIR, LocNone, LocNone, 4);
            SetEDOp(0xA9, InstrCPD, LocNone, LocNone, 4);
            SetEDOp(0xB9, InstrCPDR, LocNone, LocNone, 4);

            SetEDOp(0xA2, InstrINI, LocNone, LocNone, 4);
            SetEDOp(0xB2, InstrINIR, LocNone, LocNone, 4);
            SetEDOp(0xAA, InstrIND, LocNone, LocNone, 4);
            SetEDOp(0xBA, InstrINDR, LocNone, LocNone, 4);

            SetEDOp(0xA3, InstrOUTI, LocNone, LocAddrHL, 4);
            SetEDOp(0xB3, InstrOUTIR, LocNone, LocAddrHL, 4);
            SetEDOp(0xAB, InstrOUTD, LocNone, LocAddrHL, 4);
            SetEDOp(0xBB, InstrOUTDR, LocNone, LocAddrHL, 4);

        }

        private static void LoadOps()
        {
            opListing = new int[256];

            SetOp(0xED, PrefixED, LocNone, LocNone, 0);
            SetOp(0xCB, PrefixCB, LocNone, LocNone, 0);
            SetOp(0xDD, PrefixDD, LocNone, LocNone, 0);
            SetOp(0xFD, PrefixFD, LocNone, LocNone, 0);

            SetOp(0x7F, InstrLD, LocRegA, LocRegA, 1);
            SetOp(0x78, InstrLD, LocRegA, LocRegB, 1);
            SetOp(0x79, InstrLD, LocRegA, LocRegC, 1);
            SetOp(0x7A, InstrLD, LocRegA, LocRegD, 1);
            SetOp(0x7B, InstrLD, LocRegA, LocRegE, 1);
            SetOp(0x7C, InstrLD, LocRegA, LocRegH, 1);
            SetOp(0x7D, InstrLD, LocRegA, LocRegL, 1);
            SetOp(0x47, InstrLD, LocRegB, LocRegA, 1);
            SetOp(0x40, InstrLD, LocRegB, LocRegB, 1);
            SetOp(0x41, InstrLD, LocRegB, LocRegC, 1);
            SetOp(0x42, InstrLD, LocRegB, LocRegD, 1);
            SetOp(0x43, InstrLD, LocRegB, LocRegE, 1);
            SetOp(0x44, InstrLD, LocRegB, LocRegH, 1);
            SetOp(0x45, InstrLD, LocRegB, LocRegL, 1);
            SetOp(0x4F, InstrLD, LocRegC, LocRegA, 1);
            SetOp(0x48, InstrLD, LocRegC, LocRegB, 1);
            SetOp(0x49, InstrLD, LocRegC, LocRegC, 1);
            SetOp(0x4A, InstrLD, LocRegC, LocRegD, 1);
            SetOp(0x4B, InstrLD, LocRegC, LocRegE, 1);
            SetOp(0x4C, InstrLD, LocRegC, LocRegH, 1);
            SetOp(0x4D, InstrLD, LocRegC, LocRegL, 1);
            SetOp(0x57, InstrLD, LocRegD, LocRegA, 1);
            SetOp(0x50, InstrLD, LocRegD, LocRegB, 1);
            SetOp(0x51, InstrLD, LocRegD, LocRegC, 1);
            SetOp(0x52, InstrLD, LocRegD, LocRegD, 1);
            SetOp(0x53, InstrLD, LocRegD, LocRegE, 1);
            SetOp(0x54, InstrLD, LocRegD, LocRegH, 1);
            SetOp(0x55, InstrLD, LocRegD, LocRegL, 1);
            SetOp(0x5F, InstrLD, LocRegE, LocRegA, 1);
            SetOp(0x58, InstrLD, LocRegE, LocRegB, 1);
            SetOp(0x59, InstrLD, LocRegE, LocRegC, 1);
            SetOp(0x5A, InstrLD, LocRegE, LocRegD, 1);
            SetOp(0x5B, InstrLD, LocRegE, LocRegE, 1);
            SetOp(0x5C, InstrLD, LocRegE, LocRegH, 1);
            SetOp(0x5D, InstrLD, LocRegE, LocRegL, 1);
            SetOp(0x67, InstrLD, LocRegH, LocRegA, 1);
            SetOp(0x60, InstrLD, LocRegH, LocRegB, 1);
            SetOp(0x61, InstrLD, LocRegH, LocRegC, 1);
            SetOp(0x62, InstrLD, LocRegH, LocRegD, 1);
            SetOp(0x63, InstrLD, LocRegH, LocRegE, 1);
            SetOp(0x64, InstrLD, LocRegH, LocRegH, 1);
            SetOp(0x65, InstrLD, LocRegH, LocRegL, 1);
            SetOp(0x6F, InstrLD, LocRegL, LocRegA, 1);
            SetOp(0x68, InstrLD, LocRegL, LocRegB, 1);
            SetOp(0x69, InstrLD, LocRegL, LocRegC, 1);
            SetOp(0x6A, InstrLD, LocRegL, LocRegD, 1);
            SetOp(0x6B, InstrLD, LocRegL, LocRegE, 1);
            SetOp(0x6C, InstrLD, LocRegL, LocRegH, 1);
            SetOp(0x6D, InstrLD, LocRegL, LocRegL, 1);

            SetOp(0x3E, InstrLD, LocRegA, Loc8Immediate, 2);
            SetOp(0x06, InstrLD, LocRegB, Loc8Immediate, 2);
            SetOp(0x0E, InstrLD, LocRegC, Loc8Immediate, 2);
            SetOp(0x16, InstrLD, LocRegD, Loc8Immediate, 2);
            SetOp(0x1E, InstrLD, LocRegE, Loc8Immediate, 2);
            SetOp(0x26, InstrLD, LocRegH, Loc8Immediate, 2);
            SetOp(0x2E, InstrLD, LocRegL, Loc8Immediate, 2);

            SetOp(0x7E, InstrLD, LocRegA, LocAddrHL, 2);
            SetOp(0x46, InstrLD, LocRegB, LocAddrHL, 2);
            SetOp(0x4E, InstrLD, LocRegC, LocAddrHL, 2);
            SetOp(0x56, InstrLD, LocRegD, LocAddrHL, 2);
            SetOp(0x5E, InstrLD, LocRegE, LocAddrHL, 2);
            SetOp(0x66, InstrLD, LocRegH, LocAddrHL, 2);
            SetOp(0x6E, InstrLD, LocRegL, LocAddrHL, 2);

            SetOp(0x77, InstrLD, LocAddrHL, LocRegA, 2);
            SetOp(0x70, InstrLD, LocAddrHL, LocRegB, 2);
            SetOp(0x71, InstrLD, LocAddrHL, LocRegC, 2);
            SetOp(0x72, InstrLD, LocAddrHL, LocRegD, 2);
            SetOp(0x73, InstrLD, LocAddrHL, LocRegE, 2);
            SetOp(0x74, InstrLD, LocAddrHL, LocRegH, 2);
            SetOp(0x75, InstrLD, LocAddrHL, LocRegL, 2);

            SetOp(0x36, InstrLD, LocAddrHL, Loc8Immediate, 3);

            SetOp(0x0A, InstrLD, LocRegA, LocAddrBC, 2);
            SetOp(0x1A, InstrLD, LocRegA, LocAddrDE, 2);

            SetOp(0x3A, InstrLD, LocRegA, LocAddrAbsoulute, 4);

            SetOp(0x02, InstrLD, LocAddrBC, LocRegA, 2);
            SetOp(0x12, InstrLD, LocAddrDE, LocRegA, 2);

            SetOp(0x32, InstrLD, LocAddrAbsoulute, LocRegA, 4);

            SetOp(0x01, InstrLD, LocRegBC, Loc16Immediate, 2);
            SetOp(0x11, InstrLD, LocRegDE, Loc16Immediate, 2);
            SetOp(0x21, InstrLD, LocRegHL, Loc16Immediate, 2);
            SetOp(0x31, InstrLD, LocRegSP, Loc16Immediate, 2);

            SetOp(0x2A, InstrLD, LocRegHL, Loc16AddrAbsoulute, 5);

            SetOp(0x22, InstrLD, Loc16AddrAbsoulute, LocRegHL, 5);

            SetOp(0xF9, InstrLD, LocRegSP, LocRegHL, 1);

            SetOp(0xC5, InstrLD, Loc16Stack, LocRegBC, 3);
            SetOp(0xD5, InstrLD, Loc16Stack, LocRegDE, 3);
            SetOp(0xE5, InstrLD, Loc16Stack, LocRegHL, 3);
            SetOp(0xF5, InstrLD, Loc16Stack, LocRegAF, 3);

            SetOp(0xC1, InstrLD, LocRegBC, Loc16Stack, 3);
            SetOp(0xD1, InstrLD, LocRegDE, Loc16Stack, 3);
            SetOp(0xE1, InstrLD, LocRegHL, Loc16Stack, 3);
            SetOp(0xF1, InstrLD, LocRegAF, Loc16Stack, 3);


            SetOp(0xEB, InstrEXDEHL, LocNone, LocNone, 1);
            SetOp(0x08, InstrEXAFAF, LocNone, LocNone, 1);
            SetOp(0xD9, InstrEXX, LocNone, LocNone, 1);

            SetOp(0xE3, InstrEXSP, LocRegHL, LocRegHL, 5);

            SetOp(0x87, Instr8ADD, LocRegA, LocRegA, 1);
            SetOp(0x80, Instr8ADD, LocRegA, LocRegB, 1);
            SetOp(0x81, Instr8ADD, LocRegA, LocRegB, 1);
            SetOp(0x82, Instr8ADD, LocRegA, LocRegB, 1);
            SetOp(0x83, Instr8ADD, LocRegA, LocRegB, 1);
            SetOp(0x84, Instr8ADD, LocRegA, LocRegB, 1);
            SetOp(0x85, Instr8ADD, LocRegA, LocRegB, 1);

            SetOp(0xC6, Instr8ADD, LocRegA, Loc8Immediate, 2);

            SetOp(0x86, Instr8ADD, LocRegA, LocAddrHL, 2);

            SetOp(0x8F, Instr8ADC, LocRegA, LocRegA, 1);
            SetOp(0x88, Instr8ADC, LocRegA, LocRegB, 1);
            SetOp(0x89, Instr8ADC, LocRegA, LocRegB, 1);
            SetOp(0x8A, Instr8ADC, LocRegA, LocRegB, 1);
            SetOp(0x8B, Instr8ADC, LocRegA, LocRegB, 1);
            SetOp(0x8C, Instr8ADC, LocRegA, LocRegB, 1);
            SetOp(0x8D, Instr8ADC, LocRegA, LocRegB, 1);

            SetOp(0xCE, Instr8ADC, LocRegA, Loc8Immediate, 2);

            SetOp(0x8E, Instr8ADC, LocRegA, LocAddrHL, 2);

            SetOp(0x97, Instr8SUB, LocRegA, LocRegA, 1);
            SetOp(0x90, Instr8SUB, LocRegA, LocRegB, 1);
            SetOp(0x91, Instr8SUB, LocRegA, LocRegB, 1);
            SetOp(0x92, Instr8SUB, LocRegA, LocRegB, 1);
            SetOp(0x93, Instr8SUB, LocRegA, LocRegB, 1);
            SetOp(0x94, Instr8SUB, LocRegA, LocRegB, 1);
            SetOp(0x95, Instr8SUB, LocRegA, LocRegB, 1);

            SetOp(0xD6, Instr8SUB, LocRegA, Loc8Immediate, 2);

            SetOp(0x96, Instr8SUB, LocRegA, LocAddrHL, 2);

            SetOp(0x9F, Instr8SBC, LocRegA, LocRegA, 1);
            SetOp(0x98, Instr8SBC, LocRegA, LocRegB, 1);
            SetOp(0x99, Instr8SBC, LocRegA, LocRegB, 1);
            SetOp(0x9A, Instr8SBC, LocRegA, LocRegB, 1);
            SetOp(0x9B, Instr8SBC, LocRegA, LocRegB, 1);
            SetOp(0x9C, Instr8SBC, LocRegA, LocRegB, 1);
            SetOp(0x9D, Instr8SBC, LocRegA, LocRegB, 1);

            SetOp(0xDE, Instr8SBC, LocRegA, Loc8Immediate, 2);

            SetOp(0x9E, Instr8SBC, LocRegA, LocAddrHL, 2);

            SetOp(0xA7, InstrAND, LocRegA, LocRegA, 1);
            SetOp(0xA0, InstrAND, LocRegA, LocRegB, 1);
            SetOp(0xA1, InstrAND, LocRegA, LocRegB, 1);
            SetOp(0xA2, InstrAND, LocRegA, LocRegB, 1);
            SetOp(0xA3, InstrAND, LocRegA, LocRegB, 1);
            SetOp(0xA4, InstrAND, LocRegA, LocRegB, 1);
            SetOp(0xA5, InstrAND, LocRegA, LocRegB, 1);

            SetOp(0xE6, InstrAND, LocRegA, Loc8Immediate, 2);

            SetOp(0xA6, InstrAND, LocRegA, LocAddrHL, 2);

            SetOp(0xB7, InstrOR, LocRegA, LocRegA, 1);
            SetOp(0xB0, InstrOR, LocRegA, LocRegB, 1);
            SetOp(0xB1, InstrOR, LocRegA, LocRegB, 1);
            SetOp(0xB2, InstrOR, LocRegA, LocRegB, 1);
            SetOp(0xB3, InstrOR, LocRegA, LocRegB, 1);
            SetOp(0xB4, InstrOR, LocRegA, LocRegB, 1);
            SetOp(0xB5, InstrOR, LocRegA, LocRegB, 1);

            SetOp(0xF6, InstrOR, LocRegA, Loc8Immediate, 2);

            SetOp(0xB6, InstrOR, LocRegA, LocAddrHL, 2);

            SetOp(0xAF, InstrXOR, LocRegA, LocRegA, 1);
            SetOp(0xA8, InstrXOR, LocRegA, LocRegB, 1);
            SetOp(0xA9, InstrXOR, LocRegA, LocRegB, 1);
            SetOp(0xAA, InstrXOR, LocRegA, LocRegB, 1);
            SetOp(0xAB, InstrXOR, LocRegA, LocRegB, 1);
            SetOp(0xAC, InstrXOR, LocRegA, LocRegB, 1);
            SetOp(0xAD, InstrXOR, LocRegA, LocRegB, 1);

            SetOp(0xEE, InstrXOR, LocRegA, Loc8Immediate, 2);

            SetOp(0xAE, InstrXOR, LocRegA, LocAddrHL, 2);

            SetOp(0xBF, InstrCP, LocRegA, LocRegA, 1);
            SetOp(0xB8, InstrCP, LocRegA, LocRegB, 1);
            SetOp(0xB9, InstrCP, LocRegA, LocRegB, 1);
            SetOp(0xBA, InstrCP, LocRegA, LocRegB, 1);
            SetOp(0xBB, InstrCP, LocRegA, LocRegB, 1);
            SetOp(0xBC, InstrCP, LocRegA, LocRegB, 1);
            SetOp(0xBD, InstrCP, LocRegA, LocRegB, 1);

            SetOp(0xFE, InstrCP, LocRegA, Loc8Immediate, 2);

            SetOp(0xBE, InstrCP, LocRegA, LocAddrHL, 2);

            SetOp(0x3C, Instr8INC, LocRegA, LocRegA, 1);
            SetOp(0x04, Instr8INC, LocRegB, LocRegB, 1);
            SetOp(0x0C, Instr8INC, LocRegC, LocRegC, 1);
            SetOp(0x14, Instr8INC, LocRegD, LocRegD, 1);
            SetOp(0x1C, Instr8INC, LocRegE, LocRegE, 1);
            SetOp(0x24, Instr8INC, LocRegH, LocRegH, 1);
            SetOp(0x2C, Instr8INC, LocRegL, LocRegL, 1);

            SetOp(0x34, Instr8INC, LocAddrHL, LocAddrHL, 3);

            SetOp(0x3D, Instr8DEC, LocRegA, LocRegA, 1);
            SetOp(0x05, Instr8DEC, LocRegB, LocRegB, 1);
            SetOp(0x0D, Instr8DEC, LocRegC, LocRegC, 1);
            SetOp(0x15, Instr8DEC, LocRegD, LocRegD, 1);
            SetOp(0x1D, Instr8DEC, LocRegE, LocRegE, 1);
            SetOp(0x25, Instr8DEC, LocRegH, LocRegH, 1);
            SetOp(0x2D, Instr8DEC, LocRegL, LocRegL, 1);

            SetOp(0x35, Instr8DEC, LocAddrHL, LocAddrHL, 3);

            SetOp(0x27, InstrDAA, LocRegA, LocNone, 1);

            SetOp(0x2F, InstrCPL, LocRegA, LocNone, 1);

            SetOp(0x3F, InstrCCF, LocNone, LocNone, 1);

            SetOp(0x37, InstrSCF, LocNone, LocNone, 1);

            SetOp(0x00, InstrNOP, LocNone, LocNone, 1);

            SetOp(0x76, InstrHALT, LocNone, LocNone, 1);

            SetOp(0xF3, InstrDI, LocNone, LocNone, 1);

            SetOp(0xFB, InstrEI, LocNone, LocNone, 1);

            SetOp(0x09, InstrADD, LocRegHL, LocRegBC, 3);
            SetOp(0x19, InstrADD, LocRegHL, LocRegDE, 3);
            SetOp(0x29, InstrADD, LocRegHL, LocRegHL, 3);
            SetOp(0x39, InstrADD, LocRegHL, LocRegSP, 3);

            SetOp(0x03, InstrINC, LocRegBC, LocRegBC, 1);
            SetOp(0x13, InstrINC, LocRegDE, LocRegDE, 1);
            SetOp(0x23, InstrINC, LocRegHL, LocRegHL, 1);
            SetOp(0x33, InstrINC, LocRegSP, LocRegSP, 1);

            SetOp(0x0B, InstrDEC, LocRegBC, LocRegBC, 1);
            SetOp(0x1B, InstrDEC, LocRegDE, LocRegDE, 1);
            SetOp(0x2B, InstrDEC, LocRegHL, LocRegHL, 1);
            SetOp(0x3B, InstrDEC, LocRegSP, LocRegSP, 1);

            SetOp(0x07, InstrRLCA, LocRegA, LocNone, 1);

            SetOp(0x17, InstrRLA, LocRegA, LocNone, 1);

            SetOp(0x0F, InstrRRCA, LocRegA, LocNone, 1);

            SetOp(0x1F, InstrRRA, LocRegA, LocNone, 1);

            SetOp(0xC3, InstrJP, LocNone, Loc16Immediate, 3);

            SetOp(0xC2, InstrJPc, LocNone, Loc16Immediate, 3);
            SetOp(0xD2, InstrJPc, LocNone, Loc16Immediate, 3);
            SetOp(0xE2, InstrJPc, LocNone, Loc16Immediate, 3);
            SetOp(0xF2, InstrJPc, LocNone, Loc16Immediate, 3);
            SetOp(0xCA, InstrJPc, LocNone, Loc16Immediate, 3);
            SetOp(0xDA, InstrJPc, LocNone, Loc16Immediate, 3);
            SetOp(0xEA, InstrJPc, LocNone, Loc16Immediate, 3);
            SetOp(0xFA, InstrJPc, LocNone, Loc16Immediate, 3);

            SetOp(0x18, InstrJR, LocNone, Loc8Immediate, 3);

            SetOp(0x38, InstrJRC, LocNone, Loc8Immediate, 2);

            SetOp(0x30, InstrJRNC, LocNone, Loc8Immediate, 2);

            SetOp(0x28, InstrJRZ, LocNone, Loc8Immediate, 2);

            SetOp(0x20, InstrJRNZ, LocNone, Loc8Immediate, 2);

            SetOp(0xE9, InstrJP, LocNone, LocAddrHL, 1);

            SetOp(0x10, InstrDJNZ, LocNone, Loc8Immediate, 2);

            SetOp(0xCD, InstrCALL, LocNone, Loc16Immediate, 5);

            SetOp(0xC4, InstrCALLc, LocNone, Loc16Immediate, 3);
            SetOp(0xD4, InstrCALLc, LocNone, Loc16Immediate, 3);
            SetOp(0xE4, InstrCALLc, LocNone, Loc16Immediate, 3);
            SetOp(0xF4, InstrCALLc, LocNone, Loc16Immediate, 3);
            SetOp(0xCC, InstrCALLc, LocNone, Loc16Immediate, 3);
            SetOp(0xDC, InstrCALLc, LocNone, Loc16Immediate, 3);
            SetOp(0xEC, InstrCALLc, LocNone, Loc16Immediate, 3);
            SetOp(0xFC, InstrCALLc, LocNone, Loc16Immediate, 3);

            SetOp(0xC9, InstrRET, LocNone, LocNone, 3);

            SetOp(0xC0, InstrRET, LocNone, LocNone, 1);
            SetOp(0xD0, InstrRET, LocNone, LocNone, 1);
            SetOp(0xE0, InstrRET, LocNone, LocNone, 1);
            SetOp(0xF0, InstrRET, LocNone, LocNone, 1);
            SetOp(0xC8, InstrRET, LocNone, LocNone, 1);
            SetOp(0xD8, InstrRET, LocNone, LocNone, 1);
            SetOp(0xE8, InstrRET, LocNone, LocNone, 1);
            SetOp(0xF8, InstrRET, LocNone, LocNone, 1);

            SetOp(0xC7, InstrRST, LocNone, LocNone, 3);
            SetOp(0xD7, InstrRST, LocNone, LocNone, 3);
            SetOp(0xE7, InstrRST, LocNone, LocNone, 3);
            SetOp(0xF7, InstrRST, LocNone, LocNone, 3);
            SetOp(0xCF, InstrRST, LocNone, LocNone, 3);
            SetOp(0xDF, InstrRST, LocNone, LocNone, 3);
            SetOp(0xEF, InstrRST, LocNone, LocNone, 3);
            SetOp(0xFF, InstrRST, LocNone, LocNone, 3);

            SetOp(0xDB, InstrINA, LocNone, Loc8Immediate, 3);

            SetOp(0xD3, InstrOUTA, LocNone , Loc8Immediate, 3);

        }

        private static void SetEDOp(int opcode, int instruction, int destination, int source, int cycles)
        {
            if (edOpListing[opcode] != 0 && (edOpListing[opcode] & 0xFF) != InstrNOP)
                throw new Exception("Duplicate Opcode");
            edOpListing[opcode] = instruction | (destination << 8) | (source << 16) | (cycles << 24);
        }

        private static void SetOp(int opcode, int instruction, int destination, int source, int cycles)
        {
            if(opListing[opcode] != 0)
                throw new Exception("Duplicate Opcode");
            opListing[opcode] = instruction | (destination << 8) | (source << 16) | (cycles << 24);
        }
    }
}
