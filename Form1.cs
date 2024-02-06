using Emgu.CV;
using Emgu.CV.Structure;
using ScreenCapturerNS;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO.Ports;
namespace AmbiLight
{
    public partial class Form1 : Form
    {
        SerialPort sp;
        const int cols = 10;
        const int rows = 5;
        Stopwatch sw = new();


        byte[]? top, right, bottom, left;

        public Form1()
        {
            InitializeComponent();
        }

        bool sleep = true;
        private void writeLeds(SerialPort sp, byte[] data, int offset = 0)
        {
            if (!sp.IsOpen) return;
            sp.WriteLine(String.Format("{0},{1}", offset, data.Length));
            sp.Write(data, 0, data.Length);
        }

        Color SimpleColorToColor(SimpleColor simple)
        {
            return Color.FromArgb(simple.R, simple.G, simple.B);
        }

        void OnScreenUpdated(Object? sender, OnScreenUpdatedEventArgs e)
        {
            if (e.Bitmap == null) return;
            sw.Start();

            //RETURN ORDER: top bottom left right
            //LED ORDER: bottom left top right
            List<SimpleColor>[] colors = processRegions(e.Bitmap);

            byte[] _bottom = ColorListToByteArr(colors[1], true); //1-10
            byte[] _left = ColorListToByteArr(colors[2], true); //11-15
            byte[] _top = ColorListToByteArr(colors[0]); //16-25
            byte[] _right = ColorListToByteArr(colors[3]); //26-30

            //if (bottom == _bottom && left == _left && top == _top && right == _right) return;
            //bottom = _bottom;
            //top = _top;
            //right = _right;
            //left = _left;

            byte[] firstBlock = new byte[45];
            byte[] secondBlock = new byte[45];
            Buffer.BlockCopy(_bottom, 0, firstBlock, 0, 30);
            Buffer.BlockCopy(_left, 0, firstBlock, 30, 15);

            Buffer.BlockCopy(_top, 0, secondBlock, 0, 30);
            Buffer.BlockCopy(_right, 0, secondBlock, 30, 15);

            Thread.Sleep(1);
            writeLeds(sp, firstBlock, 0);
            Thread.Sleep(2);
            writeLeds(sp, secondBlock, 45);


            //if (bottom != _bottom)
            //{
            //
            //    bottom = _bottom;
            //    writeLeds(sp, bottom, 0);
            //}
            //if (left != _left)
            //{
            //
            //    left = _left;
            //    writeLeds(sp, left, 10 * 3);
            //}
            //if (top != _top)
            //{
            //    top = _top;
            //    writeLeds(sp, top, 15 * 3);
            //}
            //if (right != _right)
            //{
            //    right = _right;
            //    writeLeds(sp, right, 25 * 3);
            //}


            //pictureBox31.Image = e.Bitmap;

            //pictureBox16.BackColor = SimpleColorToColor(colors[0][0]);
            //pictureBox17.BackColor = SimpleColorToColor(colors[0][1]);
            //pictureBox18.BackColor = SimpleColorToColor(colors[0][2]);
            //pictureBox19.BackColor = SimpleColorToColor(colors[0][3]);
            //pictureBox20.BackColor = SimpleColorToColor(colors[0][4]);
            //pictureBox21.BackColor = SimpleColorToColor(colors[0][5]);
            //pictureBox22.BackColor = SimpleColorToColor(colors[0][6]);
            //pictureBox23.BackColor = SimpleColorToColor(colors[0][7]);
            //pictureBox24.BackColor = SimpleColorToColor(colors[0][8]);
            //pictureBox25.BackColor = SimpleColorToColor(colors[0][9]);
            //
            // pictureBox1.BackColor = SimpleColorToColor(colors[1][0]);
            // pictureBox2.BackColor = SimpleColorToColor(colors[1][1]);
            // pictureBox3.BackColor = SimpleColorToColor(colors[1][2]);
            // pictureBox4.BackColor = SimpleColorToColor(colors[1][3]);
            // pictureBox5.BackColor = SimpleColorToColor(colors[1][4]);
            // pictureBox6.BackColor = SimpleColorToColor(colors[1][5]);
            // pictureBox7.BackColor = SimpleColorToColor(colors[1][6]);
            // pictureBox8.BackColor = SimpleColorToColor(colors[1][7]);
            // pictureBox9.BackColor = SimpleColorToColor(colors[1][8]);
            //pictureBox10.BackColor = SimpleColorToColor(colors[1][9]);
            //
            //pictureBox11.BackColor = SimpleColorToColor(colors[2][0]);
            //pictureBox12.BackColor = SimpleColorToColor(colors[2][1]);
            //pictureBox13.BackColor = SimpleColorToColor(colors[2][2]);
            //pictureBox14.BackColor = SimpleColorToColor(colors[2][3]);
            //pictureBox15.BackColor = SimpleColorToColor(colors[2][4]);
            //
            //pictureBox26.BackColor = SimpleColorToColor(colors[3][0]);
            //pictureBox27.BackColor = SimpleColorToColor(colors[3][1]);
            //pictureBox28.BackColor = SimpleColorToColor(colors[3][2]);
            //pictureBox29.BackColor = SimpleColorToColor(colors[3][3]);
            //pictureBox30.BackColor = SimpleColorToColor(colors[3][4]);

            //GC.Collect();

            sw.Stop();
            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = (sw.ElapsedMilliseconds).ToString();
            });
            sw.Reset();
        }

        public static byte[] RepeatByte(byte[] array, int times)
        {
            byte[] repeated = new byte[times * array.Length];
            for (int dest = 0; dest < repeated.Length; dest += array.Length)
            {
                Array.Copy(array, 0, repeated, dest, array.Length);
            }
            return repeated;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Process[] ps = Process.GetProcessesByName("AmbiLight");
            foreach(Process p in ps) if(p.Id != Process.GetCurrentProcess().Id) p.Kill();
            sp = new SerialPort("COM24", 115200, Parity.None, 8, StopBits.One)
            {
                WriteTimeout = 500,
                ReadTimeout = 500,
                Handshake = Handshake.None
            };
            sp.Open();

            Thread.Sleep(1500);


            ScreenCapturer.OnScreenUpdated += OnScreenUpdated;
            ScreenCapturer.StartCapture();

            //Thread read = new(readSerial);
            //Thread write = new(writeSerial);

            //read.Start(sp);
            //write.Start(sp);
        }

        private byte[] ColorListToByteArr(List<SimpleColor> list, bool reverse = false)
        {
            List<byte> bytes = [];
            if (reverse) list.Reverse();
            foreach (SimpleColor color in list)
            {
                bytes.Add(color.R);
                bytes.Add(color.G);
                bytes.Add(color.B);
            }
            return bytes.ToArray();
        }

        class SimpleColor
        {
            public byte R;
            public byte G;
            public byte B;
            public SimpleColor(byte R, byte G, byte B)
            {
                this.R = R;
                this.G = G;
                this.B = B;
            }
        }

        public static Bitmap ConvertTo24bpp(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(bmp)) gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
            return bmp;
        }

        private List<SimpleColor>[] processRegions(Bitmap img)
        {
            BitmapData bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Mat screen = new Mat(img.Height, img.Width, Emgu.CV.CvEnum.DepthType.Cv8U, 4);
            using (Image<Bgra, byte> image = new(img.Width, img.Height, bmpData.Stride, bmpData.Scan0))
            {
                //image._EqualizeHist();
                image._GammaCorrect(1.8d);
                image.Mat.CopyTo(screen);
            }
            img.UnlockBits(bmpData);

            int regionWidth = img.Width / cols;
            int regionHeight = img.Height / rows;

            List<SimpleColor> TopColors = [], BottomColors = [], LeftColors = [], RightColors = [];

            //get top leds
            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Rectangle roiRect = new Rectangle(j * regionWidth, i * regionHeight, regionWidth, regionHeight);
                    Mat roi = new Mat(screen, roiRect);

                    MCvScalar averageColor = CvInvoke.Mean(roi);
                    TopColors.Add(MCvScalarToColor(averageColor));
                    roi.Dispose();
                }
            }

            //get bottom leds
            for (int i = rows - 1; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Rectangle roiRect = new Rectangle(j * regionWidth, i * regionHeight, regionWidth, regionHeight);
                    Mat roi = new Mat(screen, roiRect);

                    MCvScalar averageColor = CvInvoke.Mean(roi);
                    BottomColors.Add(MCvScalarToColor(averageColor));
                    roi.Dispose();
                }
            }

            //get left leds
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    Rectangle roiRect = new Rectangle(j * regionWidth, i * regionHeight, regionWidth, regionHeight);
                    Mat roi = new Mat(screen, roiRect);

                    MCvScalar averageColor = CvInvoke.Mean(roi);
                    LeftColors.Add(MCvScalarToColor(averageColor));
                    roi.Dispose();
                }
            }

            //get right leds
            for (int i = 0; i < rows; i++)
            {
                for (int j = cols - 1; j < cols; j++)
                {
                    Rectangle roiRect = new Rectangle(j * regionWidth, i * regionHeight, regionWidth, regionHeight);
                    Mat roi = new Mat(screen, roiRect);

                    MCvScalar averageColor = CvInvoke.Mean(roi);
                    RightColors.Add(MCvScalarToColor(averageColor));
                    roi.Dispose();
                }
            }

            List<SimpleColor>[] combined = { TopColors, BottomColors, LeftColors, RightColors };
            screen.Dispose();
            return combined;
        }

        SimpleColor MCvScalarToColor(MCvScalar scalar)
        {
            return new SimpleColor((byte)scalar.V2, (byte)scalar.V1, (byte)scalar.V0);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(sp.IsOpen) sp.Close();
            Environment.Exit(0);
        }
    }
}
