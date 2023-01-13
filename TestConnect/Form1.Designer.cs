
namespace TestConnect
{
    partial class Form1
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
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtConnectionType = new System.Windows.Forms.TextBox();
            this.txtLocalTSAP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblConn = new System.Windows.Forms.Label();
            this.btnDisConnect = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtWordValue = new System.Windows.Forms.TextBox();
            this.txtWordDevice = new System.Windows.Forms.TextBox();
            this.txtWordBitDevice = new System.Windows.Forms.TextBox();
            this.chkBit = new System.Windows.Forms.CheckBox();
            this.btnWriBit = new System.Windows.Forms.Button();
            this.btnWriWord = new System.Windows.Forms.Button();
            this.TraceLog_CV = new System.Windows.Forms.ListBox();
            this.btnReadWord = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cboPLC = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(89, 75);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(184, 29);
            this.txtIP.TabIndex = 0;
            // 
            // txtPort
            // 
            this.txtPort.Enabled = false;
            this.txtPort.Location = new System.Drawing.Point(89, 124);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(184, 29);
            this.txtPort.TabIndex = 1;
            // 
            // txtConnectionType
            // 
            this.txtConnectionType.Enabled = false;
            this.txtConnectionType.Location = new System.Drawing.Point(448, 75);
            this.txtConnectionType.Name = "txtConnectionType";
            this.txtConnectionType.Size = new System.Drawing.Size(184, 29);
            this.txtConnectionType.TabIndex = 2;
            // 
            // txtLocalTSAP
            // 
            this.txtLocalTSAP.Enabled = false;
            this.txtLocalTSAP.Location = new System.Drawing.Point(449, 124);
            this.txtLocalTSAP.Name = "txtLocalTSAP";
            this.txtLocalTSAP.Size = new System.Drawing.Size(184, 29);
            this.txtLocalTSAP.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 18);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(319, 127);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 18);
            this.label2.TabIndex = 5;
            this.label2.Text = "LocalTSAP";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(319, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 18);
            this.label3.TabIndex = 6;
            this.label3.Text = "ConnectionType";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 18);
            this.label4.TabIndex = 7;
            this.label4.Text = "Port";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(26, 180);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(101, 38);
            this.btnConnect.TabIndex = 8;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblConn
            // 
            this.lblConn.BackColor = System.Drawing.Color.Red;
            this.lblConn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblConn.Location = new System.Drawing.Point(139, 189);
            this.lblConn.Name = "lblConn";
            this.lblConn.Size = new System.Drawing.Size(23, 23);
            this.lblConn.TabIndex = 9;
            // 
            // btnDisConnect
            // 
            this.btnDisConnect.Location = new System.Drawing.Point(177, 180);
            this.btnDisConnect.Name = "btnDisConnect";
            this.btnDisConnect.Size = new System.Drawing.Size(96, 38);
            this.btnDisConnect.TabIndex = 10;
            this.btnDisConnect.Text = "Disconnect";
            this.btnDisConnect.UseVisualStyleBackColor = true;
            this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txtWordValue
            // 
            this.txtWordValue.Location = new System.Drawing.Point(449, 187);
            this.txtWordValue.Name = "txtWordValue";
            this.txtWordValue.Size = new System.Drawing.Size(184, 29);
            this.txtWordValue.TabIndex = 11;
            // 
            // txtWordDevice
            // 
            this.txtWordDevice.Location = new System.Drawing.Point(322, 187);
            this.txtWordDevice.Name = "txtWordDevice";
            this.txtWordDevice.Size = new System.Drawing.Size(116, 29);
            this.txtWordDevice.TabIndex = 13;
            // 
            // txtWordBitDevice
            // 
            this.txtWordBitDevice.Location = new System.Drawing.Point(322, 232);
            this.txtWordBitDevice.Name = "txtWordBitDevice";
            this.txtWordBitDevice.Size = new System.Drawing.Size(116, 29);
            this.txtWordBitDevice.TabIndex = 14;
            // 
            // chkBit
            // 
            this.chkBit.AutoSize = true;
            this.chkBit.Location = new System.Drawing.Point(455, 235);
            this.chkBit.Name = "chkBit";
            this.chkBit.Size = new System.Drawing.Size(55, 22);
            this.chkBit.TabIndex = 15;
            this.chkBit.Text = "Bit";
            this.chkBit.UseVisualStyleBackColor = true;
            // 
            // btnWriBit
            // 
            this.btnWriBit.Location = new System.Drawing.Point(160, 232);
            this.btnWriBit.Name = "btnWriBit";
            this.btnWriBit.Size = new System.Drawing.Size(96, 38);
            this.btnWriBit.TabIndex = 16;
            this.btnWriBit.Text = "Write Bit";
            this.btnWriBit.UseVisualStyleBackColor = true;
            this.btnWriBit.Click += new System.EventHandler(this.btnWriBit_Click);
            // 
            // btnWriWord
            // 
            this.btnWriWord.Location = new System.Drawing.Point(26, 232);
            this.btnWriWord.Name = "btnWriWord";
            this.btnWriWord.Size = new System.Drawing.Size(101, 38);
            this.btnWriWord.TabIndex = 17;
            this.btnWriWord.Text = "Write Word";
            this.btnWriWord.UseVisualStyleBackColor = true;
            this.btnWriWord.Click += new System.EventHandler(this.btnWriWord_Click);
            // 
            // TraceLog_CV
            // 
            this.TraceLog_CV.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TraceLog_CV.FormattingEnabled = true;
            this.TraceLog_CV.HorizontalScrollbar = true;
            this.TraceLog_CV.ItemHeight = 22;
            this.TraceLog_CV.Location = new System.Drawing.Point(26, 283);
            this.TraceLog_CV.Margin = new System.Windows.Forms.Padding(4);
            this.TraceLog_CV.Name = "TraceLog_CV";
            this.TraceLog_CV.Size = new System.Drawing.Size(606, 202);
            this.TraceLog_CV.TabIndex = 18;
            // 
            // btnReadWord
            // 
            this.btnReadWord.Location = new System.Drawing.Point(531, 232);
            this.btnReadWord.Name = "btnReadWord";
            this.btnReadWord.Size = new System.Drawing.Size(101, 38);
            this.btnReadWord.TabIndex = 19;
            this.btnReadWord.Text = "Read Word";
            this.btnReadWord.UseVisualStyleBackColor = true;
            this.btnReadWord.Click += new System.EventHandler(this.btnReadWord_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 34);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 18);
            this.label5.TabIndex = 20;
            this.label5.Text = "PLC";
            // 
            // cboPLC
            // 
            this.cboPLC.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPLC.FormattingEnabled = true;
            this.cboPLC.Location = new System.Drawing.Point(89, 30);
            this.cboPLC.Name = "cboPLC";
            this.cboPLC.Size = new System.Drawing.Size(184, 26);
            this.cboPLC.TabIndex = 21;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 574);
            this.Controls.Add(this.cboPLC);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnReadWord);
            this.Controls.Add(this.TraceLog_CV);
            this.Controls.Add(this.btnWriWord);
            this.Controls.Add(this.btnWriBit);
            this.Controls.Add(this.chkBit);
            this.Controls.Add(this.txtWordBitDevice);
            this.Controls.Add(this.txtWordDevice);
            this.Controls.Add(this.txtWordValue);
            this.Controls.Add(this.btnDisConnect);
            this.Controls.Add(this.lblConn);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLocalTSAP);
            this.Controls.Add(this.txtConnectionType);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtIP);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtConnectionType;
        private System.Windows.Forms.TextBox txtLocalTSAP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblConn;
        private System.Windows.Forms.Button btnDisConnect;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtWordValue;
        private System.Windows.Forms.TextBox txtWordDevice;
        private System.Windows.Forms.TextBox txtWordBitDevice;
        private System.Windows.Forms.CheckBox chkBit;
        private System.Windows.Forms.Button btnWriBit;
        private System.Windows.Forms.Button btnWriWord;
        private System.Windows.Forms.ListBox TraceLog_CV;
        private System.Windows.Forms.Button btnReadWord;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboPLC;
    }
}

