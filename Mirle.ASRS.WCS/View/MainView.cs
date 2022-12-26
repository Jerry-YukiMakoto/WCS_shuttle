using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using HslCommunicationPLC.Siemens;

namespace Mirle.ASRS.View
{
    public partial class MainView : Form
    {
        private readonly clsBufferData Plc1;
        private static int BufferCount = 4;
        private static int Lifterlevel = 10;


        public MainView(clsBufferData _Plc1)
        {
            InitializeComponent();
            Plc1= _Plc1;
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
                lblPLCConnSts.BackColor = Plc1.bConnectPLC ? Color.Lime : Color.Red;

               if(!Plc1.bConnectPLC)
               {
                    for (int index = 1; index <= BufferCount; index++)
                    {
                        if (splitContainer1.Panel1.Controls.Find("A" + index, true).FirstOrDefault() is BufferView bufferView)
                        {

                                bufferView.Refresh_BufferPLCError(Plc1);
                            
                        }
                    }
                }
               else 
               {
                    for (int index = 1; index <= BufferCount; index++)
                    {
                        if (splitContainer1.Panel1.Controls.Find("A"+ index, true).FirstOrDefault() is BufferView bufferView)
                        {
                            if (true)
                            {
                                bufferView.Refresh_Buffer(Plc1,index);
                            }
                        }
                    }
               }

                if (!Plc1.bConnectPLC)
                {

                        if (splitContainer1.Panel1.Controls.Find("lifterView1", true).FirstOrDefault() is LifterView LifterView)
                        {

                            LifterView.Refresh_LifterViewPLCError(Plc1);

                        }
                    
                }
                else
                {
                        if (splitContainer1.Panel1.Controls.Find("lifterView1", true).FirstOrDefault() is LifterView LifterView)
                        {
                            if (true)
                            {
                                LifterView.Refresh_Lifter(Plc1);
                            }
                        }
                }

                if (!Plc1.bConnectPLC)
                {
                    for (int index = 1; index <= Lifterlevel; index++)
                    {
                        if (splitContainer1.Panel1.Controls.Find("level_Signal" + index, true).FirstOrDefault() is Level_Signal level_Signal)
                        {

                            level_Signal.Refresh_LifterlevelViewPLCError(Plc1,index);

                        }
                    }

                }
                else
                {
                    for (int index = 1; index <= Lifterlevel; index++)
                    {
                        if (splitContainer1.Panel1.Controls.Find("level_Siganal" + index, true).FirstOrDefault() is Level_Signal level_Signal)
                        {
                            if (true)
                            {
                                level_Signal.RefreshLevel_Lifter(Plc1,index);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //_loggerService.WriteExceptionLog(MethodBase.GetCurrentMethod(), $"{ex.Message}\n{ex.StackTrace}");
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                //clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
            }
            finally
            {
                timerMainProc.Enabled = true;
            }
        }

        private Form Buffer_Restart;
        private void buffer_Restart(object sender, EventArgs e)
        {
            if (Buffer_Restart == null)
            {
                //Buffer_Restart = new frmBuffer_Initial(_conveyor);
                Buffer_Restart.FormClosed += new FormClosedEventHandler(bufferrestart_FormClosed);
                Buffer_Restart.Show();
            }
            else
            {
                Buffer_Restart.BringToFront();
            }
        }

        private void bufferrestart_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Buffer_Restart != null)
                Buffer_Restart = null;
        }
    }
}
