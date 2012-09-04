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
        public uint[] screen = new uint[(256 + 15 + 13) * (192 + 24 + 27)];


        uint[] colorChart = new uint[16];

        private int scanline;
        private long counter;
        private bool writeLatch;
        private byte writeData;
        private ushort writeAddress;
        private ushort readAddress;
        public bool frameComplete;
        private bool fifthSpriteFlag;
        private byte fifthSpriteNumber;
        private bool coincidenceFlag;

        public PPU()
        {
            colorChart[0] = 0xFF000000;
            colorChart[1] = 0xFF000000;
            colorChart[2] = 0xFF3FB849;
            colorChart[3] = 0xFF75D07D;
            colorChart[4] = 0xFF5A55E0;
            colorChart[5] = 0xFF8076F1;
            colorChart[6] = 0xFFB95E52;
            colorChart[7] = 0xFF65DBEF;
            colorChart[8] = 0xFFDB6559;
            colorChart[9] = 0xFFFF897D;
            colorChart[10] = 0xFFCCC35E;
            colorChart[11] = 0xFFDED087;
            colorChart[12] = 0xFF3BA242;
            colorChart[13] = 0xFFB666B5;
            colorChart[14] = 0xFFCCCCCC;
            colorChart[15] = 0xFFFFFFFF;
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
                return (registers[2] & 0xF) * 0x400; }
        }

        private int baseColorTable
        {
            get
            {
                return registers[3] * 0x40; }
        }

        private int basePatternGen
        {
            get
            {
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
                return registers[7] & 0xF; }
        }

        private int text1Color
        {
            get
            {
                return registers[7] >> 4; }
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
            if (coincidenceFlag)
                data |= 0x40;
            if (fifthSpriteFlag)
            {
                data |= 0x20;
                data |= fifthSpriteNumber;
            }
            else
            {
                data |= 0x1f;
            }
            nmi = false;
            inVblank = false;
            writeLatch = false;
            coincidenceFlag = false;
            fifthSpriteFlag = false;
            return data;
        }

        public void Write(byte value, int address)
        {
            if((address & 1) == 0)
            {
                memory[writeAddress++ & 0x3FFF] = value;
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


        private unsafe void DumpScreen(string fileName)
        {
            File.WriteAllBytes(fileName + ".bin", memory);
            Bitmap imscreen = new Bitmap(284, 243, PixelFormat.Format32bppRgb);
            var bmd = imscreen.LockBits(new Rectangle(0, 0, 284, 243), ImageLockMode.WriteOnly,
                            PixelFormat.Format32bppRgb);
            var ptr = (uint*)bmd.Scan0;
            for (int i = 0; i < 284 * 243; i++)
                ptr[i] = screen[i];
            imscreen.UnlockBits(bmd);

            imscreen.Save(fileName + ".png");
            
        }

        public int frame = 0;
        public void Clock(int cycles)
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
                    int borderLeft = 13;
                    int borderRight = 15;
                    int screenScanline = scanline - 27;
                    switch(mode)
                    {
                        case Mode.Text:
                            {
                                borderLeft = 19;
                                borderRight = 25;

                                int row = screenScanline >> 3;
                                for (int tile = 0; tile < 40; tile++)
                                {
                                    int tileNumber = memory[baseNameTable | ((40*row) + tile)];
                                    int tileRowAddress = basePatternGen | (tileNumber << 3) | (screenScanline & 7);
                                    int tileRow = memory[tileRowAddress];

                                    uint color;
                                    for (int pixel = 0; pixel < 6; pixel++)
                                    {
                                        if ((tileRow & 0x80) != 0)
                                            color = colorChart[text1Color];
                                        else
                                            color = colorChart[text0Color];
                                        screen[(scanline * 284) + (tile * 6) + pixel + borderLeft] = color;
                                        tileRow <<= 1;
                                    }

                                }
                            }
                            break;
                        case Mode.Graphics1:
                            {
                                borderLeft = 13;
                                borderRight = 15;

                                int row = screenScanline >> 3;
                                int nameTable = baseNameTable | (row << 5);
                                for (int tile = 0; tile < 32; tile++)
                                {
                                    int tileAddress = nameTable | tile;
                                    int tileNumber = memory[tileAddress];
                                    int tileRowAddress = basePatternGen | (tileNumber << 3) | (screenScanline & 7);
                                    int tileRow = memory[tileRowAddress];
                                    int colorAddress = baseColorTable | tileNumber >> 3;
                                    int tileColors = memory[colorAddress];

                                    uint color;
                                    for (int pixel = 0; pixel < 8; pixel++)
                                    {
                                        if ((tileRow & 0x80) != 0)
                                            color = colorChart[tileColors >> 4];
                                        else
                                            color = colorChart[tileColors & 0xF];
                                        screen[(scanline*284) + (tile*8) + pixel + borderLeft] = color;
                                        tileRow <<= 1;
                                    }

                                }
                            }
                            break;
                            case Mode.Graphics2:
                            {
                                borderLeft = 13;
                                borderRight = 15;

                                int row = screenScanline >> 3;
                                int segment = (screenScanline << 5) & 0x1800;
                                int nameTable = baseNameTable | (row << 5);
                                for (int tile = 0; tile < 32; tile++)
                                {
                                    int tileAddress = nameTable | tile;
                                    int tileNumber = memory[tileAddress];
                                    int tileRowAddress = (basePatternGen & 0x2000) | segment | (tileNumber << 3) | (screenScanline & 7);
                                    int tileRow = memory[tileRowAddress];
                                    int colorAddress = (baseColorTable & 0x2000) | segment | (tileNumber << 3) | (screenScanline & 7);
                                    int tileColors = memory[colorAddress];

                                    uint color;
                                    for (int pixel = 0; pixel < 8; pixel++)
                                    {
                                        if ((tileRow & 0x80) != 0)
                                            color = colorChart[tileColors >> 4];
                                        else
                                            color = colorChart[tileColors & 0xF];
                                        screen[(scanline * 284) + (tile * 8) + pixel + borderLeft] = color;
                                        tileRow <<= 1;
                                    }

                                }
                                
                            }
                            break;
                            case Mode.MultiColor:
                            {
                                borderLeft = 13;
                                borderRight = 15;
                            }
                            break;
                    }
                    for (int pixel = 0; pixel < borderLeft; pixel++)
                        screen[(scanline * 284) + pixel] = colorChart[text0Color];

                    for (int pixel = 284 - borderRight; pixel < 284; pixel++)
                        screen[(scanline * 284) + pixel] = colorChart[text0Color];

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
                    if(frame == 600)
                        DumpScreen("mode2.png");
                    frameComplete = true;
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
