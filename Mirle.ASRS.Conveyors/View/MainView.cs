using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Mirle.MPLC;
using Mirle.MPLC.MCProtocol;
using Mirle.ASRS.Conveyor.U2NMMA30.Service;

namespace Mirle.ASRS.Conveyors.View
{
    public partial class MainView : Form
    {
        private readonly Conveyor _conveyor;
        private LoggerService _loggerService;

        public MainView(Conveyor conveyor)
        {
            InitializeComponent();
            _conveyor = conveyor;
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
                lblPLCConnSts.BackColor = _conveyor.IsConnected ? Color.Lime : Color.Red;

               if(!_conveyor.IsConnected)
               {
                    for (int index = 0; index < splitContainer1.Panel1.Controls.Count; index++)
                    {
                        if (splitContainer1.Panel1.Controls[index] is BufferView bufferView)
                        {
                            if (_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                            {
                                bufferView.Refresh_BufferPLCError(buffer);
                            }
                        }
                    }
                }
               else 
               {
                    for (int index = 0; index < splitContainer1.Panel1.Controls.Count; index++)
                    {
                        if (splitContainer1.Panel1.Controls[index] is BufferView bufferView)
                        {
                            if (_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                            {
                                bufferView.Refresh_Buffer(buffer);
                            }
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
