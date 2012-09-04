using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace coleco_o_tron
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ThreadPool.QueueUserWorkItem(DebugParse);
        }

        private void DebugParse(object blah)
        {
            var fs = File.OpenRead("goodlog.bin");
            BinaryReader bs = new BinaryReader(fs);
            StringBuilder sb = new StringBuilder();
            while(fs.Position < fs.Length - 20)
            {
                var regPC = (GetByte(bs) << 8) | GetByte(bs);
                var regAF = (GetByte(bs) << 8) | GetByte(bs);
                var regBC = (GetByte(bs) << 8) | GetByte(bs);
                var regDE = (GetByte(bs) << 8) | GetByte(bs);
                var regHL = (GetByte(bs) << 8) | GetByte(bs);
                var regIX = (GetByte(bs) << 8) | GetByte(bs);
                var regIY = (GetByte(bs) << 8) | GetByte(bs);

                    sb.AppendFormat(
                        "{0:x4} AF:{1:x4} BC:{2:x4} DE:{3:x4} HL:{4:x4} IX:{5:x4} IY:{6:x4}\r\n", regPC, regAF, regBC, regDE, regHL, regIX, regIY);
            }
            fs.Close();
            File.WriteAllText("parsedGoodLog.txt", sb.ToString());
        }

        private byte lastByte = 0;
        private byte GetByte(BinaryReader bs)
        {
            var nextByte = bs.ReadByte();
            return nextByte;
        }

        private ColecoCore core;

        private bool closing = false;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            closing = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            core.debug.AddReadBreakPoint(int.Parse(textBox1.Text,NumberStyles.AllowHexSpecifier));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            core.debug.AddWriteBreakPoint(int.Parse(textBox1.Text, NumberStyles.AllowHexSpecifier));

        }

        private void button3_Click(object sender, EventArgs e)
        {

            core.debug.AddExecuteBreakPoint(int.Parse(textBox1.Text, NumberStyles.AllowHexSpecifier));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int address = int.Parse(textBox1.Text, NumberStyles.AllowHexSpecifier);
            core.debug.RemoveExecuteBreakPoint(address);
            core.debug.RemoveReadBreakPoint(address);
            core.debug.RemoveWriteBreakPoint(address);
        }

        private unsafe void Form1_Shown(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            core = new ColecoCore(File.ReadAllBytes(ofd.FileName));
            var gfx = this.CreateGraphics();
            Bitmap imscreen = new Bitmap(284, 243, PixelFormat.Format32bppRgb);
            while (!closing)
            {
                core.Run();
                var bmd = imscreen.LockBits(new Rectangle(0, 0, 284, 243), ImageLockMode.WriteOnly,
                                PixelFormat.Format32bppRgb);
                var ptr = (uint*)bmd.Scan0;
                for (int i = 0; i < 284 * 243; i++)
                    ptr[i] = core.ppu.screen[i];
                imscreen.UnlockBits(bmd);
                gfx.DrawImageUnscaled(imscreen, 0, 0);
                textBox2.Text = core.debug.DebugStatus();
                Thread.Sleep(12);
                Application.DoEvents();
            }


        }

        private void button6_Click(object sender, EventArgs e)
        {
            core.debug.Resume();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            core.debug.Step();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            core.debug.Reset();
        }
    }
}
