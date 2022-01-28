namespace WCS_API_Client.View
{
    partial class MainTestForm
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
            this.btnTaskStsUpdate = new System.Windows.Forms.Button();
            this.btnStackPalletsIn = new System.Windows.Forms.Button();
            this.btnStackPalletsOut = new System.Windows.Forms.Button();
            this.btnDisplay = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnTaskStsUpdate
            // 
            this.btnTaskStsUpdate.Location = new System.Drawing.Point(65, 39);
            this.btnTaskStsUpdate.Name = "btnTaskStsUpdate";
            this.btnTaskStsUpdate.Size = new System.Drawing.Size(144, 23);
            this.btnTaskStsUpdate.TabIndex = 0;
            this.btnTaskStsUpdate.Text = "TaskStateUpdate";
            this.btnTaskStsUpdate.UseVisualStyleBackColor = true;
            this.btnTaskStsUpdate.Click += new System.EventHandler(this.btnTaskStsUpdate_Click);
            // 
            // btnStackPalletsIn
            // 
            this.btnStackPalletsIn.Location = new System.Drawing.Point(65, 102);
            this.btnStackPalletsIn.Name = "btnStackPalletsIn";
            this.btnStackPalletsIn.Size = new System.Drawing.Size(144, 23);
            this.btnStackPalletsIn.TabIndex = 1;
            this.btnStackPalletsIn.Text = "StackPalletsIn";
            this.btnStackPalletsIn.UseVisualStyleBackColor = true;
            this.btnStackPalletsIn.Click += new System.EventHandler(this.btnStackPalletsIn_Click);
            // 
            // btnStackPalletsOut
            // 
            this.btnStackPalletsOut.Location = new System.Drawing.Point(65, 169);
            this.btnStackPalletsOut.Name = "btnStackPalletsOut";
            this.btnStackPalletsOut.Size = new System.Drawing.Size(144, 23);
            this.btnStackPalletsOut.TabIndex = 2;
            this.btnStackPalletsOut.Text = "StackPalletsOut";
            this.btnStackPalletsOut.UseVisualStyleBackColor = true;
            this.btnStackPalletsOut.Click += new System.EventHandler(this.btnStackPalletsOut_Click);
            // 
            // btnDisplay
            // 
            this.btnDisplay.Location = new System.Drawing.Point(65, 238);
            this.btnDisplay.Name = "btnDisplay";
            this.btnDisplay.Size = new System.Drawing.Size(144, 23);
            this.btnDisplay.TabIndex = 3;
            this.btnDisplay.Text = "DisplayTaskStatus";
            this.btnDisplay.UseVisualStyleBackColor = true;
            this.btnDisplay.Click += new System.EventHandler(this.btnDisplay_Click);
            // 
            // MainTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnDisplay);
            this.Controls.Add(this.btnStackPalletsOut);
            this.Controls.Add(this.btnStackPalletsIn);
            this.Controls.Add(this.btnTaskStsUpdate);
            this.Name = "MainTestForm";
            this.Text = "MainTestForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTaskStsUpdate;
        private System.Windows.Forms.Button btnStackPalletsIn;
        private System.Windows.Forms.Button btnStackPalletsOut;
        private System.Windows.Forms.Button btnDisplay;
    }
}