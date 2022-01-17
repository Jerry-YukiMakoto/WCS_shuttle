using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.ASRS.Conveyors;
using Mirle.ASRS.WCS.Controller;
using Mirle.MPLC;
using Mirle.ASRS.Conveyor.U2NMMA30.Service;

namespace Mirle.ASRS.Conveyors.U0NXMA30.View
{
    public partial class MainView : Form
    {
        private readonly Conveyor _conveyor;
        private IMPLCProvider _mplc;
        private LoggerService _loggerService;
        private BufferView _bufferView;

        public MainView()
        {
            InitializeComponent();

            _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
        }

        private void MainView_Load(object sender, EventArgs e)
        {
            //Start Timer
            timerMainProc.Interval = 300;
            timerMainProc.Enabled = true;
        }

        public BufferView GetBufferView()
        {
            return _bufferView;
        }

        private void timerMainProc_Tick(object sender, EventArgs e)
        {
            timerMainProc.Enabled = false;
            try
            {
                //Check PLC
                if (_mplc.IsConnected)
                {
                    lblPLCConnSts.BackColor = Color.Lime;
                }
                else
                {
                    lblPLCConnSts.BackColor = Color.Red;
                }


            }
            catch (Exception ex)
            {
                _loggerService.WriteExceptionLog(MethodBase.GetCurrentMethod(), $"{ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                timerMainProc.Enabled = true;
            }
        }
    }
}
