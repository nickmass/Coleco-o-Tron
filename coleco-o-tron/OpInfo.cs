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
