using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Mirle.MPLC;
using Mirle.ASRS.Conveyor.U2NMMA30.Service;

namespace Mirle.ASRS.Conveyors.U0NXMA30.View
{
    public partial class MainView : Form
    {
        private readonly Conveyor _conveyor;
        private IMPLCProvider _plchost;
        private readonly LoggerService _loggerService;

        public MainView(IMPLCProvider plchost)
        {
            InitializeComponent();
            _plchost = plchost;

        }

        private void MainView_Load(object sender, EventArgs e)
        {
            timerMainProc.Enabled = true;
            //Start Timer
            timerMainProc.Interval = 300;
            timerMainProc.Enabled = true;
        }

        private void timerMainProc_Tick(object sender, EventArgs e)
        {
            timerMainProc.Enabled = false;
            try
            {
                //Check PLC
                lblPLCConnSts.BackColor = _plchost.IsConnected  ? Color.Lime : Color.Red;
               

                for(int index = 0; index < splitContainer1.Panel1.Controls.Count; index++) 
                {
                    if(splitContainer1.Panel1.Controls[index] is BufferView bufferView)
                    {
                        if(_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                        {
                            bufferView.Refresh_Buffer(buffer);
                        }
                    }
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
