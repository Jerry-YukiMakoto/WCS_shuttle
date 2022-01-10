using System;
using System.Collections.Generic;
using Mirle.ASRS.WCS.Controller;
using Mirle.DB.Fun;
using Mirle.Def;
using Mirle.DataBase;
using Mirle.Def.U0NXMA30;
using Mirle.Grid.U0NXMA30;
using Mirle.Structure;
using Mirle.Structure.Info;
using System.Linq;
using WCS_API_Client.ReportInfo;
using System.Data;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.ASRS.WCS.Model.DataAccess;

namespace Mirle.DB.Proc
{
    public class clsProc
    {
        private clsPortDef PortDef = new clsPortDef();
        private Fun.clsCmd_Mst CMD_MST = new Fun.clsCmd_Mst();
        private Fun.clsEqu_Cmd EQU_CMD = new Fun.clsEqu_Cmd();
        private Fun.clsSno SNO = new Fun.clsSno();
        private Fun.clsLocMst LocMst = new Fun.clsLocMst();
        private Fun.clsProc proc;
        private Fun.clsAlarmData alarmData = new Fun.clsAlarmData();
        private Fun.clsCmd_Mst_His CMD_MST_HIS = new Fun.clsCmd_Mst_His();
        private Fun.clsUnitStsLog unitStsLog = new Fun.clsUnitStsLog();

        public List<Element_Port>[] GetLstPort()
        {
            return PortDef.GetLstPort();
        }

        private clsDbConfig _config = new clsDbConfig();
        private clsDbConfig _config_WMS = new clsDbConfig();
        private clsDbConfig _config_Sqlite = new clsDbConfig();
        public clsProc(clsDbConfig config, clsDbConfig config_WMS, clsDbConfig config_Sqlite)
        {
            _config = config;
            _config_WMS = config_WMS;
            _config_Sqlite = config_Sqlite;
            proc = new Fun.clsProc(_config_WMS);
        }

        public Fun.clsProc GetFunProcess()
        {
            return proc;
        }

        public bool FunMoveTaskForceClear(string taskNo, ref string strEM)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CmdMstInfo cmd = new CmdMstInfo();
                        if (CMD_MST.FunGetCommand_byTaskNo(taskNo, ref cmd, db))
                        {
                            if (cmd.CmdSts == clsConstValue.CmdSts.strCmd_Running)
                            {
                                strEM = "Error: 命令已開始執行，無法取消！";
                                return false;
                            }

                            int iRet_Task = EQU_CMD.CheckHasEquCmd(cmd.CmdSno, db);
                            if (iRet_Task == DBResult.Exception)
                            {
                                strEM = "取得Task命令失敗！";
                                return false;
                            }

                            

                            if (iRet_Task == DBResult.Success) EQU_CMD.FunInsertHisEquCmd(cmd.CmdSno, db);

                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                strEM = "Error: Begin失敗！";
                                if (strEM != cmd.Remark)
                                {
                                    CMD_MST.FunUpdateRemark(cmd.CmdSno, strEM, db);
                                }

                                return false;
                            }

