﻿using System;
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
using Mirle.CENS.U0NXMA30;

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
                                strEM = "取得設備命令失敗！";
                                return false;
                            }



                            if (iRet_Task == DBResult.Success) EQU_CMD.FunInsertHisEquCmd(cmd.CmdSno, db);

                            if (db.TransactionCtrl(TransactionTypes.Begin) != DBResult.Success)
                            {
                                strEM = "Error: Begin失敗！";
                                if (strEM != cmd.Remark)
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmd.CmdSno, strEM, db);
                                }

                                return false;
                            }

                            if (CMD_MST.UpdateCmdMstRemark(cmd.CmdSno, clsConstValue.CmdSts.strCmd_Cancel, "WMS命令取消", db).ResultCode != DBResult.Success)
                            {
                                db.TransactionCtrl(TransactionTypes.Rollback);
                                return false;
                            }

                            if (iRet_Task == DBResult.Success)
                            {
                                if (EQU_CMD.DeleteEquCmd(cmd.CmdSno, db).ResultCode != DBResult.Success)
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

        #region StoreIn
        public bool FunStoreInWriPlc(string sStnNo, int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.GetCmdMstByStoreInStart(sStnNo, out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            int IOType = Convert.ToInt32(dataObject[0].IOType);
                            int pickup = Convert.ToInt32(dataObject[0].pickup);
                            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();

                            clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Get StoreIn Command => {cmdSno}, " +
                                    $"{CmdMode}");

                            #region//根據buffer狀態更新命令
                            if (_conveyor.GetBuffer(bufferIndex).Auto != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).InMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotInMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CmdLeftOver, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).CmdMode == 6 || _conveyor.GetBuffer(bufferIndex - 1).CmdMode == 6 || _conveyor.GetBuffer(bufferIndex - 2).CmdMode == 6)//為了不跟撿料命令衝突的條件
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CycleOperating, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex - 2).Ready != Ready.StoreInReady)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotStoreInReady, db);
                                return false;
                            }
                            //&& _conveyor.GetBuffer(bufferIndex + 1).Presence == true) //在一般入庫時要確認A4是否有空棧板，沒有則不寫入命令=>目前不加入條件因為會與空棧版入庫衝突
                            #endregion

                            clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready Receive StoreIn Command=> {cmdSno}");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInWriteCmdToCV, db).ResultCode == DBResult.Success)
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
                            bool Result = WritePlccheck;
                            if (Result != true)//寫入命令和模式
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (IOType == IOtype.NormalstorIn && pickup==1)
                            {
                                WritePlccheck = _conveyor.GetBuffer(4).A4EmptysupplyOn().Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                Result = WritePlccheck;
                                if (Result != true)//請A4補充母托一版
                                {
                                    clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC A4EmptySupply Fail");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else
                            {
                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    lineId = "1",
                                    locationID = "1",
                                    taskNo = cmdSno.ToString(),
                                    state = "1", //任務開始
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    return false;
                                }
                                //填入訊息
                                TaskStateUpdateInfo info1 = new TaskStateUpdateInfo
                                {
                                    lineId = "1",
                                    taskNo = cmdSno,
                                    palletNo = cmdSno,
                                    businessType = IOType.ToString(),
                                    state = "12",
                                    errMsg = ""
                                };
                                if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info1))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                                return true;
                            }
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

        public bool FunStoreInA2ToA4WriPlc(string sStnNo, int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.GetCmdMstByStoreInStart(sStnNo, out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            string cmdSno = dataObject[0].CmdSno;
                            string IOType = dataObject[0].IOType;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();

                            clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Get StoreIn Command => {cmdSno}, " +
                                    $"{CmdMode}");

                            #region//根據buffer狀態更新命令
                            if (_conveyor.GetBuffer(bufferIndex).Auto != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).InMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotInMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CmdLeftOver, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).CmdMode == 6 || _conveyor.GetBuffer(bufferIndex - 1).CmdMode == 6)//為了不跟撿料命令衝突的條件
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CycleOperating, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex - 1).Ready != Ready.StoreInReady)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotStoreInReady, db);
                                return false;
                            }
                            #endregion

                            clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready Receive StoreIn Command=> {cmdSno}");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInWriteCmdToCV, db).ResultCode == DBResult.Success)
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
                            bool Result = WritePlccheck;
                            if (Result != true)//寫入命令和模式
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else
                            {
                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    lineId = "1",
                                    locationID = ((bufferIndex-2)/2).ToString(),
                                    taskNo = cmdSno.ToString(),
                                    state = "1", //任務開始
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    return false;
                                }
                                //填入訊息
                                TaskStateUpdateInfo info1 = new TaskStateUpdateInfo
                                {
                                    lineId = "1",
                                    taskNo = cmdSno,
                                    palletNo = cmdSno,
                                    businessType = IOType.ToString(),
                                    state = "12",
                                    errMsg = ""
                                };
                                if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info1))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                                return true;
                            }
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

        public bool FunStoreInCreateEquCmd(int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                        string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();

                        if (CMD_MST.GetCmdMstByStoreInCrane(cmdSno, out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            //clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreIn Get Command");

                            #region//根據buffer狀態更新命令
                            if (_conveyor.GetBuffer(bufferIndex).Auto != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).InMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotInMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceNotExist, db);
                                return false;
                            }
                            #endregion

                            clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready StoreIn=> {cmdSno}");

                            string source = $"{CranePortNo.A1}";
                            string IOType = dataObject[0].IOType;
                            string dest = "";
                            int pickup = Convert.ToInt32(dataObject[0].pickup);
                            if (IOType == IOtype.NormalstoreOut.ToString() && pickup==0)//如果是撿料，入庫儲位欄位是LOC，一般入庫是NewLoc
                            {
                                dest = $"{dataObject[0].Loc}";
                            }
                            else
                            {
                                dest = $"{dataObject[0].NewLoc}";
                            }

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");

                                return false;
                            }
                            if (CMD_MST.UpdateCmdMst(cmdSno, Trace.StoreInCreateCraneCmd, db).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreIn Command, Update CmdMst Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (EQU_CMD.InsertStoreInEquCmd(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5, db) == false)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreIn Command, Insert EquCmd Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreIn Command, Commit Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            return true;
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

        public bool FunStoreInA2toA4CreateEquCmd(int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                        
                        string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
                        if (CMD_MST.GetCmdMstByStoreInCrane(cmdSno, out var dataObject, db).ResultCode == DBResult.Success)
                        {

                            #region//根據buffer狀態更新命令
                            if (_conveyor.GetBuffer(bufferIndex).Auto != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).InMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotInMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceNotExist, db);
                                return false;
                            }
                            #endregion

                            clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready StoreIn=> {cmdSno}");

                            string source = "";
                            if (bufferIndex == 5)
                            {
                                source = $"{CranePortNo.A5}";
                            }
                            else if (bufferIndex == 7)
                            {
                                source = $"{CranePortNo.A7}";
                            }
                            else if (bufferIndex == 9)
                            {
                                source = $"{CranePortNo.A9}";
                            }
                            string IOType = dataObject[0].IOType;
                            string dest = "";
                            int pickup = Convert.ToInt32(dataObject[0].pickup);
                            if (IOType == IOtype.NormalstoreOut.ToString() && pickup == 0)//如果是撿料，入庫儲位欄位是LOC，一般入庫是NewLoc
                            {
                                dest = $"{dataObject[0].Loc}";
                            }
                            else
                            {
                                dest = $"{dataObject[0].NewLoc}";
                            }
                            //clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreIn Get Command");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");

                                return false;
                            }
                            if (CMD_MST.UpdateCmdMst(cmdSno, Trace.StoreInCreateCraneCmd, db).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreIn Command, Update CmdMst Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (EQU_CMD.InsertStoreInEquCmd(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5, db) == false)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreIn Command, Insert EquCmd Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreIn Command, Commit Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else return true;
                            
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

        public bool FunStoreInEquCmdFinish()
        {
            try
            {
                var stn1 = new List<string>()
                {
                    StnNo.A3,
                    StnNo.A6,
                    StnNo.A8,
                    StnNo.A10,
                };
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();

                        if (CMD_MST.GetCmdMstByStoreInFinish(stn1, out var dataObject,db).ResultCode == DBResult.Success)
                        {
                            foreach (var cmdMst in dataObject.Data)
                            {
                                string locationId = cmdMst.StnNo;
                                if(locationId==StnNo.A3)
                                {
                                    locationId = "1";
                                }
                                else if(locationId==StnNo.A6)
                                {
                                    locationId = "2";
                                }
                                else if(locationId == StnNo.A8)
                                {
                                    locationId = "3";
                                }
                                else if(locationId == StnNo.A10)
                                {
                                    locationId = "4";
                                }

                                if (EQU_CMD.GetEquCmd(cmdMst.CmdSno, out var equCmd,db).ResultCode == DBResult.Success)
                                {
                                    if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                                    {
                                        string cmdsts = "";
                                        string cmdabnormal = "";
                                        string remark = "";
                                        bool bflag = false;

                                        if (equCmd[0].CompleteCode == "92")//正常完成
                                        {
                                            cmdsts = CmdSts.CompleteWaitUpdate;
                                            cmdabnormal = "NA";
                                            remark = "存取車搬送命令完成";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info1 = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "13",
                                                errMsg =""
                                            };
                                            if(!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info1))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                            {
                                                //填入回報訊息
                                                lineId = "1",
                                                locationID = locationId,
                                                taskNo = cmdMst.CmdSno,
                                                state = "2", //任務結束
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                            {
                                                return false;
                                            }
                                        }
                                        else if (equCmd[0].CompleteCode.StartsWith("W"))
                                        {
                                            if (EQU_CMD.UpdateEquCmdRetry(equCmd[0].CmdSno, db).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                            bflag = false;
                                        }
                                        else if (equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.EF.ToString()) //地上盤強制取消 EF
                                        {
                                            cmdsts = CmdSts.CmdCancel;
                                            cmdabnormal = clsEnum.Cmd_Abnormal.EF.ToString();
                                            remark = "存取車地上盤強制取消命令";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "15",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                                            {
                                                //填入回報訊息
                                                lineId = "1",
                                                locationID = locationId,
                                                taskNo = cmdMst.CmdSno,
                                                state = "2", //任務結束
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1))
                                            {
                                                return false;
                                            }
                                        }
                                        else if (equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.FF.ToString()) //地上盤強制完成 FF
                                        {
                                            cmdsts = CmdSts.CompleteWaitUpdate;
                                            cmdabnormal = clsEnum.Cmd_Abnormal.FF.ToString();
                                            remark = "存取車地上盤強制完成命令";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "14",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                                            {
                                                //填入回報訊息
                                                lineId = "1",
                                                locationID = locationId,
                                                taskNo = cmdMst.CmdSno,
                                                state = "2", //任務結束
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1))
                                            {
                                                return false;
                                            }
                                        }
                                        if (bflag == true)
                                        {
                                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                            if (CMD_MST.UpdateCmdMst(equCmd[0].CmdSno, cmdsts, Trace.StoreInCraneCmdFinish, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (CMD_MST.UpdateCmdMstRemarkandAbnormal(equCmd[0].CmdSno, remark, cmdabnormal, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (EQU_CMD.DeleteEquCmd(equCmd[0].CmdSno, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                            return true;
                        }
                        else return true;
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

        #endregion StoreIn

        #region StoreOut
        public bool FunStoreOutWriPlc(string sStnNo, int bufferIndex)//出庫主要流程與空棧板出庫共用
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.GetCmdMstByStoreOutStart(sStnNo, out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {
                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            int IOType = Convert.ToInt32(dataObject[0].IOType);
                            int pickup = Convert.ToInt32(dataObject[0].pickup);
                            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                            bool Result;

                            clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Get StoreOut Command => {cmdSno}, " +
                                    $"{CmdMode}");

                            #region//確認目前模式，是否可以切換模式，可以就寫入切換成出庫的請求
                            if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady
                                && _conveyor.GetBuffer(bufferIndex).Switch_Ack == 1)
                            {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreOut Ready, Can Switchmode");

                                var WritePlccheck1 = _conveyor.GetBuffer(bufferIndex).Switch_Mode(2).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                Result = WritePlccheck1;
                                if (Result != true)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Normal-StoreOut Switchmode fail");
                                    return false;
                                }
                                else
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Normal-StoreOut Switchmode Complete");
                                }
                            }
                            #endregion

                            #region//確認站口狀態
                            if (_conveyor.GetBuffer(bufferIndex).Auto!=true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if(_conveyor.GetBuffer(bufferIndex).OutMode!=true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotOutMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).CommandId >0)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CmdLeftOver, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).CmdMode == 6 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 6 || _conveyor.GetBuffer(bufferIndex + 2).CmdMode == 6)//為了不跟減料命令衝突的條件
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CycleOperating, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence==true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotStoreOutReady, db);
                                return false;
                            }
                            if (CheckEmptyWillBefullOrNot() == true)//檢查一樓buffer是否整體滿九版空棧板了
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.EmptyWillBefull, db);
                                return false;
                            }
                            #endregion

                            clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready Receive StoreOut Command => {cmdSno}, " +
                                    $"{CmdMode}");
                            int LastCargoOrNotchek = LastCargoOrNot();


                                if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Begin Fail => {cmdSno}");
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreOutWriteCraneCmdToCV, db).ResultCode == DBResult.Success)
                                {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd Success => {cmdSno}, " +
                                $"{CmdMode}");
                                }
                                else
                                {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd fail => {cmdSno}, " +
                                $"{CmdMode}");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                                }
                                var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//寫入命令和模式//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                Result = WritePlccheck;
                                if (Result != true)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                //出庫都要寫入路徑編號，編號1為堆疊，編號2為直接出庫，編號3為補充母棧板
                                if ((IOType == IOtype.NormalstoreOut && pickup == 0 ) || IOType == IOtype.EmptyStoreOutbyWMS || IOType == IOtype.Cycle)//Iotype如果是撿料,空棧板整版出,盤點出庫或是出庫命令的最後一版，直接到A3
                                {
                                    WritePlccheck = _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path2_toA3).Result;//錯誤時回傳exmessage
                                    Result = WritePlccheck;
                                    if (Result != true)
                                    {
                                        clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Path2_toA3 Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                }
                                else if(IOType == IOtype.EmptyStroeOut)
                                {
                                    WritePlccheck = _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path3_toA4).Result;//錯誤時回傳exmessage
                                    Result = WritePlccheck;
                                    if (Result != true)
                                    {
                                        clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Path3_toA4 Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                }
                                else
                                {
                                    WritePlccheck = _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path1_toA2).Result;//錯誤時回傳exmessage
                                    Result = WritePlccheck;
                                    if (Result != true)
                                    {
                                        clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Path1_toA2 Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                }
                                if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                                }
                            else
                            {
                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    lineId = "1",
                                    locationID = "1",
                                    taskNo = cmdSno.ToString(),
                                    state = "1", //任務開始
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                                //填入訊息
                                TaskStateUpdateInfo info1 = new TaskStateUpdateInfo
                                {
                                    lineId = "1",
                                    taskNo = cmdSno,
                                    palletNo = cmdSno,
                                    businessType = IOType.ToString(),
                                    state = "12",
                                    errMsg = ""
                                };
                                if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info1))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                                return true;
                            }
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

        public bool FunStoreOutA2ToA4WriPlc(string sStnNo, int bufferIndex) 
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.GetCmdMstByStoreOutStart(sStnNo, out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string iotype = dataObject[0].IOType;
                            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();

                            clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Get StoreOut Command => {cmdSno}, " +
                                    $"{CmdMode}");

                            #region//確認站口狀態
                            if (_conveyor.GetBuffer(bufferIndex).Auto != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).OutMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotOutMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CmdLeftOver, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).CmdMode == 6 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 6)//為了不跟撿料命令衝突的條件
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CycleOperating, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotStoreOutReady, db);
                                return false;
                            }
                            #endregion


                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready Receive StoreOut Command => {cmdSno}, " +
                                    $"{CmdMode}");

                                if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Begin Fail => {cmdSno}");
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreOutWriteCraneCmdToCV, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd Success => {cmdSno}, " +
                                    $"{CmdMode}");
                                }
                                else
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd fail => {cmdSno}, " +
                                    $"{CmdMode}");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//寫入命令和模式//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                bool Result = WritePlccheck;
                                if (Result != true)//寫入命令和模式
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else
                            {
                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    lineId = "1",
                                    locationID = ((bufferIndex - 1) / 2).ToString(),
                                    taskNo = cmdSno.ToString(),
                                    state = "1", //任務開始
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    return false;
                                }
                                //填入訊息
                                TaskStateUpdateInfo info1 = new TaskStateUpdateInfo
                                {
                                    lineId = "1",
                                    taskNo =cmdSno,
                                    palletNo =cmdSno,
                                    businessType = iotype,
                                    state = "12",
                                    errMsg =""
                                };
                                if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info1))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                                return true;
                            }
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

        public bool FunStoreOutCreateEquCmd(int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                        string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();

                        if (CMD_MST.GetCmdMstByStoreOutCrane(cmdSno, out var dataObject, db).ResultCode == DBResult.Success)
                        {
                                #region//站口狀態確認
                            if (_conveyor.GetBuffer(bufferIndex).Auto!=true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotOutMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).OutMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotOutMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                            #endregion

                                #region 擋下出庫命令，當儲位是外儲位檢查內儲位現在是否有命令(庫對庫)，如果有就擋下
                                string source = dataObject[0].Loc;
                                string dest = $"{CranePortNo.A1}";
                                int checkcource = Int32.Parse(source.Substring(0, 2));
                                bool bcheck;
                                if (checkcource > 2)
                                {
                                    bcheck = funChkInsideLoc(source, db);
                                    if (bcheck == true)
                                    {
                                        CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.InsideLocWait, db);
                                        clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"InsideLoc has Cmd,Please Wait => {cmdSno}");
                                        return false;
                                    }
                                }
                                #endregion

                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready StoreOut => {cmdSno}");
                                
                                if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Begin Fail => {cmdSno}");
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMst(cmdSno, Trace.StoreOutCreateCraneCmd, db).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Update CmdMst Fail => {cmdSno}");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                if (EQU_CMD.InsertStoreOutEquCmd(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5, db) == false)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Insert EquCmd Fail => {cmdSno}");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Commit Fail => {cmdSno}");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                else return true;
                                
                        }
                        return false;
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

        public bool FunStoreOutA2toA4CreateEquCmd(int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                        string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();

                        

                        if (CMD_MST.GetCmdMstByStoreOutCrane(cmdSno, out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            cmdSno = dataObject[0].CmdSno;
                            string source = dataObject[0].Loc;
                            string dest = "";
                                
                            #region//站口狀態確認
                            if (_conveyor.GetBuffer(bufferIndex).Auto != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).OutMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotOutMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                        #endregion

                            #region 擋下出庫命令，當儲位是外儲位檢查內儲位現在是否有命令(庫對庫)，如果有就擋下
                            int checkcource = Int32.Parse(source.Substring(0, 2));
                            bool bcheck;
                            if (checkcource > 2)
                            {
                                bcheck = funChkInsideLoc(source, db);
                                if (bcheck == true)
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.InsideLocWait, db);
                                    clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"InsideLoc has Cmd,Please Wait => {cmdSno}");
                                    return false;
                                }
                            }
                            #endregion

                            switch (bufferIndex)
                            {
                                case 5:
                                    dest = $"{CranePortNo.A5}";
                                    break;
                                case 7:
                                    dest = $"{CranePortNo.A7}";
                                    break;
                                case 9:
                                    dest = $"{CranePortNo.A9}";
                                    break;
                            }
                                
                            clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready StoreOut => {cmdSno}");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Begin Fail => {cmdSno}");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMst(cmdSno, Trace.StoreOutCreateCraneCmd, db).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Update CmdMst Fail => {cmdSno}");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (EQU_CMD.InsertStoreOutEquCmd(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5, db) == false)
                            {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Insert EquCmd Fail => {cmdSno}");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Commit Fail => {cmdSno}");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            return true;
                            
                        }
                            return true;
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

        public bool FunStoreOutEquCmdFinish(IEnumerable<string> stations) 
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.GetCmdMstByStoreOutFinish(stations, out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            foreach (var cmdMst in dataObject.Data)
                            {
                                if (EQU_CMD.GetEquCmd(cmdMst.CmdSno, out var equCmd, db).ResultCode == DBResult.Success)
                                {
                                    if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                                    {
                                        string cmdsts="";
                                        string cmdabnormal="";
                                        string remark="";
                                        bool bflag=false;

                                        if (equCmd[0].CompleteCode == "92")//正常完成
                                        {
                                            cmdsts = CmdSts.CompleteWaitUpdate;
                                            cmdabnormal = "NA";
                                            remark = "存取車搬送命令完成";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId ="1" ,
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "13",
                                                errMsg =""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                        }
                                        else if (equCmd[0].CompleteCode.StartsWith("W"))
                                        {
                                            if (EQU_CMD.UpdateEquCmdRetry(equCmd[0].CmdSno, db).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                            bflag = false;
                                        }
                                        else if(equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.EF.ToString()) //地上盤強制取消 EF
                                        {
                                            cmdsts = CmdSts.CmdCancel;
                                            cmdabnormal = clsEnum.Cmd_Abnormal.EF.ToString();
                                            remark = "存取車地上盤強制取消命令";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "15",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                        }
                                        else if (equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.FF.ToString()) //地上盤強制完成 FF
                                        {
                                            cmdsts = CmdSts.CompleteWaitUpdate;
                                            cmdabnormal = clsEnum.Cmd_Abnormal.FF.ToString();
                                            remark = "存取車地上盤強制完成命令";
                                            bflag=true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "14",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                        }
                                        if (bflag == true)
                                        {
                                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                            if ((cmdMst.IOType != "2" && cmdMst.pickup != "0") || equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.EF.ToString())
                                            {
                                                if (CMD_MST.UpdateCmdMst(equCmd[0].CmdSno, cmdsts, Trace.StoreOutCraneCmdFinish, db) != ExecuteSQLResult.Success)
                                                {
                                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                                    return false;
                                                }
                                            }
                                            if (CMD_MST.UpdateCmdMstRemarkandAbnormal(equCmd[0].CmdSno, remark, cmdabnormal, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (EQU_CMD.DeleteEquCmd(equCmd[0].CmdSno, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                            return true;
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
        
        #endregion StoreOut

        #region Other
        public bool FunEmptyStoreInWriPlc(string sStnNo, int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                            if (CMD_MST.GetCmdMstByStoreInStart(sStnNo, out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                            {
                                string cmdSno = dataObject[0].CmdSno;
                            string iotype = dataObject[0].IOType;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

                                clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Get EmptyStoreIn Command => {cmdSno}");

                                #region//站口狀態確認
                                if (_conveyor.GetBuffer(bufferIndex).Auto != true)
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                    return false;
                                }
                                if (_conveyor.GetBuffer(bufferIndex).InMode != true)
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotOutMode, db);
                                    return false;
                                }
                                if (_conveyor.GetBuffer(bufferIndex).Error == true)
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                    return false;
                                }
                                if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CmdLeftOver, db);
                                    return false;
                                }
                                if (_conveyor.GetBuffer(bufferIndex - 1).CmdMode == 6 || _conveyor.GetBuffer(bufferIndex - 2).CmdMode == 6 || _conveyor.GetBuffer(bufferIndex - 3).CmdMode == 6)//為了不跟撿料命令衝突的條件
                                {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CycleOperating, db);
                                return false;
                                }
                                if (_conveyor.GetBuffer(bufferIndex-3).Ready != Ready.StoreInReady)
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotStoreInReady, db);
                                    return false;
                                }
                                #endregion

                                    clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive EmptyStoreIn Command");

                                    if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
                                        return false;
                                    }
                                    if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.EmptyStoreInWriteCraneCmdToCV, db).ResultCode == DBResult.Success)
                                    {
                                        clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Update cmd suceess => {cmdSno}");
                                    }
                                    else
                                    {
                                        clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd fail => {cmdSno}");

                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                                    bool Result = WritePlccheck;
                                    if (Result != true)//寫入命令和模式
                                    {
                                        clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                            else
                            {
                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    lineId = "1",
                                    locationID = "1",
                                    taskNo = cmdSno.ToString(),
                                    state = "1", //任務開始
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                                //填入訊息
                                TaskStateUpdateInfo info1 = new TaskStateUpdateInfo
                                {
                                    lineId = "1",
                                    taskNo = cmdSno,
                                    palletNo = cmdSno,
                                    businessType = iotype,
                                    state = "12",
                                    errMsg = ""
                                };
                                if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info1))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                                return true;
                            }
                        }
                            return true;
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

        public bool FunEmptyStoreInCreateEquCmd(int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                        string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
 
                        if (CMD_MST.GetEmptyCmdMstByStoreIn(cmdSno, out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            string source = $"{CranePortNo.A1}";
                            string dest = $"{dataObject[0].NewLoc}";

                            #region//站口狀態確認
                            if (_conveyor.GetBuffer(bufferIndex).Auto != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).InMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotInMode, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Error == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
                                return false;
                            }
                            if (_conveyor.GetBuffer(bufferIndex).Presence == false)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceNotExist, db);
                                return false;
                            }
                            #endregion


                            clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready EmptyStoreIn => {cmdSno}");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane EmptyStoreIn Command, Begin Fail => {cmdSno}");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMst(cmdSno, Trace.EmptyStoreInCreateCraneCmd, db).ResultCode != DBResult.Success)
                            {
                                clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane EmptyStoreIn Command, Update CmdMst Fail => {cmdSno}");
                                   
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (EQU_CMD.InsertStoreInEquCmd(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5, db) == false)
                            {
                                clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane EmptyStoreIn Command, Insert EquCmd Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.EmptyStoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreIn Command, Commit Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                             return true;
                        }
                        return true;
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

        public bool FunEmptyStoreInEquCmdFinish()
        {
            try
            {
                var stn1 = new List<string>()
                {
                    StnNo.A4,
                };
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                        if (CMD_MST.GetEmptyCmdMstByStoreInFinish(stn1, out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            foreach (var cmdMst in dataObject.Data)
                            {
                                if (EQU_CMD.GetEquCmd(cmdMst.CmdSno, out var equCmd, db).ResultCode == DBResult.Success)
                                {
                                    if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                                    {
                                        string cmdsts = "";
                                        string cmdabnormal = "";
                                        string remark = "";
                                        bool bflag = false;

                                        if (equCmd[0].CompleteCode == "92")//正常完成
                                        {
                                            cmdsts = CmdSts.CompleteWaitUpdate;
                                            cmdabnormal = "NA";
                                            remark = "存取車搬送命令完成";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "13",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                                            {
                                                //填入回報訊息
                                                lineId = "1",
                                                locationID = "1",
                                                taskNo = cmdMst.CmdSno,
                                                state = "2", //任務結束
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1))
                                            {
                                                return false;
                                            }
                                        }
                                        else if (equCmd[0].CompleteCode.StartsWith("W"))
                                        {
                                            if (EQU_CMD.UpdateEquCmdRetry(equCmd[0].CmdSno, db).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                            bflag = false;
                                        }
                                        else if (equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.EF.ToString()) //地上盤強制取消 EF
                                        {
                                            cmdsts = CmdSts.CmdCancel;
                                            cmdabnormal = clsEnum.Cmd_Abnormal.EF.ToString();
                                            remark = "存取車地上盤強制取消命令";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "15",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                                            {
                                                //填入回報訊息
                                                lineId = "1",
                                                locationID = "1",
                                                taskNo = cmdMst.CmdSno,
                                                state = "2", //任務結束
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1))
                                            {
                                                return false;
                                            }
                                        }
                                        else if (equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.FF.ToString()) //地上盤強制完成 FF
                                        {
                                            cmdsts = CmdSts.CompleteWaitUpdate;
                                            cmdabnormal = clsEnum.Cmd_Abnormal.FF.ToString();
                                            remark = "存取車地上盤強制完成命令";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "14",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                                            {
                                                //填入回報訊息
                                                lineId = "1",
                                                locationID = "1",
                                                taskNo = cmdMst.CmdSno,
                                                state = "2", //任務結束
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1))
                                            {
                                                return false;
                                            }
                                        }
                                        if (bflag == true)
                                        {
                                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                            if (CMD_MST.UpdateCmdMst(equCmd[0].CmdSno, cmdsts, Trace.EmptyStoreInCraneCmdFinish, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (CMD_MST.UpdateCmdMstRemarkandAbnormal(equCmd[0].CmdSno, remark, cmdabnormal, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (EQU_CMD.DeleteEquCmd(equCmd[0].CmdSno, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                            return true;
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

        //public bool FunEmptyStoreOutWriPlc(string sStnNo, int bufferIndex)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                string cmdSno = "";
        //                var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
        //                    if (CMD_MST.GetCmdMstByStoreOutStart(sStnNo, out var dataObject, db) == GetDataResult.Success) //讀取CMD_MST 
        //                    {
        //                        cmdSno = dataObject[0].CmdSno;
        //                        int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

        //                        clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Get EmptyStoreOut Command => {cmdSno}, " +
        //                            $"{CmdMode}");

        //                        #region//確認目前模式，是否可以切換模式，可以就寫入切換成出庫的請求
        //                        if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreInReady
        //                        && _conveyor.GetBuffer(bufferIndex).Switch_Ack == 1)
        //                        {
        //                            clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreOut Ready, Can Switchmode");

        //                            var WritePlccheck1 = _conveyor.GetBuffer(bufferIndex).Switch_Mode(2).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
        //                            bool Result1 = WritePlccheck1.Item1;
        //                            string exmessage1 = WritePlccheck1.Item2;
        //                            if (Result1 != true)
        //                            {
        //                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Empty StoreOut Switchmode fail:{exmessage1}");
        //                                return false;
        //                            }
        //                            else
        //                            {
        //                                clsWriLog.StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Empty StoreOut Switchmode Complete");
        //                            }
        //                        }
        //                        #endregion

        //                        #region//站口狀態確認
        //                        if (_conveyor.GetBuffer(bufferIndex).Auto != true)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).OutMode != true)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotOutMode, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).Error == true)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CmdLeftOver, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).Presence == true)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotStoreOutReady, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 2).CmdMode == 3)//為了不跟盤點命令衝突的條件
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CycleOperating, db);
        //                            return false;
        //                        }
        //                        #endregion


        //                        clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive EmptyStoreOut Command");

        //                            if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
        //                            {
        //                                clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Begin Fail => {cmdSno}");
        //                                return false;
        //                            }
        //                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.EmptyStoreOutWriteCraneCmdToCV, db) == ExecuteSQLResult.Success)
        //                            {
        //                                clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Update cmd success => {cmdSno}");
        //                            }
        //                            else
        //                            {
        //                                clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Upadte cmd fail => {cmdSno}");
        //                                db.TransactionCtrl2(TransactionTypes.Rollback);
        //                                return false;
        //                            }
        //                            var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
        //                            bool Result = WritePlccheck.Item1;
        //                            string exmessage = WritePlccheck.Item2;
        //                            if (Result != true)//寫入命令和模式
        //                            {
        //                                clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");
        //                                db.TransactionCtrl2(TransactionTypes.Rollback);
        //                                return false;
        //                            }

        //                            WritePlccheck = _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path3_toA4).Result;//一樓出庫都要寫入路徑編號，確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
        //                            Result = WritePlccheck.Item1;
        //                            exmessage = WritePlccheck.Item2;
        //                            if (Result != true)
        //                            {
        //                                clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Path3_toA4 Fail:{exmessage}");
        //                                db.TransactionCtrl2(TransactionTypes.Rollback);
        //                                return false;
        //                            }
        //                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
        //                        {
        //                            clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
        //                            db.TransactionCtrl2(TransactionTypes.Rollback);
        //                            return false;
        //                        }
        //                        else return true;
        //                    }
        //                    return true;
        //            }
        //            else
        //            {
        //                string strEM = "Error: 開啟DB失敗！";
        //                clsWriLog.Log.FunWriTraceLog_CV(strEM);
        //                return false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return false;
        //    }
        //}

        //public bool FunEmptyStoreOutCreateEquCmd(int bufferIndex)
        //{
        //    try
        //    {
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();

        //                    clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready EmptyStoreOut");

        //                    string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
        //                    if (CMD_MST.GetCmdMstByEmptyStoreOutCrane(cmdSno, out var dataObject, db) == GetDataResult.Success)
        //                    {

        //                        string source = $"{dataObject[0].Loc}";
        //                        string dest = $"{CranePortNo.A1}";

        //                        #region//站口狀態確認
        //                        if (_conveyor.GetBuffer(bufferIndex).Auto != true)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).OutMode != true)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotOutMode, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).Error == true)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BufferError, db);
        //                            return false;
        //                        }
        //                        if (_conveyor.GetBuffer(bufferIndex).Presence == true)
        //                        {
        //                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
        //                            return false;
        //                        }
        //                        #endregion


        //                        clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Get Command => {cmdSno}");

        //                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
        //                        {
        //                            clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Begin Fail => {cmdSno}");
        //                            return false;
        //                        }
        //                        if (CMD_MST.UpdateCmdMst(cmdSno, Trace.EmptyStoreOutCreateCraneCmd, db) != ExecuteSQLResult.Success)
        //                        {
        //                            clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Update CmdMst Fail => {cmdSno}");

        //                            db.TransactionCtrl2(TransactionTypes.Rollback);
        //                            return false;
        //                        }
        //                        if (EQU_CMD.InsertStoreOutEquCmd(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5, db) == false)
        //                        {
        //                            clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Insert EquCmd Fail => {cmdSno}");
        //                            db.TransactionCtrl2(TransactionTypes.Rollback);
        //                            return false;
        //                        }
        //                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
        //                        {
        //                            clsWriLog.EmptyStoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Create Crane StoreOut Command, Commit Fail => {cmdSno}");
        //                            db.TransactionCtrl2(TransactionTypes.Rollback);
        //                            return false;
        //                        }
        //                    }
        //                    return true;
                       
        //            }
        //            else
        //            {
        //                string strEM = "Error: 開啟DB失敗！";
        //                clsWriLog.Log.FunWriTraceLog_CV(strEM);
        //                return false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return false;
        //    }
        //}

        //public bool FunEmptyStoreOutEquCmdFinish()
        //{
        //    try
        //    {
        //        var stn1 = new List<string>()
        //        {
        //            StnNo.A4
        //        };
        //        using (var db = clsGetDB.GetDB(_config))
        //        {
        //            int iRet = clsGetDB.FunDbOpen(db);
        //            if (iRet == DBResult.Success)
        //            {
        //                var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
        //                if (CMD_MST.GetEmptyCmdMstByStoreOutFinish(stn1, out var dataObject, db) == GetDataResult.Success)
        //                {
        //                    foreach (var cmdMst in dataObject.Data)
        //                    {
        //                        if (EQU_CMD.GetEquCmd(cmdMst.CmdSno, out var equCmd, db) == GetDataResult.Success)
        //                        {
        //                            if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
        //                            {
        //                                if (equCmd[0].CompleteCode == "92")
        //                                {
        //                                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
        //                                    {
        //                                        return false;
        //                                    }
        //                                    if (CMD_MST.UpdateCmdMst(equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.EmptyStoreOutCraneCmdFinish, db) != ExecuteSQLResult.Success)
        //                                    {
        //                                        db.TransactionCtrl2(TransactionTypes.Rollback);
        //                                        return false;
        //                                    }
        //                                    if (EQU_CMD.DeleteEquCmd(equCmd[0].CmdSno, db) != ExecuteSQLResult.Success)
        //                                    {
        //                                        db.TransactionCtrl2(TransactionTypes.Rollback);
        //                                        return false;
        //                                    }
        //                                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
        //                                    {
        //                                        return false;
        //                                    }
        //                                }
        //                                else if (equCmd[0].CompleteCode.StartsWith("W"))
        //                                {
        //                                    if (EQU_CMD.UpdateEquCmdRetry(equCmd[0].CmdSno, db) != ExecuteSQLResult.Success)
        //                                    {
        //                                        return false;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    return true;
        //                }
        //                else return false;
        //            }
        //            else
        //            {
        //                string strEM = "Error: 開啟DB失敗！";
        //                clsWriLog.Log.FunWriTraceLog_CV(strEM);
        //                return false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return false;
        //    }
        //}

        public bool FunLocToLoc()
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.GetLocToLoc(out var dataObject, db).ResultCode == DBResult.Success)
                        {

                            string source = $"{dataObject[0].Loc}";
                            string dest = $"{dataObject[0].NewLoc}";
                            string cmdSno = $"{dataObject[0].CmdSno}";
                            string IOtype = $"{dataObject[0].IOType}";


                            clsWriLog.L2LLogTrace(5, "LocToLoc", $"LocToLoc Command Received => {cmdSno}");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(5, "LocToLoc", $"Create Crane LocToLoc Command, Begin Fail => {cmdSno}");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.LoctoLocReady, db).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(5, "LocToLoc", $"Create Crane LocToLoc Command, Update CmdMst Fail => {cmdSno}");
                                
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (EQU_CMD.InsertLocToLocEquCmd(5, "LocToLoc", 1, cmdSno, source, dest, 1, db) == false)
                            {
                                clsWriLog.L2LLogTrace(5, "LocToLoc", $"Create Crane LocToLoc Command, Insert EquCmd Fail => {cmdSno}");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(5, "LocToLoc", $"Create Crane LocToLoc Command Commit Fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else
                            {
                                //填入訊息
                                TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                {
                                    lineId = "1",
                                    taskNo = cmdSno,
                                    palletNo = cmdSno,
                                    businessType = IOtype,
                                    state = "12",
                                    errMsg = ""
                                };
                                if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                {
                                    db.TransactionCtrl(TransactionTypes.Rollback);
                                    return false;
                                }
                                return true;
                            }
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

        public bool FunLocToLocCmdFinish()
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                        if (CMD_MST.GetLoctoLocFinish(out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            foreach (var cmdMst in dataObject.Data)
                            {
                                if (EQU_CMD.GetEquCmd(cmdMst.CmdSno, out var equCmd, db).ResultCode == DBResult.Success)
                                {
                                    if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                                    {
                                        string cmdsts = "";
                                        string cmdabnormal = "";
                                        string remark = "";
                                        bool bflag = false;

                                        if (equCmd[0].CompleteCode == "92")//正常完成
                                        {
                                            cmdsts = CmdSts.CompleteWaitUpdate;
                                            cmdabnormal = "NA";
                                            remark = "存取車搬送命令完成";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "13",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                        }
                                        else if (equCmd[0].CompleteCode.StartsWith("W"))
                                        {
                                            if (EQU_CMD.UpdateEquCmdRetry(equCmd[0].CmdSno, db).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                            bflag = false;
                                        }
                                        else if (equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.EF.ToString()) //地上盤強制取消 EF
                                        {
                                            cmdsts = CmdSts.CmdCancel;
                                            cmdabnormal = clsEnum.Cmd_Abnormal.EF.ToString();
                                            remark = "存取車地上盤強制取消命令";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "15",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                        }
                                        else if (equCmd[0].CompleteCode == clsEnum.Cmd_Abnormal.FF.ToString()) //地上盤強制完成 FF
                                        {
                                            cmdsts = CmdSts.CompleteWaitUpdate;
                                            cmdabnormal = clsEnum.Cmd_Abnormal.FF.ToString();
                                            remark = "存取車地上盤強制完成命令";
                                            bflag = true;

                                            //填入訊息
                                            TaskStateUpdateInfo info = new TaskStateUpdateInfo
                                            {
                                                lineId = "1",
                                                taskNo = cmdMst.CmdSno,
                                                palletNo = cmdMst.CmdSno,
                                                businessType = cmdMst.IOType,
                                                state = "14",
                                                errMsg = ""
                                            };
                                            if (!clsWmsApi.GetApiProcess().GetTaskStateUpdate().FunReport(info))
                                            {
                                                db.TransactionCtrl(TransactionTypes.Rollback);
                                                return false;
                                            }
                                        }
                                        if (bflag == true)
                                        {
                                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                            if (CMD_MST.UpdateCmdMst(equCmd[0].CmdSno, cmdsts, Trace.LoctoLocReadyFinish, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (CMD_MST.UpdateCmdMstRemarkandAbnormal(equCmd[0].CmdSno, remark, cmdabnormal, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (EQU_CMD.DeleteEquCmd(equCmd[0].CmdSno, db).ResultCode != DBResult.Success)
                                            {
                                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                                return false;
                                            }
                                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                            return true;
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
        #endregion

        #region//根據判斷去決定一樓空棧板總數是否滿了，去擋下出庫命令
        private bool CheckEmptyWillBefullOrNot()
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            if ((_conveyor.GetBuffer(4).EmptyINReady == 8 && _conveyor.GetBuffer(1).CommandId != 0) || (_conveyor.GetBuffer(4).EmptyINReady == 8 && _conveyor.GetBuffer(2).CommandId != 0) || _conveyor.GetBuffer(4).EmptyINReady == 9)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region//判斷function:當檢查命令是最後一個以及一樓buffer沒有貨物，便要直接路徑到A3，狀態正確寫入1:現在沒有用這個作為判斷，現在用WMS傳我們的pickup參數作為判斷
        private int LastCargoOrNot()
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    if (CMD_MST.GetCmdMstByStoreOutcheck(StnNo.A3, out var dataObject, db).ResultCode == DBResult.Success)
                    {
                        int COUNT = Convert.ToInt32(dataObject[0].COUNT);
                        var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
                        if (_conveyor.GetBuffer(2).A2LV2 == 0 && COUNT == 1 && _conveyor.GetBuffer(2).CommandId == 0 && _conveyor.GetBuffer(1).CommandId == 0)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return 99;//異常連不到資料庫
                    }
                }
                else
                {
                    string strEM = "Error: 開啟DB失敗！";
                    clsWriLog.Log.FunWriTraceLog_CV(strEM);
                    return 0;
                }
            }
        }
        #endregion

        #region//判斷是否是外儲位，是的話檢查內儲位是否有命令，如果有命令傳回true擋下命令
        private bool funChkInsideLoc(string Loc, SqlServer db)
        {
            string sInsideLoc = "";

            if (Loc.Substring(0, 2) == "03")
            {
                sInsideLoc = "01";
            }
            else if (Loc.Substring(0, 2) == "04")
            {
                sInsideLoc = "02";
            }
            sInsideLoc = sInsideLoc + Loc.Substring(2, 5);
            if(CMD_MST.GetCmdMstByLOC(sInsideLoc, out var dataObject, db).ResultCode==DBResult.NoDataSelect)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

    }
}
