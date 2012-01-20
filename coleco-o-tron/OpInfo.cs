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
        public const int InstrEXSPHL = 4;
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


        public static int[] opListing;
        public static int[] GetOps()
        {
            if (opListing == null)
                LoadOps();
            return opListing;
        }
        public static void LoadOps()
        {
            opListing = new int[10];
        }
    }
}
