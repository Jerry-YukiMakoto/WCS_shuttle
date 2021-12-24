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
    }
}
