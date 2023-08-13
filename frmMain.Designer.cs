namespace nRF5DFUTool
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.butOpenFirmware = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.lvwBLEDevices = new System.Windows.Forms.ListView();
			this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colModelNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSerialNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colFirmwareRevision = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colHardwareRevision = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colSoftwareRevision = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colManufacturerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colIsPaired = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colIsPresent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colIsConnected = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colIsConnectable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colBatteryLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.butFlashFirmware = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// butOpenFirmware
			// 
			this.butOpenFirmware.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOpenFirmware.Location = new System.Drawing.Point(429, 62);
			this.butOpenFirmware.Margin = new System.Windows.Forms.Padding(2);
			this.butOpenFirmware.Name = "butOpenFirmware";
			this.butOpenFirmware.Size = new System.Drawing.Size(131, 29);
			this.butOpenFirmware.TabIndex = 0;
			this.butOpenFirmware.Text = "Open Firmware";
			this.butOpenFirmware.UseVisualStyleBackColor = true;
			this.butOpenFirmware.Click += new System.EventHandler(this.butOpenFirmware_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.progressBar1);
			this.splitContainer1.Panel2.Controls.Add(this.butFlashFirmware);
			this.splitContainer1.Panel2.Controls.Add(this.butOpenFirmware);
			this.splitContainer1.Size = new System.Drawing.Size(572, 433);
			this.splitContainer1.SplitterDistance = 325;
			this.splitContainer1.SplitterWidth = 2;
			this.splitContainer1.TabIndex = 1;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.lvwBLEDevices);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.txtOutput);
			this.splitContainer2.Size = new System.Drawing.Size(572, 325);
			this.splitContainer2.SplitterDistance = 141;
			this.splitContainer2.SplitterWidth = 2;
			this.splitContainer2.TabIndex = 0;
			// 
			// lvwBLEDevices
			// 
			this.lvwBLEDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colAddress,
            this.colModelNumber,
            this.colSerialNumber,
            this.colFirmwareRevision,
            this.colHardwareRevision,
            this.colSoftwareRevision,
            this.colManufacturerName,
            this.colIsPaired,
            this.colIsPresent,
            this.colIsConnected,
            this.colIsConnectable,
            this.colBatteryLevel});
			this.lvwBLEDevices.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lvwBLEDevices.FullRowSelect = true;
			this.lvwBLEDevices.GridLines = true;
			this.lvwBLEDevices.HideSelection = false;
			this.lvwBLEDevices.Location = new System.Drawing.Point(0, 0);
			this.lvwBLEDevices.Margin = new System.Windows.Forms.Padding(2);
			this.lvwBLEDevices.Name = "lvwBLEDevices";
			this.lvwBLEDevices.Size = new System.Drawing.Size(572, 141);
			this.lvwBLEDevices.TabIndex = 0;
			this.lvwBLEDevices.UseCompatibleStateImageBehavior = false;
			this.lvwBLEDevices.View = System.Windows.Forms.View.Details;
			// 
			// colName
			// 
			this.colName.Text = "Name";
			// 
			// colAddress
			// 
			this.colAddress.Text = "Address";
			// 
			// colModelNumber
			// 
			this.colModelNumber.Text = "Model";
			// 
			// colSerialNumber
			// 
			this.colSerialNumber.Text = "Serial";
			// 
			// colFirmwareRevision
			// 
			this.colFirmwareRevision.Text = "Firmware";
			// 
			// colHardwareRevision
			// 
			this.colHardwareRevision.Text = "Hardware";
			// 
			// colSoftwareRevision
			// 
			this.colSoftwareRevision.Text = "Software";
			// 
			// colManufacturerName
			// 
			this.colManufacturerName.Text = "Manufacturer";
			// 
			// colIsPaired
			// 
			this.colIsPaired.Text = "IsPaired";
			// 
			// colIsPresent
			// 
			this.colIsPresent.Text = "IsPresent";
			// 
			// colIsConnected
			// 
			this.colIsConnected.Text = "IsConnected";
			// 
			// colIsConnectable
			// 
			this.colIsConnectable.Text = "IsConnectable";
			// 
			// colBatteryLevel
			// 
			this.colBatteryLevel.Text = "Battery";
			// 
			// txtOutput
			// 
			this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtOutput.Location = new System.Drawing.Point(0, 0);
			this.txtOutput.Margin = new System.Windows.Forms.Padding(2);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOutput.Size = new System.Drawing.Size(572, 182);
			this.txtOutput.TabIndex = 0;
			this.txtOutput.WordWrap = false;
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(156, 71);
			this.progressBar1.Margin = new System.Windows.Forms.Padding(2);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(263, 12);
			this.progressBar1.TabIndex = 2;
			// 
			// butFlashFirmware
			// 
			this.butFlashFirmware.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butFlashFirmware.Location = new System.Drawing.Point(14, 62);
			this.butFlashFirmware.Margin = new System.Windows.Forms.Padding(2);
			this.butFlashFirmware.Name = "butFlashFirmware";
			this.butFlashFirmware.Size = new System.Drawing.Size(131, 29);
			this.butFlashFirmware.TabIndex = 1;
			this.butFlashFirmware.Text = "Flash Firmware";
			this.butFlashFirmware.UseVisualStyleBackColor = true;
			this.butFlashFirmware.Click += new System.EventHandler(this.butFlashFirmware_Click);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(572, 433);
			this.Controls.Add(this.splitContainer1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "frmMain";
			this.Text = "nRF5 DFU Tool";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butOpenFirmware;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.ListView lvwBLEDevices;
        private System.Windows.Forms.ColumnHeader colAddress;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colModelNumber;
        private System.Windows.Forms.ColumnHeader colSerialNumber;
        private System.Windows.Forms.ColumnHeader colFirmwareRevision;
        private System.Windows.Forms.ColumnHeader colHardwareRevision;
        private System.Windows.Forms.ColumnHeader colSoftwareRevision;
        private System.Windows.Forms.ColumnHeader colManufacturerName;
        private System.Windows.Forms.ColumnHeader colIsPaired;
        private System.Windows.Forms.ColumnHeader colIsConnected;
        private System.Windows.Forms.ColumnHeader colIsConnectable;
        private System.Windows.Forms.ColumnHeader colBatteryLevel;
        private System.Windows.Forms.Button butFlashFirmware;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ColumnHeader colIsPresent;
    }
}

