namespace Mirle.ASRS.Conveyors.U0NXMA30.View
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblPLCConnSts = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.A1 = new Mirle.ASRS.Conveyors.U0NXMA30.View.BufferView();
            this.A2 = new Mirle.ASRS.Conveyors.U0NXMA30.View.BufferView();
            this.A3 = new Mirle.ASRS.Conveyors.U0NXMA30.View.BufferView();
            this.A4 = new Mirle.ASRS.Conveyors.U0NXMA30.View.BufferView();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timerMainProc
            // 
            this.timerMainProc.Interval = 500;
            this.timerMainProc.Tick += new System.EventHandler(this.timerMainProc_Tick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblPLCConnSts, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.75F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.75F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 62.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(157, 606);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblPLCConnSts
            // 
            this.lblPLCConnSts.BackColor = System.Drawing.Color.Red;
            this.lblPLCConnSts.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPLCConnSts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPLCConnSts.Font = new System.Drawing.Font("Times New Roman", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPLCConnSts.ForeColor = System.Drawing.Color.Black;
            this.lblPLCConnSts.Location = new System.Drawing.Point(2, 115);
            this.lblPLCConnSts.Margin = new System.Windows.Forms.Padding(2);
            this.lblPLCConnSts.Name = "lblPLCConnSts";
            this.lblPLCConnSts.Size = new System.Drawing.Size(153, 109);
            this.lblPLCConnSts.TabIndex = 12;
            this.lblPLCConnSts.Text = "PLC Connect Status";
            this.lblPLCConnSts.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Panel1.Controls.Add(this.A1);
            this.splitContainer1.Panel1.Controls.Add(this.A2);
            this.splitContainer1.Panel1.Controls.Add(this.A3);
            this.splitContainer1.Panel1.Controls.Add(this.A4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.splitContainer1.Size = new System.Drawing.Size(1218, 608);
            this.splitContainer1.SplitterDistance = 1030;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 0;
            // 
            // A1
            // 
            this.A1.BufferIndex = 1;
            this.A1.Location = new System.Drawing.Point(227, 36);
            this.A1.Margin = new System.Windows.Forms.Padding(4);
            this.A1.MaximumSize = new System.Drawing.Size(107, 81);
            this.A1.MinimumSize = new System.Drawing.Size(107, 81);
            this.A1.Name = "A1";
            this.A1.Size = new System.Drawing.Size(107, 81);
            this.A1.TabIndex = 1;
            // 
            // A2
            // 
            this.A2.BufferIndex = 2;
            this.A2.Location = new System.Drawing.Point(227, 125);
            this.A2.Margin = new System.Windows.Forms.Padding(4);
            this.A2.MaximumSize = new System.Drawing.Size(107, 81);
            this.A2.MinimumSize = new System.Drawing.Size(107, 81);
            this.A2.Name = "A2";
            this.A2.Size = new System.Drawing.Size(107, 81);
            this.A2.TabIndex = 2;
            // 
            // A3
            // 
            this.A3.BufferIndex = 3;
            this.A3.Location = new System.Drawing.Point(227, 214);
            this.A3.Margin = new System.Windows.Forms.Padding(4);
            this.A3.MaximumSize = new System.Drawing.Size(107, 81);
            this.A3.MinimumSize = new System.Drawing.Size(107, 81);
            this.A3.Name = "A3";
            this.A3.Size = new System.Drawing.Size(107, 81);
            this.A3.TabIndex = 0;
            // 
            // A4
            // 
            this.A4.BufferIndex = 4;
            this.A4.Location = new System.Drawing.Point(112, 125);
            this.A4.Margin = new System.Windows.Forms.Padding(4);
            this.A4.MaximumSize = new System.Drawing.Size(107, 81);
            this.A4.MinimumSize = new System.Drawing.Size(107, 81);
            this.A4.Name = "A4";
            this.A4.Size = new System.Drawing.Size(107, 81);
            this.A4.TabIndex = 0;
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1218, 608);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MainView";
            this.Text = "MainView";
            this.Load += new System.EventHandler(this.MainView_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
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
    }
}