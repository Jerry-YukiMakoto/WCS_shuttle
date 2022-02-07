using System.Data;
using Mirle.Def;
using System.Linq;
using WCS_API_Client.Functions;
using WCS_API_Client.ReportInfo;
using System.Windows.Forms;


namespace WCS_API_Client.View
{
    public partial class DisplayTaskStatusForm : Form
    {
        private DisplayTaskStatus displayTaskStatus;
        public DisplayTaskStatusForm(WebApiConfig Config)
        {

            displayTaskStatus = new DisplayTaskStatus(Config);
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            button1.Enabled = false;
            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
            {
                lineId = txtLineID.Text,
                locationID = txtLocationID.Text,
                taskNo = txtTaskNo.Text,
                state = txtState.Text,
            };
            displayTaskStatus.FunReport(info);
            button1.Enabled = true;
        }
    }
}
