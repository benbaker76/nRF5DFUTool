using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

using Org.BouncyCastle.Utilities.Encoders;

namespace nRF5DFUTool
{
    public partial class frmMain : Form
    {
        private static NordicBLEDevice m_nordicBLEDevice = null;
        private static bool m_hasValidFirmware = false;

        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LogFile.FileName = Path.Combine(Application.StartupPath, "nRF5DFUTool.log");
            LogFile.ClearLog();
            LogFile.LogFileEvent += OnLogFileEvent;

			// From dfu_public_key.c
			NordicBLEDevice.PublicKey = Hex.Decode("84b7ac5dba001ccdbe4928f7cbd974445da08494db12a36db24a17a13d05b938dba4214542101bbfeb09b53367ab91146df5f17dd69d178820dfcfec866407c1");

            m_nordicBLEDevice = new NordicBLEDevice("Nordic_Blinky", "DfuTarg");
            m_nordicBLEDevice.ProgressChanged += OnProgressChanged;
            m_nordicBLEDevice.PropertyChanged += OnPropertyChanged;
		}

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate ()
            {
                progressBar1.Value = e.ProgressPercentage;
            });
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_nordicBLEDevice.Dispose();
        }

        private void butOpenFirmware_Click(object sender, EventArgs e)
        {
            m_hasValidFirmware = false;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Firmware File";
            openFileDialog.InitialDirectory = null;
            openFileDialog.Filter = "Zip Files (*.zip)|*.zip|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            if (m_nordicBLEDevice.TryOpenAppDfuPackage(openFileDialog.FileName))
                m_hasValidFirmware = true;
        }

        private void butFlashFirmware_Click(object sender, EventArgs e)
        {
            if (m_nordicBLEDevice == null || !m_hasValidFirmware)
                return;

            m_nordicBLEDevice.StartDFU();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)async delegate ()
            {
                string[] items = new string[] { m_nordicBLEDevice.Name, m_nordicBLEDevice.Address, m_nordicBLEDevice.ModelNumber, m_nordicBLEDevice.SerialNumber, m_nordicBLEDevice.FirmwareNumber, m_nordicBLEDevice.HardwareNumber, m_nordicBLEDevice.SoftwareNumber, m_nordicBLEDevice.ManufacturerName, m_nordicBLEDevice.IsPaired.ToString(), m_nordicBLEDevice.IsPresent.ToString(), m_nordicBLEDevice.IsConnected.ToString(), m_nordicBLEDevice.IsConnectable.ToString(), String.Format("{0}%", m_nordicBLEDevice.BatteryLevel) };

                if (lvwBLEDevices.Items.Count == 0)
                    lvwBLEDevices.Items.Add(new ListViewItem(items));
                else
                {
                    ImageList imageList = new ImageList();

                    using (var stream = new InMemoryRandomAccessStream())
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                        encoder.SetSoftwareBitmap(m_nordicBLEDevice.GlyphBitmap);
                        await encoder.FlushAsync();
                        Bitmap bitmap = new Bitmap(stream.AsStream());
                        imageList.Images.Add(bitmap);
                    }

                    lvwBLEDevices.SmallImageList = imageList;
                    lvwBLEDevices.Items[0].ImageIndex = 0;

                    for (int i = 0; i < lvwBLEDevices.Items[0].SubItems.Count; i++)
                        lvwBLEDevices.Items[0].SubItems[i].Text = items[i];
                }

                foreach (ColumnHeader columnHeader in lvwBLEDevices.Columns)
                    columnHeader.Width = -2;
            });
        }

        private void OnLogFileEvent(object sender, LogFileEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate ()
            {
                OutputText(e.Text);
            });
        }

        private void OutputText(string format, params object[] args)
        {
            lock (txtOutput)
            {
                txtOutput.AppendText(String.Format(format, args));
                txtOutput.AppendText(Environment.NewLine);
            }
        }
    }
}
