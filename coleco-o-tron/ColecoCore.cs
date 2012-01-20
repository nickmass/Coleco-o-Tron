using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coleco_o_tron
{
    class ColecoCore
    {
        enum InterruptMode
        {
            IM0,
            IM1,
            IM2
        }
        private int regA;
        private int regB;
        private int regC;
        private int regD;
        private int regE;
        private int regH;
        private int regL;
        private int regSP;
        private int regPC;
        private int intZ;
        private bool flagZ
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
        private bool flagN;
        private bool flagH;
        private bool flagC;
        private bool flagS;
        private bool flagV;
        private int regF
        {
            get
            {
                byte value = 0;
                if (flagS)
                    value |= 0x80;
                if (flagZ)
                    value |= 0x60;
                if (flagN)
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
        private int regAF
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
        private int regBC
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
        private int regDE
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
        private int regHL
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
        private int regI;
        private int regR;
        private int regIX;
        private int regIY;
        private int shadowRegAF;
        private int shadowRegBC;
        private int shadowRegDE;
        private int shadowRegHL;
        private bool halted;
        private bool IFF1;
        private bool IFF2;
        private InterruptMode IM;

        bool emulationRunning = false;
        byte[] memory = new byte[0x10000];
        int counter;
        public void Run()
        {
            int[] opTable = OpInfo.GetOps();
            int op, source, destination, instruction, cycles, result = 0, data = 0, temp = 0;
            while (emulationRunning)
            {
                op = opTable[Read()];
                instruction = op & 0xFF;
                destination = (op >> 8) & 0xFF;
                source = (op >> 16) & 0xFF;
                cycles = (op >> 24) & 0xFF;
                switch (source)
                {
                    case OpInfo.LocNone:
                        break;
                    case OpInfo.LocRegA:
                        data = regA;
                        break;
                    case OpInfo.LocRegB:
                        data = regB;
                        break;
                    case OpInfo.LocRegC:
                        data = regC;
                        break;
                    case OpInfo.LocRegD:
                        data = regD;
                        break;
                    case OpInfo.LocRegE:
                        data = regE;
                        break;
                    case OpInfo.LocRegF:
                        data = regF;
                        break;
                    case OpInfo.LocRegH:
                        data = regH;
                        break;
                    case OpInfo.LocRegL:
                        data = regL;
                        break;
                    case OpInfo.Loc8Immediate:
                        data = Read();
                        break;
                    case OpInfo.LocAddrAF:
                        data = Read(regAF);
                        break;
                    case OpInfo.LocAddrBC:
                        data = Read(regBC);
                        break;
                    case OpInfo.LocAddrDE:
                        data = Read(regDE);
                        break;
                    case OpInfo.LocAddrHL:
                        data = Read(regHL);
                        break;
                    case OpInfo.LocRegAF:
                        data = regAF;
                        break;
                    case OpInfo.LocRegBC:
                        data = regBC;
                        break;
                    case OpInfo.LocRegDE:
                        data = regDE;
                        break;
                    case OpInfo.LocRegHL:
                        data = regHL;
                        break;
                    case OpInfo.LocAddrAbsoulute:
                        data = Read(ReadWord());
                        break;
                    case OpInfo.Loc16Immediate:
                        data = ReadWord();
                        break;
                    case OpInfo.Loc8Stack:
                        data = PopStack();
                        break;
                    case OpInfo.Loc16Stack:
                        data = PopWordStack();
                        break;
                }
                switch (instruction)
                {
                    case OpInfo.InstrLD:
                        result = data;
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
                    case OpInfo.InstrEXSPHL:
                        temp = regHL;
                        regHL = PopWordStack();
                        PushWordStack(regHL);
                        break;
                        //Skipped some
                    case OpInfo.Instr8ADD:
                        result = regA + data;
                        flagN = false;
                        flagC = result > 0xFF;
                        flagV = result > 127;
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
                    case OpInfo.InstrNEG:
                        result = intZ = ((~regA) + 1) & 0xFF;
                        flagS = (result & 0x80) == 0x80;
                        flagH = (regA & 0xF) == 0;
                        flagV = regA == 0x80;
                        flagN = true;
                        flagC = regA == 0;
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
                        halted = true;
                        break;
                    case OpInfo.InstrDI:
                        IFF1 = false;
                        IFF2 = false;
                        break;
                    case OpInfo.InstrEI:
                        IFF1 = true;
                        IFF2 = true;
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
                    case OpInfo.InstrADD:
                        result = (regHL + data);
                        flagN = false;
                        flagH = ((regHL ^ data ^ (result & 0xFFFF)) & 0x1000) != 0; //I don't really get how the H flag works here, this is from VBA source.
                        flagC = result > 0xFFFF;
                        result = result & 0xFFFF;
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
                        result = data << 1;
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
                    case OpInfo.InstrSRL:
                        result = data >> 1;
                        flagC = (data & 0x01) == 0x01;
                        flagS = false;
                        flagC = (data & 0x01) == 0x01;
                        intZ = result;
                        flagV = Parity8(result);
                        flagH = false;
                        flagN = false;
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
                    case OpInfo.InstrBIT:
                        flagZ = (temp & data) == 0;
                        flagH = true;
                        flagN = false;
                        break;
                    case OpInfo.InstrSET:
                        result = (data | temp) & 0xFF;
                        break;
                    case OpInfo.InstrRES:
                        result = (data & ~temp) & 0xFF;
                        break;
                    case OpInfo.InstrJP:
                        regPC = data;
                        break;
                    case OpInfo.InstrJPc:
                        {
                            bool takeJump = false;
                            switch (temp)
                            {
                                case 0:
                                    takeJump = flagZ;
                                    break;
                                case 1:
                                    takeJump = !flagZ;
                                    break;
                                case 2:
                                    takeJump = flagC;
                                    break;
                                case 3:
                                    takeJump = !flagC;
                                    break;
                                case 4:
                                    takeJump = flagV;
                                    break;
                                case 5:
                                    takeJump = !flagV;
                                    break;
                                case 6:
                                    takeJump = flagS;
                                    break;
                                case 7:
                                    takeJump = !flagS;
                                    break;
                            }
                            if (takeJump)
                                regPC = data;
                        }
                        break;
                    case OpInfo.InstrJR:
                        if ((data & 0x80) == 0x80)
                            data = (data & 0xEF) * -1;
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
                            switch (temp)
                            {
                                case 0:
                                    takeCall = flagZ;
                                    break;
                                case 1:
                                    takeCall = !flagZ;
                                    break;
                                case 2:
                                    takeCall = flagC;
                                    break;
                                case 3:
                                    takeCall = !flagC;
                                    break;
                                case 4:
                                    takeCall = flagV;
                                    break;
                                case 5:
                                    takeCall = !flagV;
                                    break;
                                case 6:
                                    takeCall = flagS;
                                    break;
                                case 7:
                                    takeCall = !flagS;
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
                            switch (temp)
                            {
                                case 0:
                                    takeRet = flagZ;
                                    break;
                                case 1:
                                    takeRet = !flagZ;
                                    break;
                                case 2:
                                    takeRet = flagC;
                                    break;
                                case 3:
                                    takeRet = !flagC;
                                    break;
                                case 4:
                                    takeRet = flagV;
                                    break;
                                case 5:
                                    takeRet = !flagV;
                                    break;
                                case 6:
                                    takeRet = flagS;
                                    break;
                                case 7:
                                    takeRet = !flagS;
                                    break;
                            }
                            if (takeRet)
                                goto case OpInfo.InstrRET;
                        }
                        break;
                    case OpInfo.InstrRETI:
                        regPC = PopWordStack();
                        break;
                    case OpInfo.InstrRETN:
                        IFF1 = IFF2;
                        regPC = PopWordStack();
                        break;
                    case OpInfo.InstrRST:
                        PushWordStack(regPC);
                        regPC = data << 3;
                        break;


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
                        regH = result;
                        break;
                    case OpInfo.LocRegL:
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
                        regHL = result;
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
                }
                counter += cycles;
            }
        }

        private byte Read(int address)
        {
            return memory[address & 0xFFFF];
        }
        private byte Read()
        {
            regPC = (regPC + 1) & 0xFFFF;
            return Read(regPC);
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
            memory[address & 0xFF] = (byte)(value & 0xFF);
        }
        private static bool Parity8(int reg)
        {
            reg &= 0xFF;
            reg ^= reg >> 4;
            reg &= 0xF;
            return ((0x6996 >> reg) & 1) == 1;
        }
        private static bool Parity16(int reg)
        {
            reg &= 0xFFFF;
            reg ^= reg >> 8;
            reg ^= reg >> 4;
            reg &= 0xF;
            return ((0x6996 >> reg) & 1) == 1;
        }
    }
}
