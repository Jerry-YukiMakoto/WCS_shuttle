using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using System.Threading.Tasks;
using Mirle.Grid.U0NXMA30;
using System.Collections.Generic;
using Mirle.Def;
using Mirle.Def.U0NXMA30;
using Mirle.Structure.Info;
using Mirle.DB.Object;
using Mirle.DataBase;
using WCS_API_Server;
using Unity;
using Mirle.Logger;
using WCS_API_Client.View;
using Mirle.ASRS.Close.Program;
using System.Threading;
using Mirle.Structure;
using WCS_API_Client.ReportInfo;

namespace Mirle.ASRS.WCS.View
{
    public partial class MainForm : Form
    {
        //public static clsCheckPathIsWork CheckPathIsWork = new clsCheckPathIsWork();
        

        private DB.ClearCmd.Proc.clsHost clearCmd;
        private WebApiHost _webApiHost;
        private UnityContainer _unityContainer;
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

            clsWriLog.Log.FunWriTraceLog_CV("WCS程式已開啟");
            timRead.Enabled = true;
            timer1.Enabled = true;
        }

        private void FunEventInit()
        {
            /*
            clsMicronStocker.GetStockerById(1).GetCraneById(1).OnStatusChanged += Stocker_OnStatusChanged_1;
            clsMicronStocker.GetStockerById(2).GetCraneById(1).OnStatusChanged += Stocker_OnStatusChanged_2;
            clsMicronStocker.GetStockerById(3).GetCraneById(1).OnStatusChanged += Stocker_OnStatusChanged_3;
            clsMicronStocker.GetStockerById(4).GetCraneById(1).OnStatusChanged += Stocker_OnStatusChanged_4;
            clsMicronCV.GetConveyorController().GetMainView_Object().GetMonitor().OnStkLabelClick += MainForm_OnStkLabelClick;

            for (int i = 1; i <= Conveyor.U2NMMA30.Signal.SignalMapper.BufferCount; i++)
            {
                clsMicronCV.GetConveyorController().GetBuffer(i).OnStatusChanged += Buffer_OnStatusChanged;
                clsMicronCV.GetConveyorController().GetBuffer(i).OnPresenceChanged += Buffer_OnPresenceChanged;
                clsMicronCV.GetConveyorController().GetBuffer(i).OnAlarmBitChanged += MainForm_OnAlarmBitChanged;
                clsMicronCV.GetConveyorController().GetBuffer(i).OnAlarmBitChanged_2 += MainForm_OnAlarmBitChanged_2;
            }

            stockoutToStnCheck.OnLoadPortDataChanged += StockoutToStnCheck_OnLoadPortDataChanged;
            */
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
                    clsWriLog.Log.FunWriTraceLog_CV("WCS程式已關閉！");
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
                clsWriLog.Log.FunWriTraceLog_CV("WCS OnLine.");
            else
                clsWriLog.Log.FunWriTraceLog_CV("WCS OffLine.");
            /*
            for (int i = 1; i <= 4; i++)
            {
                clsMicronCV.GetConveyorController().GetMainView_Object().GetMonitor().FunSetOnline(i, chkOnline.Checked);
            }
            */
        }

        #region Micron
        //private void MainForm_OnAlarmBitChanged(object sender, Conveyor.U2NMMA30.Events.AlarmBitEventArgs e)
        //{
        //    string alarmMsg = clsMicronCV.GetCVAlarm(e.BufferIndex, e.AlarmBit);
        //    if (e.Signal == true)
        //    {
        //        clsDB_Proc.GetDB_Object().GetEQ_Alarm().FunInsSts(e.BufferIndex, alarmMsg, clsEnum.AlarmSts.OnGoing);
        //    }
        //    else
        //    {
        //        clsDB_Proc.GetDB_Object().GetEQ_Alarm().FunUpdSts(e.BufferIndex, alarmMsg, clsEnum.AlarmSts.Clear);
        //    }
        //}

        //private void MainForm_OnAlarmBitChanged_2(object sender, Conveyor.U2NMMA30.Events.AlarmBitEventArgs e)
        //{
        //    string alarmMsg = clsMicronCV.GetCVAlarm(e.BufferIndex, e.AlarmBit+16);
        //    if (e.Signal == true)
        //    {
        //        clsDB_Proc.GetDB_Object().GetEQ_Alarm().FunInsSts(e.BufferIndex, alarmMsg, clsEnum.AlarmSts.OnGoing);
        //    }
        //    else
        //    {
        //        clsDB_Proc.GetDB_Object().GetEQ_Alarm().FunUpdSts(e.BufferIndex, alarmMsg, clsEnum.AlarmSts.Clear);
        //    }
        //}

        //private void MainForm_KeyDown(object sender, KeyEventArgs e)
        //{
        //    //Ctrl + L
        //    if (e.KeyCode == Keys.L && e.Modifiers == Keys.Control)
        //    {
        //        Def.clsTool.FunVisbleChange(ref btnSendAPITest);
        //    }
        //}

        //private Form[] stkcView = new Form[4];
        //private void MainForm_OnStkLabelClick(object sender, Conveyor.U2NMMA30.Events.StkLabelClickArgs e)
        //{
        //    if (stkcView[e.StockerID - 1] == null)
        //        stkcView[e.StockerID - 1] = clsMicronStocker.GetSTKCHostById(e.StockerID).GetMainView();
        //    stkcView[e.StockerID - 1].Show();
        //    stkcView[e.StockerID - 1].Activate();
        //}

        