                            if (CMD_MST.UpdateCmdMst(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Cancel, "WMS命令取消", db) == ExecuteSQLResult.Initial)
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet_Task == DBResult.Success)
                            {
                                if (EQU_CMD.DeleteEquCmd(cmd.CmdSno, db) == ExecuteSQLResult.Initial)
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            

                            db.TransactionCtrl(TransactionTypes.Commit);
                            return true;
                        }
                        else
                        {
                            strEM = $"<taskNo> {taskNo} => 取得命令資料失敗！";
                            return false;
                        }
                    }
                    else
                    {
                        strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                strEM = ex.Message;
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }

        public bool FunStockInWriPlc(string sStnNo, int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.GetCmdMstByStoreInstart(sStnNo, out var dataObject, db) == GetDataResult.Success) //讀取CMD_MST
                        {
                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            int IOType = Convert.ToInt32(dataObject[0].IOType);
                            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();

                            if (IOType == IOtype.NormalstorIn
                             && _conveyor.GetBuffer(bufferIndex).Auto
                            && _conveyor.GetBuffer(bufferIndex).InMode
                            && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                            && _conveyor.GetBuffer(bufferIndex).Presence == false
                            && _conveyor.GetBuffer(bufferIndex).Error == false
                            && _conveyor.GetBuffer(bufferIndex - 2).Ready == Ready.StoreInReady
                            && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
                            && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
                            && _conveyor.GetBuffer(bufferIndex - 2).CmdMode != 3   //為了不跟盤點命令衝突的條件
                             && _conveyor.GetBuffer(bufferIndex + 1).Presence == true) //在一般入庫時要確認A4是否有空棧板，沒有則不寫入命令
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");

                                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInWriteCmdToCV, db) == ExecuteSQLResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }

                                var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                bool Result = WritePlccheck.Item1;
                                string exmessage = WritePlccheck.Item2;
                                if (Result != true)//寫入命令和模式
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                if (IOType == IOtype.NormalstorIn)
                                {
                                    WritePlccheck = _conveyor.GetBuffer(4).A4EmptysupplyOn().Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                    Result = WritePlccheck.Item1;
                                    exmessage = WritePlccheck.Item2;
                                    if (Result != true)//請A4補充母托一版
                                    {
                                        clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC A4EmptySupply Fail:{exmessage}");

                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                }
                                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }

                                return true;
                            }
                            else if (IOType == IOtype.NormalstorIn //待確認類別，目前尚未確定
                            && _conveyor.GetBuffer(bufferIndex).Auto
                            && _conveyor.GetBuffer(bufferIndex).InMode
                            && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                            && _conveyor.GetBuffer(bufferIndex).Presence == false
                            && _conveyor.GetBuffer(bufferIndex).Error == false
                            && _conveyor.GetBuffer(bufferIndex - 2).Ready == Ready.StoreInReady
                            && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
                            && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
                            && _conveyor.GetBuffer(bufferIndex - 2).CmdMode != 3)  //為了不跟盤點命令衝突的條件
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");

                                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInWriteCmdToCV, db) == ExecuteSQLResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Update cmd Success => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                bool Result = WritePlccheck.Item1;
                                string exmessage = WritePlccheck.Item2;
                                if (Result != true)//寫入命令和模式
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }

                                return true;
                            }
                            #region 站口狀態自動確認-Update-CMD-Remark
                            else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotInMode, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 1).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 2).CmdMode == 3)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CycleOperating, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CmdLeftOver, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex + 1).Presence == false)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.A4EmptyisEmpty, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex - 2).Ready != Ready.StoreInReady)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotStoreInReady, db);
                                return false;
                            }
                            else return false;
                            #endregion
                        }
                        else return false;
                    }
                    else
                    {
                        string strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
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

        public bool FunStockInA2ToA4WriPlc(string sStnNo, int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.GetCmdMstByStoreInstart(sStnNo, out var dataObject, db) == GetDataResult.Success)
                        {
                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                            if (_conveyor.GetBuffer(bufferIndex).Auto
                            && _conveyor.GetBuffer(bufferIndex).InMode
                            && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                            && _conveyor.GetBuffer(bufferIndex).Presence == false
                            && _conveyor.GetBuffer(bufferIndex).Error == false
                            && _conveyor.GetBuffer(bufferIndex - 1).Ready == Ready.StoreInReady
                            && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
                            && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3) //為了不跟盤點命令衝突的條件)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");

                                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInWriteCmdToCV, db) == ExecuteSQLResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                bool Result = WritePlccheck.Item1;
                                string exmessage = WritePlccheck.Item2;
                                if (Result != true)//寫入命令和模式
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                else return true;

                            }
                            #region 站口狀態自動確認-Update-CMD-Remark
                            else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotInMode, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 1).CmdMode == 3)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CycleOperating, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CmdLeftOver, db);
                                return false;
                            }
                            else if (_conveyor.GetBuffer(bufferIndex - 1).Ready != Ready.StoreInReady)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotStoreInReady, db);
                                return false;
                            }
                            else return false;
                            #endregion
                        }
                        else return false;
                    }
                    else
                    {
                        string strEM = "Error: 開啟DB失敗！";
                        clsWriLog.Log.FunWriTraceLog_CV(strEM);
                        return false;
                    }
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


    }
}
