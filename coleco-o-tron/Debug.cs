using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coleco_o_tron
{
    class Debug
    {
        private ColecoCore core;

        public const byte READ = 1;
        public const byte WRITE = 2;
        public const byte EXECUTE = 4;
        public const byte IN = 8;
        public const byte OUT = 16;

        public bool Interrupt;

        private int debugCounter;

        public byte[] breakpoints = new byte[0x10000];

        public Queue<string> Log = new Queue<string>(5); 

        public Debug(ColecoCore core)
        {
            this.core = core;
        }

        public void Step()
        {
            Interrupt = false;
            debugCounter = 1;
        }

        public void Resume()
        {
            Interrupt = false;
            debugCounter = 0;
        }

        public void AddCycles(int cycles)
        {
            if (debugCounter > 0)
            {
                debugCounter -= cycles;
                if (debugCounter <= 0)
                {
                    Interrupt = true;
                }
            }
        }

        public void Execute(int address)
        {
            if((breakpoints[address] & EXECUTE) != 0)
            {
                Interrupt = true;
                Log.Enqueue(string.Format("Execute at {0:x4}", address));
            }
        }

        public void Read(int address)
        {
            if ((breakpoints[address] & READ) != 0)
            {
                Interrupt = true;
                Log.Enqueue(string.Format("Read at {0:x4}", address));
            }
        }

        public void Write(int address)
        {
            if ((breakpoints[address] & WRITE) != 0)
            {
                Interrupt = true;
                Log.Enqueue(string.Format("Write at {0:x4}", address));
            }
        }

        public void AddExecuteBreakPoint(int address)
        {
            breakpoints[address] |= EXECUTE;
        }

        public void AddReadBreakPoint(int address)
        {
            breakpoints[address] |= READ;
        }

        public void AddWriteBreakPoint(int address)
        {
            breakpoints[address] |= WRITE;
        }

        public void RemoveExecuteBreakPoint(int address)
        {
            breakpoints[address] &= EXECUTE ^ 0xFF;
        }

        public void RemoveReadBreakPoint(int address)
        {
            breakpoints[address] &= READ ^ 0xFF;
        }

        public void RemoveWriteBreakPoint(int address)
        {
            breakpoints[address] &= WRITE ^ 0xFF;
        }

        public void Reset()
        {
            core.regPC = 0x0000;
            core.ppu.frame = 0;
        }

        public string DebugStatus()
        {
            while(Log.Count > 5)
                Log.Dequeue();


            string logs = "";

            foreach (var line in Log.Reverse())
            {
                logs += line + "\r\n";
            }

            string statusLine = "";

            if (core.flagS)
                statusLine += 'S';
            else
                statusLine += '.';

            if (core.flagZ)
                statusLine += 'Z';
            else
                statusLine += '.';

            statusLine += '.';

            if (core.flagH)
                statusLine += 'H';
            else
                statusLine += '.';

            statusLine += '.';

            if (core.flagV)
                statusLine += 'V';
            else
                statusLine += '.';

            if (core.flagN)
                statusLine += 'N';
            else
                statusLine += '.';

            if (core.flagC)
                statusLine += 'C';
            else
                statusLine += '.';

            return
                string.Format(
                    "{9}\r\n\r\nPC: {0:x4}\r\nAF: {1:x4}\r\nBC: {2:x4}\r\nDE: {3:x4}\r\nHL: {4:x4}\r\nIX: {5:x4}\r\nIY: {6:x4}\r\nSP: {7:x4}\r\n\r\n\r\n{8}",
                    core.regPC, core.regAF, core.regBC, core.regDE, core.regHL, core.regIX, core.regIY, core.regSP, logs, statusLine);
        }
    }
}
