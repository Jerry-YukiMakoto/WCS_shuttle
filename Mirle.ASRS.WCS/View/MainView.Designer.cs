namespace Mirle.ASRS.View
{
    partial class MainView
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
            this.timerMainProc = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.level_Signal10 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal9 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal8 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal7 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal6 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal5 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal4 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal3 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal2 = new Mirle.ASRS.View.Level_Signal();
            this.level_Signal1 = new Mirle.ASRS.View.Level_Signal();
            this.lifterView1 = new Mirle.ASRS.View.LifterView();
            this.lbl_1F = new System.Windows.Forms.Label();
            this.A1 = new Mirle.ASRS.View.BufferView();
            this.A2 = new Mirle.ASRS.View.BufferView();
            this.A3 = new Mirle.ASRS.View.BufferView();
            this.A4 = new Mirle.ASRS.View.BufferView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.testbutton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPLCConnSts = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerMainProc
            // 
            this.timerMainProc.Interval = 500;
            this.timerMainProc.Tick += new System.EventHandler(this.timerMainProc_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal10);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal9);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal8);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal7);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal6);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal5);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal4);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal3);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal2);
            this.splitContainer1.Panel1.Controls.Add(this.level_Signal1);
            this.splitContainer1.Panel1.Controls.Add(this.lifterView1);
            this.splitContainer1.Panel1.Controls.Add(this.lbl_1F);
            this.splitContainer1.Panel1.Controls.Add(this.A1);
            this.splitContainer1.Panel1.Controls.Add(this.A2);
            this.splitContainer1.Panel1.Controls.Add(this.A3);
            this.splitContainer1.Panel1.Controls.Add(this.A4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(1316, 788);
            this.splitContainer1.SplitterDistance = 1101;
            this.splitContainer1.SplitterWidth = 7;
            this.splitContainer1.TabIndex = 0;
            // 
            // level_Signal10
            // 
            this.level_Signal10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal10.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal10.Location = new System.Drawing.Point(911, 227);
            this.level_Signal10.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal10.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal10.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal10.Name = "level_Signal10";
            this.level_Signal10.Size = new System.Drawing.Size(120, 97);
            this.level_Signal10.TabIndex = 18;
            // 
            // level_Signal9
            // 
            this.level_Signal9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal9.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal9.Location = new System.Drawing.Point(911, 320);
            this.level_Signal9.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal9.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal9.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal9.Name = "level_Signal9";
            this.level_Signal9.Size = new System.Drawing.Size(120, 97);
            this.level_Signal9.TabIndex = 17;
            // 
            // level_Signal8
            // 
            this.level_Signal8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal8.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal8.Location = new System.Drawing.Point(911, 415);
            this.level_Signal8.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal8.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal8.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal8.Name = "level_Signal8";
            this.level_Signal8.Size = new System.Drawing.Size(120, 97);
            this.level_Signal8.TabIndex = 16;
            // 
            // level_Signal7
            // 
            this.level_Signal7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal7.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal7.Location = new System.Drawing.Point(911, 511);
            this.level_Signal7.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal7.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal7.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal7.Name = "level_Signal7";
            this.level_Signal7.Size = new System.Drawing.Size(120, 97);
            this.level_Signal7.TabIndex = 15;
            // 
            // level_Signal6
            // 
            this.level_Signal6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal6.Location = new System.Drawing.Point(911, 608);
            this.level_Signal6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal6.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal6.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal6.Name = "level_Signal6";
            this.level_Signal6.Size = new System.Drawing.Size(120, 97);
            this.level_Signal6.TabIndex = 14;
            // 
            // level_Signal5
            // 
            this.level_Signal5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal5.Location = new System.Drawing.Point(644, 227);
            this.level_Signal5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal5.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal5.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal5.Name = "level_Signal5";
            this.level_Signal5.Size = new System.Drawing.Size(120, 97);
            this.level_Signal5.TabIndex = 13;
            // 
            // level_Signal4
            // 
            this.level_Signal4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal4.Location = new System.Drawing.Point(644, 320);
            this.level_Signal4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal4.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal4.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal4.Name = "level_Signal4";
            this.level_Signal4.Size = new System.Drawing.Size(120, 97);
            this.level_Signal4.TabIndex = 12;
            // 
            // level_Signal3
            // 
            this.level_Signal3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal3.Location = new System.Drawing.Point(644, 415);
            this.level_Signal3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal3.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal3.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal3.Name = "level_Signal3";
            this.level_Signal3.Size = new System.Drawing.Size(120, 97);
            this.level_Signal3.TabIndex = 11;
            // 
            // level_Signal2
            // 
            this.level_Signal2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal2.Location = new System.Drawing.Point(644, 511);
            this.level_Signal2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal2.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal2.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal2.Name = "level_Signal2";
            this.level_Signal2.Size = new System.Drawing.Size(120, 97);
            this.level_Signal2.TabIndex = 10;
            // 
            // level_Signal1
            // 
            this.level_Signal1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.level_Signal1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.level_Signal1.Location = new System.Drawing.Point(644, 608);
            this.level_Signal1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.level_Signal1.MaximumSize = new System.Drawing.Size(120, 97);
            this.level_Signal1.MinimumSize = new System.Drawing.Size(120, 97);
            this.level_Signal1.Name = "level_Signal1";
            this.level_Signal1.Size = new System.Drawing.Size(120, 97);
            this.level_Signal1.TabIndex = 9;
            // 
            // lifterView1
            // 
            this.lifterView1.Location = new System.Drawing.Point(781, 108);
            this.lifterView1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lifterView1.MaximumSize = new System.Drawing.Size(120, 97);
            this.lifterView1.MinimumSize = new System.Drawing.Size(120, 97);
            this.lifterView1.Name = "lifterView1";
            this.lifterView1.Size = new System.Drawing.Size(120, 97);
            this.lifterView1.TabIndex = 8;
            // 
            // lbl_1F
            // 
            this.lbl_1F.AutoSize = true;
            this.lbl_1F.Font = new System.Drawing.Font("Times New Roman", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_1F.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbl_1F.Location = new System.Drawing.Point(190, 431);
            this.lbl_1F.Name = "lbl_1F";
            this.lbl_1F.Size = new System.Drawing.Size(54, 37);
            this.lbl_1F.TabIndex = 7;
            this.lbl_1F.Text = "1F";
            // 
            // A1
            // 
            this.A1.BufferIndex = 1;
            this.A1.Location = new System.Drawing.Point(258, 482);
            this.A1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.A1.MaximumSize = new System.Drawing.Size(120, 97);
            this.A1.MinimumSize = new System.Drawing.Size(120, 97);
            this.A1.Name = "A1";
            this.A1.Size = new System.Drawing.Size(120, 97);
            this.A1.TabIndex = 1;
            // 
            // A2
            // 
            this.A2.BufferIndex = 2;
            this.A2.Location = new System.Drawing.Point(386, 482);
            this.A2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.A2.MaximumSize = new System.Drawing.Size(120, 97);
            this.A2.MinimumSize = new System.Drawing.Size(120, 97);
            this.A2.Name = "A2";
            this.A2.Size = new System.Drawing.Size(120, 97);
            this.A2.TabIndex = 2;
            // 
            // A3
            // 
            this.A3.BufferIndex = 3;
            this.A3.Location = new System.Drawing.Point(258, 589);
            this.A3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.A3.MaximumSize = new System.Drawing.Size(120, 97);
            this.A3.MinimumSize = new System.Drawing.Size(120, 97);
            this.A3.Name = "A3";
            this.A3.Size = new System.Drawing.Size(120, 97);
            this.A3.TabIndex = 0;
            // 
            // A4
            // 
            this.A4.BufferIndex = 4;
            this.A4.Location = new System.Drawing.Point(386, 589);
            this.A4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.A4.MaximumSize = new System.Drawing.Size(120, 97);
            this.A4.MinimumSize = new System.Drawing.Size(120, 97);
            this.A4.Name = "A4";
            this.A4.Size = new System.Drawing.Size(120, 97);
            this.A4.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.testbutton, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblPLCConnSts, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 6F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(206, 786);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // testbutton
            // 
            this.testbutton.AutoSize = true;
            this.testbutton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testbutton.Font = new System.Drawing.Font("Times New Roman", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testbutton.Location = new System.Drawing.Point(4, 147);
            this.testbutton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.testbutton.Name = "testbutton";
            this.testbutton.Size = new System.Drawing.Size(198, 82);
            this.testbutton.TabIndex = 14;
            this.testbutton.Text = "輸送機初始";
            this.testbutton.UseVisualStyleBackColor = true;
            this.testbutton.Click += new System.EventHandler(this.buffer_Restart);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 47);
            this.label1.TabIndex = 13;
            this.label1.Text = "PLC Status";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPLCConnSts
            // 
            this.lblPLCConnSts.BackColor = System.Drawing.Color.Red;
            this.lblPLCConnSts.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPLCConnSts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPLCConnSts.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPLCConnSts.ForeColor = System.Drawing.Color.Black;
            this.lblPLCConnSts.Location = new System.Drawing.Point(2, 49);
            this.lblPLCConnSts.Margin = new System.Windows.Forms.Padding(2);
            this.lblPLCConnSts.Name = "lblPLCConnSts";
            this.lblPLCConnSts.Size = new System.Drawing.Size(202, 90);
            this.lblPLCConnSts.TabIndex = 12;
            this.lblPLCConnSts.Text = "Connect Status";
            this.lblPLCConnSts.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label2.Location = new System.Drawing.Point(788, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 37);
            this.label2.TabIndex = 19;
            this.label2.Text = "Lifter";
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1316, 788);
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainView";
            this.Text = "后科倉儲系統";
            this.Load += new System.EventHandler(this.MainView_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Timer timerMainProc;
        private System.Windows.Forms.Label lblPLCConnSts;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private BufferView A1;
        private BufferView A2;
        private BufferView A3;
        private BufferView A4;
        private System.Windows.Forms.Label lbl_1F;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button testbutton;
        private LifterView lifterView1;
        private Level_Signal level_Signal10;
        private Level_Signal level_Signal9;
        private Level_Signal level_Signal8;
        private Level_Signal level_Signal7;
        private Level_Signal level_Signal6;
        private Level_Signal level_Signal5;
        private Level_Signal level_Signal4;
        private Level_Signal level_Signal3;
        private Level_Signal level_Signal2;
        private Level_Signal level_Signal1;
        private System.Windows.Forms.Label label2;
    }
}