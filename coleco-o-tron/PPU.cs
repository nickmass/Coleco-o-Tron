using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace coleco_o_tron
{
    class PPU
    {
        private enum Mode
        {
            Graphics1,
            Graphics2,
            MultiColor,
            Text,
            Illegal
        }

        public bool nmi;
        public bool inVblank;

        byte[] memory = new byte[0x4000];
        byte[] registers = new byte[8];
        uint[] screen = new uint[(256 + 15 + 13) * (192 + 24 + 27)];


        uint[] colorChart = new uint[16];

        private int scanline;
        private long counter;
        private bool writeLatch;
        private byte writeData;
        private ushort writeAddress;
        private ushort readAddress;

        public PPU()
        {
            colorChart[0] = (uint)Color.Black.ToArgb();
            colorChart[1] = (uint)Color.Black.ToArgb();
            colorChart[2] = (uint)Color.Green.ToArgb();
            colorChart[3] = (uint)Color.LightGreen.ToArgb();
            colorChart[4] = (uint)Color.DarkBlue.ToArgb();
            colorChart[5] = (uint)Color.LightBlue.ToArgb();
            colorChart[6] = (uint)Color.DarkRed.ToArgb();
            colorChart[7] = (uint)Color.Cyan.ToArgb();
            colorChart[8] = (uint)Color.Red.ToArgb();
            colorChart[9] = (uint)Color.LightCoral.ToArgb();
            colorChart[10] = (uint)Color.DarkGoldenrod.ToArgb();
            colorChart[11] = (uint)Color.LightYellow.ToArgb();
            colorChart[12] = (uint)Color.DarkGreen.ToArgb();
            colorChart[13] = (uint)Color.Magenta.ToArgb();
            colorChart[14] = (uint)Color.Gray.ToArgb();
            colorChart[15] = (uint)Color.White.ToArgb();
        }

        private bool ev
        {
            get { return (registers[0] & 1) == 1; }
        }

        private Mode mode
        {
            get
            {
                var m1 = (registers[1] & 0x10) != 0;
                var m2 = (registers[1] & 0x08) != 0;
                var m3 = (registers[0] & 0x02) != 0;

                if(!m1 && !m2 && !m3)
                    return Mode.Graphics1;
                if(m3 && !m1 && !m2)
                    return Mode.Graphics2;
                if(m2 && !m1 && !m3)
                    return Mode.MultiColor;
                if(m1 && !m2 && !m3)
                    return Mode.Text;
                return Mode.Illegal;
            }
        }

        private bool magnification
        {
            get { return (registers[1] & 1) == 1; }
        }

        private bool largeSprites
        {
            get { return (registers[1] & 2) == 2; }
        }

        private bool interruptEnabled
        {
            get { return (registers[1] & 0x20) == 0x20; }
        }

        private bool blank
        {
            get { return (registers[1] & 0x40) == 0x0; }
        }

        private bool largeRam
        {
            get { return (registers[1] & 0x80) == 0x80; }
        }

        private int baseNameTable
        {
            get
            {
                return 0x1400;
                return (registers[2] & 0xF) * 0x400; }
        }

        private int baseColorTable
        {
            get
            {
                return 0x1800;
                return registers[3] * 0x40; }
        }

        private int basePatternGen
        {
            get
            {
                return 0x1800;
                return (registers[4] & 0x7) * 0x800; }
        }

        private int baseSpriteAttrTable
        {
            get { return (registers[5] & 0x7F) * 0x80; }
        }

        private int baseSpriteGen
        {
            get { return (registers[6] & 0x7) * 0x800; }
        }

        private int text0Color
        {
            get
            {
                return 13; return registers[7] & 0xF; }
        }

        private int text1Color
        {
            get { return registers[7] >> 4; }
        }

        public byte Read(int address)
        {
            if((address & 1) == 0)
            {
                return memory[readAddress++];
            }
            byte data = 0;
            if (inVblank)
                data |= 0x80;
            nmi = false;
            inVblank = false;
            writeLatch = false;
            return (byte)(data | 0x1f);
        }

        public void Write(byte value, int address)
        {
            if((address & 1) == 0)
            {
                memory[writeAddress++] = value;
                return;
            }
            if(!writeLatch)
            {
                writeData = value;
            }
            else
            {
                switch((value & 0xC0))
                {
                    case 0x80:
                        registers[value & 0x07] = writeData;
                        break;
                    case 0x40:
                        writeAddress = (ushort)(writeData | ((value & 0x3f) << 8));
                        break;
                    case 0x00:
                        readAddress = (ushort)(writeData | (value << 8));
                        break;
                }
            }
            writeLatch = !writeLatch;
        }

        private int frame = 0;
        public unsafe void Clock(int cycles)
        {
            counter += cycles;
            if(counter > 342)
            {
                if(scanline < 27)//TopBorder
                {
                    for (int i = 0; i < 284; i++)
                        screen[(scanline * 284) + i] = colorChart[text0Color];
                }
                else if(scanline < 219)//MainScreen
                {
                    int screenScanline = scanline - 27;
                    switch(mode)
                    {
                        case Mode.Graphics1:
                            for (int pixel = 0; pixel < 13; pixel++)
                                screen[(scanline * 284) + pixel] = colorChart[text0Color];

                            int row = screenScanline / 8;
                            int nameTable = baseNameTable | (row << 5);
                            for (int tile = 0; tile < 32; tile++)
                            {
                                int tileAddress = nameTable | tile;
                                int tileNumber = memory[tileAddress];
                                int tileRowAddress = basePatternGen | (tileNumber << 3) | (screenScanline % 8);
                                int tileRow = memory[tileRowAddress];
                                int colorAddress = baseColorTable | tileNumber >> 3;
                                int tileColors = memory[colorAddress];

                                //tileRow = 0x55;

                                uint color;
                                for(int pixel = 0; pixel < 8; pixel++)
                                {
                                    if ((tileRow & 0x80) != 0)
                                        color = colorChart[tileColors >> 4];
                                    else
                                        color = colorChart[tileColors & 0xF];
                                    screen[(scanline * 284) + (tile * 8) + pixel + 13] = color;
                                    tileRow <<= 1;
                                }

                            }
                            for (int pixel = 0; pixel < 15; pixel++)
                                screen[(scanline * 284) + pixel + 256 + 13] = colorChart[text0Color];

                                break;
                    }
                }
                else if(scanline < 243)//BottomBorder
                {
                    for (int i = 0; i < 284; i++)
                        screen[(scanline * 284) + i] = colorChart[text0Color];
                }


                scanline++;
                if (scanline == 219)
                {
                    frame++;
                    if (frame ==120)
                    {
                        File.WriteAllBytes("vramdump.bin", memory);
                        Bitmap imscreen = new Bitmap(284,243, PixelFormat.Format32bppRgb);
                        var bmd = imscreen.LockBits(new Rectangle(0, 0, 284, 243), ImageLockMode.WriteOnly,
                                        PixelFormat.Format32bppRgb);
                        var ptr = (uint*) bmd.Scan0;
                        for (int i = 0; i < 284 * 243; i++)
                            ptr[i] = screen[i];
                        imscreen.UnlockBits(bmd);

                        imscreen.Save("screen.png");

                    }
                    inVblank = true;
                    if (interruptEnabled)
                        nmi = true;
                }
                else if (scanline == 262)
                {
                    scanline = 0;
                }

                counter -= 342;
            }
        }
    }
}
