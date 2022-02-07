namespace WCS_API_Client.View
{
    partial class TaskStateUpdateForm
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
            this.txtTaskNo = new System.Windows.Forms.TextBox();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.txtErrMsg = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBusinessType = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPalletNo = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtTaskNo
            // 
            this.txtTaskNo.Location = new System.Drawing.Point(193, 39);
            this.txtTaskNo.Name = "txtTaskNo";
            this.txtTaskNo.Size = new System.Drawing.Size(145, 25);
            this.txtTaskNo.TabIndex = 0;
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(193, 213);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(100, 25);
            this.txtStatus.TabIndex = 1;
            // 
            // txtErrMsg
            // 
            this.txtErrMsg.Location = new System.Drawing.Point(193, 281);
            this.txtErrMsg.Name = "txtErrMsg";
            this.txtErrMsg.Size = new System.Drawing.Size(262, 25);
            this.txtErrMsg.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "TaskNo";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(80, 223);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Status";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(80, 287);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "ErrorMessage";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(83, 362);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Send";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(80, 159);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "BusinessType";
            // 
            // txtBusinessType
            // 
            this.txtBusinessType.Location = new System.Drawing.Point(193, 159);
            this.txtBusinessType.Name = "txtBusinessType";
            this.txtBusinessType.Size = new System.Drawing.Size(100, 25);
            this.txtBusinessType.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(80, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 19);
            this.label5.TabIndex = 9;
            this.label5.Text = "PalletNo";
            // 
            // txtPalletNo
            // 
            this.txtPalletNo.Location = new System.Drawing.Point(193, 102);
            this.txtPalletNo.Name = "txtPalletNo";
            this.txtPalletNo.Size = new System.Drawing.Size(100, 25);
            this.txtPalletNo.TabIndex = 10;
            // 
            // TaskStateUpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtPalletNo);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtBusinessType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtErrMsg);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.txtTaskNo);
            this.Name = "TaskStateUpdateForm";
            this.Text = "TaskStateUpdate";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtTaskNo;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.TextBox txtErrMsg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtBusinessType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtPalletNo;
    }
}