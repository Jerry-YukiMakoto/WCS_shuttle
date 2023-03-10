using System;
using System.Drawing;
using System.Windows.Forms;
using Mirle.DB.Object;
using WCS_API_Server;
using Unity;
using Mirle.Logger;
using WCS_API_Client.View;
using Mirle.ASRS.Close.Program;
using System.Threading;
using Mirle.ASRS.WCS.Library;
using Mirle.ASRS.WCS.Controller;
using Mirle.Def;
using Mirle.DataBase;
using HslCommunicationPLC.Siemens;
using Mirle.IASC;
using Mirle.ASRS.View;
using Mirle.BarcodeReader;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.Grid.T26YGAP0;
using PLCConfigSetting.PLCsetting;

namespace Mirle.ASRS.WCS.View
{
    public partial class MainForm : Form
    {

        private WebApiHost _webApiHost;
        private UnityContainer _unityContainer;
        private static System.Timers.Timer timRead = new System.Timers.Timer();
        public static clsBufferData Plc1 = new clsBufferData();
        private static WCSManager _wcsManager;
        private ShuttleController _shuttleController;
        private readonly MainView _mainView;
        public SocketListen SocketListen;
        private DB.ClearCmd.Proc.clsHost clearCmd;
        private ShuttleCommand _shuttleCommand;

        public MainForm()
        {
            InitializeComponent();
            timRead.Elapsed += new System.Timers.ElapsedEventHandler(timRead_Elapsed);
            timRead.Enabled = false; timRead.Interval = 1000;
        }

        #region Event
        private void MainForm_Load(object sender, EventArgs e)
        {
            ChkAppIsAlreadyRunning();
            this.Text = this.Text + "  v " + ProductVersion;
            clInitSys.FunLoadIniSys();
            FunInit();
            GridInit();

            Library.clsWriLog.Log.FunWriTraceLog_CV("WCS程式已開啟");
            timRead.Start();
            timer1.Start();
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            frmCloseProgram objCloseProgram;
            try
            {
                e.Cancel = true;

                objCloseProgram = new frmCloseProgram();
                
                if (objCloseProgram.ShowDialog() == DialogResult.OK)
                {
                    chkOnline.Checked = false;
                    SpinWait.SpinUntil(() => false, 1000);
                    Library.clsWriLog.Log.FunWriTraceLog_CV("WCS程式已關閉！");
                    _shuttleController.Dispose();
                    throw new Exception();
                }
            }
            catch
            {
                Environment.Exit(0);
            }
            finally
            {
                objCloseProgram = null;
            }
        }

        private void chkOnline_CheckedChanged(object sender, EventArgs e)
        {
            if (chkOnline.Checked)
                Library.clsWriLog.Log.FunWriTraceLog_CV("WCS OnLine.");
            else
                Library.clsWriLog.Log.FunWriTraceLog_CV("WCS OffLine.");
        }

        #endregion Event



        #region 側邊欄buttons


        private frmCmdMaintance cmdMaintance;

        private frmTaskMaintance TaskMaintance;
        private void btnCmdMaintain_Click(object sender, EventArgs e)
        {
            if (cmdMaintance == null)
            {
                cmdMaintance = new frmCmdMaintance();
                cmdMaintance.TopMost = true;
                cmdMaintance.FormClosed += new FormClosedEventHandler(funCmdMaintain_FormClosed);
                cmdMaintance.Show();
            }
            else
            {
                cmdMaintance.BringToFront();
            }
        }



        private void funCmdMaintain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (cmdMaintance != null)
                cmdMaintance = null;
        }

        private void btnTaskMaintain_Click(object sender, EventArgs e)
        {
            if (TaskMaintance == null)
            {
                TaskMaintance = new frmTaskMaintance();
                TaskMaintance.TopMost = true;
                TaskMaintance.FormClosed += new FormClosedEventHandler(funTaskMaintain_FormClosed);
                TaskMaintance.Show();
            }
            else
            {
                TaskMaintance.BringToFront();
            }
        }



