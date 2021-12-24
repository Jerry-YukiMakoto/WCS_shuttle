using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using System.Threading.Tasks;
using Mirle.Grid.U0NXMA30;
using System.Collections.Generic;
using Mirle.Micron.U2NMMA30;
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
using Mirle.ASRS.Conveyor.U2NMMA30.View;

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

        private void MainForm_OnAlarmBitChanged(object sender, Conveyor.U2NMMA30.Events.AlarmBitEventArgs e)
        {
            string alarmMsg = clsMicronCV.GetCVAlarm(e.BufferIndex, e.AlarmBit);
            if (e.Signal == true)
            {
                clsDB_Proc.GetDB_Object().GetEQ_Alarm().FunInsSts(e.BufferIndex, alarmMsg, clsEnum.AlarmSts.OnGoing);
            }
            else
            {
                clsDB_Proc.GetDB_Object().GetEQ_Alarm().FunUpdSts(e.BufferIndex, alarmMsg, clsEnum.AlarmSts.Clear);
            }
        }

        private void MainForm_OnAlarmBitChanged_2(object sender, Conveyor.U2NMMA30.Events.AlarmBitEventArgs e)
        {
            string alarmMsg = clsMicronCV.GetCVAlarm(e.BufferIndex, e.AlarmBit+16);
            if (e.Signal == true)
            {
                clsDB_Proc.GetDB_Object().GetEQ_Alarm().FunInsSts(e.BufferIndex, alarmMsg, clsEnum.AlarmSts.OnGoing);
            }
            else
            {
                clsDB_Proc.GetDB_Object().GetEQ_Alarm().FunUpdSts(e.BufferIndex, alarmMsg, clsEnum.AlarmSts.Clear);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            //Ctrl + L
            if (e.KeyCode == Keys.L && e.Modifiers == Keys.Control)
            {
                Def.clsTool.FunVisbleChange(ref btnSendAPITest);
            }
        }

        private Form[] stkcView = new Form[4];
        private void MainForm_OnStkLabelClick(object sender, Conveyor.U2NMMA30.Events.StkLabelClickArgs e)
        {
            if (stkcView[e.StockerID - 1] == null)
                stkcView[e.StockerID - 1] = clsMicronStocker.GetSTKCHostById(e.StockerID).GetMainView();
            stkcView[e.StockerID - 1].Show();
            stkcView[e.StockerID - 1].Activate();
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

        private void StockoutToStnCheck_OnLoadPortDataChanged(object sender, Event.LoadPortEventArgs e)
        {
            if (autoPickup_Proc.LoadPortData.TryGetValue(e.PortID, out _))
            {
                autoPickup_Proc.LoadPortData[e.PortID] = e.Command;
            }
            else
            {
                autoPickup_Proc.LoadPortData.Add(e.PortID, e.Command);
            }
        }

        private void Buffer_OnPresenceChanged(object sender, Conveyor.U2NMMA30.Events.BufferEventArgs e)
        {
            //if (clsMicronCV.CycleIndex.Any(v => v == e.BufferIndex))
            //{
            //    if (e.Presence) clsMicronCV.GetConveyorController().CycleCount++;
            //    else
            //    {
            //        if (clsMicronCV.GetConveyorController().CycleCount > 0)
            //            clsMicronCV.GetConveyorController().CycleCount--;
            //    }
            //}
        }

        private void Buffer_OnStatusChanged(object sender, Conveyor.U2NMMA30.Events.BufferEventArgs e)
        {
            ConveyorInfo buffer = new ConveyorInfo();
            bool bUpdate;
            switch(e.BufferIndex)
            {
                case 41:
                    buffer = ConveyorDef.A1_41;
                    bUpdate = true;
                    break;
                case 42:
                    buffer = ConveyorDef.A1_42;
                    bUpdate = true;
                    break;
                case 43:
                    buffer = ConveyorDef.A1_43;
                    bUpdate = true;
                    break;
                case 44:
                    buffer = ConveyorDef.A1_44;
                    bUpdate = true;
                    break;
                default:
                    bUpdate = false; break;
            }

            if(bUpdate)
            {
                EQPStatusUpdateInfo info = new EQPStatusUpdateInfo
                {
                    portId = buffer.StnNo,
                    portStatus = ((int)e.NewStatus).ToString()
                };

                clsWmsApi.GetApiProcess().GetEQPStatusUpdate().FunReport(info);
            }
        }


        #region Stocker Event
        
        private void Stocker_OnStatusChanged_4(object sender, Stocker.R46YP320.Events.CraneEventArgs args)
        {
            clsWmsApi.FunReportStkStatusChanged(4, args.NewStatus);
        }

        private void Stocker_OnStatusChanged_3(object sender, Stocker.R46YP320.Events.CraneEventArgs args)
        {
            clsWmsApi.FunReportStkStatusChanged(3, args.NewStatus);
        }

        private void Stocker_OnStatusChanged_2(object sender, Stocker.R46YP320.Events.CraneEventArgs args)
        {
            clsWmsApi.FunReportStkStatusChanged(2, args.NewStatus);
        }

        private void Stocker_OnStatusChanged_1(object sender, Stocker.R46YP320.Events.CraneEventArgs args)
        {
            clsWmsApi.FunReportStkStatusChanged(1, args.NewStatus);
        }
         
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
        private void btnCrandSpeedMaintain_Click(object sender, EventArgs e)
        {
            if (StockerSpeed == null)
            {
                StockerSpeed = new StockerSpeedMaintainView();
                StockerSpeed.TopMost = true;
                StockerSpeed.FormClosed += new FormClosedEventHandler(funStockerSpeedMaintain_FormClosed);
                StockerSpeed.Show();
            }
            else
            {
                StockerSpeed.BringToFront();
            }
        }

        private void funStockerSpeedMaintain_FormClosed(object sender, FormClosedEventArgs e)
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
                cycleRun.IsCycleRun = chkCycleRun.Checked;
                DB.Object.clsTool.RefreshCanGoForStockOut();

                FunCommandProc();
                if (DB.Proc.clsHost.IsConn)
                {
                    clsDB_Proc.GetDB_Object().GetTask().FunCheckTaskCmdFinish();
                    FunCheckWCS_CmdFinish();
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

        private void FunCommandProc()
        {
            try
            {
                SubShowCmdtoGrid(ref Grid1);

                if (chkOnline.Checked &&
                   DB.Proc.clsHost.IsConn && Grid1.RowCount > 0)
                {
                    FunStockIn_Begin(Grid1);
                }

                for (int i = 1; i <= 4; i++)
                {
                    //if (clsMicronStocker.CheckCraneIsIdle(i) && chkOnline.Checked && 
                    //    DB.Proc.clsHost.IsConn && Grid1.RowCount > 0)
                    if (clsMicronStocker.CheckCraneIsIdle(i) && 
                        clsMicronCV.GetConveyorController().GetMainView_Object().GetMonitor().Online[i-1] &&
                        DB.Proc.clsHost.IsConn && Grid1.RowCount > 0)
                    {
                        if (!FunBatchProc_WMS(Grid1, i))
                        {
                            if (!clsMicronStocker.CheckCraneIsIdle(i)) continue;

                            if (!FunNormalCommandProc(Grid1, i))
                            {
                                if (!clsMicronStocker.CheckCraneIsIdle(i)) continue;
                                FunMoveCommandProc(Grid1, i);
                            }
                        }
                    }

                    //Task.Delay(100).Wait();
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        private void FunStockIn_Begin(DataGridView oGrid)
        {
            try
            {
                IEnumerable<DataGridViewRow> obj = (from DataGridViewRow drv in oGrid.Rows
                                                    where (Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value) == clsConstValue.CmdMode.StockIn &&
                                                           Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value) == clsConstValue.CmdSts.strCmd_Initial) 
                                                    select drv);
                if (obj == null || obj.Count() == 0) return;
                foreach (var row in obj)
                {
                    string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                    int iEquNo = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                    string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                    string sRemark = "";

                    //if (clsMicronCV.GetConveyorController().CycleCount >= clsMicronCV.GetConveyorController().CycleCountMax)
                    //{
                    //    sRemark = $"Error: 等候大循環消化料盒";
                    //    if (sRemark != sRemark_Pre)
                    //    {
                    //        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    //    }

                    //    continue;
                    //}

                    if (!DB.Object.clsTool.CheckSts_ForStockInBegin(iEquNo, sCmdSno, sRemark_Pre, ref sRemark)) continue;
                    
                    if (clsDB_Proc.FunStockIn_WriPlc_Proc(row)) return;
                    else continue;
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        private bool FunMoveCommandProc(DataGridView oGrid, int EquNo)
        {
            try
            {
                for (int fork = 1; fork <= 2; fork++)
                {
                    if (clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).GetForkById(fork).HasCarrier)
                    {
                        return false;
                    }
                }        
                string sCmdSno = "";
                int Crane_Bay = clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).CurrentBay;
                if (clsMicronCV.CheckStockOutPortHasError(EquNo))
                {
                    ConveyorInfo buffer = new ConveyorInfo();
                    if (clsMicronCV.CheckStockInPortIsReady(EquNo, ref buffer))
                    {
                        if (Crane_Bay == 1) return true;
                        else
                        {
                            sCmdSno = clsMicronCV.GetConveyorController().GetBuffer(buffer.Index).CommandID;
                            return clsDB_Proc.GetDB_Object().GetTask().FunInsertTaskCmd(sCmdSno, EquNo.ToString(),
                                clsEnum.TaskMode.Move, buffer.StkPortID.ToString(), buffer.StkPortID.ToString(), 1);
                        }
                    }
                    else return false;
                }
                else
                {
                    IEnumerable<DataGridViewRow> obj = (from DataGridViewRow drv in oGrid.Rows
                                                        where Convert.ToInt32(drv.Cells[ColumnDef.CMD_MST.EquNO.Index].Value) == EquNo &&
                                                              Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_01.ToString() &&
                                                              Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_07.ToString() &&
                                                              Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_13.ToString() &&
                                                              Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_19.ToString() &&
                                                              Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value) == clsConstValue.CmdMode.StockOut &&
                                                              Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value) == clsConstValue.CmdSts.strCmd_Initial
                                                        orderby drv.Cells[ColumnDef.CMD_MST.Loc.Index].Value descending
                                                        select drv);
                    foreach (var row in obj)
                    {
                        sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                        string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                        string sBay = sLoc.Substring(2, 3);
                        int iBay = int.Parse(sBay);
                        if (iBay != 1)
                        {
                            int iCmdBay = (iBay + 1) / 2;
                            if (iCmdBay == Crane_Bay) return true;

                            string sCmdLoc = $"01{iCmdBay.ToString().PadLeft(3, '0')}01";
                            return clsDB_Proc.GetDB_Object().GetTask().FunInsertTaskCmd(sCmdSno, EquNo.ToString(),
                                clsEnum.TaskMode.Move, sCmdLoc, sCmdLoc, 1);
                        }
                        else continue;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        private bool FunNormalCommandProc(DataGridView oGrid, int EquNo)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                IEnumerable<DataGridViewRow> obj = (from DataGridViewRow drv in oGrid.Rows
                                                    where (Convert.ToInt32(drv.Cells[ColumnDef.CMD_MST.EquNO.Index].Value) == EquNo ||
                                                           (Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value) == EquNo.ToString() &&
                                                           Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != MicronLocation.GetLocation_ByStockOutPort(EquNo).LocationId)
                                                          ) &&
                                                          Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_01.ToString() &&
                                                          Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_07.ToString() &&
                                                          Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_13.ToString() &&
                                                          Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_19.ToString() &&
                                                          Convert.ToString(drv.Cells[ColumnDef.CMD_MST.BatchID.Index].Value) == ""
                                                    select drv);

                string sRemark = "";
                if (!DB.Object.clsTool.CheckSts(EquNo, ref sRemark))
                {
                    if (obj == null || obj.Count() == 0) return false;
                    foreach (var row in obj)
                    {
                        string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                        string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }
                    }
                    return false;
                }

                IEnumerable<DataGridViewRow> obj_order;
                int Crane_Bay = clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).CurrentBay;
                if (Crane_Bay == 1)
                {
                    obj_order = obj.OrderBy(r => r.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                }
                else
                {
                    obj_order = obj.OrderByDescending(r => r.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                }

                if (obj_order == null || obj_order.Count() == 0) return false;
                foreach (var row in obj_order)
                {
                    string sCurLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);
                    string sCurDeviceID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value);
                    string sCmdSts = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value);
                    string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                    string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                    string sLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                    string sNewLoc = Convert.ToString(row.Cells[ColumnDef.CMD_MST.NewLoc.Index].Value);
                    int iEquNo = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                    string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                    
                    if (!DB.Object.clsTool.CheckSts(EquNo, sCmdSno, sRemark_Pre, ref sRemark)) continue;

                    if (sCurLoc == LocationDef.Location.LeftFork.ToString() ||
                        sCurLoc == LocationDef.Location.RightFork.ToString())
                    { }
                    else
                    {
                        for (int fork = 1; fork <= 2; fork++)
                        {
                            CmdMstInfo cmd = new CmdMstInfo();
                            int iRet = clsDB_Proc.GetDB_Object().GetTask().GetForkCommand(EquNo, fork, ref cmd);
                            if (iRet == DBResult.Success)
                            {
                                if (!string.IsNullOrWhiteSpace(cmd.BatchID))
                                {
                                    sRemark = $"Error: 等候Stocker{EquNo}執行完Fork上的Batch命令";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }

                                    return false;
                                }
                            }
                            else
                            {
                                if (iRet == DBResult.Exception)
                                {
                                    sRemark = $"Error:取得Stocker{EquNo}的Fork{fork}命令失敗";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }

                                    return false;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(sCurLoc))
                    {
                        if (!DB.Object.clsTool.CheckCraneIsIdle(EquNo, sCmdSno, sRemark_Pre, ref sRemark)) continue;
                        bool IsTeach_NewLoc = false; int iRet = DBResult.Initial;

                        if (sCmdMode == clsConstValue.CmdMode.L2L)
                        {
                            iRet = clsDB_Proc.GetDB_Object().GetLocMst().CheckIsTeach(sCurDeviceID, Micron.U2NMMA30.clsTool.FunChangeLoc_byTask(sNewLoc), ref IsTeach_NewLoc);
                            if (iRet == DBResult.Exception)
                            {
                                sRemark = $"Error: 確認{sNewLoc}是否是校正儲位失敗！";
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }

                                continue;
                            }
                        }

                        Location Start = null; Location End = null; int fork = 0;
                        #region 判斷當前位置
                        Start = MicronLocation.GetLocation(sCurDeviceID, sCurLoc);
                        if (Start == null)
                        {
                            sRemark = $"Error: 取得CurLocation失敗 => <DeviceID> {sCurDeviceID} <Location> {sCurLoc}";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }

                            continue;
                        }
                        #endregion 判斷當前位置
                        #region 判斷最終目的位置
                        switch (sCmdMode)
                        {
                            case clsConstValue.CmdMode.StockOut:
                            case clsConstValue.CmdMode.S2S:
                                End = MicronLocation.GetLocation_ByStockOutPort(int.Parse(sCurDeviceID));
                                break;
                            case clsConstValue.CmdMode.StockIn:
                                End = MicronLocation.GetLocation_ByShelf(iEquNo);
                                break;
                            case clsConstValue.CmdMode.L2L:
                                if (IsTeach_NewLoc)
                                    End = MicronLocation.GetLocation_ByTeach(Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc));
                                else
                                    End = MicronLocation.GetLocation_ByShelf(Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc));
                                break;
                            default:
                                sRemark = $"Error: CmdMode有誤，請檢查 => {sCmdMode}";
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }
                                continue;
                        }

                        if (End == null)
                        {
                            sRemark = "Error: 取得最終位置失敗！";
                            if (sRemark != sRemark_Pre)
                            {
                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                            }
                            continue;
                        }
                        #endregion 判斷最終目的位置
                        if (Start != End)
                        {
                            if (Start.LocationId == LocationDef.Location.LeftFork.ToString() ||
                                Start.LocationId == LocationDef.Location.RightFork.ToString())
                            {
                                fork = Start.LocationId == LocationDef.Location.LeftFork.ToString() ? 1 : 2;
                                if (
                                    sCmdMode == clsConstValue.CmdMode.StockOut ||
                                    (sCmdMode == clsConstValue.CmdMode.StockIn && iEquNo != int.Parse(Start.DeviceId)) ||
                                    (sCmdMode == clsConstValue.CmdMode.L2L && Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc) != int.Parse(Start.DeviceId))
                                   )
                                {
                                    if (int.Parse(Start.DeviceId) != 4) //將Single Deep排除掉
                                    {
                                        int NextFork = fork switch
                                        {
                                            1 => 2,
                                            _ => 1,
                                        };
                                        bool bCheckOutside = false;
                                        string sLocDD = ""; bool IsEmpty_DD = false; string BoxID_DD = "";
                                        iRet = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().CheckLocIsOutside(sLoc, ref bCheckOutside,
                                                     ref sLocDD, ref IsEmpty_DD, ref BoxID_DD);
                                        if (iRet == DBResult.Success)
                                        {
                                            if (clsMicronStocker.GetCommand(EquNo, fork).CmdMode == clsConstValue.CmdMode.StockOut &&
                                                clsMicronStocker.GetCommand(EquNo, NextFork).CmdMode == clsConstValue.CmdMode.L2L &&
                                                clsMicronStocker.GetCommand(EquNo, NextFork).Loc == sLocDD)
                                            {
                                                sRemark = $"Error: 等待料盒放入內儲位";
                                                if (sRemark != sRemark_Pre)
                                                {
                                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                }
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            sRemark = $"Error: 取得儲位資料失敗 => {sLoc}";
                                            if (sRemark != sRemark_Pre)
                                            {
                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                            }

                                            continue;
                                        }
                                    }

                                    if (clsDeposit.FunDepositToCV_Proc(int.Parse(Start.DeviceId), fork)) return true;
                                    else continue;
                                }
                                else if (sCmdMode == clsConstValue.CmdMode.S2S)
                                {
                                    if (clsDeposit.FunDepositToCV_Proc(int.Parse(Start.DeviceId), fork)) return true;
                                    else continue;
                                }
                                else
                                {
                                    #region 置物至儲位
                                    if (int.Parse(Start.DeviceId) == 4 || sCmdMode != clsConstValue.CmdMode.L2L)
                                    {   //Single Deep直接做，不須額外判斷   只有庫對庫有可能是因為外儲位要搬出而產生的
                                        if (clsDB_Proc.GetDB_Object().GetProcess().FunDepositToShelf_Proc(row)) return true;
                                        else continue;
                                    }
                                    else
                                    {
                                        #region 庫對庫置物
                                        if (IsTeach_NewLoc)
                                        {
                                            if (clsDB_Proc.GetDB_Object().GetProcess().FunDepositToShelf_Proc(row)) return true;
                                            else continue;
                                        }
                                        else
                                        {
                                            bool bCheckOutside = false;
                                            string sLocDD = ""; bool IsEmpty_DD = false; string BoxID_DD = "";
                                            iRet = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().CheckLocIsOutside(sNewLoc, ref bCheckOutside,
                                                         ref sLocDD, ref IsEmpty_DD, ref BoxID_DD);
                                            if (iRet == DBResult.Success)
                                            {
                                                if (bCheckOutside)
                                                {
                                                    sRemark = $"{sNewLoc}為外儲位";
                                                    if (sRemark != sRemark_Pre)
                                                    {
                                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                    }

                                                    if (IsEmpty_DD)
                                                    {
                                                        CmdMstInfo cmd = new CmdMstInfo();
                                                        iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, ref cmd);

                                                        if (
                                                            iRet == DBResult.NoDataSelect || cmd.CmdSno == sCmdSno ||
                                                            (iRet == DBResult.Success && cmd.CmdSts != clsConstValue.CmdSts.strCmd_Initial &&
                                                             cmd.CurLoc != LocationDef.Location.Shelf.ToString())
                                                           )
                                                        {
                                                            sRemark = $"內儲位{sLocDD}為空，可以置物";
                                                            if (sRemark != sRemark_Pre)
                                                            {
                                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                            }

                                                            if (clsDB_Proc.GetDB_Object().GetProcess().FunDepositToShelf_Proc(row)) return true;
                                                            else continue;
                                                        }
                                                        else
                                                        {
                                                            if (clsDeposit.SubDepositToOutPortCV_Proc(row, fork)) return true;
                                                            else continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        sRemark = $"內儲位{sLocDD}有物，故改先置物至CV";
                                                        if (sRemark != sRemark_Pre)
                                                        {
                                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                        }

                                                        if (clsDeposit.SubDepositToOutPortCV_Proc(row, fork)) return true;
                                                        else continue;
                                                    }
                                                }
                                                else
                                                {
                                                    sRemark = $"{sNewLoc}為內儲位";
                                                    if (sRemark != sRemark_Pre)
                                                    {
                                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                    }

                                                    if (IsEmpty_DD)
                                                    {
                                                        sRemark = $"外儲位{sLocDD}為空";
                                                        if (sRemark != sRemark_Pre)
                                                        {
                                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                        }

                                                        CmdMstInfo cmd = new CmdMstInfo();
                                                        iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, ref cmd);
                                                        if (iRet == DBResult.Success &&
                                                            (
                                                              cmd.CmdMode == clsConstValue.CmdMode.StockIn ||
                                                              (cmd.CmdMode == clsConstValue.CmdMode.L2L && Micron.U2NMMA30.clsTool.funGetEquNoByLoc(cmd.NewLoc) == EquNo)
                                                            )
                                                           )
                                                        {
                                                            if (clsDeposit.FunDepositToCV_Proc(int.Parse(Start.DeviceId), fork)) return true;
                                                            else
                                                            {
                                                                sRemark = $"Error: 等候外儲位{sLocDD}的命令{cmd.CmdSno}結束";
                                                                if (sRemark != sRemark_Pre)
                                                                {
                                                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                }

                                                                continue;
                                                            }
                                                        }
                                                        else if (iRet == DBResult.Exception)
                                                        {
                                                            sRemark = $"Error: 確認外儲位{sLocDD}的命令失敗！";
                                                            if (sRemark != sRemark_Pre)
                                                            {
                                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                            }

                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            if (clsDB_Proc.GetDB_Object().GetProcess().FunDepositToShelf_Proc(row)) return true;
                                                            else continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        dtTmp = new DataTable();
                                                        iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, clsConstValue.CmdSts.strCmd_Initial, ref dtTmp);
                                                        if (iRet == DBResult.Success)
                                                        {
                                                            clsEnum.Fork checkLimitFork = clsEnum.Fork.None;
                                                            if (Micron.U2NMMA30.clsTool.IsLimit(sLocDD, ref checkLimitFork))
                                                            {
                                                                if (clsDeposit.SubDepositToOutPortCV_Proc(row, fork)) return true;
                                                                else continue;
                                                            }
                                                            else
                                                            {
                                                                clsEnum.NeedL2L needL2L_DD = Convert.ToString(dtTmp.Rows[0]["NeedShelfToShelf"]) == clsEnum.NeedL2L.Y.ToString() ?
                                                                     clsEnum.NeedL2L.Y : clsEnum.NeedL2L.N;
                                                                if (needL2L_DD == clsEnum.NeedL2L.Y)
                                                                {
                                                                    if (clsDeposit.SubDepositToOutPortCV_Proc(row, fork)) return true;
                                                                    else continue;
                                                                }
                                                                else
                                                                {
                                                                    if (DB.Object.clsTool.CheckCanGoForStockOut(sCmdSno, sRemark_Pre, ref sRemark))
                                                                    {
                                                                        sRemark = $"Error: 等候外儲位{sLocDD}命令完成 => {Convert.ToString(dtTmp.Rows[0]["CmdSno"])}";
                                                                        if (sRemark != sRemark_Pre)
                                                                        {
                                                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                        }
                                                                        continue;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (clsDB_Proc.GetDB_Object().GetProcess().FunDepositToShelf_Proc(row)) return true;
                                                                        else continue;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else if (iRet != DBResult.NoDataSelect)
                                                        {
                                                            sRemark = $"Error: 確認外儲位{sLocDD}的命令失敗！";
                                                            if (sRemark != sRemark_Pre)
                                                            {
                                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                            }

                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            if (clsDB_Proc.GetDB_Object().GetProcess().FunDepositToShelf_Proc(row)) return true;
                                                            else continue;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                sRemark = $"Error: 取得儲位資料失敗 => {sNewLoc}";
                                                if (sRemark != sRemark_Pre)
                                                {
                                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                }

                                                continue;
                                            }
                                        }
                                        #endregion 庫對庫置物
                                    }
                                    #endregion 置物至儲位
                                }
                            }
                            else if (Start.LocationId == LocationDef.Location.Teach.ToString())
                            {
                                if (clsDB_Proc.GetDB_Object().GetProcess().FunPickupFromTeach_Proc(row)) return true;
                                else continue;
                            }
                            else
                            {
                                Location sLoc_Start = null; Location sLoc_End = null;
                                int iFinishEquNo = 0; string sFinishLoc = "";
                                if (sCmdMode == clsConstValue.CmdMode.L2L)
                                {
                                    iFinishEquNo = Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc);
                                    sFinishLoc = sNewLoc;
                                }
                                else
                                {
                                    iFinishEquNo = iEquNo;
                                    sFinishLoc = sLoc;
                                }

                                bool bCheck = MicronLocation.GetPath(Start, End, ref sLoc_Start, ref sLoc_End);
                                if (bCheck == false)
                                {
                                    sRemark = "Error: Route給出的路徑為Null，WCS給的Location => Start: <Device>" + Start.DeviceId + " <Location>" + Start.LocationId +
                                       "，End: <Device>" + End.DeviceId + " <Location>" + End.LocationId;
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }

                                    continue;
                                }
                                else
                                {
                                    if (sCmdMode == clsConstValue.CmdMode.StockIn || sCmdMode == clsConstValue.CmdMode.L2L)
                                    {
                                        int iCurDeviceID = int.Parse(sCurDeviceID);
                                        if (iCurDeviceID == iFinishEquNo)
                                        {
                                            #region 站口入庫
                                            sLoc_End = clsMicronStocker.GetForkLocation(sLoc_Start, sFinishLoc);
                                            if (iCurDeviceID == 4)
                                            {
                                                #region Single Deep
                                                if (clsPickUp.FunPickupForStockIn_Proc(row, sLoc_Start, sLoc_End)) return true;
                                                else continue;
                                                #endregion Single Deep
                                            }
                                            else
                                            {
                                                string sCheckLoc = "";
                                                if (sCmdMode == clsConstValue.CmdMode.StockIn) sCheckLoc = sLoc;
                                                else sCheckLoc = sNewLoc;

                                                if (sCmdMode == clsConstValue.CmdMode.L2L && IsTeach_NewLoc)
                                                {
                                                    if (clsPickUp.FunPickupForStockIn_Proc(row, sLoc_Start, sLoc_End)) return true;
                                                    else continue;
                                                }
                                                else
                                                {
                                                    bool bCheckOutside = false;
                                                    string sLocDD = ""; bool IsEmpty_DD = false; string BoxID_DD = "";
                                                    iRet = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().CheckLocIsOutside(sCheckLoc, ref bCheckOutside,
                                                                 ref sLocDD, ref IsEmpty_DD, ref BoxID_DD);

                                                    clsEnum.NeedL2L needL2L = Convert.ToString(row.Cells[ColumnDef.CMD_MST.NeedShelfToShelf.Index].Value) == clsEnum.NeedL2L.Y.ToString() ?
                                                                            clsEnum.NeedL2L.Y : clsEnum.NeedL2L.N;
                                                    if (needL2L == clsEnum.NeedL2L.Y)
                                                    {
                                                        if (iRet == DBResult.Success)
                                                        {
                                                            if (!bCheckOutside || IsEmpty_DD)
                                                            {
                                                                #region 更新NeedShelfToShelf欄位為N
                                                                if (clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateNeedL2L(sCmdSno, clsEnum.NeedL2L.N)) return true;
                                                                else
                                                                {
                                                                    sRemark = $"Error: 更新NeedShelfToShelf欄位失敗 => {clsEnum.NeedL2L.N.ToString()}";
                                                                    if (sRemark != sRemark_Pre)
                                                                    {
                                                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                    }

                                                                    continue;
                                                                }
                                                                #endregion 更新NeedShelfToShelf欄位為N
                                                            }
                                                            else
                                                            {
                                                                CmdMstInfo cmd_DD = new CmdMstInfo();
                                                                iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, ref cmd_DD);
                                                                if (iRet == DBResult.Success)
                                                                {
                                                                    sRemark = $"Error: 等候內儲位{sLocDD}的命令完成！";
                                                                    if (sRemark != sRemark_Pre)
                                                                    {
                                                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                    }

                                                                    continue;
                                                                }
                                                                else if (iRet != DBResult.NoDataSelect)
                                                                {
                                                                    sRemark = $"Error: 確認內儲位{sLocDD}的命令失敗！";
                                                                    if (sRemark != sRemark_Pre)
                                                                    {
                                                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                    }

                                                                    continue;
                                                                }
                                                                else
                                                                {
                                                                    #region 上報Shelf Request
                                                                    if (clsDB_Proc.GetDB_Object().GetProcess().FunShelfRequest_Proc(sCmdSno, sLocDD, sRemark_Pre)) return true;
                                                                    else continue;
                                                                    #endregion 上報Shelf Request
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            sRemark = $"Error: 取得儲位資料失敗 => {sCheckLoc}";
                                                            if (sRemark != sRemark_Pre)
                                                            {
                                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                            }

                                                            continue;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (iRet == DBResult.Success)
                                                        {
                                                            if (bCheckOutside && !IsEmpty_DD)
                                                            {
                                                                #region 更新NeedShelfToShelf欄位為Y
                                                                if (clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateNeedL2L(sCmdSno, clsEnum.NeedL2L.Y)) return true;
                                                                else
                                                                {
                                                                    sRemark = $"Error: 更新NeedShelfToShelf欄位失敗 => {clsEnum.NeedL2L.Y.ToString()}";
                                                                    if (sRemark != sRemark_Pre)
                                                                    {
                                                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                    }

                                                                    continue;
                                                                }
                                                                #endregion 更新NeedShelfToShelf欄位為Y
                                                            }
                                                            else
                                                            {
                                                                if (clsPickUp.FunPickupForStockIn_Proc(row, sLoc_Start, sLoc_End)) return true;
                                                                else continue;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            sRemark = $"Error: 取得儲位資料失敗 => {sCheckLoc}";
                                                            if (sRemark != sRemark_Pre)
                                                            {
                                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                            }

                                                            continue;
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion 站口入庫
                                        }
                                        else
                                        {
                                            #region 還沒到正確的Crane，直接站對站搬走
                                            if (clsPickUp.FunPickupForStockIn_Proc(row, sLoc_Start, sLoc_End)) return true;
                                            else continue;
                                            #endregion 還沒到正確的Crane，直接站對站搬走
                                        }
                                    }
                                    else
                                    {
                                        //站對站也可以直接搬走
                                        #region 出庫命令直接站對站搬走
                                        if (clsPickUp.FunPickupForStockIn_Proc(row, sLoc_Start, sLoc_End)) return true;
                                        else continue;
                                        #endregion 出庫命令直接站對站搬走
                                    }
                                }
                            }
                        }
                        else continue;
                    }
                    else if (sCmdSts == clsConstValue.CmdSts.strCmd_Initial)
                    {
                        if (sCmdMode == clsConstValue.CmdMode.StockIn)
                        {
                            continue;
                        }
                        else if (sCmdMode == clsConstValue.CmdMode.StockOut ||
                                 sCmdMode == clsConstValue.CmdMode.L2L)
                        {
                            if(sCmdMode == clsConstValue.CmdMode.StockOut)
                            {                               
                                if (!DB.Object.clsTool.CheckCanGoForStockOut(sCmdSno,sRemark_Pre, ref sRemark))
                                {
                                    sRemark = $"Error: 等候大循環消化料盒";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }
                                    continue;
                                }


                                string sStnNoList = Convert.ToString(row.Cells[ColumnDef.CMD_MST.StnNo.Index].Value);
                                //bool bCheckCanDo = false;
                                string[] sStnNo = sStnNoList.Split(',');
                                //if (sStnNo.Length > 1) bCheckCanDo = true;
                                //else bCheckCanDo = clsTool.FunCheckIsCanGo(sStnNoList, oGrid);

                                //if (!bCheckCanDo)
                                //{
                                //    sRemark = $"Error: Port{sStnNoList}料盒數量水位已到極限！";
                                //    if (sRemark != sRemark_Pre)
                                //    {
                                //        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                //    }

                                //    continue;
                                //}

                                //訂單號
                                string sTicketId = Convert.ToString(row.Cells[ColumnDef.CMD_MST.TicketId.Index].Value);                                
                                if (Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.PRT.Index].Value) != 1)
                                {
                                    if (!DB.Object.clsTool.CheckCanGoForTicket(sCmdSno, sLoc, sTicketId, sStnNo[0], iEquNo, sRemark_Pre, ref sRemark)) continue;
                                }
                                
                            }

                            if (!DB.Object.clsTool.CheckCraneIsIdle(EquNo, sCmdSno, sRemark_Pre, ref sRemark)) continue;
                            if (iEquNo == 4)
                            {
                                #region Single Deep
                                if (
                                    sCmdMode == clsConstValue.CmdMode.StockOut ||
                                    (sCmdMode == clsConstValue.CmdMode.L2L && Micron.U2NMMA30.clsTool.funGetEquNoByLoc(sNewLoc) != iEquNo)
                                   )
                                {
                                    if (clsPickUp.FunPickupForStockOut_Proc(row, iEquNo, ref sRemark)) return true;
                                    else continue;
                                }
                                else
                                {
                                    if (clsPickUp.FunShelfToFork_Proc(row, sLoc, sNewLoc, iEquNo)) return true;
                                    else continue;
                                }
                                #endregion Single Deep
                            }
                            else
                            {
                                int iRet = DBResult.Initial;
                                if (sCmdMode == clsConstValue.CmdMode.L2L)
                                {
                                    bool bCanGo = true;
                                    var crane = clsMicronStocker.GetStockerById(EquNo).GetCraneById(1);
                                    for (int iFork = 1; iFork < 3; iFork++)
                                    {
                                        if (crane.GetForkById(iFork).HasCarrier &&
                                            clsMicronStocker.GetCommand(EquNo, iFork).CmdMode == clsConstValue.CmdMode.L2L)
                                        {
                                            CmdMstInfo cmd_DD = new CmdMstInfo();
                                            var sLoc_Check = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().GetLocDD(clsMicronStocker.GetCommand(EquNo, iFork).Loc);
                                            iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLoc_Check, ref cmd_DD);
                                            if(iRet == DBResult.Success || iRet == DBResult.Exception)
                                            {
                                                if (iRet == DBResult.Success && cmd_DD.CmdSno == sCmdSno) continue;
                                                else
                                                {
                                                    bCanGo = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (!bCanGo) continue;
                                }

                                bool IsTeach = false;
                                iRet = clsDB_Proc.GetDB_Object().GetLocMst().CheckIsTeach(EquNo.ToString(), Micron.U2NMMA30.clsTool.FunChangeLoc_byTask(sLoc), ref IsTeach);
                                if (iRet == DBResult.Exception)
                                {
                                    sRemark = $"Error: 確認{sLoc}是否是校正儲位失敗！";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }

                                    continue;
                                }

                                if (IsTeach)
                                {
                                    if (clsPickUp.FunShelfToFork_Proc(row, sLoc, sNewLoc, iEquNo)) return true;
                                    else continue;
                                }
                                else
                                {
                                    clsEnum.NeedL2L needL2L = Convert.ToString(row.Cells[ColumnDef.CMD_MST.NeedShelfToShelf.Index].Value) == clsEnum.NeedL2L.Y.ToString() ?
                                        clsEnum.NeedL2L.Y : clsEnum.NeedL2L.N;
                                    if (needL2L == clsEnum.NeedL2L.Y)
                                    {
                                        if (clsPickUp.FunShelfToFork_NeedL2L_Proc(row, ref sRemark)) return true;
                                        else continue;
                                    }
                                    else
                                    {
                                        bool bCheckOutside = false; string sLocDD = ""; bool IsEmpty_DD = false; string BoxID_DD = "";
                                        iRet = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().CheckLocIsOutside(sLoc, ref bCheckOutside,
                                                    ref sLocDD, ref IsEmpty_DD, ref BoxID_DD);
                                        if (iRet == DBResult.Success)
                                        {
                                            if (bCheckOutside)
                                            {
                                                if (IsEmpty_DD)
                                                {
                                                    CmdMstInfo cmd_DD = new CmdMstInfo();
                                                    iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, ref cmd_DD);
                                                    if (iRet == DBResult.Success)
                                                    {
                                                        if (
                                                            cmd_DD.CmdMode == clsConstValue.CmdMode.StockOut ||
                                                            (cmd_DD.CmdMode == clsConstValue.CmdMode.L2L &&
                                                             cmd_DD.Loc == sLocDD && cmd_DD.Loc != cmd_DD.NewLoc)
                                                            ||
                                                            cmd_DD.CmdSts == clsConstValue.CmdSts.strCmd_Initial ||
                                                            cmd_DD.CurDeviceID != EquNo.ToString()
                                                           )
                                                        {
                                                            if (clsPickUp.FunShelfToFork_Proc(row)) return true;
                                                            else continue;
                                                        }
                                                        else
                                                        {
                                                            if (cmd_DD.CurLoc == LocationDef.Location.LeftFork.ToString() ||
                                                                cmd_DD.CurLoc == LocationDef.Location.RightFork.ToString())
                                                            {
                                                                iRet = clsDB_Proc.GetDB_Object().GetTask().CheckHasTaskCmd(cmd_DD.CmdSno);
                                                                if (iRet == DBResult.Success || iRet == DBResult.Exception)
                                                                {
                                                                    sRemark = $"Error: 等候內儲位命令完成 => {sLocDD}";
                                                                    if (sRemark != sRemark_Pre)
                                                                    {
                                                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                    }

                                                                    continue;
                                                                }
                                                                else
                                                                {
                                                                    if (clsPickUp.FunShelfToFork_Proc(row)) return true;
                                                                    else continue;
                                                                }
                                                            }
                                                            else if (cmd_DD.CurLoc == LocationDef.Location.Shelf.ToString())
                                                            {
                                                                sRemark = $"Error: 等候內儲位命令完成 => {sLocDD}";
                                                                if (sRemark != sRemark_Pre)
                                                                {
                                                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                }

                                                                continue;
                                                            }
                                                            else
                                                            {
                                                                if (clsPickUp.FunShelfToFork_Proc(row)) return true;
                                                                else continue;
                                                            }
                                                        }
                                                    }
                                                    else if (iRet == DBResult.NoDataSelect)
                                                    {
                                                        if (clsPickUp.FunShelfToFork_Proc(row)) return true;
                                                        else continue;
                                                    }
                                                    else
                                                    {
                                                        sRemark = $"Error: 找尋內儲位命令失敗 => {sLocDD}";
                                                        if (sRemark != sRemark_Pre)
                                                        {
                                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                        }

                                                        continue;
                                                    }
                                                }
                                                else
                                                {
                                                    #region 更新NeedShelfToShelf欄位為Y
                                                    if (clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateNeedL2L(sCmdSno, clsEnum.NeedL2L.Y))
                                                        return true;
                                                    else
                                                    {
                                                        sRemark = $"Error: {sCmdSno}更新NeedShelfToShelf欄位失敗！";
                                                        if (sRemark != sRemark_Pre)
                                                        {
                                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                        }

                                                        continue;
                                                    }
                                                    #endregion 更新NeedShelfToShelf欄位為Y
                                                }
                                            }
                                            else
                                            {
                                                if (sCmdMode == clsConstValue.CmdMode.L2L)
                                                {
                                                    CmdMstInfo cmd_DD = new CmdMstInfo();
                                                    iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCheckHasCommand(sLocDD, ref cmd_DD);
                                                    if (iRet == DBResult.Success)
                                                    {
                                                        if (cmd_DD.CmdSts == clsConstValue.CmdSts.strCmd_Initial &&
                                                            cmd_DD.EquNo == EquNo.ToString() && cmd_DD.CmdMode != clsConstValue.CmdMode.StockIn)
                                                        {
                                                            if (!DB.Object.clsTool.CheckCanGoForStockOut(sCmdSno, sRemark_Pre, ref sRemark))
                                                            {
                                                                sRemark = $"Error: 等候大循環消化料盒";
                                                                if (sRemark != sRemark_Pre)
                                                                {
                                                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                }
                                                                continue;
                                                            }

                                                            if (string.IsNullOrWhiteSpace(cmd_DD.BatchID))
                                                            {
                                                                if (cmd_DD.CmdMode != clsConstValue.CmdMode.L2L)
                                                                {
                                                                    if (clsMicronCV.StockOutPortIsAllNoReady(EquNo))
                                                                    {
                                                                        sRemark = $"Error: Stocker{EquNo}的出庫站皆不是出庫Ready ({(int)clsEnum.Ready.OUT})";
                                                                        if (sRemark != sRemark_Pre)
                                                                        {
                                                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                        }

                                                                        continue;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (!clsMicronCV.StockOutPortIsAllReady(EquNo))
                                                                {
                                                                    sRemark = $"Error: Stocker{EquNo}的出庫站並非都是出庫Ready ({(int)clsEnum.Ready.OUT})";
                                                                    if (sRemark != sRemark_Pre)
                                                                    {
                                                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                    }

                                                                    continue;
                                                                }
                                                            }

                                                            iRet = clsDB_Proc.GetDB_Object().GetTask().CheckHasTaskCmd(EquNo);
                                                            if (iRet == DBResult.Success)
                                                            {
                                                                sRemark = $"Error: 等候Stocker{EquNo}的Fork命令淨空";
                                                                if (sRemark != sRemark_Pre)
                                                                {
                                                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                }

                                                                continue;
                                                            }
                                                            else if (iRet == DBResult.Exception)
                                                            {
                                                                sRemark = $"Error: 取得Stocker{EquNo}的Fork命令失敗！";
                                                                if (sRemark != sRemark_Pre)
                                                                {
                                                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                                }

                                                                continue;
                                                            }
                                                        }
                                                    }
                                                    else if (iRet == DBResult.Exception) continue;
                                                }

                                                if (clsPickUp.FunShelfToFork_Proc(row)) return true;
                                                else continue;
                                            }
                                        }
                                        else
                                        {
                                            sRemark = $"Error: 取得儲位資料失敗 => {sLoc}";
                                            if (sRemark != sRemark_Pre)
                                            {
                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                            }

                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        else continue;
                    }
                    else { }
                }

                return false;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
            finally
            {
                dtTmp = null;
            }
        }

        private bool FunBatchProc_WMS(DataGridView oGrid, int EquNo)
        {
            try
            {
                IEnumerable<DataGridViewRow>  obj = (from DataGridViewRow drv in oGrid.Rows
                       where (
                              Convert.ToInt32(drv.Cells[ColumnDef.CMD_MST.EquNO.Index].Value) == EquNo ||
                              (Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurDeviceID.Index].Value) == EquNo.ToString() &&
                               Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != MicronLocation.GetLocation_ByStockOutPort(EquNo).LocationId)
                             ) &&
                       Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_01.ToString() &&
                       Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_07.ToString() &&
                       Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_13.ToString() &&
                       Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) != LocationDef.Location.A1_19.ToString() &&
                       Convert.ToString(drv.Cells[ColumnDef.CMD_MST.BatchID.Index].Value) != ""
                       select drv);
                if (obj == null || obj.Count() == 0) return false;

                Dictionary<string, string> Batches = new Dictionary<string, string>();
                foreach (var row in obj)
                {
                    string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                    string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                    string sRemark = "";
                    string BatchID = Convert.ToString(row.Cells[ColumnDef.CMD_MST.BatchID.Index].Value);
                    if (Batches.TryGetValue(BatchID, out sRemark))
                    {
                        if (sRemark != sRemark_Pre)
                        {
                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        }

                        continue;
                    }

                    string sCmdMode = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value);
                    if(sCmdMode != clsConstValue.CmdMode.StockOut)
                    {
                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCancelBatch(sCmdMode, BatchID);

                        sRemark = "Error: 只有出庫才有Batch！";
                        Batches.Add(BatchID, sRemark);
                        continue;
                    }

                    string sCmdSts = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value);
                    string sStnNoList = Convert.ToString(row.Cells[ColumnDef.CMD_MST.StnNo.Index].Value);
                    //bool bCheckCanDo = false;
                    //if (sCmdSts == clsConstValue.CmdSts.strCmd_Initial)
                    //{
                    //    string[] sStnNo = sStnNoList.Split(',');
                    //    if (sStnNo.Length > 1) bCheckCanDo = true;
                    //    else bCheckCanDo = clsTool.FunCheckIsCanGo(sStnNoList, oGrid);
                    //}
                    //else bCheckCanDo = true;

                    //if (!bCheckCanDo)
                    //{
                    //    sRemark = $"Error: Port{sStnNoList}料盒數量水位已到極限！";
                    //    if (sRemark != sRemark_Pre)
                    //    {
                    //        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    //    }

                    //    Batches.Add(BatchID, sRemark);
                    //    continue;
                    //}

                    if (!DB.Object.clsTool.CheckSts(EquNo, sCmdSno, sRemark_Pre, ref sRemark))
                    {
                        Batches.Add(BatchID, sRemark);
                        continue;
                    }

                    IEnumerable<DataGridViewRow> obj_Batch = (from DataGridViewRow drv in obj
                                                              where Convert.ToString(drv.Cells[ColumnDef.CMD_MST.BatchID.Index].Value) == BatchID
                                                              select drv);

                    
                    bool checkL2LCorrectBatch = true;
                    for (int fork = 1; fork <= 2; fork++)
                    {
                        if (clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).GetForkById(fork).HasCarrier &&
                           string.IsNullOrWhiteSpace(clsMicronStocker.GetCommand(EquNo, fork).BatchID))
                        {
                            if (EquNo != 4 && clsMicronStocker.GetCommand(EquNo, fork).CmdMode == clsConstValue.CmdMode.L2L)
                            {
                                string sLoc_Check = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().GetLocDD(clsMicronStocker.GetCommand(EquNo, fork).Loc);
                                if (obj_Batch.Any(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.Loc.Index].Value) == sLoc_Check))
                                {
                                    break;
                                }
                                else
                                {
                                    sRemark = $"Error: 等候Stocker{EquNo}執行完Fork上非Batch的命令";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }
                                    checkL2LCorrectBatch = false;
                                    break;
                                }
                            }
                            else
                            {
                                sRemark = $"Error: 等候Stocker{EquNo}執行完Fork上非Batch的命令";
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }

                                return false;
                            }
                        }                        
                        //else
                        //{
                        //    CmdMstInfo cmd = new CmdMstInfo();
                        //    int iRet = clsDB_Proc.GetDB_Object().GetTask().GetForkCommand(EquNo, fork, ref cmd);
                        //    if (iRet == DBResult.Success)
                        //    {
                        //        if (string.IsNullOrWhiteSpace(cmd.BatchID) && cmd.CmdMode != clsConstValue.CmdMode.L2L)
                        //        {
                        //            sRemark = $"Error: 等候Stocker{EquNo}執行完Fork上非Batch的命令";
                        //            if (sRemark != sRemark_Pre)
                        //            {
                        //                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        //            }

                        //            Batches.Add(BatchID, sRemark);
                        //            bFlag = false; break;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (iRet == DBResult.Exception)
                        //        {
                        //            sRemark = $"Error:取得Stocker{EquNo}的Fork{fork}命令失敗";
                        //            if (sRemark != sRemark_Pre)
                        //            {
                        //                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                        //            }

                        //            Batches.Add(BatchID, sRemark);
                        //            bFlag = false; break;
                        //        }
                        //    }
                        //}
                    }
                    if (!checkL2LCorrectBatch)
                    {
                        continue;
                    }

                    if (obj_Batch.Count() > 1)
                    {
                        if (clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).TwinForkIsLoad)
                        {
                            if (clsMicronStocker.GetCommand(EquNo, 1).CmdMode == clsConstValue.CmdMode.L2L)
                            {
                                if (FunBatchCombineL2LProc_TwinForkAllLoad(obj_Batch, EquNo, 1)) return true;
                                else continue;
                            }
                            else if (clsMicronStocker.GetCommand(EquNo, 2).CmdMode == clsConstValue.CmdMode.L2L)
                            {
                                if (FunBatchCombineL2LProc_TwinForkAllLoad(obj_Batch, EquNo, 2)) return true;
                                else continue;
                            }
                            else
                            {
                                bool bCheck = false;
                                for (int fork = 1; fork <= 2; fork++)
                                {
                                    bCheck = obj_Batch.Any(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value) ==
                                                                clsMicronStocker.GetCommand(EquNo, fork).CommandID);
                                    if (bCheck == false) break;
                                }

                                if (bCheck == false)
                                {
                                    sRemark = $"Error: 等候Stocker{EquNo}執行完Fork上的命令({clsMicronStocker.GetCommand(EquNo, 1).CommandID}," +
                                        $"{clsMicronStocker.GetCommand(EquNo, 2).CommandID})";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }

                                    Batches.Add(BatchID, sRemark);
                                    continue;
                                }
                                else
                                {
                                    if (clsDeposit.FunBatchCmdDeposit_Proc(obj_Batch)) return true;
                                    else
                                    {
                                        sRemark = "Error: 下達Batch命令失敗！";
                                        if (sRemark != sRemark_Pre)
                                        {
                                            clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                        }

                                        Batches.Add(BatchID, sRemark);
                                        continue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            TransferBatch cmd = new TransferBatch
                            {
                                CommandID = sCmdSno,
                                CmdMode = sCmdMode,
                                Remark = sRemark_Pre,
                                BatchID = BatchID
                            };

                            if (clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).GetForkById(1).HasCarrier)
                            {
                                if (clsDB_Proc.FunBatchAndAnotherHasCarrior_Proc(obj_Batch, EquNo, 1, cmd)) return true;
                                else
                                {
                                    sRemark = "Error: clsDB_Proc.FunBatchAndAnotherHasCarrior_Proc";
                                    Batches.Add(BatchID, sRemark);
                                    continue;
                                }
                            }
                            else if (clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).GetForkById(2).HasCarrier)
                            {
                                if (clsDB_Proc.FunBatchAndAnotherHasCarrior_Proc(obj_Batch, EquNo, 2, cmd)) return true;
                                else
                                {
                                    sRemark = "Error: clsDB_Proc.FunBatchAndAnotherHasCarrior_Proc";
                                    Batches.Add(BatchID, sRemark);
                                    continue;
                                }
                            }
                            else
                            {
                                #region 兩隻Fork皆荷無

                                //優先度為1 不判斷訂單號
                                
                                bool bTicketCanGo = false;
                                foreach (var obj_tick in obj_Batch)
                                {
                                    string sTicketId = Convert.ToString(obj_tick.Cells[ColumnDef.CMD_MST.TicketId.Index].Value);
                                    string sLoc = Convert.ToString(obj_tick.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                                    string sStnNoList_tick = Convert.ToString(obj_tick.Cells[ColumnDef.CMD_MST.StnNo.Index].Value);
                                    string[] sStnNo_tick = sStnNoList_tick.Split(',');
                                    int iPRT = Convert.ToInt32(obj_tick.Cells[ColumnDef.CMD_MST.PRT.Index].Value);
                                    if (iPRT == 1)
                                    {
                                        bTicketCanGo = true;
                                        break;
                                    }
                                    if (DB.Object.clsTool.CheckCanGoForTicket(sCmdSno, sLoc, sTicketId, sStnNo_tick[0], EquNo, sCmdMode, BatchID, sRemark_Pre, ref sRemark))
                                    {
                                        bTicketCanGo = true;
                                        break;
                                    }

                                }
                                if (!bTicketCanGo)
                                {
                                    Batches.Add(BatchID, sRemark);
                                    continue;
                                }

                                if (!clsMicronCV.StockOutPortIsAllReady(EquNo))
                                {
                                    sRemark = $"Error: Stocker{EquNo}的兩個出庫站並非都是出庫Ready ({(int)clsEnum.Ready.OUT})";
                                    Batches.Add(BatchID, sRemark);
                                    continue;
                                }

                                if (!DB.Object.clsTool.CheckCanGoForStockOut(sCmdSno, sRemark_Pre, ref sRemark))
                                {
                                    sRemark = $"Error: 等候大循環消化料盒";
                                    if (sRemark != sRemark_Pre)
                                    {
                                        clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                    }
                                    continue;
                                }

                                if (EquNo == 4)
                                {
                                    #region Single Deep
                                    if (clsPickUp.FunPickupProc_ForStockOutBatch(obj_Batch, EquNo, ref sRemark)) return true;
                                    else
                                    {
                                        Batches.Add(BatchID, sRemark);
                                        continue;
                                    }
                                    #endregion Single Deep
                                }
                                else
                                {
                                    bool bCheckL2L = obj_Batch.Any(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.NeedShelfToShelf.Index].Value) == clsEnum.NeedL2L.Y.ToString());
                                    if (bCheckL2L)
                                    {
                                        var obj_NeedL2L = obj_Batch.Where(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.NeedShelfToShelf.Index].Value) == clsEnum.NeedL2L.Y.ToString());
                                        foreach (var NeedL2L in obj_NeedL2L)
                                        {
                                            if (clsPickUp.FunShelfToFork_NeedL2L_Proc(NeedL2L, ref sRemark)) return true;
                                            else
                                            {
                                                Batches.Add(BatchID, sRemark);
                                                break;
                                            }
                                        }

                                        continue;
                                    }
                                    else
                                    {
                                        LocCheckInfo[] locCheckInfo = new LocCheckInfo[obj_Batch.Count()];
                                        int i = 0; bool bGetOut = false;
                                        foreach (var batch_out in obj_Batch)
                                        {
                                            bool IsOutside = false; string sLocDD = ""; bool IsEmpty_DD = false; string BoxID_DD = "";
                                            locCheckInfo[i] = new LocCheckInfo
                                            {
                                                Loc = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.Loc.Index].Value)
                                            };
                                            int iRet = clsDB_Proc.GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().CheckLocIsOutside(locCheckInfo[i].Loc, ref IsOutside,
                                                ref sLocDD, ref IsEmpty_DD, ref BoxID_DD);
                                            if (iRet == DBResult.Success)
                                            {
                                                locCheckInfo[i].IsOutside = IsOutside;
                                                locCheckInfo[i].LocDD = sLocDD;
                                                locCheckInfo[i].IsEmpty_DD = IsEmpty_DD;
                                                locCheckInfo[i].BoxID_DD = BoxID_DD;

                                                if (locCheckInfo[i].IsOutside)
                                                {
                                                    locCheckInfo[i].CmdSno_Outside = Convert.ToString(batch_out.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                                                    //if (IsEmpty_DD)
                                                    //{
                                                    //    if (clsPickUp.FunPickupProc_ForStockOut(batch_out, EquNo, ref sRemark)) return true;
                                                    //    else
                                                    //    {
                                                    //        bGetOut = true;
                                                    //        Batches.Add(BatchID, sRemark);
                                                    //        break;
                                                    //    }
                                                    //}
                                                    if(!locCheckInfo[i].IsEmpty_DD)
                                                    {
                                                        #region 更新NeedShelfToShelf欄位為Y
                                                        if (clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateNeedL2L(locCheckInfo[i].CmdSno_Outside, clsEnum.NeedL2L.Y))
                                                            return true;
                                                        else
                                                        {
                                                            bGetOut = true;
                                                            sRemark = $"Error: {locCheckInfo[i].CmdSno_Outside}更新NeedShelfToShelf欄位失敗！";
                                                            if (sRemark != sRemark_Pre)
                                                            {
                                                                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(locCheckInfo[i].CmdSno_Outside, sRemark);
                                                            }

                                                            Batches.Add(BatchID, sRemark);
                                                            break;
                                                        }
                                                        #endregion 更新NeedShelfToShelf欄位為Y
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bGetOut = true;
                                                sRemark = $"Error: 取得儲位資料失敗 => {locCheckInfo[i].Loc}";
                                                if (sRemark != sRemark_Pre)
                                                {
                                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                                }

                                                Batches.Add(BatchID, sRemark);
                                                break;
                                            }

                                            i++;
                                        }

                                        if (bGetOut) continue;
                                        bool CheckCanDo = true;
                                        for (int check = 0; check < locCheckInfo.Length; check++)
                                        {
                                            if (locCheckInfo[check].IsOutside && !locCheckInfo[check].IsEmpty_DD)
                                            {
                                                CheckCanDo = false; break;
                                            }
                                        }

                                        if (CheckCanDo)
                                        {
                                            if (clsPickUp.FunPickupProc_ForStockOutBatch(obj_Batch, EquNo, ref sRemark)) return true;
                                            //foreach (var batch_out in obj_Batch)
                                            //{
                                            //    if (clsPickUp.FunPickupProc_ForStockOut(batch_out, EquNo, ref sRemark)) return true;
                                            else
                                            {
                                                Batches.Add(BatchID, sRemark);
                                                continue;
                                            }
                                            //}
                                        }

                                        continue;
                                    }
                                }
                                #endregion 兩隻Fork皆荷無
                            }
                        }
                    }
                    else
                    {
                        #region 單筆Batch命令的處理流程
                        foreach (var singleData in obj_Batch)
                        {
                            string sCmdSno_Single = Convert.ToString(singleData.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                            string sCmdSts_Single = Convert.ToString(singleData.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value);
                            string sCrtDate_Single = Convert.ToString(singleData.Cells[ColumnDef.CMD_MST.CrtDate.Index].Value);
                            string sExpDate_Single = Convert.ToString(singleData.Cells[ColumnDef.CMD_MST.ExpDate.Index].Value);

                            TimeSpan ts1;
                            TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
                            if (sCmdSts_Single == clsConstValue.CmdSts.strCmd_Initial)
                            {
                                ts1 = new TimeSpan(DateTime.ParseExact(sCrtDate_Single,
                                              "yyyy-MM-dd HH:mm:ss",
                                              System.Globalization.CultureInfo.InvariantCulture
                                              ).Ticks);
                            }
                            else
                            {
                                ts1 = new TimeSpan(DateTime.ParseExact(sExpDate_Single,
                                              "yyyy-MM-dd HH:mm:ss",
                                              System.Globalization.CultureInfo.InvariantCulture
                                              ).Ticks);
                            }

                            TimeSpan ts = ts1.Subtract(ts2).Duration();
                            if (ts.TotalMinutes > 5)
                            {
                                if (clsDB_Proc.GetDB_Object().GetCmd_Mst().FunCancelBatch(sCmdSno_Single)) return true;
                                else
                                {
                                    sRemark = $"Error: {sCmdSno_Single}取消Batch失敗！";
                                    Batches.Add(BatchID, sRemark);
                                    break;
                                }
                            }
                            else
                            {
                                sRemark = $"Error: 等候Batch{BatchID}的下一筆命令";
                                if (sRemark != sRemark_Pre)
                                {
                                    clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                }

                                Batches.Add(BatchID, sRemark);
                                break;
                            }
                        }

                        continue;
                        #endregion 單筆Batch命令的處理流程
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        private bool FunBatchCombineL2LProc_TwinForkAllLoad(IEnumerable<DataGridViewRow> obj_Batch, int StockerID, int Fork)
        {
            try
            {
                int NextFork = Fork switch
                {
                    1 => 2,
                    _ => 1,
                };

                if (clsMicronStocker.GetCommand(StockerID, Fork).CmdMode == clsConstValue.CmdMode.L2L &&
                    !string.IsNullOrWhiteSpace(clsMicronStocker.GetCommand(StockerID, NextFork).BatchID))
                {
                    if (Micron.U2NMMA30.clsTool.funGetEquNoByLoc(clsMicronStocker.GetCommand(StockerID, Fork).NewLoc) == StockerID)
                    {
                        bool bCheck = obj_Batch.Any(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value) ==
                                                          clsMicronStocker.GetCommand(StockerID, NextFork).CommandID);
                        if (bCheck)
                        {
                            return clsDB_Proc.GetDB_Object().GetProcess().FunDepositToShelf_Proc(StockerID, Fork, clsConstValue.CmdMode.L2L);
                        }
                        else return false;
                    }
                    else
                    {
                        return clsDeposit.SubDepositToOutPortCV_Proc(clsMicronStocker.GetCommand(StockerID, Fork), StockerID, Fork);
                    }
                }
                else return false;
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 判斷WCS命令是否完成
        /// </summary>
        private void FunCheckWCS_CmdFinish()
        {
            string sCmdSno_Ex = "";
            try
            {
                if (Grid1.RowCount > 0)
                {
                    for (int i = 0; i <= Grid1.RowCount - 1; i++)
                    {
                        string sCmdSno = Convert.ToString(Grid1[ColumnDef.CMD_MST.CmdSno.Index, i].Value);
                        sCmdSno_Ex = sCmdSno;
                        string sCmdMode = Convert.ToString(Grid1[ColumnDef.CMD_MST.CmdMode.Index, i].Value);
                        string sCurLoc = Convert.ToString(Grid1[ColumnDef.CMD_MST.CurLoc.Index, i].Value);
                        string sStnNoList = Convert.ToString(Grid1[ColumnDef.CMD_MST.StnNo.Index, i].Value);

                        if (
                            sCmdMode == clsConstValue.CmdMode.StockOut &&
                            (
                             sCurLoc == ConveyorDef.A1_01.bufferLocation.LocationId ||
                             sCurLoc == ConveyorDef.A1_07.bufferLocation.LocationId ||
                             sCurLoc == ConveyorDef.A1_13.bufferLocation.LocationId ||
                             sCurLoc == ConveyorDef.A1_19.bufferLocation.LocationId
                            )
                            &&
                            string.IsNullOrWhiteSpace(sStnNoList)
                           )
                        {
                            string sRemark = "出庫命令已完成";
                            if (clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateCmdSts(sCmdSno, clsConstValue.CmdSts.strCmd_Finished, sRemark)) return;
                            else continue;
                        }
                        else if (
                                  (sCmdMode == clsConstValue.CmdMode.StockIn || sCmdMode == clsConstValue.CmdMode.L2L)
                                  &&
                                  sCurLoc == LocationDef.Location.Shelf.ToString()
                                )
                        {
                            if (clsDB_Proc.GetDB_Object().GetProcess().FunStockIn_L2L_Finish_Proc(sCmdSno)) return;
                            else continue;
                        }
                        else if (sCmdMode == clsConstValue.CmdMode.L2L && sCurLoc == LocationDef.Location.Teach.ToString())
                        {
                            if (clsDB_Proc.GetDB_Object().GetProcess().FunL2LToTeach_Finish_Proc(sCmdSno)) return;
                            else continue;
                        }
                        else continue;
                    }
                }
            }
            catch (Exception ex)
            {
                clsDB_Proc.GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno_Ex, ex.Message);
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }

        private void FunInit()
        {
            var archive = new AutoArchive();
            archive.Start();
            clsDB_Proc.Initial(clInitSys.DbConfig, clInitSys.DbConfig_WMS, clInitSys.OEEparamConfig);
            clsMicronCV.FunInitalCVController(clInitSys.CV_Config);
            clsMicronStocker.StockerInitial(clInitSys.lcsini);
            clsWmsApi.FunInit(clInitSys.WmsApi_Config);

            bool bFlag;
            do
            {
                bFlag = clsDB_Proc.GetDB_Object().GetProcess().GetDevicePortProc();
                Task.Delay(500).Wait();
            } while (!bFlag);

            MicronLocation.FunMapPort(clsDB_Proc.GetDB_Object().GetLstPort(), clInitSys.gsStockerID);
            CheckPathIsWork.subStart();
            stockoutToStnCheck.subStart();
            autoPickup_Proc.subStart();
            putawayCheck_Proc.subStart();
            bcr_ForCraneStockInPort.subStart();
            updLoc_ForCraneStockInPort.subStart();
            reportPortSts_Proc.subStart();
            reUpdateTaskCompleteCmd_Proc.subStart();
            cycleRun.subStart();
            toTeach_Proc.subStart();
            carrierWithoudCmd_Proc.subStart(); 
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterInstance(new WCSController());
            _webApiHost = new WebApiHost(new Startup(_unityContainer), clInitSys.WcsApi_Config.IP);
            clearCmd = new DB.ClearCmd.Proc.clsHost();
            ChangeSubForm(clsMicronCV.GetConveyorController().GetMainView());
            FunInitStockerStsForm();
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

        private void FunInitStockerStsForm()
        {
            for (int i = 1; i < 5; i++)
            {
                var subForm = clsMicronStocker.GetSTKCHostById(i).GetStockerStsView();
                subForm.TopLevel = false;
                subForm.Dock = DockStyle.Fill;//適應窗體大小
                subForm.FormBorderStyle = FormBorderStyle.None;//隱藏右上角的按鈕
                subForm.Parent = tlpMainSts;
                tlpMainSts.Controls.Add(subForm, i + 1, 0);
                subForm.Show();
            }
        }

        
    }
}
