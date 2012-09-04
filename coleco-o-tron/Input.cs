using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coleco_o_tron
{
    class Input
    {
        public enum Mode
        {
            Keys,
            Joystick
        }

        private Mode readMode;

        public bool[] keys = new bool[18];

        public void Write(byte value, int address)
        {
            if(address == 0x80)
            {
                readMode = Mode.Keys;
            }
            else if(address == 0xC0)
            {
                readMode = Mode.Joystick;
            }
        }

        public byte Read(int address)
        {
            if(address != 0xFC)
                return 0;
            byte data = 0;
            switch (readMode)
            {
                    case Mode.Keys:
                    if (keys[0])
                        return (byte)(data | 0xA);
                    if (keys[1])
                        return (byte)(data | 0xD);
                    if (keys[2])
                        return (byte)(data | 0x7);
                    if (keys[3])
                        return (byte)(data | 0xC);
                    if (keys[4])
                        return (byte)(data | 0x2);
                    if (keys[5])
                        return (byte)(data | 0x3);
                    if (keys[6])
                        return (byte)(data | 0xE);
                    if (keys[7])
                        return (byte)(data | 0x5);
                    if (keys[8])
                        return (byte)(data | 0x1);
                    if (keys[9])
                        return (byte)(data | 0xB);
                    return (byte)(data | 0xF);
                    break;
                    case Mode.Joystick:
                    return 0xF;
                    break;
            }
            return 0xF;
        }
    }
}