        private void funTaskMaintain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (TaskMaintance != null)
                TaskMaintance = null;
        }

        #endregion 側邊欄buttons

        #region Timer

        private void timRead_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            timRead.Enabled = false;
            try
            {
                SubShowCmdtoGrid(ref GridCmd);
                Plc1.FunProcessReadOnly();
                _wcsManager = new WCSManager();
                _wcsManager.getshuttle(_shuttleController);
                _wcsManager.WCSManagerControl(Plc1);
                Plc1.FunProcessReadandClear();
                if(_shuttleController.IsConnected)
                {
                    _shuttleController.P00();
                }
               
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                Library.clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
            finally
            {
                timRead.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            try
            {
                lblDBConn.BackColor = DB.Proc.clsHost.IsConn ? Color.Blue : Color.Red;
                lblTimer.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                Library.clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
            finally
            {
                timer1.Start();
            }
        }

        #endregion Timer


        private void FunInit()
        {
            var archive = new AutoArchive();
            archive.Start();
            clsDB_Proc.Initial(clInitSys.DbConfig, clInitSys.DbConfig_WMS); //原DataAccessController功能
            ControllerReader.FunGetController(clInitSys.CV_Config);
            _shuttleController = new ShuttleController(clInitSys.SHC_IP, clInitSys.SHC_port);
            _shuttleController.ChangeLayer += _shuttleController_OnLayerChange;
            _shuttleController.OnCommandReceive += _shuttleController_CommandReceive;
            _shuttleController.OnCommandStatusChange += _shuttleController_CommandStatusChange;
            _shuttleController.Open();
            SocketListen = new SocketListen(Convert.ToInt32(ASRS_Setting.BCR_port));
            SocketListen.OnDataReceive += SocketListen_OnDataReceive;
            SocketListen.Listen();
            Plc1 = ControllerReader.GetCVControllerr().GetPLC1();

            //clearCmd = new DB.ClearCmd.Proc.clsHost();//移動完成命令致歷史資料表以及定期清理歷史命令
            ChangeSubForm(new MainView(Plc1,_shuttleController));
        }

        private void _shuttleController_OnLayerChange(object sender, ChangeLayerEventArgsLayer e)
        {
            _wcsManager.WCSManageControlSHC_Call(e);
        }

        private void _shuttleController_CommandReceive(object sender, CommandReceiveEventArgs e)
        {
            _wcsManager.WCSManageControlSHC_CallComandstatus(e);
        }

        private void _shuttleController_CommandStatusChange(object sender, CommandStatusEventArgs e)
        {
            _wcsManager.WCSManageControlSHC_ChangeComandstatus(e);
        }

        private void SocketListen_OnDataReceive(object sender, SocketDataReceiveEventArgs arg)
        {
            _wcsManager.WCSManageControlBCRSocket_Call(arg);
        }

        public clsBufferData GetPLC1()
        {
            return Plc1;
        }

        #region Grid顯示
        private void GridInit()
        {
            Grid.clInitSys.GridSysInit(ref GridCmd);
            ColumnDef.CMD_MST.GridSetLocRange(ref GridCmd);
        }

        delegate void degShowCmdtoGrid(ref DataGridView oGrid);
        private void SubShowCmdtoGrid(ref DataGridView oGrid)
        {
            degShowCmdtoGrid obj;
            try
            {
                if (InvokeRequired)
                {
                    obj = new degShowCmdtoGrid(SubShowCmdtoGrid);
                    Invoke(obj, oGrid);
                }
                else
                {
                    Grid.IGrid grid;
                    grid = new DB.Object.GridData.CmdMst();
                    grid.SubShowCmdtoGrid(ref oGrid);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                Library.clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }
        #endregion Grid顯示

        /// <summary>
        /// 檢查程式是否重複開啟
        /// </summary>
        private void ChkAppIsAlreadyRunning()
        {
            try
            {
                string aFormName = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
                string aProcName = System.IO.Path.GetFileNameWithoutExtension(aFormName);
                if (System.Diagnostics.Process.GetProcessesByName(aProcName).Length > 1)
                {
                    MessageBox.Show("程式已開啟", "Communication System", MessageBoxButtons.OK);
                    //Application.Exit();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                Library.clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                Environment.Exit(0);
            }
        }

        private void ChangeSubForm(Form subForm)
        {
            try
            {
                var children = spcMainView.Panel1.Controls;
                foreach (Control c in children)
                {
                    if (c is Form)
                    {
                        var thisChild = c as Form;
                        //thisChild.Hide();
                        spcMainView.Panel1.Controls.Remove(thisChild);
                        thisChild.Width = 0;
                    }
                }

                if (subForm != null)
                {
                    subForm.TopLevel = false;
                    subForm.Dock = DockStyle.Fill;//適應窗體大小
                    subForm.FormBorderStyle = FormBorderStyle.None;//隱藏右上角的按鈕
                    subForm.Parent = spcMainView.Panel1;
                    spcMainView.Panel1.Controls.Add(subForm);
                    subForm.Show();
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                Library.clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

    }
}
