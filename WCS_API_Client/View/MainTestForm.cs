using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Def;

namespace WCS_API_Client.View
{
    public partial class MainTestForm : Form
    {
        private WebApiConfig _config = new WebApiConfig();
        public MainTestForm(WebApiConfig config)
        {
            _config = config;
            InitializeComponent();
        }

        private TaskStateUpdateForm taskStateUpdate;
        private void btnTaskStsUpdate_Click(object sender, EventArgs e)
        {
            if(taskStateUpdate == null)
            {
                taskStateUpdate = new TaskStateUpdateForm(_config);
                taskStateUpdate.TopMost = true;
                taskStateUpdate.FormClosed += new FormClosedEventHandler(funTaskStatusUpdate_FormClosed);
                taskStateUpdate.Show();
            }
            else
            {
                taskStateUpdate.BringToFront();
            }
        }

        private void funTaskStatusUpdate_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (taskStateUpdate != null)
                taskStateUpdate = null;
        }

        private StackPalletsInForm StackPalletsIn;
        private void btnStackPalletsIn_Click(object sender, EventArgs e)
        {
            if (StackPalletsIn == null)
            {
                StackPalletsIn = new StackPalletsInForm(_config);
                StackPalletsIn.TopMost = true;
                StackPalletsIn.FormClosed += new FormClosedEventHandler(funStackPalletsIn_FormClosed);
            }
            else
            {
                StackPalletsIn.BringToFront();
            }
        }

        private void funStackPalletsIn_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (StackPalletsIn != null)
                StackPalletsIn = null;
        }

        private StackPalletsOutForm StackPalletsOut;
        private void btnStackPalletsOut_Click(object sender, EventArgs e)
        {
            if (StackPalletsOut == null)
            {
                StackPalletsOut = new StackPalletsOutForm(_config);
                StackPalletsOut.TopMost = true;
                StackPalletsOut.FormClosed += new FormClosedEventHandler(funStackPalletsOut_FormClosed);
            }
            else
            {
                StackPalletsOut.BringToFront();
            }
        }

        private void funStackPalletsOut_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (StackPalletsOut != null)
                StackPalletsOut = null;
        }

        private DisplayTaskStatusForm DisplayTaskStatus;
        private void btnDisplay_Click(object sender, EventArgs e)
        {
            if (DisplayTaskStatus == null)
            {
                DisplayTaskStatus = new DisplayTaskStatusForm(_config);
                DisplayTaskStatus.TopMost = true;
                DisplayTaskStatus.FormClosed += new FormClosedEventHandler(funDisplayTaskStatust_FormClosed);
            }
            else
            {
                DisplayTaskStatus.BringToFront();
            }
        }

        private void funDisplayTaskStatust_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DisplayTaskStatus != null)
                DisplayTaskStatus = null;
        }

    }
}
