using System.Data;
using Mirle.Def;
using System.Linq;
using WCS_API_Client.Functions;
using WCS_API_Client.ReportInfo;
using System.Windows.Forms;

namespace WCS_API_Client.View
{
    public partial class StackPalletsInForm : Form
    {
        
        private StackPalletsIn stackPalletsIn;
        public StackPalletsInForm(WebApiConfig Config)
        {
            stackPalletsIn = new StackPalletsIn(Config);
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            button1.Enabled = false;
            StackPalletsInInfo info = new StackPalletsInInfo
            {
                lineId = txtLineID.Text,
                locationFrom = txtLocationFrom.Text,
            };
            stackPalletsIn.FunReport(info);
            button1.Enabled = true;
        }
    }
}
