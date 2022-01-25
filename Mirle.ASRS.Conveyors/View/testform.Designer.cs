
namespace Mirle.ASRS.WCS.Controller
{
    partial class Form1
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
            this.test = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.CMD = new System.Windows.Forms.CheckBox();
            this.mode = new System.Windows.Forms.CheckBox();
            this.Auto = new System.Windows.Forms.CheckBox();
            this.initialnotice = new System.Windows.Forms.CheckBox();
            this.presence = new System.Windows.Forms.CheckBox();
            this.ready = new System.Windows.Forms.CheckBox();
            this.path = new System.Windows.Forms.CheckBox();
            this.switchmode = new System.Windows.Forms.CheckBox();
            this.bufferindex = new System.Windows.Forms.Label();
            this.content = new System.Windows.Forms.Label();
            this.Outmode = new System.Windows.Forms.CheckBox();
            this.Inmode = new System.Windows.Forms.CheckBox();
            this.error = new System.Windows.Forms.CheckBox();
            this.emptynumber = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // test
            // 
            this.test.Location = new System.Drawing.Point(67, 47);
            this.test.Name = "test";
            this.test.Size = new System.Drawing.Size(104, 77);
            this.test.TabIndex = 0;
            this.test.Text = "寫入點位";
            this.test.UseVisualStyleBackColor = true;
            this.test.Click += new System.EventHandler(this.buttonwrite);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(517, 131);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(164, 48);
            this.textBox1.TabIndex = 1;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(517, 283);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(164, 48);
            this.textBox2.TabIndex = 2;
            // 
            // CMD
            // 
            this.CMD.AutoSize = true;
            this.CMD.Checked = true;
            this.CMD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CMD.Location = new System.Drawing.Point(237, 75);
            this.CMD.Name = "CMD";
            this.CMD.Size = new System.Drawing.Size(72, 22);
            this.CMD.TabIndex = 3;
            this.CMD.Text = "CMD";
            this.CMD.UseVisualStyleBackColor = true;
            // 
            // mode
            // 
            this.mode.AutoSize = true;
            this.mode.Checked = true;
            this.mode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mode.Location = new System.Drawing.Point(237, 133);
            this.mode.Name = "mode";
            this.mode.Size = new System.Drawing.Size(71, 22);
            this.mode.TabIndex = 6;
            this.mode.Text = "mode";
            this.mode.UseVisualStyleBackColor = true;
            // 
            // Auto
            // 
            this.Auto.AutoSize = true;
            this.Auto.Checked = true;
            this.Auto.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Auto.Location = new System.Drawing.Point(237, 189);
            this.Auto.Name = "Auto";
            this.Auto.Size = new System.Drawing.Size(67, 22);
            this.Auto.TabIndex = 7;
            this.Auto.Text = "Auto";
            this.Auto.UseVisualStyleBackColor = true;
            // 
            // initialnotice
            // 
            this.initialnotice.AutoSize = true;
            this.initialnotice.Checked = true;
            this.initialnotice.CheckState = System.Windows.Forms.CheckState.Checked;
            this.initialnotice.Location = new System.Drawing.Point(237, 240);
            this.initialnotice.Name = "initialnotice";
            this.initialnotice.Size = new System.Drawing.Size(117, 22);
            this.initialnotice.TabIndex = 8;
            this.initialnotice.Text = "initialnotice";
            this.initialnotice.UseVisualStyleBackColor = true;
            // 
            // presence
            // 
            this.presence.AutoSize = true;
            this.presence.Checked = true;
            this.presence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.presence.Location = new System.Drawing.Point(237, 298);
            this.presence.Name = "presence";
            this.presence.Size = new System.Drawing.Size(95, 22);
            this.presence.TabIndex = 9;
            this.presence.Text = "presence";
            this.presence.UseVisualStyleBackColor = true;
            // 
            // ready
            // 
            this.ready.AutoSize = true;
            this.ready.Checked = true;
            this.ready.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ready.Location = new System.Drawing.Point(237, 351);
            this.ready.Name = "ready";
            this.ready.Size = new System.Drawing.Size(72, 22);
            this.ready.TabIndex = 10;
            this.ready.Text = "ready";
            this.ready.UseVisualStyleBackColor = true;
            // 
            // path
            // 
            this.path.AutoSize = true;
            this.path.Checked = true;
            this.path.CheckState = System.Windows.Forms.CheckState.Checked;
            this.path.Location = new System.Drawing.Point(237, 411);
            this.path.Name = "path";
            this.path.Size = new System.Drawing.Size(63, 22);
            this.path.TabIndex = 11;
            this.path.Text = "path";
            this.path.UseVisualStyleBackColor = true;
            // 
            // switchmode
            // 
            this.switchmode.AutoSize = true;
            this.switchmode.Checked = true;
            this.switchmode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.switchmode.Location = new System.Drawing.Point(237, 469);
            this.switchmode.Name = "switchmode";
            this.switchmode.Size = new System.Drawing.Size(116, 22);
            this.switchmode.TabIndex = 12;
            this.switchmode.Text = "switchmode";
            this.switchmode.UseVisualStyleBackColor = true;
            // 
            // bufferindex
            // 
            this.bufferindex.Font = new System.Drawing.Font("新細明體", 15F);
            this.bufferindex.Location = new System.Drawing.Point(512, 66);
            this.bufferindex.Name = "bufferindex";
            this.bufferindex.Size = new System.Drawing.Size(155, 50);
            this.bufferindex.TabIndex = 13;
            this.bufferindex.Text = "bufferindex";
            // 
            // content
            // 
            this.content.AutoSize = true;
            this.content.Font = new System.Drawing.Font("新細明體", 15F);
            this.content.Location = new System.Drawing.Point(512, 235);
            this.content.Name = "content";
            this.content.Size = new System.Drawing.Size(103, 30);
            this.content.TabIndex = 14;
            this.content.Text = "輸入值";
            // 
            // Outmode
            // 
            this.Outmode.AutoSize = true;
            this.Outmode.Checked = true;
            this.Outmode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Outmode.Location = new System.Drawing.Point(237, 564);
            this.Outmode.Name = "Outmode";
            this.Outmode.Size = new System.Drawing.Size(96, 22);
            this.Outmode.TabIndex = 15;
            this.Outmode.Text = "Outmode";
            this.Outmode.UseVisualStyleBackColor = true;
            // 
            // Inmode
            // 
            this.Inmode.AutoSize = true;
            this.Inmode.Checked = true;
            this.Inmode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Inmode.Location = new System.Drawing.Point(237, 511);
            this.Inmode.Name = "Inmode";
            this.Inmode.Size = new System.Drawing.Size(85, 22);
            this.Inmode.TabIndex = 16;
            this.Inmode.Text = "Inmode";
            this.Inmode.UseVisualStyleBackColor = true;
            // 
            // error
            // 
            this.error.AutoSize = true;
            this.error.Checked = true;
            this.error.CheckState = System.Windows.Forms.CheckState.Checked;
            this.error.Location = new System.Drawing.Point(415, 469);
            this.error.Name = "error";
            this.error.Size = new System.Drawing.Size(68, 22);
            this.error.TabIndex = 17;
            this.error.Text = "error";
            this.error.UseVisualStyleBackColor = true;
            // 
            // emptynumber
            // 
            this.emptynumber.AutoSize = true;
            this.emptynumber.Checked = true;
            this.emptynumber.CheckState = System.Windows.Forms.CheckState.Checked;
            this.emptynumber.Location = new System.Drawing.Point(415, 520);
            this.emptynumber.Name = "emptynumber";
            this.emptynumber.Size = new System.Drawing.Size(191, 33);
            this.emptynumber.TabIndex = 18;
            this.emptynumber.Text = "emptynumber";
            this.emptynumber.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(945, 660);
            this.Controls.Add(this.emptynumber);
            this.Controls.Add(this.error);
            this.Controls.Add(this.Inmode);
            this.Controls.Add(this.Outmode);
            this.Controls.Add(this.content);
            this.Controls.Add(this.bufferindex);
            this.Controls.Add(this.switchmode);
            this.Controls.Add(this.path);
            this.Controls.Add(this.ready);
            this.Controls.Add(this.presence);
            this.Controls.Add(this.initialnotice);
            this.Controls.Add(this.Auto);
            this.Controls.Add(this.mode);
            this.Controls.Add(this.CMD);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.test);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.SimMainView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button test;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckBox CMD;
        private System.Windows.Forms.CheckBox mode;
        private System.Windows.Forms.CheckBox Auto;
        private System.Windows.Forms.CheckBox initialnotice;
        private System.Windows.Forms.CheckBox presence;
        private System.Windows.Forms.CheckBox ready;
        private System.Windows.Forms.CheckBox path;
        private System.Windows.Forms.CheckBox switchmode;
        private System.Windows.Forms.Label bufferindex;
        private System.Windows.Forms.Label content;
        private System.Windows.Forms.CheckBox Outmode;
        private System.Windows.Forms.CheckBox Inmode;
        private System.Windows.Forms.CheckBox error;
        private System.Windows.Forms.CheckBox emptynumber;
    }
}