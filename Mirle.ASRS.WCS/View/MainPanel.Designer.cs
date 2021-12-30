
namespace Mirle.ASRS.WCS.View
{
    partial class MainPanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainPanel));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.butExitProgram = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.butExitProgram);
            this.splitContainer1.Panel2MinSize = 35;
            this.splitContainer1.Size = new System.Drawing.Size(1584, 861);
            this.splitContainer1.SplitterDistance = 822;
            this.splitContainer1.TabIndex = 0;
            // 
            // butExitProgram
            // 
            this.butExitProgram.Dock = System.Windows.Forms.DockStyle.Right;
            this.butExitProgram.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.butExitProgram.Image = ((System.Drawing.Image)(resources.GetObject("butExitProgram.Image")));
            this.butExitProgram.Location = new System.Drawing.Point(1484, 0);
            this.butExitProgram.Margin = new System.Windows.Forms.Padding(0);
            this.butExitProgram.Name = "butExitProgram";
            this.butExitProgram.Size = new System.Drawing.Size(100, 35);
            this.butExitProgram.TabIndex = 3;
            this.butExitProgram.Text = " &Exit";
            this.butExitProgram.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.butExitProgram.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butExitProgram.UseVisualStyleBackColor = true;
            this.butExitProgram.Click += new System.EventHandler(this.ExitProgram_Click);
            // 
            // MainPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1584, 861);
            this.Controls.Add(this.splitContainer1);
            this.MaximumSize = new System.Drawing.Size(1600, 900);
            this.MinimumSize = new System.Drawing.Size(1600, 900);
            this.Name = "MainPanel";
            this.Text = "Mirle Automated Warehouse Control System";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainPanel_FormClosing);
            this.Load += new System.EventHandler(this.MainPanel_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainPanel_KeyDown);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button butExitProgram;
    }
}