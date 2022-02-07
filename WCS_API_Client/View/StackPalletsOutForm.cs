using System.Data;
using Mirle.Def;
using System.Linq;
using WCS_API_Client.Functions;
using WCS_API_Client.ReportInfo;
using System.Windows.Forms;

namespace WCS_API_Client.View
{
    public partial class StackPalletsOutForm : Form
    {
        
        private StackPalletsOut stackPalletsOut;
        public StackPalletsOutForm(WebApiConfig Config)
        {
            stackPalletsOut = new StackPalletsOut(Config);
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            button1.Enabled = false;
            StackPalletsOutInfo info = new StackPalletsOutInfo
            {
                lineId = txtLineID.Text,
                locationTo = txtLocationTo.Text,
            };
            stackPalletsOut.FunReport(info);
            button1.Enabled = true;
        }
    }
}
