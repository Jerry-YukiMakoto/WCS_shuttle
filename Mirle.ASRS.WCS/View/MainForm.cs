using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using Mirle.Grid.U0NXMA30;
using Mirle.DB.Object;
using Mirle.DataBase;
using WCS_API_Server;
using Unity;
using Mirle.Logger;
using WCS_API_Client.View;
using Mirle.ASRS.Close.Program;
using System.Threading;
using Mirle.ASRS.WCS.Library;
using Mirle.ASRS.WCS.Controller;

namespace Mirle.ASRS.WCS.View
{
    public partial class MainForm : Form
    {
        //public static clsCheckPathIsWork CheckPathIsWork = new clsCheckPathIsWork();
        

        private DB.ClearCmd.Proc.clsHost clearCmd;
        private WebApiHost _webApiHost;
        private UnityContainer _unityContainer;
        private static WCSManager _wcsManager;
        private static System.Timers.Timer timRead = new System.Timers.Timer();

        public MainForm()
        {
            InitializeComponent();

            timRead.Elapsed += new System.Timers.ElapsedEventHandler(timRead_Elapsed);
            timRead.Enabled = false; timRead.Interval = 100;
        }

        #region Event
        private void MainForm_Load(object sender, EventArgs e)
        {
            ChkAppIsAlreadyRunning();
            this.Text = this.Text + "  v " + ProductVersion;
            clInitSys.FunLoadIniSys();
            FunInit();
            FunEventInit();
            GridInit();

            Library.clsWriLog.Log.FunWriTraceLog_CV("WCS程式已開啟");
            timRead.Enabled = true;
            timer1.Enabled = true;
        }

        private void FunEventInit()
        {
            
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
            /*
            for (int i = 1; i <= 4; i++)
            {
                clsMicronCV.GetConveyorController().GetMainView_Object().GetMonitor().FunSetOnline(i, chkOnline.Checked);
            }
            */
        }





        #region 側邊欄buttons

        private MainTestForm mainTest;
        private void btnSendAPITest_Click(object sender, EventArgs e)
        {
            if (mainTest == null)
            {
                mainTest = new MainTestForm(clInitSys.WmsApi_Config);
                mainTest.TopMost = true;
                mainTest.FormClosed += new FormClosedEventHandler(funMainTest_FormClosed);
                mainTest.Show();
            }
            else
            {
                mainTest.BringToFront();
            }
        }

        private void funMainTest_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mainTest != null)
                mainTest = null;
        }

        private CraneSpeedMaintainView StockerSpeed;
        private void btnCraneSpeedMaintain_Click(object sender, EventArgs e)
        {
            if (StockerSpeed == null)
            {
                StockerSpeed = new CraneSpeedMaintainView();
                StockerSpeed.TopMost = true;
                StockerSpeed.FormClosed += new FormClosedEventHandler(funCraneSpeedMaintain_FormClosed);
                StockerSpeed.Show();
            }
            else
            {
                StockerSpeed.BringToFront();
            }
        }

        private void funCraneSpeedMaintain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (StockerSpeed != null)
                StockerSpeed = null;
        }

        private frmCmdMaintance cmdMaintance;
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

        #endregion 側邊欄buttons

        

        #endregion Event

        #region Timer
        private void timRead_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            timRead.Enabled = false;
            try
            {
                 if (DB.Proc.clsHost.IsConn)
                {
                    clsDB_Proc.GetDB_Object().GetEqu_Cmd().FunCheckEquCmdFinish();
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
            _wcsManager = new WCSManager(clInitSys.DbConfig);
            _wcsManager.Start();
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterInstance(new WMSWCSController());
            _webApiHost = new WebApiHost(new Startup(_unityContainer), clInitSys.WcsApi_Config.IP);
            clearCmd = new DB.ClearCmd.Proc.clsHost();

            #region Mark
            //clsWmsApi.FunInit(clInitSys.WmsApi_Config);

            //bool bFlag;
            //do
            //{
            //    bFlag = clsDB_Proc.GetDB_Object().GetProcess().GetDevicePortProc();
            //    Task.Delay(500).Wait();
            //} while (!bFlag);

            //CheckPathIsWork.subStart();

            //reUpdateTaskCompleteCmd_Proc.subStart();

            //ChangeSubForm(clsMicronCV.GetConveyorController().GetMainView());
            //FunInitStockerStsForm();
            #endregion Mark
        }

        #region Grid顯示
        private void GridInit()
        {
            Grid.clInitSys.GridSysInit(ref Grid1);
            ColumnDef.CMD_MST.GridSetLocRange(ref Grid1);
        }

        delegate void degShowCmdtoGrid(ref DataGridView oGrid);
        private void SubShowCmdtoGrid(ref DataGridView oGrid)
        {
            degShowCmdtoGrid obj;
            string strSql = string.Empty;
            string strEM = string.Empty;
            DataTable dtTmp = new DataTable();
            try
            {
                if (InvokeRequired)
                {
                    obj = new degShowCmdtoGrid(SubShowCmdtoGrid);
                    Invoke(obj, oGrid);
                }
                else
                {
                    oGrid.SuspendLayout();
                    oGrid.Rows.Clear();
                    int iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunGetCmdMst_Grid(ref dtTmp);
                    if (iRet == DBResult.Success)
                    {
                        for (int i = 0; i < dtTmp.Rows.Count; i++)
                        {
                            oGrid.Rows.Add();
                            oGrid.Rows[oGrid.RowCount - 1].HeaderCell.Value = Convert.ToString(oGrid.RowCount);
                            oGrid[ColumnDef.CMD_MST.CmdSno.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["CMDSNO"]);
                            oGrid[ColumnDef.CMD_MST.taskNo.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["taskNo"]);
                            oGrid[ColumnDef.CMD_MST.CmdSts.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["CMDSTS"]);
                            oGrid[ColumnDef.CMD_MST.PRT.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["PRT"]);
                            oGrid[ColumnDef.CMD_MST.CmdMode.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["CmdMode"]);
                            oGrid[ColumnDef.CMD_MST.StnNo.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["StnNo"]);
                            oGrid[ColumnDef.CMD_MST.Loc.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Loc"]);
                            oGrid[ColumnDef.CMD_MST.NewLoc.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["NewLoc"]);
                            oGrid[ColumnDef.CMD_MST.EquNO.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["EquNO"]);
                            oGrid[ColumnDef.CMD_MST.Remark.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Remark"]);
                            oGrid[ColumnDef.CMD_MST.CrtDate.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["CrtDate"]);
                            oGrid[ColumnDef.CMD_MST.ExpDate.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["ExpDate"]);
                        }
                    }
                    oGrid.ResumeLayout();
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
                dtTmp = null;
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

        //private void FunInitStockerStsForm()
        //{
        //    for (int i = 1; i < 5; i++)
        //    {
        //        var subForm = clsMicronStocker.GetSTKCHostById(i).GetStockerStsView();
        //        subForm.TopLevel = false;
        //        subForm.Dock = DockStyle.Fill;//適應窗體大小
        //        subForm.FormBorderStyle = FormBorderStyle.None;//隱藏右上角的按鈕
        //        subForm.Parent = tlpMainSts;
        //        tlpMainSts.Controls.Add(subForm, i + 1, 0);
        //        subForm.Show();
        //    }
        //}

        
    }
}
