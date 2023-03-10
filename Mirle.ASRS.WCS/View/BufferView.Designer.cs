
namespace Mirle.ASRS.View
{
    partial class BufferView
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

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lblBufferIndex = new System.Windows.Forms.Label();
            this.lblBufferName = new System.Windows.Forms.Label();
            this.lblCommandId = new System.Windows.Forms.Label();
            this.lblCmdMode = new System.Windows.Forms.Label();
            this.lblAuto = new System.Windows.Forms.Label();
            this.BCRReadNotice = new System.Windows.Forms.Label();
            this.AllowWriteCommand = new System.Windows.Forms.Label();
            this.lblPresence = new System.Windows.Forms.Label();
            this.lblOther1 = new System.Windows.Forms.Label();
            this.lblOther2 = new System.Windows.Forms.Label();
            this.WriteCommandComplete = new System.Windows.Forms.Label();
            this.StoreInInfo = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblBufferIndex
            // 
            this.lblBufferIndex.AutoSize = true;
            this.lblBufferIndex.BackColor = System.Drawing.Color.White;
            this.lblBufferIndex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBufferIndex.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBufferIndex.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblBufferIndex.Location = new System.Drawing.Point(0, 0);
            this.lblBufferIndex.Margin = new System.Windows.Forms.Padding(0);
            this.lblBufferIndex.Name = "lblBufferIndex";
            this.lblBufferIndex.Size = new System.Drawing.Size(42, 25);
            this.lblBufferIndex.TabIndex = 1;
            this.lblBufferIndex.Text = "99";
            this.lblBufferIndex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblBufferName
            // 
            this.lblBufferName.AutoSize = true;
            this.lblBufferName.BackColor = System.Drawing.Color.ForestGreen;
            this.lblBufferName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblBufferName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBufferName.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblBufferName.Location = new System.Drawing.Point(42, 0);
            this.lblBufferName.Margin = new System.Windows.Forms.Padding(0);
            this.lblBufferName.Name = "lblBufferName";
            this.lblBufferName.Size = new System.Drawing.Size(65, 25);
            this.lblBufferName.TabIndex = 2;
            this.lblBufferName.Text = "AAAA";
            this.lblBufferName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCommandId
            // 
            this.lblCommandId.AutoSize = true;
            this.lblCommandId.BackColor = System.Drawing.Color.White;
            this.lblCommandId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.lblCommandId, 3);
            this.lblCommandId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommandId.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCommandId.Location = new System.Drawing.Point(0, 0);
            this.lblCommandId.Margin = new System.Windows.Forms.Padding(0);
            this.lblCommandId.Name = "lblCommandId";
            this.lblCommandId.Size = new System.Drawing.Size(78, 18);
            this.lblCommandId.TabIndex = 3;
            this.lblCommandId.Text = "00000";
            this.lblCommandId.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCmdMode
            // 
            this.lblCmdMode.AutoSize = true;
            this.lblCmdMode.BackColor = System.Drawing.Color.White;
            this.lblCmdMode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCmdMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCmdMode.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCmdMode.Location = new System.Drawing.Point(78, 0);
            this.lblCmdMode.Margin = new System.Windows.Forms.Padding(0);
            this.lblCmdMode.Name = "lblCmdMode";
            this.lblCmdMode.Size = new System.Drawing.Size(29, 18);
            this.lblCmdMode.TabIndex = 4;
            this.lblCmdMode.Text = "0";
            this.lblCmdMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAuto
            // 
            this.lblAuto.AutoSize = true;
            this.lblAuto.BackColor = System.Drawing.Color.White;
            this.lblAuto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblAuto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAuto.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblAuto.Location = new System.Drawing.Point(0, 18);
            this.lblAuto.Margin = new System.Windows.Forms.Padding(0);
            this.lblAuto.Name = "lblAuto";
            this.lblAuto.Size = new System.Drawing.Size(26, 18);
            this.lblAuto.TabIndex = 4;
            this.lblAuto.Text = "A";
            this.lblAuto.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblReady
            // 
            this.BCRReadNotice.AutoSize = true;
            this.BCRReadNotice.BackColor = System.Drawing.Color.White;
            this.BCRReadNotice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BCRReadNotice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BCRReadNotice.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.BCRReadNotice.Location = new System.Drawing.Point(26, 18);
            this.BCRReadNotice.Margin = new System.Windows.Forms.Padding(0);
            this.BCRReadNotice.Name = "lblReady";
            this.BCRReadNotice.Size = new System.Drawing.Size(26, 18);
            this.BCRReadNotice.TabIndex = 7;
            this.BCRReadNotice.Text = "0";
            this.BCRReadNotice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPathNotice
            // 
            this.AllowWriteCommand.AutoSize = true;
            this.AllowWriteCommand.BackColor = System.Drawing.Color.White;
            this.AllowWriteCommand.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AllowWriteCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AllowWriteCommand.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.AllowWriteCommand.Location = new System.Drawing.Point(52, 18);
            this.AllowWriteCommand.Margin = new System.Windows.Forms.Padding(0);
            this.AllowWriteCommand.Name = "lblPathNotice";
            this.AllowWriteCommand.Size = new System.Drawing.Size(26, 18);
            this.AllowWriteCommand.TabIndex = 9;
            this.AllowWriteCommand.Text = "0";
            this.AllowWriteCommand.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPresence
            // 
            this.lblPresence.AutoSize = true;
            this.lblPresence.BackColor = System.Drawing.Color.White;
            this.lblPresence.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPresence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPresence.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPresence.Location = new System.Drawing.Point(78, 18);
            this.lblPresence.Margin = new System.Windows.Forms.Padding(0);
            this.lblPresence.Name = "lblPresence";
            this.lblPresence.Size = new System.Drawing.Size(29, 18);
            this.lblPresence.TabIndex = 11;
            this.lblPresence.Text = "0";
            this.lblPresence.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblOther1
            // 
            this.lblOther1.AutoSize = true;
            this.lblOther1.BackColor = System.Drawing.Color.White;
            this.lblOther1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblOther1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOther1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblOther1.Location = new System.Drawing.Point(0, 36);
            this.lblOther1.Margin = new System.Windows.Forms.Padding(0);
            this.lblOther1.Name = "lblOther1";
            this.lblOther1.Size = new System.Drawing.Size(26, 20);
            this.lblOther1.TabIndex = 16;
            this.lblOther1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblOther2
            // 
            this.lblOther2.AutoSize = true;
            this.lblOther2.BackColor = System.Drawing.Color.White;
            this.lblOther2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblOther2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblOther2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblOther2.Location = new System.Drawing.Point(26, 36);
            this.lblOther2.Margin = new System.Windows.Forms.Padding(0);
            this.lblOther2.Name = "lblOther2";
            this.lblOther2.Size = new System.Drawing.Size(26, 20);
            this.lblOther2.TabIndex = 15;
            this.lblOther2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSwitch_Ack
            // 
            this.WriteCommandComplete.AutoSize = true;
            this.WriteCommandComplete.BackColor = System.Drawing.Color.White;
            this.WriteCommandComplete.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WriteCommandComplete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WriteCommandComplete.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.WriteCommandComplete.Location = new System.Drawing.Point(52, 36);
            this.WriteCommandComplete.Margin = new System.Windows.Forms.Padding(0);
            this.WriteCommandComplete.Name = "lblSwitch_Ack";
            this.WriteCommandComplete.Size = new System.Drawing.Size(26, 20);
            this.WriteCommandComplete.TabIndex = 5;
            this.WriteCommandComplete.Text = "0";
            this.WriteCommandComplete.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInitialNotice
            // 
            this.StoreInInfo.AutoSize = true;
            this.StoreInInfo.BackColor = System.Drawing.Color.White;
            this.StoreInInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.StoreInInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StoreInInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.StoreInInfo.Location = new System.Drawing.Point(78, 36);
            this.StoreInInfo.Margin = new System.Windows.Forms.Padding(0);
            this.StoreInInfo.Name = "lblInitialNotice";
            this.StoreInInfo.Size = new System.Drawing.Size(29, 20);
            this.StoreInInfo.TabIndex = 12;
            this.StoreInInfo.Text = "0";
            this.StoreInInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.lblCommandId, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.BCRReadNotice, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.AllowWriteCommand, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.StoreInInfo, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblPresence, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblAuto, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.WriteCommandComplete, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblOther1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblCmdMode, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblOther2, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.MaximumSize = new System.Drawing.Size(107, 56);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(107, 56);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(107, 56);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel2.Controls.Add(this.lblBufferIndex, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblBufferName, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.MaximumSize = new System.Drawing.Size(107, 25);
            this.tableLayoutPanel2.MinimumSize = new System.Drawing.Size(107, 25);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(107, 25);
            this.tableLayoutPanel2.TabIndex = 14;
            // 
            // BufferView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximumSize = new System.Drawing.Size(107, 81);
            this.MinimumSize = new System.Drawing.Size(107, 81);
            this.Name = "BufferView";
            this.Size = new System.Drawing.Size(107, 81);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblBufferIndex;
        private System.Windows.Forms.Label lblBufferName;
        private System.Windows.Forms.Label lblCommandId;
        private System.Windows.Forms.Label lblCmdMode;
        private System.Windows.Forms.Label lblAuto;
        private System.Windows.Forms.Label BCRReadNotice;
        private System.Windows.Forms.Label AllowWriteCommand;
        private System.Windows.Forms.Label lblPresence;
        private System.Windows.Forms.Label lblOther1;
        private System.Windows.Forms.Label lblOther2;
        private System.Windows.Forms.Label WriteCommandComplete;
        private System.Windows.Forms.Label StoreInInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;

    }
}
