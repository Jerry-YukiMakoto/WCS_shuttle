namespace Mirle.ASRS.WCS.View
{
    partial class MainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tlpMainSts = new System.Windows.Forms.TableLayoutPanel();
            this.lblTimer = new System.Windows.Forms.Label();
            this.picMirle = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.chkOnline = new System.Windows.Forms.CheckBox();
            this.lblDBConn = new System.Windows.Forms.Label();
            this.spcView = new System.Windows.Forms.SplitContainer();
            this.spcMainView = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnCrandSpeedMaintain = new System.Windows.Forms.Button();
            this.btnCmdMaintain = new System.Windows.Forms.Button();
            this.btnSendAPITest = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageCmd = new System.Windows.Forms.TabPage();
            this.Grid1 = new System.Windows.Forms.DataGridView();
            this.tabPagePallet = new System.Windows.Forms.TabPage();
            this.Grid2 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tlpMainSts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMirle)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcView)).BeginInit();
            this.spcView.Panel1.SuspendLayout();
            this.spcView.Panel2.SuspendLayout();
            this.spcView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spcMainView)).BeginInit();
            this.spcMainView.Panel2.SuspendLayout();
            this.spcMainView.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageCmd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid1)).BeginInit();
            this.tabPagePallet.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid2)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tlpMainSts);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.spcView);
            this.splitContainer1.Size = new System.Drawing.Size(1171, 599);
            this.splitContainer1.SplitterDistance = 60;
            this.splitContainer1.TabIndex = 0;
            // 
            // tlpMainSts
            // 
            this.tlpMainSts.ColumnCount = 7;
            this.tlpMainSts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpMainSts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpMainSts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpMainSts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpMainSts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpMainSts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpMainSts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tlpMainSts.Controls.Add(this.lblTimer, 0, 0);
            this.tlpMainSts.Controls.Add(this.picMirle, 0, 0);
            this.tlpMainSts.Controls.Add(this.tableLayoutPanel2, 6, 0);
            this.tlpMainSts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMainSts.Location = new System.Drawing.Point(0, 0);
            this.tlpMainSts.Name = "tlpMainSts";
            this.tlpMainSts.RowCount = 1;
            this.tlpMainSts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMainSts.Size = new System.Drawing.Size(1171, 60);
            this.tlpMainSts.TabIndex = 0;
            // 
            // lblTimer
            // 
            this.lblTimer.BackColor = System.Drawing.SystemColors.Control;
            this.lblTimer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTimer.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTimer.ForeColor = System.Drawing.Color.Black;
            this.lblTimer.Location = new System.Drawing.Point(170, 0);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(161, 60);
            this.lblTimer.TabIndex = 268;
            this.lblTimer.Text = "yyyy/MM/dd hh:mm:ss";
            this.lblTimer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picMirle
            // 
            this.picMirle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picMirle.Image = ((System.Drawing.Image)(resources.GetObject("picMirle.Image")));
            this.picMirle.Location = new System.Drawing.Point(3, 3);
            this.picMirle.Name = "picMirle";
            this.picMirle.Size = new System.Drawing.Size(161, 54);
            this.picMirle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picMirle.TabIndex = 267;
            this.picMirle.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.chkOnline, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.lblDBConn, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(1005, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(163, 54);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // chkOnline
            // 
            this.chkOnline.AutoSize = true;
            this.chkOnline.Checked = true;
            this.chkOnline.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOnline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkOnline.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkOnline.Location = new System.Drawing.Point(3, 30);
            this.chkOnline.Name = "chkOnline";
            this.chkOnline.Size = new System.Drawing.Size(157, 21);
            this.chkOnline.TabIndex = 2;
            this.chkOnline.Text = "OnLine";
            this.chkOnline.UseVisualStyleBackColor = true;
            this.chkOnline.Visible = false;
            this.chkOnline.CheckedChanged += new System.EventHandler(this.chkOnline_CheckedChanged);
            // 
            // lblDBConn
            // 
            this.lblDBConn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDBConn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDBConn.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDBConn.Location = new System.Drawing.Point(3, 0);
            this.lblDBConn.Name = "lblDBConn";
            this.lblDBConn.Size = new System.Drawing.Size(157, 27);
            this.lblDBConn.TabIndex = 1;
            this.lblDBConn.Text = "DB Sts";
            this.lblDBConn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // spcView
            // 
            this.spcView.BackColor = System.Drawing.SystemColors.Control;
            this.spcView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcView.Location = new System.Drawing.Point(0, 0);
            this.spcView.Name = "spcView";
            this.spcView.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spcView.Panel1
            // 
            this.spcView.Panel1.Controls.Add(this.spcMainView);
            // 
            // spcView.Panel2
            // 
            this.spcView.Panel2.Controls.Add(this.tabControl1);
            this.spcView.Size = new System.Drawing.Size(1171, 535);
            this.spcView.SplitterDistance = 395;
            this.spcView.TabIndex = 0;
            // 
            // spcMainView
            // 
            this.spcMainView.BackColor = System.Drawing.SystemColors.Control;
            this.spcMainView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.spcMainView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spcMainView.Location = new System.Drawing.Point(0, 0);
            this.spcMainView.Name = "spcMainView";
            // 
            // spcMainView.Panel2
            // 
            this.spcMainView.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.spcMainView.Size = new System.Drawing.Size(1171, 395);
            this.spcMainView.SplitterDistance = 1049;
            this.spcMainView.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.btnCrandSpeedMaintain, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnCmdMaintain, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnSendAPITest, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(116, 393);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnCrandSpeedMaintain
            // 
            this.btnCrandSpeedMaintain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCrandSpeedMaintain.Font = new System.Drawing.Font("Times New Roman", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCrandSpeedMaintain.Location = new System.Drawing.Point(3, 77);
            this.btnCrandSpeedMaintain.Name = "btnCrandSpeedMaintain";
            this.btnCrandSpeedMaintain.Size = new System.Drawing.Size(110, 39);
            this.btnCrandSpeedMaintain.TabIndex = 6;
            this.btnCrandSpeedMaintain.Text = "Crane Speed Maintain";
            this.btnCrandSpeedMaintain.UseVisualStyleBackColor = true;
            this.btnCrandSpeedMaintain.Click += new System.EventHandler(this.btnCrandSpeedMaintain_Click);
            // 
            // btnCmdMaintain
            // 
            this.btnCmdMaintain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCmdMaintain.Font = new System.Drawing.Font("Times New Roman", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCmdMaintain.Location = new System.Drawing.Point(3, 32);
            this.btnCmdMaintain.Name = "btnCmdMaintain";
            this.btnCmdMaintain.Size = new System.Drawing.Size(110, 39);
            this.btnCmdMaintain.TabIndex = 4;
            this.btnCmdMaintain.Text = "Command Maintain";
            this.btnCmdMaintain.UseVisualStyleBackColor = true;
            this.btnCmdMaintain.Click += new System.EventHandler(this.btnCmdMaintain_Click);
            // 
            // btnSendAPITest
            // 
            this.btnSendAPITest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSendAPITest.Font = new System.Drawing.Font("Times New Roman", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSendAPITest.Location = new System.Drawing.Point(3, 3);
            this.btnSendAPITest.Name = "btnSendAPITest";
            this.btnSendAPITest.Size = new System.Drawing.Size(110, 23);
            this.btnSendAPITest.TabIndex = 3;
            this.btnSendAPITest.Text = "Send API Test";
            this.btnSendAPITest.UseVisualStyleBackColor = true;
            this.btnSendAPITest.Visible = false;
            this.btnSendAPITest.Click += new System.EventHandler(this.btnSendAPITest_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageCmd);
            this.tabControl1.Controls.Add(this.tabPagePallet);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1171, 136);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPageCmd
            // 
            this.tabPageCmd.Controls.Add(this.Grid1);
            this.tabPageCmd.Location = new System.Drawing.Point(4, 22);
            this.tabPageCmd.Name = "tabPageCmd";
            this.tabPageCmd.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCmd.Size = new System.Drawing.Size(1163, 110);
            this.tabPageCmd.TabIndex = 0;
            this.tabPageCmd.Text = "即時命令";
            this.tabPageCmd.UseVisualStyleBackColor = true;
            // 
            // Grid1
            // 
            this.Grid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Grid1.Location = new System.Drawing.Point(3, 3);
            this.Grid1.Name = "Grid1";
            this.Grid1.RowHeadersWidth = 62;
            this.Grid1.RowTemplate.Height = 24;
            this.Grid1.Size = new System.Drawing.Size(1157, 104);
            this.Grid1.TabIndex = 0;
            // 
            // tabPagePallet
            // 
            this.tabPagePallet.Controls.Add(this.Grid2);
            this.tabPagePallet.Location = new System.Drawing.Point(4, 22);
            this.tabPagePallet.Name = "tabPagePallet";
            this.tabPagePallet.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePallet.Size = new System.Drawing.Size(1163, 110);
            this.tabPagePallet.TabIndex = 1;
            this.tabPagePallet.Text = "母板相關";
            this.tabPagePallet.UseVisualStyleBackColor = true;
            // 
            // Grid2
            // 
            this.Grid2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Grid2.Location = new System.Drawing.Point(3, 3);
            this.Grid2.Name = "Grid2";
            this.Grid2.RowHeadersWidth = 62;
            this.Grid2.RowTemplate.Height = 24;
            this.Grid2.Size = new System.Drawing.Size(1157, 104);
            this.Grid2.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1171, 599);
            this.Controls.Add(this.splitContainer1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tlpMainSts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picMirle)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.spcView.Panel1.ResumeLayout(false);
            this.spcView.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcView)).EndInit();
            this.spcView.ResumeLayout(false);
            this.spcMainView.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spcMainView)).EndInit();
            this.spcMainView.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageCmd.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Grid1)).EndInit();
            this.tabPagePallet.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Grid2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblDBConn;
        private System.Windows.Forms.SplitContainer spcView;
        private System.Windows.Forms.DataGridView Grid1;
        private System.Windows.Forms.DataGridView Grid2;
        private System.Windows.Forms.CheckBox chkOnline;
        private System.Windows.Forms.SplitContainer spcMainView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tlpMainSts;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.PictureBox picMirle;
        private System.Windows.Forms.Label lblTimer;
        private System.Windows.Forms.Button btnCrandSpeedMaintain;
        private System.Windows.Forms.Button btnCmdMaintain;
        private System.Windows.Forms.Button btnSendAPITest;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageCmd;
        private System.Windows.Forms.TabPage tabPagePallet;
    }
}

