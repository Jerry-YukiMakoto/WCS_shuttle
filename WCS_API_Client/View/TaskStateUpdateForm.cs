using System.Data;
using Mirle.Def;
using System.Linq;
using WCS_API_Client.Functions;
using WCS_API_Client.ReportInfo;
using System.Windows.Forms;

namespace WCS_API_Client.View
{
    public partial class TaskStateUpdateForm : Form
    {
        
        private TaskStateUpdate taskStateUpdate;
        public TaskStateUpdateForm(WebApiConfig Config)
        {
           
            taskStateUpdate = new TaskStateUpdate(Config);
            InitializeComponent();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            button1.Enabled = false;
            TaskStateUpdateInfo info = new TaskStateUpdateInfo
            {
                taskNo = txtTaskNo.Text,
                state = txtStatus.Text,
                errMsg = txtErrMsg.Text
            };
            taskStateUpdate.FunReport(info);
            button1.Enabled = true;
        }
    }
}
