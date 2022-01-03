using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.ASRS.WCS.Controller;

namespace Mirle.ASRS.WCS.View
{
    public partial class MainPanel : Form
    {
        private readonly ControllerReader _host = new ControllerReader();
        public MainPanel()
        {
            InitializeComponent();
        }

        private void MainPanel_Load(object sender, EventArgs e)
        {
            MainForm frm = new MainForm();
            frm.Show();

            #region Mark
            //var child = _host.GetMainView();

            //child.TopLevel = false;
            //child.FormBorderStyle = FormBorderStyle.None;
            //child.Dock = DockStyle.Fill;
            //child.Parent = splitContainer1.Panel1;
            //splitContainer1.Panel1.Controls.Add(child);
            //child.Show();
            #endregion Mark
        }

        private void MainPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void ExitProgram_Click(object sender, EventArgs e)
        {
            _host.AppClosing();

            Dispose();
            Close();
        }

        private void MainPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
            {
                e.Handled = true;
            }
        }
    }
}
