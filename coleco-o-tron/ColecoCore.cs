using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace coleco_o_tron
{
    class ColecoCore
    {
        enum Prefix
        {
            None,
            DD,
            FD
        }
        enum InterruptMode
        {
            IM0,
            IM1,
            IM2
        }
        private int regA = 0xFF;
        private int regB = 0xFF;
        private int regC = 0xFF;
        private int regD = 0xFF;
        private int regE = 0xFF;
        private int regH = 0xFF;
        private int regL = 0xFF;
        private int regIXh = 0xFF;
        private int regIXl = 0xFF;
        private int regIYh = 0xFF;
        private int regIYl = 0xFF;
        public int regSP;
        public int regPC;
        private int intZ = 1;
        public bool flagZ
        {
            get
            {
                return intZ == 0;
            }
            set
            {
                if (value)
                    intZ = 0;
                else
                    intZ = 1;
            }
        }
        public bool flagN;
        public bool flagH;
        public bool flagC;
        public bool flagS;
        public bool flagV;
        private int regF
        {
            get
            {
                byte value = 0;
                if (flagS)
                    value |= 0x80;
                if (flagZ)
                    value |= 0x40;
                if (flagH)
                    value |= 0x10;
                if (flagV)
                    value |= 0x04;
                if (flagN)
                    value |= 0x02;
                if (flagC)
                    value |= 0x01;
                return value;
            }
            set
            {
                flagS = ((value & 0x80) != 0);
                flagZ = ((value & 0x40) != 0);
                flagH = ((value & 0x10) != 0);
                flagV = ((value & 0x04) != 0);
                flagN = ((value & 0x02) != 0);
                flagC = ((value & 0x01) != 0);
            }
        }
        public int regAF
        {
            get
            {
                return ((regA << 8) | regF) & 0xFFFF;
            }
            set
            {
                regA = (value >> 8) & 0xFF;
                regF = (value & 0xFF);
            }
        }
        public int regBC
        {
            get
            {
                return ((regB << 8) | regC) & 0xFFFF;
            }
            set
            {
                regB = (value >> 8) & 0xFF;
                regC = (value & 0xFF);
            }
        }
        public int regDE
        {
            get
            {
                return ((regD << 8) | regE) & 0xFFFF;
            }
            set
            {
                regD = (value >> 8) & 0xFF;
                regE = (value & 0xFF);
            }
        }
        public int regHL
        {
            get
            {
                return ((regH << 8) | regL) & 0xFFFF;
            }
            set
            {
                regH = (value >> 8) & 0xFF;
                regL = (value & 0xFF);
            }
        }
        public int regIX
        {
            get
            {
                return ((regIXh << 8) | regIXl) & 0xFFFF;
            }
            set
            {
                regIXh = (value >> 8) & 0xFF;
                regIXl = (value & 0xFF);
            }
        }
        public int regIY
        {
            get
            {
                return ((regIYh << 8) | regIYl) & 0xFFFF;
            }
            set
            {
                regIYh = (value >> 8) & 0xFF;
                regIYl = (value & 0xFF);
            }
        }
        private int regI;
        private int regR;
        private int shadowRegAF = 0xFFD7;
        private int shadowRegBC = 0xFFFF;
        private int shadowRegDE = 0xFFFF;
        private int shadowRegHL = 0xFFFF;
        private int interruptDataByte;
        private bool IFF1;
        private bool IFF2;
        private InterruptMode IM;
        private Prefix prefix = Prefix.None;

        bool emulationRunning = true;
        byte[] memory = new byte[0x10000];
        int counter;

        public PPU ppu = new PPU();
        public Input input = new Input();
        StringBuilder debugBuilder = new StringBuilder();

        public Debug debug;

        private int[] opTable;
        private int[] edOpTable;
        private long debugCount;
        public ColecoCore(byte[] rom)
        {
            debug = new Debug(this);
            regF = 0xD7;
            var fs = File.OpenRead("coleco.rom");
            for (int i = 0; i < 0x10000 && fs.CanRead; i++)
                memory[i] = (byte)fs.ReadByte();

            for (int i = 0x00; i < rom.Length && i < 0x8000; i++)
            {
                memory[i + 0x8000] = rom[i];
            }

            opTable = OpInfo.GetOps();
            edOpTable = OpInfo.GetEDOps();

            debugCount = -1;
            File.Delete("log.txt");
            
        }

        private bool dbgEnabled = false;
        public void Run()
        {
            int op, opCode, source, destination, instruction, cycles, result = 0, data = 0, temp = 0;

            emulationRunning = true;
            while (emulationRunning && !debug.Interrupt)
            {
                if(prefix == Prefix.None)
                    debug.Execute(regPC);
                opCode = Read();
                if (debugCount < 100000 && prefix == Prefix.None && (dbgEnabled || regPC == 0x1979))
                {
                    dbgEnabled = true;
                    debugCount++;
                    debugBuilder.AppendFormat(
                        "{0:x4} AF:{1:x4} BC:{2:x4} DE:{3:x4} HL:{4:x4} IX:{5:x4} IY:{6:x4}\r\n", regPC - 1, regAF, regBC, regDE, regHL, regIX, regIY);

                    if (debugCount == 100000)
                        File.AppendAllText("log.txt", debugBuilder.ToString());
                }
                op = opTable[opCode];
                instruction = op & 0xFF;
                destination = (op >> 8) & 0xFF;
                source = (op >> 16) & 0xFF;
                cycles = (op >> 24) & 0xFF;

                if (prefix != Prefix.None) //TODO - Wrong Wrong Wrong
                    cycles += 9;

                data = GetSource(source, opCode);

                switch (instruction)
                {
                    case OpInfo.InstrLD:
                        result = data;
                        break;
                    case OpInfo.InstrLDInterrupt:
                        result = data;
                        flagS = (result & 0x80) == 0x80;
                        intZ = result;
                        flagV = IFF2;
                        break;
                    case OpInfo.InstrEXDEHL:
                        temp = regDE;
                        regDE = regHL;
                        regHL = temp;
                        break;
                    case OpInfo.InstrEXAFAF:
                        temp = regAF;
                        regAF = shadowRegAF;
                        shadowRegAF = temp;
                        break;
                    case OpInfo.InstrEXX:
                        temp = regBC;
                        regBC = shadowRegBC;
                        shadowRegBC = temp;
                        temp = regDE;
                        regDE = shadowRegDE;
                        shadowRegDE = temp;
                        temp = regHL;
                        regHL = shadowRegHL;
                        shadowRegHL = temp;
                        break;
                    case OpInfo.InstrEXSP:
                        result = PopWordStack();
                        PushWordStack(data);
                        break;
                    case OpInfo.Instr8ADD:
                        result = regA + data;
                        flagN = false;
                        flagC = result > 0xFF;
                        flagV = (((data ^ regA ^ 0x80) & (data ^ result)) & 0x80) != 0; 
                        flagH = ((regA & 0xF) + (data & 0xF)) > 0xF;
                        flagS = (result & 0x80) == 0x80;
                        result = intZ = result & 0xFF;
                        break;
                    case OpInfo.Instr8ADC:
                        result = regA + data + (flagC ? 1 : 0);
                        flagN = false;
                        flagC = result > 0xFF;
                        flagV = result > 127;
                        flagH = ((regA & 0xF) + (data & 0xF) + (flagC ? 1 : 0)) > 0xF;
                        flagS = (result & 0x80) == 0x80;
                        result = intZ = result & 0xFF;
                        break;
                    case OpInfo.Instr8SUB:
                        result = regA - data;
                        flagN = true;
                        flagC = result < 0x00;
                        flagV = result < -128;
                        flagH = ((regA & 0xF) - (data & 0xF)) < 0x00;
                        flagS = (result & 0x80) == 0x80;
                        result = intZ = result & 0xFF;
                        break;
                    case OpInfo.Instr8SBC:
                        result = regA - data - (flagC ? 1 : 0);
                        flagN = true;
                        flagC = result < 0x00;
                        flagV = result < -128;
                        flagH = ((regA & 0xF) - (data & 0xF) - (flagC ? 1 : 0)) < 0x00;
                        flagS = (result & 0x80) == 0x80;
                        result = intZ = result & 0xFF;
                        break;
                    case OpInfo.InstrAND:
                        result = intZ = (regA & data) & 0xFF;
                        flagS = (result & 0x80) == 0x80;
                        flagH = true;
                        flagV = Parity8(result);
                        flagN = false;
                        flagC = false;
                        break;
                    case OpInfo.InstrOR:
                        result = intZ = (regA | data) & 0xFF;
                        flagS = (result & 0x80) == 0x80;
                        flagH = false;
                        flagV = Parity8(result);
                        flagN = false;
                        flagC = false;
                        break;
                    case OpInfo.InstrXOR:
                        result = intZ = (regA ^ data) & 0xFF;
                        flagS = (result & 0x80) == 0x80;
                        flagH = false;
                        flagV = Parity8(result);
                        flagN = false;
                        flagC = false;
                        break;
                    case OpInfo.InstrCP:
                        temp = regA - data;
                        flagN = true;
                        flagC = temp < 0x00;
                        flagV = temp < -128;
                        flagH = ((regA & 0xF) - (data & 0xF)) < 0x00;
                        flagS = (temp & 0x80) == 0x80;
                        intZ = temp & 0xFF;
                        break;
                    case OpInfo.Instr8INC:
                        result = data + 1;
                        flagN = false;
                        flagV = data == 0x7F;
                        flagH = (data & 0xF) == 0xF;
                        flagS = (result & 0x80) == 0x80;
                        result = intZ = result & 0xFF;
                        break;
                    case OpInfo.Instr8DEC:
                        result = data - 1;
                        flagN = true;
                        flagV = data == 0x80;
                        flagH = (data & 0xF) == 0x00;
                        flagS = (result & 0x80) == 0x80;
                        result = intZ = result & 0xFF;
                        break;
                    case OpInfo.InstrDAA:
                        temp = flagC ? 0x60 : 0x00;
                        if (flagH)
                            temp |= 0x06;
                        if (!flagN)
                        {
                            if ((regA & 0xF) > 0x9)
                                temp |= 0x06;
                            if (regA > 0x99)
                                temp |= 0x60;
                            result = regA + temp;
                        }
                        else
                        {
                            result = regA - temp;
                        }
                        flagC = (temp & 0x60) != 0;
                        flagH = false;
                        flagV = Parity8(result);
                        result = intZ = result & 0xFF;
                        break;
                    case OpInfo.InstrCPL:
                        result = (~regA) & 0xFF;
                        flagH = true;
                        flagN = true;
                        break;
                    case OpInfo.InstrCCF:
                        flagH = flagC;
                        flagN = false;
                        flagC = !flagC;
                        break;
                    case OpInfo.InstrSCF:
                        flagH = false;
                        flagN = false;
                        flagC = true;
                        break;
                    case OpInfo.InstrNOP:
                        break;
                    case OpInfo.InstrHALT:
                        regPC = (regPC - 1) & 0xFFFF;
                        break;
                    case OpInfo.InstrDI:
                        IFF1 = false;
                        IFF2 = false;
                        break;
                    case OpInfo.InstrEI:
                        IFF1 = true;
                        IFF2 = true;
                        break;
                    case OpInfo.InstrADD:
                        if (prefix == Prefix.DD)
                            temp = regIX;
                        else if (prefix == Prefix.FD)
                            temp = regIY;
                        else
                            temp = regHL;
                        result = (temp + data);
                        flagN = false;
                        flagH = ((temp ^ data ^ (result & 0xFFFF)) & 0x1000) != 0; //I don't really get how the H flag works here, this is from VBA source.
                        flagC = result > 0xFFFF;
                        result = result & 0xFFFF;
                        break;
                    case OpInfo.InstrINC:
                        result = (data + 1) & 0xFFFF;
                        break;
                    case OpInfo.InstrDEC:
                        result = (data - 1) & 0xFFFF;
                        break;
                    case OpInfo.InstrRLCA:
                        flagC = (regA & 0x80) == 0x80;
                        result = ((regA << 1) | (flagC ? 0x01 : 0x00)) & 0xFF;
                        flagH = false;
                        flagN = false;
                        break;
                    case OpInfo.InstrRLA:
                        temp = regA;
                        result = ((regA << 1) | (flagC ? 0x01 : 0x00)) & 0xFF;
                        flagC = (temp & 0x80) == 0x80;
                        flagH = false;
                        flagN = false;
                        break;
                    case OpInfo.InstrRRCA:
                        flagC = (regA & 0x01) == 0x01;
                        result = ((regA >> 1) | (flagC ? 0x80 : 0x00)) & 0xFF;
                        flagH = false;
                        flagN = false;
                        break;
                    case OpInfo.InstrRRA:
                        temp = regA;
                        result = ((regA >> 1) | (flagC ? 0x80 : 0x00)) & 0xFF;
                        flagC = (temp & 0x01) == 0x01;
                        flagH = false;
                        flagN = false;
                        break;
                    case OpInfo.InstrJP:
                        regPC = data;
                        break;
                    case OpInfo.InstrJPc:
                        {
                            bool takeJump = false;
                            switch ((opCode >> 3) & 7)
                            {
                                case 0:
                                    takeJump = !flagZ;
                                    break;
                                case 1:
                                    takeJump = flagZ;
                                    break;
                                case 2:
                                    takeJump = !flagC;
                                    break;
                                case 3:
                                    takeJump = flagC;
                                    break;
                                case 4:
                                    takeJump = !flagV;
                                    break;
                                case 5:
                                    takeJump = flagV;
                                    break;
                                case 6:
                                    takeJump = !flagS;
                                    break;
                                case 7:
                                    takeJump = flagS;
                                    break;
                            }
                            if (takeJump)
                                regPC = data;
                        }
                        break;
                    case OpInfo.InstrJR:
                        if ((data & 0x80) == 0x80)
                            data = (sbyte)(data & 0xFF);
                        regPC += data;
                        break;
                    case OpInfo.InstrJRC:
                        if(flagC)
                            goto case OpInfo.InstrJR;
                        break;
                    case OpInfo.InstrJRNC:
                        if (!flagC)
                            goto case OpInfo.InstrJR;
                        break;
                    case OpInfo.InstrJRZ:
                        if (flagZ)
                            goto case OpInfo.InstrJR;
                        break;
                    case OpInfo.InstrJRNZ:
                        if (!flagZ)
                            goto case OpInfo.InstrJR;
                        break;
                    case OpInfo.InstrDJNZ:
                        regB = (regB - 1) & 0xFF;
                        if (regB != 0)
                            goto case OpInfo.InstrJR;
                        break;
                    case OpInfo.InstrCALL:
                        PushWordStack(regPC);
                        regPC = data;
                        break;
                    case OpInfo.InstrCALLc:
                        {
                            bool takeCall = false;
                            switch ((opCode >> 3) & 7)
                            {
                                case 0:
                                    takeCall = !flagZ;
                                    break;
                                case 1:
                                    takeCall = flagZ;
                                    break;
                                case 2:
                                    takeCall = !flagC;
                                    break;
                                case 3:
                                    takeCall = flagC;
                                    break;
                                case 4:
                                    takeCall = !flagV;
                                    break;
                                case 5:
                                    takeCall = flagV;
                                    break;
                                case 6:
                                    takeCall = !flagS;
                                    break;
                                case 7:
                                    takeCall = flagS;
                                    break;
                            }
                            if (takeCall)
                                goto case OpInfo.InstrCALL;
                        }
                        break;
                    case OpInfo.InstrRET:
                        regPC = PopWordStack();
                        break;
                    case OpInfo.InstrRETc:
                        {
                            bool takeRet = false;
                            switch ((opCode >> 3) & 7)
                            {
                                case 0:
                                    takeRet = !flagZ;
                                    break;
                                case 1:
                                    takeRet = flagZ;
                                    break;
                                case 2:
                                    takeRet = !flagC;
                                    break;
                                case 3:
                                    takeRet = flagC;
                                    break;
                                case 4:
                                    takeRet = !flagV;
                                    break;
                                case 5:
                                    takeRet = flagV;
                                    break;
                                case 6:
                                    takeRet = !flagS;
                                    break;
                                case 7:
                                    takeRet = flagS;
                                    break;
                            }
                            if (takeRet)
                                goto case OpInfo.InstrRET;
                        }
                        break;
                    case OpInfo.InstrRST:
                        PushWordStack(regPC);
                        regPC = opCode & 0x38;
                        break;
                    case OpInfo.InstrINA:
                        regA = In(data);
                        break;
                    case OpInfo.InstrOUTA:
                        Out(regA, data);
                        break;
                    case OpInfo.PrefixCB:
                        var bitOp = Read();
                        cycles = 8; //TODO - WRONG WRONG WRONG
                        switch(bitOp & 0x7)
                        {
                            case 0:
                                source = OpInfo.LocRegB;
                                break;
                            case 1:
                                source = OpInfo.LocRegC;
                                break;
                            case 2:
                                source = OpInfo.LocRegD;
                                break;
                            case 3:
                                source = OpInfo.LocRegE;
                                break;
                            case 4:
                                source = OpInfo.LocRegH;
                                break;
                            case 5:
                                source = OpInfo.LocRegL;
                                break;
                            case 6:
                                source = OpInfo.LocAddrHL;
                                cycles += 7;
                                break;
                            case 7:
                                source = OpInfo.LocRegA;
                                break;
                        }
                        destination = source;
                        data = GetSource(source, bitOp);

                        switch ((bitOp & 0xF8) >> 3)
                        {
                            case 0:
                                instruction = OpInfo.InstrRLC;
                                break;
                            case 1:
                                instruction = OpInfo.InstrRRC;
                                break;
                            case 2:
                                instruction = OpInfo.InstrRL;
                                break;
                            case 3:
                                instruction = OpInfo.InstrRR;
                                break;
                            case 4:
                                instruction = OpInfo.InstrSLA;
                                break;
                            case 5:
                                instruction = OpInfo.InstrSRA;
                                break;
                            case 6:
                                instruction = OpInfo.InstrSLL;
                                break;
                            case 7:
                                instruction = OpInfo.InstrSRL;
                                break;
                            default:
                                switch ((bitOp & 0xC0) >> 6)
                                {
                                    case 1:
                                        instruction = OpInfo.InstrBIT;
                                        break;
                                    case 2:
                                        instruction = OpInfo.InstrRES;
                                        break;
                                    case 3:
                                        instruction = OpInfo.InstrSET;
                                        break;
                                }
                                break;
                        }

                        switch (instruction)
                        {
                            case OpInfo.InstrRLC:
                                flagC = (data & 0x80) == 0x80;
                                result = ((data << 1) | (flagC ? 0x01 : 0x00)) & 0xFF;
                                flagS = (result & 0x80) == 0x80;
                                intZ = result;
                                flagV = Parity8(result);
                                flagH = false;
                                flagN = false;
                                break;
                            case OpInfo.InstrRL:
                                temp = data;
                                result = ((data << 1) | (flagC ? 0x01 : 0x00)) & 0xFF;
                                flagC = (temp & 0x80) == 0x80;
                                flagS = (result & 0x80) == 0x80;
                                intZ = result;
                                flagV = Parity8(result);
                                flagH = false;
                                flagN = false;
                                break;
                            case OpInfo.InstrRRC:
                                flagC = (data & 0x01) == 0x01;
                                result = ((data >> 1) | (flagC ? 0x80 : 0x00)) & 0xFF;
                                flagS = (result & 0x80) == 0x80;
                                intZ = result;
                                flagV = Parity8(result);
                                flagH = false;
                                flagN = false;
                                break;
                            case OpInfo.InstrRR:
                                temp = data;
                                result = ((data >> 1) | (flagC ? 0x80 : 0x00)) & 0xFF;
                                flagC = (temp & 0x01) == 0x01;
                                flagS = (result & 0x80) == 0x80;
                                intZ = result;
                                flagV = Parity8(result);
                                flagH = false;
                                flagN = false;
                                break;
                            case OpInfo.InstrSLA:
                                result = (data << 1) & 0xFF;
                                flagS = (result & 0x80) == 0x80;
                                flagC = (data & 0x80) == 0x80;
                                intZ = result;
                                flagV = Parity8(result);
                                flagH = false;
                                flagN = false;
                                break;
                            case OpInfo.InstrSRA:
                                result = (data >> 1) | (data & 0x80);
                                flagS = (result & 0x80) == 0x80;
                                flagC = (data & 0x01) == 0x01;
                                intZ = result;
                                flagV = Parity8(result);
                                flagH = false;
                                flagN = false;
                                break;
                            case OpInfo.InstrSLL:
                                result = (data << 1) & 0xFF;
                                flagC = (data & 0x80) == 0x80;
                                flagS = false;
                                intZ = result;
                                flagV = Parity8(result);
                                flagH = false;
                                flagN = false;
                                break;
                            case OpInfo.InstrSRL:
                                result = data >> 1;
                                flagC = (data & 0x01) == 0x01;
                                flagS = false;
                                intZ = result;
                                flagV = Parity8(result);
                                flagH = false;
                                flagN = false;
                                break;
                            case OpInfo.InstrBIT:
                                switch ((bitOp >> 3) & 7)
                                {
                                    case 0:
                                        temp = 1;
                                        break;
                                    case 1:
                                        temp = 2;
                                        break;
                                    case 2:
                                        temp = 4;
                                        break;
                                    case 3:
                                        temp = 8;
                                        break;
                                    case 4:
                                        temp = 0x10;
                                        break;
                                    case 5:
                                        temp = 0x20;
                                        break;
                                    case 6:
                                        temp = 0x40;
                                        break;
                                    case 7:
                                        temp = 0x80;
                                        break;
                                }
                                flagZ = (temp & data) == 0;
                                flagH = true;
                                flagN = false;
                                destination = OpInfo.LocNone;
                                break;
                            case OpInfo.InstrRES:
                                switch ((bitOp >> 3) & 7)
                                {
                                    case 0:
                                        temp = 1;
                                        break;
                                    case 1:
                                        temp = 2;
                                        break;
                                    case 2:
                                        temp = 4;
                                        break;
                                    case 3:
                                        temp = 8;
                                        break;
                                    case 4:
                                        temp = 0x10;
                                        break;
                                    case 5:
                                        temp = 0x20;
                                        break;
                                    case 6:
                                        temp = 0x40;
                                        break;
                                    case 7:
                                        temp = 0x80;
                                        break;
                                }
                                result = (data & ~temp) & 0xFF;
                                break;
                            case OpInfo.InstrSET:
                                switch ((bitOp >> 3) & 7)
                                {
                                    case 0:
                                        temp = 1;
                                        break;
                                    case 1:
                                        temp = 2;
                                        break;
                                    case 2:
                                        temp = 4;
                                        break;
                                    case 3:
                                        temp = 8;
                                        break;
                                    case 4:
                                        temp = 0x10;
                                        break;
                                    case 5:
                                        temp = 0x20;
                                        break;
                                    case 6:
                                        temp = 0x40;
                                        break;
                                    case 7:
                                        temp = 0x80;
                                        break;
                                }
                                result = (data | temp) & 0xFF;
                                break;
                        }
                        break;
                    case OpInfo.PrefixED:
                        opCode = Read();
                        op = edOpTable[opCode];
                        instruction = op & 0xFF;
                        destination = (op >> 8) & 0xFF;
                        source = (op >> 16) & 0xFF;
                        cycles = (op >> 24) & 0xFF;

                        data = GetSource(source, opCode);

                        switch (instruction)
                        {
                            case OpInfo.InstrNOP:
                                break;
                            case OpInfo.InstrLD:
                                result = data;
                                break;
                            case OpInfo.InstrIN:
                                result = In(regC);
                                break;
                            case OpInfo.InstrINI:
                                Write(In(regC), regHL);
                                regB = (regB - 1) & 0xFF;
                                regHL = (regHL + 1) & 0xFFFF;
                                flagN = true;
                                flagZ = regB == 0;
                                break;
                            case OpInfo.InstrINIR:
                                Write(In(regC), regHL);
                                regB = (regB - 1) & 0xFF;
                                regHL = (regHL + 1) & 0xFFFF;
                                flagN = true;
                                flagZ = true;
                                if (regB != 0)
                                    regPC = (regPC - 2) & 0xFFFF;
                                break;
                            case OpInfo.InstrIND:
                                Write(In(regC), regHL);
                                regB = (regB - 1) & 0xFF;
                                regHL = (regHL - 1) & 0xFFFF;
                                flagN = true;
                                flagZ = (regB - 1) == 0;
                                break;
                            case OpInfo.InstrINDR:
                                Write(In(regC), regHL);
                                regB = (regB - 1) & 0xFF;
                                regHL = (regHL - 1) & 0xFFFF;
                                flagN = true;
                                flagZ = true;
                                if (regB != 0)
                                    regPC = (regPC - 2) & 0xFFFF;
                                break;
                            case OpInfo.InstrOUT:
                                Out(data, regC);
                                break;
                            case OpInfo.InstrOUTI:
                                Out(data, regC);
                                regB = (regB - 1) & 0xFF;
                                regHL = (regHL + 1) & 0xFFFF;
                                flagN = true;
                                flagZ = regB == 0;
                                break;
                            case OpInfo.InstrOUTIR:
                                Out(data, regC);
                                regB = (regB - 1) & 0xFF;
                                regHL = (regHL + 1) & 0xFFFF;
                                flagN = true;
                                flagZ = true;
                                if (regB != 0)
                                    regPC = (regPC - 2) & 0xFFFF;
                                break;
                            case OpInfo.InstrOUTD:
                                Out(data, regC);
                                regB = (regB - 1) & 0xFF;
                                regHL = (regHL - 1) & 0xFFFF;
                                flagN = true;
                                flagZ = (regB - 1) == 0;
                                break;
                            case OpInfo.InstrOUTDR:
                                Out(data, regC);
                                regB = (regB - 1) & 0xFF;
                                regHL = (regHL - 1) & 0xFFFF;
                                flagN = true;
                                flagZ = true;
                                if (regB != 0)
                                    regPC = (regPC - 2) & 0xFFFF;
                                break;
                            case OpInfo.InstrADC:
                                result = regHL + data + (flagC ? 1 : 0);
                                flagN = false;
                                flagH = ((regHL ^ data ^ (result & 0xFFFF) ^ (flagC ? 1 : 0)) & 0x1000) != 0; //I don't really get how the H flag works here, this is from VBA source.
                                flagC = result > 0xFFFF;
                                result = result & 0xFFFF;
                                break;
                            case OpInfo.InstrSBC:
                                result = regHL - data - (flagC ? 1 : 0);
                                flagN = false;
                                flagH = ((regHL ^ data ^ (result & 0xFFFF) ^ (flagC ? 1 : 0)) & 0x1000) != 0; //I don't really get how the H flag works here, this is from VBA source.
                                flagC = result < 0x00;
                                result = result & 0xFFFF;
                                break;
                            case OpInfo.InstrNEG:
                                result = intZ = ((~regA) + 1) & 0xFF;
                                flagS = (result & 0x80) == 0x80;
                                flagH = (regA & 0xF) == 0;
                                flagV = regA == 0x80;
                                flagN = true;
                                flagC = regA == 0;
                                break;
                            case OpInfo.InstrRETI:
                                regPC = PopWordStack();
                                break;
                            case OpInfo.InstrRETN:
                                IFF1 = IFF2;
                                regPC = PopWordStack();
                                break;
                            case OpInfo.InstrIM0:
                                IM = InterruptMode.IM0;
                                break;
                            case OpInfo.InstrIM1:
                                IM = InterruptMode.IM1;
                                break;
                            case OpInfo.InstrIM2:
                                IM = InterruptMode.IM2;
                                break;
                            case OpInfo.InstrRLD:
                                result = ((data << 4) | (regA & 0x0F)) & 0xFF;
                                regA = ((regA & 0xF0) | ((data >> 4) & 0x0F)) & 0xFF;
                                flagS = (regA & 0x80) == 0x80;
                                intZ = regA;
                                flagH = false;
                                flagV = Parity8(regA);
                                flagN = false;
                                break;
                            case OpInfo.InstrRRD:
                                result = (((data >> 4) & 0x0F) | ((regA << 4) & 0xF0)) & 0xFF;
                                regA = ((regA & 0xF0) | (data & 0x0F)) & 0xFF;
                                flagS = (regA & 0x80) == 0x80;
                                intZ = regA;
                                flagH = false;
                                flagV = Parity8(regA);
                                flagN = false;
                                break;
                            case OpInfo.InstrLDI:
                                Write(Read(regHL), Read(regDE));
                                regDE = (regDE + 1) & 0xFFFF;
                                regHL = (regHL + 1) & 0xFFFF;
                                regBC = (regBC - 1) & 0xFFFF;
                                flagH = false;
                                flagN = false;
                                flagV = (regBC - 1 != 0);
                                break;
                            case OpInfo.InstrLDIR:
                                Write(Read(regHL), Read(regDE));
                                regDE = (regDE + 1) & 0xFFFF;
                                regHL = (regHL + 1) & 0xFFFF;
                                regBC = (regBC - 1) & 0xFFFF;
                                flagH = false;
                                flagN = false;
                                flagV = false;
                                if (regBC != 0)
                                    regPC = (regPC - 2) & 0xFFFF;
                                break;
                            case OpInfo.InstrLDD:
                                Write(Read(regHL), Read(regDE));
                                regDE = (regDE - 1) & 0xFFFF;
                                regHL = (regHL - 1) & 0xFFFF;
                                regBC = (regBC - 1) & 0xFFFF;
                                flagH = false;
                                flagN = false;
                                flagV = (regBC - 1 != 0);
                                break;
                            case OpInfo.InstrLDDR:
                                Write(Read(regHL), Read(regDE));
                                regDE = (regDE - 1) & 0xFFFF;
                                regHL = (regHL - 1) & 0xFFFF;
                                regBC = (regBC - 1) & 0xFFFF;
                                flagH = false;
                                flagN = false;
                                flagV = false;
                                if (regBC != 0)
                                    regPC = (regPC - 2) & 0xFFFF;
                                break;
                            case OpInfo.InstrCPI:
                                temp = Read(regHL);
                                regHL = (regHL + 1) & 0xFFFF;
                                regBC = (regBC - 1) & 0xFFFF;
                                flagN = true;
                                flagV = (regBC - 1 != 0);
                                flagH = ((regA & 0xF) - (temp & 0xF)) < 0x00;
                                temp = regA - temp;
                                flagS = (temp & 0x80) == 0x80;
                                intZ = temp & 0xFF;
                                break;
                            case OpInfo.InstrCPIR:
                                temp = Read(regHL);
                                regHL = (regHL + 1) & 0xFFFF;
                                regBC = (regBC - 1) & 0xFFFF;
                                flagN = true;
                                flagV = (regBC - 1 != 0);
                                flagH = ((regA & 0xF) - (temp & 0xF)) < 0x00;
                                temp = regA - temp;
                                flagS = (temp & 0x80) == 0x80;
                                intZ = temp & 0xFF;
                                if (regBC != 0)
                                    regPC = (regPC - 2) & 0xFFFF;
                                break;
                            case OpInfo.InstrCPD:
                                temp = Read(regHL);
                                regHL = (regHL - 1) & 0xFFFF;
                                regBC = (regBC - 1) & 0xFFFF;
                                flagN = true;
                                flagV = (regBC - 1 != 0);
                                flagH = ((regA & 0xF) - (temp & 0xF)) < 0x00;
                                temp = regA - temp;
                                flagS = (temp & 0x80) == 0x80;
                                intZ = temp & 0xFF;
                                break;
                            case OpInfo.InstrCPDR:
                                temp = Read(regHL);
                                regHL = (regHL - 1) & 0xFFFF;
                                regBC = (regBC - 1) & 0xFFFF;
                                flagN = true;
                                flagV = (regBC - 1 != 0);
                                flagH = ((regA & 0xF) - (temp & 0xF)) < 0x00;
                                temp = regA - temp;
                                flagS = (temp & 0x80) == 0x80;
                                intZ = temp & 0xFF;
                                if (regBC != 0)
                                    regPC = (regPC - 2) & 0xFFFF;
                                break;
                        }

                        break;
                    case OpInfo.PrefixDD:
                        regR = (regR + 1) & 0xFF;
                        prefix = Prefix.DD;
                        continue;
                    case OpInfo.PrefixFD:
                        regR = (regR + 1) & 0xFF;
                        prefix = Prefix.FD;
                        continue;

                }
                switch (destination)
                {
                    case OpInfo.LocNone:
                        break;
                    case OpInfo.LocRegA:
                        regA = result;
                        break;
                    case OpInfo.LocRegB:
                        regB = result;
                        break;
                    case OpInfo.LocRegC:
                        regC = result;
                        break;
                    case OpInfo.LocRegD:
                        regD = result;
                        break;
                    case OpInfo.LocRegE:
                        regE = result;
                        break;
                    case OpInfo.LocRegF:
                        regF = result;
                        break;
                    case OpInfo.LocRegH:
                        if (prefix == Prefix.DD && opCode != 0x66)
                            regIXh = result;
                        else if (prefix == Prefix.FD && opCode != 0x66)
                            regIYh = result;
                        else 
                            regH = result;
                        break;
                    case OpInfo.LocRegL:
                        if (prefix == Prefix.DD && opCode != 0x6E)
                            regIXl = result;
                        else if (prefix == Prefix.FD && opCode != 0x6E)
                            regIYl = result;
                        else
                            regL = result;
                        break;
                    case OpInfo.LocAddrAF:
                        Write(result, regAF);
                        break;
                    case OpInfo.LocAddrBC:
                        Write(result, regBC);
                        break;
                    case OpInfo.LocAddrDE:
                        Write(result, regDE);
                        break;
                    case OpInfo.LocAddrHL:
                        if (prefix == Prefix.DD)
                            Write(result, regIX + (sbyte)Read());
                        else if (prefix == Prefix.FD)
                            Write(result, regIY + (sbyte)Read());
                        else
                            Write(result, regHL);
                        break;
                    case OpInfo.LocRegAF:
                        regAF = result;
                        break;
                    case OpInfo.LocRegBC:
                        regBC = result;
                        break;
                    case OpInfo.LocRegDE:
                        regDE = result;
                        break;
                    case OpInfo.LocRegHL:
                        if (prefix == Prefix.DD)
                            regIX = result;
                        else if (prefix == Prefix.FD)
                            regIY = result;
                        else
                            regHL = result;
                        break;
                    case OpInfo.LocRegSP:
                        regSP = result;
                        break;
                    case OpInfo.LocAddrAbsoulute:
                        Write(result, ReadWord());
                        break;
                    case OpInfo.Loc8Stack:
                        PushStack(result);
                        break;
                    case OpInfo.Loc16Stack:
                        PushWordStack(result);
                        break;
                    case OpInfo.Loc16AddrAbsoulute:
                        WriteWord(result, ReadWord());
                        break;
                    case OpInfo.LocRegI:
                        regI = result;
                        break;
                    case OpInfo.LocRegR:
                        regR = result;
                        break;
                }
                regR = (regR + 1) & 0xFF;
                ppu.Clock(cycles);
                if (ppu.frameComplete)
                {
                    emulationRunning = false;
                    ppu.frameComplete = false;
                }
                if(ppu.nmi)
                {
                    PushWordStack(regPC);
                    regPC = 0x0066;
                }
                if(IFF1 && false)
                {
                    switch(IM)
                    {
                        case InterruptMode.IM1:
                            IFF1 = false;
                            PushWordStack(regPC);
                            regPC = 0x0038;
                            break;
                        case InterruptMode.IM2:
                            IFF1 = false;
                            PushWordStack(regPC);
                            regPC = ReadWord((regI << 8) | (interruptDataByte & 0xFE));
                        break;
                    }
                }

                if (ppu.frame == 700)
                    input.keys[1] = true;
                debug.AddCycles(cycles);
                if (ppu.frame == 705)
                    input.keys[1] = false;
                counter += cycles;
                prefix = Prefix.None;
            }
        }

        private byte Read(int address)
        {
            debug.Read(address);
            return memory[address & 0xFFFF];
        }
        private byte Read()
        {
            var data = Read(regPC);
            regPC = (regPC + 1) & 0xFFFF;
            return data;
        }
        private ushort ReadWord()
        {
            return (ushort)(Read() | (Read() << 8));
        }
        private ushort ReadWord(int address)
        {
            return (ushort)(Read(address) | (Read((address + 1) & 0xFFFF) << 8));
        }
        private void PushStack(int value)
        {
            regSP = (regSP - 1) & 0xFFFF;
            Write(value, regSP);
        }
        private void PushWordStack(int value)
        {
            PushStack(value >> 8);
            PushStack(value);
        }
        private byte PopStack()
        {
            byte value = Read(regSP);
            regSP = (regSP + 1) & 0xFFFF;
            return value;
        }
        private ushort PopWordStack()
        {
            return (ushort)(PopStack() | (PopStack() << 8));
        }
        private void Write(int value, int address)
        {
            debug.Write(address);
            /*
            if (address == 0x73fe)
                debugBuilder.AppendLine("WRITE: " + value.ToString("x2"));
            if (address == 0x73ff)
                debugBuilder.AppendLine("WRITE: " + value.ToString("x2"));
            */
            if(address >= 0x2000 && address < 0x8000)
                memory[address & 0xFFFF] = (byte)(value & 0xFF);
        }

        private void WriteWord(int value, int address)
        {
            Write(value, address);
            Write(value >> 8, address + 1);
        }

        private void Out(int value, int address)
        {
            if((address & 0xE0) == 0xA0)
            {
                ppu.Write((byte)value, address);
            }
            input.Write((byte)value, address);
        }
        private byte In(int address)
        {
            if ((address & 0xE0) == 0xA0)
            {
                return ppu.Read(address);
            }

            return input.Read(address);
        }
        private static bool Parity8(int reg)
        {
            reg &= 0xFF;
            reg ^= reg >> 4;
            reg &= 0xF;
            return ((0x6996 >> reg) & 1) != 1;
        }
        private static bool Parity16(int reg)
        {
            reg &= 0xFFFF;
            reg ^= reg >> 8;
            reg ^= reg >> 4;
            reg &= 0xF;
            return ((0x6996 >> reg) & 1) != 1;
        }
        private int GetSource(int source, int opcode)
        {
            switch (source)
            {
                case OpInfo.LocNone:
                    return 0;
                case OpInfo.LocRegA:
                    return regA;
                case OpInfo.LocRegB:
                    return regB;
                case OpInfo.LocRegC:
                    return regC;
                case OpInfo.LocRegD:
                    return regD;
                case OpInfo.LocRegE:
                    return regE;
                case OpInfo.LocRegF:
                    return regF;
                case OpInfo.LocRegH:
                    if (prefix == Prefix.DD && opcode != 0x74)
                        return regIXh;
                    if (prefix == Prefix.FD && opcode != 0x74)
                        return regIYh;
                    return regH;
                case OpInfo.LocRegL:
                    if (prefix == Prefix.DD && opcode != 0x75)
                        return regIXl;
                    if (prefix == Prefix.FD && opcode != 0x75)
                        return regIYl;
                    return regL;
                case OpInfo.Loc8Immediate:
                    return Read();
                case OpInfo.LocAddrAF:
                    return Read(regAF);
                case OpInfo.LocAddrBC:
                    return Read(regBC);
                case OpInfo.LocAddrDE:
                    return Read(regDE);
                case OpInfo.LocAddrHL:
                    if (prefix == Prefix.DD)
                        return Read(regIX + (sbyte)Read());
                    if (prefix == Prefix.FD)
                        return Read(regIY + (sbyte)Read());
                    return Read(regHL);
                case OpInfo.LocRegAF:
                    return regAF;
                case OpInfo.LocRegBC:
                    return regBC;
                case OpInfo.LocRegDE:
                    return regDE;
                case OpInfo.LocRegHL:
                    if (prefix == Prefix.DD)
                        return regIX;
                    if (prefix == Prefix.FD)
                        return regIY;
                    return regHL;
                case OpInfo.LocRegSP:
                    return regSP;
                case OpInfo.LocAddrAbsoulute:
                    return Read(ReadWord());
                case OpInfo.Loc16Immediate:
                    return ReadWord();
                case OpInfo.Loc8Stack:
                    return PopStack();
                case OpInfo.Loc16Stack:
                    return PopWordStack();
                case OpInfo.Loc16AddrAbsoulute:
                    return ReadWord(ReadWord());
                case OpInfo.LocRegI:
                    return regI;
                case OpInfo.LocRegR:
                    return regR;
            }
            return 0;
        }
    }
}