        //private void StockoutToStnCheck_OnLoadPortDataChanged(object sender, Event.LoadPortEventArgs e)
        //{
        //    if (autoPickup_Proc.LoadPortData.TryGetValue(e.PortID, out _))
        //    {
        //        autoPickup_Proc.LoadPortData[e.PortID] = e.Command;
        //    }
        //    else
        //    {
        //        autoPickup_Proc.LoadPortData.Add(e.PortID, e.Command);
        //    }
        //}

        //private void Buffer_OnPresenceChanged(object sender, Conveyor.U2NMMA30.Events.BufferEventArgs e)
        //{
        //    //if (clsMicronCV.CycleIndex.Any(v => v == e.BufferIndex))
        //    //{
        //    //    if (e.Presence) clsMicronCV.GetConveyorController().CycleCount++;
        //    //    else
        //    //    {
        //    //        if (clsMicronCV.GetConveyorController().CycleCount > 0)
        //    //            clsMicronCV.GetConveyorController().CycleCount--;
        //    //    }
        //    //}
        //}

        //private void Buffer_OnStatusChanged(object sender, Conveyor.U2NMMA30.Events.BufferEventArgs e)
        //{
        //    ConveyorInfo buffer = new ConveyorInfo();
        //    bool bUpdate;
        //    switch(e.BufferIndex)
        //    {
        //        case 41:
        //            buffer = ConveyorDef.A1_41;
        //            bUpdate = true;
        //            break;
        //        case 42:
        //            buffer = ConveyorDef.A1_42;
        //            bUpdate = true;
        //            break;
        //        case 43:
        //            buffer = ConveyorDef.A1_43;
        //            bUpdate = true;
        //            break;
        //        case 44:
        //            buffer = ConveyorDef.A1_44;
        //            bUpdate = true;
        //            break;
        //        default:
        //            bUpdate = false; break;
        //    }

        //    if(bUpdate)
        //    {
        //        EQPStatusUpdateInfo info = new EQPStatusUpdateInfo
        //        {
        //            portId = buffer.StnNo,
        //            portStatus = ((int)e.NewStatus).ToString()
        //        };

        //        clsWmsApi.GetApiProcess().GetEQPStatusUpdate().FunReport(info);
        //    }
        //}

        #endregion Micron

        #region Stocker Event

        //private void Stocker_OnStatusChanged_4(object sender, Stocker.R46YP320.Events.CraneEventArgs args)
        //{
        //    clsWmsApi.FunReportStkStatusChanged(4, args.NewStatus);
        //}

        //private void Stocker_OnStatusChanged_3(object sender, Stocker.R46YP320.Events.CraneEventArgs args)
        //{
        //    clsWmsApi.FunReportStkStatusChanged(3, args.NewStatus);
        //}

        //private void Stocker_OnStatusChanged_2(object sender, Stocker.R46YP320.Events.CraneEventArgs args)
        //{
        //    clsWmsApi.FunReportStkStatusChanged(2, args.NewStatus);
        //}

        //private void Stocker_OnStatusChanged_1(object sender, Stocker.R46YP320.Events.CraneEventArgs args)
        //{
        //    clsWmsApi.FunReportStkStatusChanged(1, args.NewStatus);
        //}

        #endregion Stocker Event

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

        private StockerSpeedMaintainView StockerSpeed;
        private void btnCraneSpeedMaintain_Click(object sender, EventArgs e)
        {
            if (StockerSpeed == null)
            {
                StockerSpeed = new StockerSpeedMaintainView();
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
                #region 需再修改
                 if (DB.Proc.clsHost.IsConn)
                {
                    clsDB_Proc.GetDB_Object().GetTask().FunCheckTaskCmdFinish();
                }
                #endregion
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
            finally
            {
                timRead.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                lblDBConn.BackColor = DB.Proc.clsHost.IsConn ? Color.Blue : Color.Red;
                lblTimer.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
            finally
            {
                timer1.Enabled = true;
            }
        }

        #endregion Timer

       
        private void FunInit()
        {
            var archive = new AutoArchive();
            archive.Start();
            
            //clsWmsApi.FunInit(clInitSys.WmsApi_Config);

            //bool bFlag;
            //do
            //{
            //    bFlag = clsDB_Proc.GetDB_Object().GetProcess().GetDevicePortProc();
            //    Task.Delay(500).Wait();
            //} while (!bFlag);

            
            //CheckPathIsWork.subStart();
            
            //reUpdateTaskCompleteCmd_Proc.subStart();
            
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterInstance(new WMSWCSController());
            _webApiHost = new WebApiHost(new Startup(_unityContainer), clInitSys.WcsApi_Config.IP);
            clearCmd = new DB.ClearCmd.Proc.clsHost();
            //ChangeSubForm(clsMicronCV.GetConveyorController().GetMainView());
            //FunInitStockerStsForm();
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
                            oGrid[ColumnDef.CMD_MST.CmdSno.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["CmdSno"]);
                            oGrid[ColumnDef.CMD_MST.taskNo.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["taskNo"]);
                            oGrid[ColumnDef.CMD_MST.CmdSts.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["CmdSts"]);
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
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
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
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
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
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
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
