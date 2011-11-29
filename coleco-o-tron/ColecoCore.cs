using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coleco_o_tron
{
    class ColecoCore
    {

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
                    case OpInfo.Instr8ADD:
                        temp = regA + data;
                        
                        flagN = false;
                        flagC = temp > 0xFF;
                        flagH = ((regA & 0xF) + (data & 0xF)) > 0xF;
                        return intZ = reg3 & 0xFF;
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
    }
}
