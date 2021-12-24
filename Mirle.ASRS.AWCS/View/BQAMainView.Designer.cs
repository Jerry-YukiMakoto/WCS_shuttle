
namespace Mirle.ASRS.AWCS.View
{
    partial class BQAMainView
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
            this.components = new System.ComponentModel.Container();
            this.timRefresh = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblDateTime = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnGeneralHP = new System.Windows.Forms.Button();
            this.btn3ALinkLowerFloor = new System.Windows.Forms.Button();
            this.btn3ALinkUpperFloor = new System.Windows.Forms.Button();
            this.btnLogTrace = new System.Windows.Forms.Button();
            this.btnPLCDataModify = new System.Windows.Forms.Button();
            this.btnBufferInfo = new System.Windows.Forms.Button();
            this.pnl3ALinkUpperFloor = new System.Windows.Forms.Panel();
            this.A17_1 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A16_1 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A13 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A16 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A14 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A17 = new Mirle.ASRS.AWCS.View.BufferView();
            this.pnl3ALinkLowerFloor = new System.Windows.Forms.Panel();
            this.A19 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A17_3 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A17_4 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A13_1 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A17_2 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A16_3 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A14_1 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A16_2 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A16_4 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A18 = new Mirle.ASRS.AWCS.View.BufferView();
            this.pnlGeneralEnvironment = new System.Windows.Forms.Panel();
            this.A09_4 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A08 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A09 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A09_1 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A09_2 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A09_3 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A09_5 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A10 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A11 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A11_1 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A11_2 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A12_2 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A12_1 = new Mirle.ASRS.AWCS.View.BufferView();
            this.A12 = new Mirle.ASRS.AWCS.View.BufferView();
            this.lstLogTrace = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnl3ALinkUpperFloor.SuspendLayout();
            this.pnl3ALinkLowerFloor.SuspendLayout();
            this.pnlGeneralEnvironment.SuspendLayout();
            this.SuspendLayout();
            // 
            // timRefresh
            // 
            this.timRefresh.Tick += new System.EventHandler(this.Refresh_Tick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDateTime, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1182, 98);
            this.tableLayoutPanel1.TabIndex = 31;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::Mirle.ASRS.AWCS.Properties.Resources.短式_彩色Logo_去背;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.tableLayoutPanel1.SetRowSpan(this.pictureBox1, 3);
            this.pictureBox1.Size = new System.Drawing.Size(194, 92);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // lblDateTime
            // 
            this.lblDateTime.AutoSize = true;
            this.lblDateTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDateTime.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDateTime.Location = new System.Drawing.Point(203, 29);
            this.lblDateTime.Name = "lblDateTime";
            this.lblDateTime.Size = new System.Drawing.Size(294, 40);
            this.lblDateTime.TabIndex = 1;
            this.lblDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(1184, 761);
            this.splitContainer2.SplitterDistance = 100;
            this.splitContainer2.SplitterWidth = 1;
            this.splitContainer2.TabIndex = 32;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.lstLogTrace);
            this.splitContainer3.Panel2MinSize = 300;
            this.splitContainer3.Size = new System.Drawing.Size(1182, 658);
            this.splitContainer3.SplitterDistance = 878;
            this.splitContainer3.TabIndex = 33;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer1.Panel1MinSize = 50;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pnl3ALinkUpperFloor);
            this.splitContainer1.Panel2.Controls.Add(this.pnl3ALinkLowerFloor);
            this.splitContainer1.Panel2.Controls.Add(this.pnlGeneralEnvironment);
            this.splitContainer1.Size = new System.Drawing.Size(878, 658);
            this.splitContainer1.TabIndex = 31;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btnGeneralHP);
            this.flowLayoutPanel1.Controls.Add(this.btn3ALinkLowerFloor);
            this.flowLayoutPanel1.Controls.Add(this.btn3ALinkUpperFloor);
            this.flowLayoutPanel1.Controls.Add(this.btnLogTrace);
            this.flowLayoutPanel1.Controls.Add(this.btnPLCDataModify);
            this.flowLayoutPanel1.Controls.Add(this.btnBufferInfo);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(876, 48);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // btnGeneralHP
            // 
            this.btnGeneralHP.Location = new System.Drawing.Point(3, 3);
            this.btnGeneralHP.Name = "btnGeneralHP";
            this.btnGeneralHP.Size = new System.Drawing.Size(75, 40);
            this.btnGeneralHP.TabIndex = 0;
            this.btnGeneralHP.Text = "General (HP)";
            this.btnGeneralHP.UseVisualStyleBackColor = true;
            this.btnGeneralHP.Click += new System.EventHandler(this.GeneralHP_Click);
            // 
            // btn3ALinkLowerFloor
            // 
            this.btn3ALinkLowerFloor.Location = new System.Drawing.Point(84, 3);
            this.btn3ALinkLowerFloor.Name = "btn3ALinkLowerFloor";
            this.btn3ALinkLowerFloor.Size = new System.Drawing.Size(75, 40);
            this.btn3ALinkLowerFloor.TabIndex = 1;
            this.btn3ALinkLowerFloor.Text = "3A Link Lower Floor";
            this.btn3ALinkLowerFloor.UseVisualStyleBackColor = true;
            this.btn3ALinkLowerFloor.Click += new System.EventHandler(this.LowerFloor_Click);
            // 
            // btn3ALinkUpperFloor
            // 
            this.btn3ALinkUpperFloor.Location = new System.Drawing.Point(165, 3);
            this.btn3ALinkUpperFloor.Name = "btn3ALinkUpperFloor";
            this.btn3ALinkUpperFloor.Size = new System.Drawing.Size(75, 40);
            this.btn3ALinkUpperFloor.TabIndex = 2;
            this.btn3ALinkUpperFloor.Text = "3A Link Upper Floor";
            this.btn3ALinkUpperFloor.UseVisualStyleBackColor = true;
            this.btn3ALinkUpperFloor.Click += new System.EventHandler(this.UpperFloor_Click);
            // 
            // btnLogTrace
            // 
            this.btnLogTrace.Location = new System.Drawing.Point(246, 3);
            this.btnLogTrace.Name = "btnLogTrace";
            this.btnLogTrace.Size = new System.Drawing.Size(75, 40);
            this.btnLogTrace.TabIndex = 3;
            this.btnLogTrace.Text = "Log Trace";
            this.btnLogTrace.UseVisualStyleBackColor = true;
            this.btnLogTrace.Click += new System.EventHandler(this.LogTrace_Click);
            // 
            // btnPLCDataModify
            // 
            this.btnPLCDataModify.Location = new System.Drawing.Point(327, 3);
            this.btnPLCDataModify.Name = "btnPLCDataModify";
            this.btnPLCDataModify.Size = new System.Drawing.Size(75, 40);
            this.btnPLCDataModify.TabIndex = 4;
            this.btnPLCDataModify.Text = "PLC Data Modify";
            this.btnPLCDataModify.UseVisualStyleBackColor = true;
            // 
            // btnBufferInfo
            // 
            this.btnBufferInfo.Location = new System.Drawing.Point(408, 3);
            this.btnBufferInfo.Name = "btnBufferInfo";
            this.btnBufferInfo.Size = new System.Drawing.Size(75, 40);
            this.btnBufferInfo.TabIndex = 5;
            this.btnBufferInfo.Text = "Buffer Info";
            this.btnBufferInfo.UseVisualStyleBackColor = true;
            // 
            // pnl3ALinkUpperFloor
            // 
            this.pnl3ALinkUpperFloor.AutoSize = true;
            this.pnl3ALinkUpperFloor.Controls.Add(this.A17_1);
            this.pnl3ALinkUpperFloor.Controls.Add(this.A16_1);
            this.pnl3ALinkUpperFloor.Controls.Add(this.A13);
            this.pnl3ALinkUpperFloor.Controls.Add(this.A16);
            this.pnl3ALinkUpperFloor.Controls.Add(this.A14);
            this.pnl3ALinkUpperFloor.Controls.Add(this.A17);
            this.pnl3ALinkUpperFloor.Location = new System.Drawing.Point(477, 74);
            this.pnl3ALinkUpperFloor.Name = "pnl3ALinkUpperFloor";
            this.pnl3ALinkUpperFloor.Size = new System.Drawing.Size(430, 213);
            this.pnl3ALinkUpperFloor.TabIndex = 2;
            // 
            // A17_1
            // 
            this.A17_1.BufferIndex = 25;
            this.A17_1.Location = new System.Drawing.Point(3, 3);
            this.A17_1.MaximumSize = new System.Drawing.Size(80, 65);
            this.A17_1.MinimumSize = new System.Drawing.Size(80, 65);
            this.A17_1.Name = "A17_1";
            this.A17_1.Size = new System.Drawing.Size(80, 65);
            this.A17_1.TabIndex = 42;
            // 
            // A16_1
            // 
            this.A16_1.BufferIndex = 27;
            this.A16_1.Location = new System.Drawing.Point(220, 3);
            this.A16_1.MaximumSize = new System.Drawing.Size(80, 65);
            this.A16_1.MinimumSize = new System.Drawing.Size(80, 65);
            this.A16_1.Name = "A16_1";
            this.A16_1.Size = new System.Drawing.Size(80, 65);
            this.A16_1.TabIndex = 45;
            // 
            // A13
            // 
            this.A13.BufferIndex = 30;
            this.A13.Location = new System.Drawing.Point(347, 145);
            this.A13.MaximumSize = new System.Drawing.Size(80, 65);
            this.A13.MinimumSize = new System.Drawing.Size(80, 65);
            this.A13.Name = "A13";
            this.A13.Size = new System.Drawing.Size(80, 65);
            this.A13.TabIndex = 40;
            // 
            // A16
            // 
            this.A16.BufferIndex = 28;
            this.A16.Location = new System.Drawing.Point(347, 3);
            this.A16.MaximumSize = new System.Drawing.Size(80, 65);
            this.A16.MinimumSize = new System.Drawing.Size(80, 65);
            this.A16.Name = "A16";
            this.A16.Size = new System.Drawing.Size(80, 65);
            this.A16.TabIndex = 44;
            // 
            // A14
            // 
            this.A14.BufferIndex = 29;
            this.A14.Location = new System.Drawing.Point(347, 74);
            this.A14.MaximumSize = new System.Drawing.Size(80, 65);
            this.A14.MinimumSize = new System.Drawing.Size(80, 65);
            this.A14.Name = "A14";
            this.A14.Size = new System.Drawing.Size(80, 65);
            this.A14.TabIndex = 41;
            // 
            // A17
            // 
            this.A17.BufferIndex = 26;
            this.A17.Location = new System.Drawing.Point(134, 3);
            this.A17.MaximumSize = new System.Drawing.Size(80, 65);
            this.A17.MinimumSize = new System.Drawing.Size(80, 65);
            this.A17.Name = "A17";
            this.A17.Size = new System.Drawing.Size(80, 65);
            this.A17.TabIndex = 43;
            // 
            // pnl3ALinkLowerFloor
            // 
            this.pnl3ALinkLowerFloor.AutoSize = true;
            this.pnl3ALinkLowerFloor.Controls.Add(this.A19);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A17_3);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A17_4);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A13_1);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A17_2);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A16_3);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A14_1);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A16_2);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A16_4);
            this.pnl3ALinkLowerFloor.Controls.Add(this.A18);
            this.pnl3ALinkLowerFloor.Location = new System.Drawing.Point(301, 3);
            this.pnl3ALinkLowerFloor.Name = "pnl3ALinkLowerFloor";
            this.pnl3ALinkLowerFloor.Size = new System.Drawing.Size(603, 284);
            this.pnl3ALinkLowerFloor.TabIndex = 1;
            // 
            // A19
            // 
            this.A19.BufferIndex = 24;
            this.A19.Location = new System.Drawing.Point(4, 74);
            this.A19.MaximumSize = new System.Drawing.Size(80, 65);
            this.A19.MinimumSize = new System.Drawing.Size(80, 65);
            this.A19.Name = "A19";
            this.A19.Size = new System.Drawing.Size(80, 65);
            this.A19.TabIndex = 54;
            // 
            // A17_3
            // 
            this.A17_3.BufferIndex = 21;
            this.A17_3.Location = new System.Drawing.Point(176, 74);
            this.A17_3.MaximumSize = new System.Drawing.Size(80, 65);
            this.A17_3.MinimumSize = new System.Drawing.Size(80, 65);
            this.A17_3.Name = "A17_3";
            this.A17_3.Size = new System.Drawing.Size(80, 65);
            this.A17_3.TabIndex = 52;
            // 
            // A17_4
            // 
            this.A17_4.BufferIndex = 22;
            this.A17_4.Location = new System.Drawing.Point(176, 3);
            this.A17_4.MaximumSize = new System.Drawing.Size(80, 65);
            this.A17_4.MinimumSize = new System.Drawing.Size(80, 65);
            this.A17_4.Name = "A17_4";
            this.A17_4.Size = new System.Drawing.Size(80, 65);
            this.A17_4.TabIndex = 55;
            // 
            // A13_1
            // 
            this.A13_1.BufferIndex = 15;
            this.A13_1.Location = new System.Drawing.Point(520, 216);
            this.A13_1.MaximumSize = new System.Drawing.Size(80, 65);
            this.A13_1.MinimumSize = new System.Drawing.Size(80, 65);
            this.A13_1.Name = "A13_1";
            this.A13_1.Size = new System.Drawing.Size(80, 65);
            this.A13_1.TabIndex = 46;
            // 
            // A17_2
            // 
            this.A17_2.BufferIndex = 20;
            this.A17_2.Location = new System.Drawing.Point(262, 74);
            this.A17_2.MaximumSize = new System.Drawing.Size(80, 65);
            this.A17_2.MinimumSize = new System.Drawing.Size(80, 65);
            this.A17_2.Name = "A17_2";
            this.A17_2.Size = new System.Drawing.Size(80, 65);
            this.A17_2.TabIndex = 51;
            // 
            // A16_3
            // 
            this.A16_3.BufferIndex = 18;
            this.A16_3.Location = new System.Drawing.Point(434, 74);
            this.A16_3.MaximumSize = new System.Drawing.Size(80, 65);
            this.A16_3.MinimumSize = new System.Drawing.Size(80, 65);
            this.A16_3.Name = "A16_3";
            this.A16_3.Size = new System.Drawing.Size(80, 65);
            this.A16_3.TabIndex = 49;
            // 
            // A14_1
            // 
            this.A14_1.BufferIndex = 16;
            this.A14_1.Location = new System.Drawing.Point(520, 145);
            this.A14_1.MaximumSize = new System.Drawing.Size(80, 65);
            this.A14_1.MinimumSize = new System.Drawing.Size(80, 65);
            this.A14_1.Name = "A14_1";
            this.A14_1.Size = new System.Drawing.Size(80, 65);
            this.A14_1.TabIndex = 47;
            // 
            // A16_2
            // 
            this.A16_2.BufferIndex = 17;
            this.A16_2.Location = new System.Drawing.Point(520, 74);
            this.A16_2.MaximumSize = new System.Drawing.Size(80, 65);
            this.A16_2.MinimumSize = new System.Drawing.Size(80, 65);
            this.A16_2.Name = "A16_2";
            this.A16_2.Size = new System.Drawing.Size(80, 65);
            this.A16_2.TabIndex = 48;
            // 
            // A16_4
            // 
            this.A16_4.BufferIndex = 19;
            this.A16_4.Location = new System.Drawing.Point(348, 74);
            this.A16_4.MaximumSize = new System.Drawing.Size(80, 65);
            this.A16_4.MinimumSize = new System.Drawing.Size(80, 65);
            this.A16_4.Name = "A16_4";
            this.A16_4.Size = new System.Drawing.Size(80, 65);
            this.A16_4.TabIndex = 50;
            // 
            // A18
            // 
            this.A18.BufferIndex = 23;
            this.A18.Location = new System.Drawing.Point(90, 74);
            this.A18.MaximumSize = new System.Drawing.Size(80, 65);
            this.A18.MinimumSize = new System.Drawing.Size(80, 65);
            this.A18.Name = "A18";
            this.A18.Size = new System.Drawing.Size(80, 65);
            this.A18.TabIndex = 53;
            // 
            // pnlGeneralEnvironment
            // 
            this.pnlGeneralEnvironment.AutoSize = true;
            this.pnlGeneralEnvironment.Controls.Add(this.A09_4);
            this.pnlGeneralEnvironment.Controls.Add(this.A08);
            this.pnlGeneralEnvironment.Controls.Add(this.A09);
            this.pnlGeneralEnvironment.Controls.Add(this.A09_1);
            this.pnlGeneralEnvironment.Controls.Add(this.A09_2);
            this.pnlGeneralEnvironment.Controls.Add(this.A09_3);
            this.pnlGeneralEnvironment.Controls.Add(this.A09_5);
            this.pnlGeneralEnvironment.Controls.Add(this.A10);
            this.pnlGeneralEnvironment.Controls.Add(this.A11);
            this.pnlGeneralEnvironment.Controls.Add(this.A11_1);
            this.pnlGeneralEnvironment.Controls.Add(this.A11_2);
            this.pnlGeneralEnvironment.Controls.Add(this.A12_2);
            this.pnlGeneralEnvironment.Controls.Add(this.A12_1);
            this.pnlGeneralEnvironment.Controls.Add(this.A12);
            this.pnlGeneralEnvironment.Location = new System.Drawing.Point(3, 71);
            this.pnlGeneralEnvironment.Name = "pnlGeneralEnvironment";
            this.pnlGeneralEnvironment.Size = new System.Drawing.Size(774, 213);
            this.pnlGeneralEnvironment.TabIndex = 0;
            // 
            // A09_4
            // 
            this.A09_4.BufferIndex = 6;
            this.A09_4.Location = new System.Drawing.Point(261, 3);
            this.A09_4.MaximumSize = new System.Drawing.Size(80, 65);
            this.A09_4.MinimumSize = new System.Drawing.Size(80, 65);
            this.A09_4.Name = "A09_4";
            this.A09_4.Size = new System.Drawing.Size(80, 65);
            this.A09_4.TabIndex = 59;
            // 
            // A08
            // 
            this.A08.BufferIndex = 1;
            this.A08.Location = new System.Drawing.Point(691, 3);
            this.A08.MaximumSize = new System.Drawing.Size(80, 65);
            this.A08.MinimumSize = new System.Drawing.Size(80, 65);
            this.A08.Name = "A08";
            this.A08.Size = new System.Drawing.Size(80, 65);
            this.A08.TabIndex = 54;
            // 
            // A09
            // 
            this.A09.BufferIndex = 2;
            this.A09.Location = new System.Drawing.Point(605, 3);
            this.A09.MaximumSize = new System.Drawing.Size(80, 65);
            this.A09.MinimumSize = new System.Drawing.Size(80, 65);
            this.A09.Name = "A09";
            this.A09.Size = new System.Drawing.Size(80, 65);
            this.A09.TabIndex = 55;
            // 
            // A09_1
            // 
            this.A09_1.BufferIndex = 3;
            this.A09_1.Location = new System.Drawing.Point(519, 3);
            this.A09_1.MaximumSize = new System.Drawing.Size(80, 65);
            this.A09_1.MinimumSize = new System.Drawing.Size(80, 65);
            this.A09_1.Name = "A09_1";
            this.A09_1.Size = new System.Drawing.Size(80, 65);
            this.A09_1.TabIndex = 56;
            // 
            // A09_2
            // 
            this.A09_2.BufferIndex = 4;
            this.A09_2.Location = new System.Drawing.Point(433, 3);
            this.A09_2.MaximumSize = new System.Drawing.Size(80, 65);
            this.A09_2.MinimumSize = new System.Drawing.Size(80, 65);
            this.A09_2.Name = "A09_2";
            this.A09_2.Size = new System.Drawing.Size(80, 65);
            this.A09_2.TabIndex = 57;
            // 
            // A09_3
            // 
            this.A09_3.BufferIndex = 5;
            this.A09_3.Location = new System.Drawing.Point(347, 3);
            this.A09_3.MaximumSize = new System.Drawing.Size(80, 65);
            this.A09_3.MinimumSize = new System.Drawing.Size(80, 65);
            this.A09_3.Name = "A09_3";
            this.A09_3.Size = new System.Drawing.Size(80, 65);
            this.A09_3.TabIndex = 58;
            // 
            // A09_5
            // 
            this.A09_5.BufferIndex = 7;
            this.A09_5.Location = new System.Drawing.Point(175, 3);
            this.A09_5.MaximumSize = new System.Drawing.Size(80, 65);
            this.A09_5.MinimumSize = new System.Drawing.Size(80, 65);
            this.A09_5.Name = "A09_5";
            this.A09_5.Size = new System.Drawing.Size(80, 65);
            this.A09_5.TabIndex = 60;
            // 
            // A10
            // 
            this.A10.BufferIndex = 8;
            this.A10.Location = new System.Drawing.Point(89, 3);
            this.A10.MaximumSize = new System.Drawing.Size(80, 65);
            this.A10.MinimumSize = new System.Drawing.Size(80, 65);
            this.A10.Name = "A10";
            this.A10.Size = new System.Drawing.Size(80, 65);
            this.A10.TabIndex = 61;
            // 
            // A11
            // 
            this.A11.BufferIndex = 9;
            this.A11.Location = new System.Drawing.Point(3, 3);
            this.A11.MaximumSize = new System.Drawing.Size(80, 65);
            this.A11.MinimumSize = new System.Drawing.Size(80, 65);
            this.A11.Name = "A11";
            this.A11.Size = new System.Drawing.Size(80, 65);
            this.A11.TabIndex = 62;
            // 
            // A11_1
            // 
            this.A11_1.BufferIndex = 10;
            this.A11_1.Location = new System.Drawing.Point(3, 74);
            this.A11_1.MaximumSize = new System.Drawing.Size(80, 65);
            this.A11_1.MinimumSize = new System.Drawing.Size(80, 65);
            this.A11_1.Name = "A11_1";
            this.A11_1.Size = new System.Drawing.Size(80, 65);
            this.A11_1.TabIndex = 63;
            // 
            // A11_2
            // 
            this.A11_2.BufferIndex = 11;
            this.A11_2.Location = new System.Drawing.Point(3, 145);
            this.A11_2.MaximumSize = new System.Drawing.Size(80, 65);
            this.A11_2.MinimumSize = new System.Drawing.Size(80, 65);
            this.A11_2.Name = "A11_2";
            this.A11_2.Size = new System.Drawing.Size(80, 65);
            this.A11_2.TabIndex = 64;
            // 
            // A12_2
            // 
            this.A12_2.BufferIndex = 12;
            this.A12_2.Location = new System.Drawing.Point(89, 145);
            this.A12_2.MaximumSize = new System.Drawing.Size(80, 65);
            this.A12_2.MinimumSize = new System.Drawing.Size(80, 65);
            this.A12_2.Name = "A12_2";
            this.A12_2.Size = new System.Drawing.Size(80, 65);
            this.A12_2.TabIndex = 65;
            // 
            // A12_1
            // 
            this.A12_1.BufferIndex = 13;
            this.A12_1.Location = new System.Drawing.Point(175, 145);
            this.A12_1.MaximumSize = new System.Drawing.Size(80, 65);
            this.A12_1.MinimumSize = new System.Drawing.Size(80, 65);
            this.A12_1.Name = "A12_1";
            this.A12_1.Size = new System.Drawing.Size(80, 65);
            this.A12_1.TabIndex = 66;
            // 
            // A12
            // 
            this.A12.BufferIndex = 14;
            this.A12.Location = new System.Drawing.Point(261, 145);
            this.A12.MaximumSize = new System.Drawing.Size(80, 65);
            this.A12.MinimumSize = new System.Drawing.Size(80, 65);
            this.A12.Name = "A12";
            this.A12.Size = new System.Drawing.Size(80, 65);
            this.A12.TabIndex = 67;
            // 
            // lstLogTrace
            // 
            this.lstLogTrace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLogTrace.FormattingEnabled = true;
            this.lstLogTrace.ItemHeight = 12;
            this.lstLogTrace.Location = new System.Drawing.Point(0, 0);
            this.lstLogTrace.Name = "lstLogTrace";
            this.lstLogTrace.Size = new System.Drawing.Size(300, 658);
            this.lstLogTrace.TabIndex = 32;
            // 
            // BQAMainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1184, 761);
            this.Controls.Add(this.splitContainer2);
            this.Name = "BQAMainView";
            this.Text = "ConveryorView";
            this.Load += new System.EventHandler(this.BQAMainView_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.pnl3ALinkUpperFloor.ResumeLayout(false);
            this.pnl3ALinkLowerFloor.ResumeLayout(false);
            this.pnlGeneralEnvironment.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timRefresh;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label lblDateTime;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnGeneralHP;
        private System.Windows.Forms.Button btn3ALinkLowerFloor;
        private System.Windows.Forms.Button btn3ALinkUpperFloor;
        private System.Windows.Forms.Panel pnl3ALinkUpperFloor;
        private BufferView A17_1;
        private BufferView A16_1;
        private BufferView A13;
        private BufferView A16;
        private BufferView A14;
        private BufferView A17;
        private System.Windows.Forms.Panel pnl3ALinkLowerFloor;
        private BufferView A19;
        private BufferView A17_3;
        private BufferView A17_4;
        private BufferView A13_1;
        private BufferView A17_2;
        private BufferView A16_3;
        private BufferView A14_1;
        private BufferView A16_2;
        private BufferView A16_4;
        private BufferView A18;
        private System.Windows.Forms.Panel pnlGeneralEnvironment;
        private BufferView A09_4;
        private BufferView A08;
        private BufferView A09;
        private BufferView A09_1;
        private BufferView A09_2;
        private BufferView A09_3;
        private BufferView A09_5;
        private BufferView A10;
        private BufferView A11;
        private BufferView A11_1;
        private BufferView A11_2;
        private BufferView A12_2;
        private BufferView A12_1;
        private BufferView A12;
        private System.Windows.Forms.Button btnLogTrace;
        private System.Windows.Forms.Button btnPLCDataModify;
        private System.Windows.Forms.Button btnBufferInfo;
        private System.Windows.Forms.ListBox lstLogTrace;
        private System.Windows.Forms.SplitContainer splitContainer3;
    }
}