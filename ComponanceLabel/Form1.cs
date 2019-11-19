using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BarcodeLib;
using Dynamsoft.Barcode;
using TouchlessLib;

namespace ComponanceLabel
{
    public partial class Form1 : Form
    {
        private BarcodeReader _barcodeReader;
        private TouchlessMgr _touch;
        private const int _previewWidth = 640;
        private const int _previewHeight = 360;
        private Barcode _b;

        Bitmap MemoryImage;

        public Form1()
        {
            InitializeComponent();

            _barcodeReader = new BarcodeReader();
            _touch = new TouchlessMgr();

            _b = new Barcode();

            _b.ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            _b.IncludeLabel = true;
            _b.LabelFont = new Font("Microsoft Himalaya", 9, FontStyle.Regular);
            _b.LabelPosition = LabelPositions.BOTTOMCENTER;
            _b.Alignment = AlignmentPositions.CENTER;
            _b.RotateFlipType = RotateFlipType.RotateNoneFlipNone;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = _touch.Cameras;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string button_text = button1.Text;
            if (button_text.Equals("Start Scan"))
            {
                label1.Visible = false;
                button2.Enabled = false;
                button1.Text = "Stop Scan";
                StartCamera();
            }
            else
            {
                button1.Text = "Start Scan";
                StopCamera();
                label1.Visible = true;
                button2.Enabled = true;
            }


        }

        private void StopCamera()
        {
            label1.Visible = true;
            button1.Text = "Start Scan";
            if (_touch.CurrentCamera != null)
            {
                _touch.CurrentCamera.OnImageCaptured -= new EventHandler<CameraEventArgs>(OnImageCaptured);

            }
        }
        private void StartCamera()
        {
            if (comboBox1.Text == null)
            {
                MessageBox.Show("No USB webcam connected Or Select Device");
                button1.Text = "Start Scan";
                return;
            }

            // Start to acquire images

            _touch.CurrentCamera = _touch.Cameras[comboBox1.SelectedIndex];
            _touch.CurrentCamera.CaptureWidth = _previewWidth; // Set width
            _touch.CurrentCamera.CaptureWidth = _previewHeight; // Set height
            _touch.CurrentCamera.OnImageCaptured += new EventHandler<CameraEventArgs>(OnImageCaptured); // Set preview callback function
        }

        private void OnImageCaptured(object sender, CameraEventArgs args)
        {
            // Get the bitmap
            Bitmap bitmap = args.Image;

            // Read barcode and show results in UI thread
            this.Invoke((MethodInvoker)delegate
            {
                pictureBox1.Image = bitmap;
                ReadBarcode(bitmap);
            });
        }

        private void ReadBarcode(Bitmap bitmap)
        {

            try
            {

                // Read barcodes with Dynamsoft Barcode Reader
                Stopwatch sw = Stopwatch.StartNew();
                sw.Start();
                BarcodeResult[] results = _barcodeReader.DecodeBitmap(bitmap);
                sw.Stop();
                Console.WriteLine(sw.Elapsed.TotalMilliseconds + "ms");

                // Clear previous results
                textBox1.Clear();

                if (results == null)
                {
                    textBox1.Text = "No barcode detected!";
                    pictureBox2.Image = null;
                    return;
                }

                String[] strlist = results[0].BarcodeText.Split(',');

                Array.Resize(ref strlist, strlist.Length - 1);

                // Display barcode results
                string newLine = Environment.NewLine;

                for (int i = 0; i <= 6; i++)
                {
                    textBox1.AppendText(strlist[i] + newLine);

                }

                if (strlist == null)
                {
                    return;
                }

                Image img = _b.Encode(TYPE.CODE39, strlist[1].ToString(), Color.Black, Color.Transparent, 200, 20);

                pictureBox2.Image = img;

                button2.Enabled = true;

                StopCamera();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                StopCamera();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GetPrintArea(pictureBox2);
            printDocument1.Print();
        }
        public void GetPrintArea(PictureBox pnl)
        {
            MemoryImage = new Bitmap(pnl.Width, pnl.Height);
            Rectangle rect = new Rectangle(0, 0, pnl.Width, pnl.Height);
            pnl.DrawToBitmap(MemoryImage, new Rectangle(0, 0, pnl.Width, pnl.Height));
        }
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Rectangle pagearea = e.PageBounds;
            e.Graphics.DrawImage(MemoryImage, 0, 0);
        }
    }
}
