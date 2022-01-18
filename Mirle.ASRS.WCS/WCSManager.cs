using System;
using System.Collections.Generic;
using System.Timers;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.ASRS.WCS.Model.LogTrace;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.ASRS.WCS.Controller;
using Mirle.ASRS.Conveyors;
using Mirle.DataBase;
using Mirle.DB.Object.Service;
using Mirle.Def;
using Mirle.ASRS.WCS.Service;
using Mirle.DB.Proc;

namespace Mirle.ASRS.WCS
{
    public class WCSManager
    {
        private readonly Conveyors.Conveyor _conveyor;
        private readonly LoggerManager _loggerManager;
        private readonly EmptyReport emptyReport = new EmptyReport();

        private readonly Timer _emptyInReport = new Timer();
        private readonly Timer _emptyOutReport = new Timer();
        
        private readonly Timer _storeInProcess = new Timer();
        private readonly Timer _storeOutProcess = new Timer();
        private readonly Timer _otherProcess = new Timer();
        private bool IsConnected => ControllerReader.GetCVControllerr().GetConnect();

        public WCSManager()
        {
            _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            _loggerManager = ControllerReader.GetLoggerManager();

            _storeOutProcess.Interval = 500;
            _storeInProcess.Interval = 500;
            _otherProcess.Interval = 500;
            
            _emptyInReport.Interval = 500;
            _emptyOutReport.Interval = 500;

            _storeOutProcess.Elapsed += StoreOutProcess;
            _storeInProcess.Elapsed += StoreInProcess;
            _otherProcess.Elapsed += OtherProcess;
            
            _emptyInReport.Elapsed += emptyReport.EmptyInWMS;
            _emptyOutReport.Elapsed += emptyReport.EmptyOutWMS;

        }

        public void Start()
        {
            _storeOutProcess.Start();
            _storeInProcess.Start();
            _otherProcess.Start();
        }
        public void Stop()
        {
            _storeOutProcess.Stop();
            _storeInProcess.Stop();
            _otherProcess.Stop();
        }

        
        private void StoreOutProcess(object sender, ElapsedEventArgs e)
        {
            _storeOutProcess.Stop();
            try
            {
                if (IsConnected)
                {
                    clsStoreOut.StoreOut_A1_WriteCV();

                    clsStoreOut.StoreOut_A1_CreateEquCmd();

                    clsStoreOut.StoreOut_A2ToA4_WriteCV();

                    clsStoreOut.StoreOut_A2ToA4_CreateEquCmd();

                    clsStoreOut.StoreOut_EquCmdFinish();
                }
            }
            catch (Exception ex)
            {
                System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
                var log = new StoreOutLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                _loggerManager.WriteLogTrace(log);
            }
            finally
            {
                _storeOutProcess.Start();
            }
        }

        private void StoreInProcess(object sender, ElapsedEventArgs e)
        {
            _storeInProcess.Stop();
            try
            {
                if (IsConnected)
                {
                    SwitchInMode.Switch_InMode(_conveyor, _loggerManager);//自動切入庫模式

                    if (_conveyor.GetBuffer(1).Ready == Ready.StoreInReady)
                    {
                        clsStoreIn.StoreIn_A1_WriteCV();

                        clsStoreIn.StoreIn_A1_CreateEquCmd();
                    }

                    clsStoreIn.StoreIn_A2ToA4_WriteCV();

                    clsStoreIn.StoreIn_A2toA4_CreateEquCmd();

                    clsStoreIn.StoreIn_EquCmdFinish();//OK
                }
            }
            catch (Exception ex)
            {
                System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
                var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                _loggerManager.WriteLogTrace(log);
            }
            finally
            {
                _storeInProcess.Start();
            }
        }

        private void OtherProcess(object sender, ElapsedEventArgs e)
        {
            _otherProcess.Stop();
            try
            {
                if (IsConnected)
                {
                    clsOther.clsEmptyPallets.EmptyStoreIn_A1_WriteCV();

                    clsOther.clsEmptyPallets.EmptyStoreIn_A1_CreateEquCmd();

                    clsOther.clsEmptyPallets.EmptyStoreIn_EquCmdFinish();

                    clsOther.clsEmptyPallets.EmptyStoreOut_A1_WriteCV();

                    //clsOther.clsEmptyPallets.EmptyStoreOut_A1_CreateEquCmd();

                    //clsOther.clsEmptyPallets.EmptyStoreOut_EquCmdFinish();

                    clsOther.clsL2L.Other_LocToLoc();

                    clsOther.clsL2L.Other_LocToLocfinish();
                }
            }
            catch (Exception ex)
            {
                System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
                var log = new StoreOutLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                _loggerManager.WriteLogTrace(log);
            }
            finally
            {
                _otherProcess.Start();
            }
        }

        #region Dispose
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();

                    _storeInProcess.Dispose();
                    _storeOutProcess.Dispose();
                    _otherProcess.Dispose();
                }

                disposedValue = true;
            }
        }

        ~WCSManager()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion Dispose
    }
}



#region Mark

#region StoreOut
//private void StoreOut_A1_WriteCV()
//{
//    try
//    {
//        int bufferIndex = 1; //A1
//        using (var db = _dataAccessManger.GetDB())
//        {

//            if (_dataAccessManger.GetCmdMstByStoreOutStart(StnNo.A3, out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
//            {
//                string CmdSno = dataObject[0].CmdSno;
//                int IOType = Convert.ToInt32(dataObject[0].IOType);
//                int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

//                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get StoreOut Command");
//                log.CmdSno = CmdSno;
//                log.LoadCategory = CmdMode;
//                _loggerManager.WriteLogTrace(log);

//                #region//確認目前模式，是否可以切換模式，可以就寫入切換成出庫的請求
//                if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady
//                    && _conveyor.GetBuffer(bufferIndex).Switch_Ack == 1)
//                {
//                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreOut Ready, Can Switchmode");
//                    _loggerManager.WriteLogTrace(log);

//                    var WritePlccheck = _conveyor.GetBuffer(bufferIndex).Switch_Mode(2).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
//                    bool Result = WritePlccheck.Item1;
//                    string exmessage = WritePlccheck.Item2;
//                    if (Result != true)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Normal-StoreOut Switchmode fail:{exmessage}");
//                        _loggerManager.WriteLogTrace(log);
//                        return;
//                    }
//                    else
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Normal-StoreOut Switchmode Complete");
//                        _loggerManager.WriteLogTrace(log);
//                    }
//                }
//                #endregion

//                if (_conveyor.GetBuffer(bufferIndex).Auto
//                    && _conveyor.GetBuffer(bufferIndex).OutMode
//                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
//                    && _conveyor.GetBuffer(bufferIndex).Presence == false
//                    && _conveyor.GetBuffer(bufferIndex).Error == false
//                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
//                    && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
//                    && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
//                    && _conveyor.GetBuffer(bufferIndex + 2).CmdMode != 3//為了不跟盤點命令衝突的條件
//                    && CheckEmptyWillBefullOrNot() == false) //檢查一樓buffer是否整體滿九版空棧板了
//                {
//                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
//                    log.CmdSno = CmdSno;
//                    log.LoadCategory = CmdMode;
//                    _loggerManager.WriteLogTrace(log);

//                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Begin Fail");
//                        log.CmdSno = CmdSno;
//                        return;
//                    }
//                    if (_dataAccessManger.UpdateCmdMstTransferring(db, CmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd Success");
//                        log.CmdSno = CmdSno;
//                        log.LoadCategory = CmdMode;
//                        _loggerManager.WriteLogTrace(log);
//                    }
//                    else
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
//                        log.CmdSno = CmdSno;
//                        log.LoadCategory = CmdMode;
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }

//                    var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(CmdSno, CmdMode).Result;//寫入命令和模式//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
//                    bool Result = WritePlccheck.Item1;
//                    string exmessage = WritePlccheck.Item2;
//                    if (Result != true)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }

//                    //出庫都要寫入路徑編號，編號1為堆疊，編號2為直接出庫，編號3為補充母棧板
//                    if (IOType == IOtype.Cycle || LastCargoOrNot() == 1)//Iotype如果是盤點或是空棧板整版出或是出庫命令的最後一版，直接到A3
//                    {
//                        WritePlccheck = _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path2_toA3).Result;//錯誤時回傳exmessage
//                        Result = WritePlccheck.Item1;
//                        exmessage = WritePlccheck.Item2;
//                        if (Result != true)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Path2_toA3 Fail:{exmessage}");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                    }
//                    else
//                    {
//                        WritePlccheck = _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path1_toA2).Result;//錯誤時回傳exmessage
//                        Result = WritePlccheck.Item1;
//                        exmessage = WritePlccheck.Item2;
//                        if (Result != true)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Path1_toA2 Fail:{exmessage}");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                    }

//                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }

//                }
//                #region 站口狀態自動確認-Update-CMD-Remark
//                else if (_conveyor.GetBuffer(bufferIndex).OutMode == false)
//                {
//                    _dataAccessManger.UpdateCmdMstRemark(db, CmdSno, Remark.NotOutMode);
//                    return;
//                }
//                else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
//                {
//                    _dataAccessManger.UpdateCmdMstRemark(db, CmdSno, Remark.NotAutoMode);
//                    return;
//                }
//                else if (_conveyor.GetBuffer(bufferIndex).Error == true)
//                {
//                    _dataAccessManger.UpdateCmdMstRemark(db, CmdSno, Remark.BufferError);
//                    return;
//                }
//                else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 2).CmdMode == 3)
//                {
//                    _dataAccessManger.UpdateCmdMstRemark(db, CmdSno, Remark.CycleOperating);
//                    return;
//                }
//                else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
//                {
//                    _dataAccessManger.UpdateCmdMstRemark(db, CmdSno, Remark.PresenceExist);
//                    return;
//                }
//                else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
//                {
//                    _dataAccessManger.UpdateCmdMstRemark(db, CmdSno, Remark.CmdLeftOver);
//                    return;
//                }
//                else if (CheckEmptyWillBefullOrNot() == true)
//                {
//                    _dataAccessManger.UpdateCmdMstRemark(db, CmdSno, Remark.EmptyWillBefull);
//                    return;
//                }
//                else if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
//                {
//                    _dataAccessManger.UpdateCmdMstRemark(db, CmdSno, Remark.NotStoreOutReady);
//                    return;
//                }
//                #endregion
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreOutLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

//private void StoreOut_A2ToA4_WriteCV()//StoreOut寫入BufferA2ToA4
//{
//    try
//    {
//        for (int bufferIndex = 5; bufferIndex <= 9; bufferIndex += 2)
//        {
//            using (var db = _dataAccessManger.GetDB())
//            {
//                string stn = "";
//                if (bufferIndex == 5)
//                {
//                    stn = StnNo.A5;
//                }
//                else if (bufferIndex == 7)
//                {
//                    stn = StnNo.A7;
//                }
//                else if (bufferIndex == 9)
//                {
//                    stn = StnNo.A9;
//                }
//                if (_dataAccessManger.GetCmdMstByStoreOutStart(stn, out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
//                {
//                    string cmdSno = dataObject[0].CmdSno;
//                    int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

//                    var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get StoreOut Command");
//                    log.CmdSno = cmdSno;
//                    log.LoadCategory = cmdmode;
//                    _loggerManager.WriteLogTrace(log);

//                    if (_conveyor.GetBuffer(bufferIndex).Auto
//                        && _conveyor.GetBuffer(bufferIndex).OutMode
//                        && _conveyor.GetBuffer(bufferIndex).CommandId == 0
//                        && _conveyor.GetBuffer(bufferIndex).Presence == false
//                        && _conveyor.GetBuffer(bufferIndex).Error == false
//                        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
//                        && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
//                        && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3)  //為了不跟盤點命令衝突的條件
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
//                        log.CmdSno = cmdSno;
//                        log.LoadCategory = cmdmode;
//                        _loggerManager.WriteLogTrace(log);

//                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Begin Fail");
//                            log.CmdSno = cmdSno;
//                            return;
//                        }
//                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd Success");
//                            log.CmdSno = cmdSno;
//                            log.LoadCategory = cmdmode;
//                            _loggerManager.WriteLogTrace(log);
//                        }
//                        else
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, cmdmode).Result;//寫入命令和模式//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
//                        bool Result = WritePlccheck.Item1;
//                        string exmessage = WritePlccheck.Item2;
//                        if (Result != true)//寫入命令和模式
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }

//                    }
//                    #region 站口狀態自動確認-Update-CMD-Remark
//                    else if (_conveyor.GetBuffer(bufferIndex).OutMode == false)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotOutMode);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotAutoMode);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Error == true)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.BufferError);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 3)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CycleOperating);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.PresenceExist);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CmdLeftOver);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotStoreOutReady);
//                        return;
//                    }
//                    #endregion
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreOutLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

////#region//用不到了Storeout寫入Buffer
////private void StoreOut_S201_WriteCV()
////{
////    int bufferIndex = 5; //A5
////    using (var db = _dataAccessManger.GetDB())
////    {

////        if (_dataAccessManger.GetCmdMstByStoreOut(StnNo.A6, out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
////        {
////            string cmdSno = dataObject[0].CmdSno;
////            int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

////            var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get StoreOut Command");
////            log.CmdSno = cmdSno;
////            log.LoadCategory = cmdmode;
////            _loggerManager.WriteLogTrace(log);

////            if (_conveyor.GetBuffer(bufferIndex).Auto
////                && _conveyor.GetBuffer(bufferIndex).OutMode
////                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
////                && _conveyor.GetBuffer(bufferIndex).Presence == false
////                && _conveyor.GetBuffer(bufferIndex).Error == false
////                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
////                && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
////                && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3)  //為了不跟盤點命令衝突的條件
////            {
////                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
////                log.CmdSno = cmdSno;
////                log.LoadCategory = cmdmode;
////                _loggerManager.WriteLogTrace(log);

////                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Begin Fail");
////                    log.CmdSno = cmdSno;
////                    return;
////                }
////                if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd Success");
////                    log.CmdSno = cmdSno;
////                    log.LoadCategory = cmdmode;
////                    _loggerManager.WriteLogTrace(log);
////                }
////                else
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if(_conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, cmdmode).Result!=true)//寫入命令和模式
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "WritePLC Command-mode Fail");
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }

////            }
////            #region 站口狀態自動確認-Update-CMD-Remark
////            else if (_conveyor.GetBuffer(bufferIndex).OutMode == false)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotOutMode);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotAutoMode);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Error == true)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.BufferError);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 3)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CycleOperating);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.PresenceExist);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CmdLeftOver);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotStoreOutReady);
////                return;
////            }
////            #endregion
////        }
////    }
////}

////private void StoreOut_S301_WriteCV()
////{
////    int bufferIndex = 7; //A7
////    using (var db = _dataAccessManger.GetDB())
////    {

////        if (_dataAccessManger.GetCmdMstByStoreOut(StnNo.A8, out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
////        {
////            string cmdSno = dataObject[0].CmdSno;
////            int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

////            var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get StoreOut Command");
////            log.CmdSno = cmdSno;
////            log.LoadCategory = cmdmode;
////            _loggerManager.WriteLogTrace(log);

////            if (_conveyor.GetBuffer(bufferIndex).Auto
////                && _conveyor.GetBuffer(bufferIndex).OutMode
////                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
////                && _conveyor.GetBuffer(bufferIndex).Presence == false
////                && _conveyor.GetBuffer(bufferIndex).Error == false
////                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
////                && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
////                && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3)  //為了不跟盤點命令衝突的條件
////            {
////                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
////                log.CmdSno = cmdSno;
////                log.LoadCategory = cmdmode;
////                _loggerManager.WriteLogTrace(log);

////                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Begin Fail");
////                    log.CmdSno = cmdSno;
////                    return;
////                }
////                if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd Success");
////                    log.CmdSno = cmdSno;
////                    log.LoadCategory = cmdmode;
////                    _loggerManager.WriteLogTrace(log);
////                }
////                else
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (_conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, cmdmode).Result != true)//寫入命令和模式
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "WritePLC Command-mode Fail");
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }

////            }
////            #region 站口狀態自動確認-Update-CMD-Remark
////            else if (_conveyor.GetBuffer(bufferIndex).OutMode == false)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotOutMode);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotAutoMode);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Error == true)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.BufferError);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 3)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CycleOperating);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.PresenceExist);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CmdLeftOver);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotStoreOutReady);
////                return;
////            }
////            #endregion
////        }
////    }
////}

////private void StoreOut_S401_WriteCV()
////{
////    int bufferIndex = 9; //A9
////    using (var db = _dataAccessManger.GetDB())
////    {

////        if (_dataAccessManger.GetCmdMstByStoreOut(StnNo.A10, out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
////        {
////            string cmdSno = dataObject[0].CmdSno;
////            int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

////            var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get StoreOut Command");
////            log.CmdSno = cmdSno;
////            log.LoadCategory = cmdmode;
////            _loggerManager.WriteLogTrace(log);


////            if (_conveyor.GetBuffer(bufferIndex).Auto
////                && _conveyor.GetBuffer(bufferIndex).OutMode
////                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
////                && _conveyor.GetBuffer(bufferIndex).Presence == false
////                && _conveyor.GetBuffer(bufferIndex).Error == false
////                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
////                && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
////                && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3)  //為了不跟盤點命令衝突的條件
////            {
////                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
////                log.CmdSno = cmdSno;
////                log.LoadCategory = cmdmode;
////                _loggerManager.WriteLogTrace(log);

////                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Begin Fail");
////                    log.CmdSno = cmdSno;
////                    return;
////                }
////                if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd Success");
////                    log.CmdSno = cmdSno;
////                    log.LoadCategory = cmdmode;
////                    _loggerManager.WriteLogTrace(log);
////                }
////                else
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (_conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, cmdmode).Result != true)//寫入命令和模式
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "WritePLC Command-mode Fail");
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////            }
////            #region 站口狀態自動確認-Update-CMD-Remark
////            else if (_conveyor.GetBuffer(bufferIndex).OutMode == false)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotOutMode);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotAutoMode);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Error == true)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.BufferError);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 3)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CycleOperating);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.PresenceExist);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CmdLeftOver);
////                return;
////            }
////            else if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
////            {
////                _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotStoreOutReady);
////                return;
////            }
////            #endregion
////        }
////    }
////}
////#endregion

////private void StoreOut_A1_CreateEquCmd()
////{
////    try {
////        int bufferIndex = 1;
////        var db1 = _dataAccessManger.GetDB();
////        string cmdSno1 = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
////        if (_conveyor.GetBuffer(bufferIndex).Auto
////            && _conveyor.GetBuffer(bufferIndex).OutMode
////            && _conveyor.GetBuffer(bufferIndex).CommandId > 0
////            && _conveyor.GetBuffer(bufferIndex).Presence == false
////            && _conveyor.GetBuffer(bufferIndex).Error == false
////            && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
////            && CheckEmptyWillBefullOrNot() == false)//檢查一樓buffer是否整體滿九版空棧板了
////        {
////            string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
////            int CmdMode = _conveyor.GetBuffer(bufferIndex).CmdMode;

////            var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut");
////            log.CmdSno = cmdSno;
////            log.LoadCategory = CmdMode;
////            _loggerManager.WriteLogTrace(log);

////            if (_dataAccessManger.GetCmdMstByStoreOutCrane(cmdSno, out var dataObject) == GetDataResult.Success)
////            {
////                string source = dataObject[0].Loc;
////                string dest = $"{CranePortNo.A1}";

////                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Get Command");
////                log.CmdSno = cmdSno;
////                _loggerManager.WriteLogTrace(log);

////                using (var db = _dataAccessManger.GetDB())
////                {
////                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                    {
////                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
////                        log.CmdSno = cmdSno;
////                        _loggerManager.WriteLogTrace(log);
////                        return;
////                    }
////                    if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
////                    {
////                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
////                        log.CmdSno = cmdSno;
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }
////                    if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
////                    {
////                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Insert EquCmd Fail");
////                        log.CmdSno = cmdSno;
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }
////                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                    {
////                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
////                        log.CmdSno = cmdSno;
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }
////                }
////            }
////        }
////        #region 站口狀態自動確認-Update-CMD-Remark
////        else if (_conveyor.GetBuffer(bufferIndex).OutMode == false)
////        {
////            _dataAccessManger.UpdateCmdMstRemark(db1, cmdSno1, Remark.NotOutMode);
////            return;
////        }
////        else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
////        {
////            _dataAccessManger.UpdateCmdMstRemark(db1, cmdSno1, Remark.NotAutoMode);
////            return;
////        }
////        else if (_conveyor.GetBuffer(bufferIndex).Error == true)
////        {
////            _dataAccessManger.UpdateCmdMstRemark(db1, cmdSno1, Remark.BufferError);
////            return;
////        }
////        else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
////        {
////            _dataAccessManger.UpdateCmdMstRemark(db1, cmdSno1, Remark.PresenceExist);
////            return;
////        }
////        else if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
////        {
////            _dataAccessManger.UpdateCmdMstRemark(db1, cmdSno1, Remark.NotStoreOutReady);
////            return;
////        }
////        else if (CheckEmptyWillBefullOrNot() == true)//在成立Crane命令時，再次確認滿板條件，如果這時確認快滿板條件符合，要把已經寫入A1的命令號清除，於是要寫入buffer初始的點位
////        {
////            _dataAccessManger.UpdateCmdMstRemark(db1, cmdSno1, Remark.Crane_EmptyWillBefull);

////            var WritePlccheck = _conveyor.GetBuffer(1).InitialNoticeTrigger().Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
////            bool Result = WritePlccheck.Item1;
////            string exmessage = WritePlccheck.Item2;
////            if (Result != true)//A1初始
////            {
////                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"A1 InitialNotice Fail:{exmessage}");
////                _loggerManager.WriteLogTrace(log);
////            }
////            return;
////        }
////        #endregion
////    }
////    catch (Exception ex)
////    {
////        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
////        var log = new StoreOutLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
////        _loggerManager.WriteLogTrace(log);
////    }
////}

////private void StoreOut_A2ToA4_CreateEquCmd()
////{
////    try
////    {
////        for (int bufferIndex = 5; bufferIndex <= 9; bufferIndex += 2)
////        {
////            if (_conveyor.GetBuffer(bufferIndex).Auto
////                && _conveyor.GetBuffer(bufferIndex).OutMode
////                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
////                && _conveyor.GetBuffer(bufferIndex).Presence == false
////                && _conveyor.GetBuffer(bufferIndex).Error == false
////                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
////            {
////                string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
////                int LoadCategory = _conveyor.GetBuffer(bufferIndex).CmdMode;

////                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut");
////                log.CmdSno = cmdSno;
////                log.LoadCategory = LoadCategory;
////                _loggerManager.WriteLogTrace(log);

////                if (_dataAccessManger.GetCmdMstByStoreOutCrane(cmdSno, out var dataObject) == GetDataResult.Success)
////                {
////                    cmdSno = dataObject[0].CmdSno;
////                    string source = dataObject[0].Loc;
////                    string dest = "";
////                    if (bufferIndex == 5)
////                    {
////                        dest = $"{CranePortNo.A5}";
////                    }
////                    else if (bufferIndex == 7)
////                    {
////                        dest = $"{CranePortNo.A7}";
////                    }
////                    else if (bufferIndex == 9)
////                    {
////                        dest = $"{CranePortNo.A9}";
////                    }

////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Get Command");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);

////                    using (var db = _dataAccessManger.GetDB())
////                    {
////                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                        {
////                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
////                            log.CmdSno = cmdSno;
////                            _loggerManager.WriteLogTrace(log);
////                            return;
////                        }
////                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
////                        {
////                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
////                            log.CmdSno = cmdSno;
////                            _loggerManager.WriteLogTrace(log);
////                            db.TransactionCtrl2(TransactionTypes.Rollback);
////                            return;
////                        }
////                        if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
////                        {
////                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Insert EquCmd Fail");
////                            log.CmdSno = cmdSno;
////                            _loggerManager.WriteLogTrace(log);
////                            db.TransactionCtrl2(TransactionTypes.Rollback);
////                            return;
////                        }
////                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                        {
////                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
////                            log.CmdSno = cmdSno;
////                            _loggerManager.WriteLogTrace(log);
////                            db.TransactionCtrl2(TransactionTypes.Rollback);
////                            return;
////                        }
////                    }
////                }
////            }
////        }
////    }
////    catch (Exception ex)
////    {
////        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
////        var log = new StoreOutLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
////        _loggerManager.WriteLogTrace(log);
////    }
////}

////#region//用不到Crane命令
////private void StoreOut_S201_CreateEquCmd()
////{
////    int bufferIndex = 5;
////    if (_conveyor.GetBuffer(bufferIndex).Auto
////        && _conveyor.GetBuffer(bufferIndex).OutMode
////        && _conveyor.GetBuffer(bufferIndex).CommandId > 0
////        && _conveyor.GetBuffer(bufferIndex).Presence == false
////        && _conveyor.GetBuffer(bufferIndex).Error == false
////        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
////    {
////        string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
////        int LoadCategory = _conveyor.GetBuffer(bufferIndex).CmdMode;

////        var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut");
////        log.CmdSno = cmdSno;
////        log.LoadCategory = LoadCategory;
////        _loggerManager.WriteLogTrace(log);

////        if (_dataAccessManger.GetCmdMstByStoreOut(StnNo.A6, cmdSno, out var dataObject) == GetDataResult.Success)
////        {
////            cmdSno = dataObject[0].CmdSno;
////            string source = dataObject[0].Loc;
////            string dest = $"{CranePortNo.A5}";

////            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Get Command");
////            log.CmdSno = cmdSno;
////            _loggerManager.WriteLogTrace(log);

////            using (var db = _dataAccessManger.GetDB())
////            {
////                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    return;
////                }
////                if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Insert EquCmd Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////            }
////        }
////    }
////}

////private void StoreOut_S301_CreateEquCmd()
////{
////    int bufferIndex = 7;
////    if (_conveyor.GetBuffer(bufferIndex).Auto
////        && _conveyor.GetBuffer(bufferIndex).OutMode
////        && _conveyor.GetBuffer(bufferIndex).CommandId > 0
////        && _conveyor.GetBuffer(bufferIndex).Presence == false
////        && _conveyor.GetBuffer(bufferIndex).Error == false
////        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
////    {
////        string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
////        int LoadCategory = _conveyor.GetBuffer(bufferIndex).CmdMode;

////        var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut");
////        log.CmdSno = cmdSno;
////        log.LoadCategory = LoadCategory;
////        _loggerManager.WriteLogTrace(log);

////        if (_dataAccessManger.GetCmdMstByStoreOut(StnNo.A8, cmdSno, out var dataObject) == GetDataResult.Success)
////        {
////            cmdSno = dataObject[0].CmdSno;
////            string source = dataObject[0].Loc;
////            string dest = $"{CranePortNo.A7}";

////            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Get Command");
////            log.CmdSno = cmdSno;
////            _loggerManager.WriteLogTrace(log);

////            using (var db = _dataAccessManger.GetDB())
////            {
////                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    return;
////                }
////                if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Insert EquCmd Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////            }
////        }
////    }
////}

////private void StoreOut_S401_CreateEquCmd()
////{
////    int bufferIndex = 1;
////    if (_conveyor.GetBuffer(bufferIndex).Auto
////        && _conveyor.GetBuffer(bufferIndex).OutMode
////        && _conveyor.GetBuffer(bufferIndex).CommandId > 0
////        && _conveyor.GetBuffer(bufferIndex).Presence == false
////        && _conveyor.GetBuffer(bufferIndex).Error == false
////        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
////    {
////        string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
////        int LoadCategory = _conveyor.GetBuffer(bufferIndex).CmdMode;

////        var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut");
////        log.CmdSno = cmdSno;
////        log.LoadCategory = LoadCategory;
////        _loggerManager.WriteLogTrace(log);

////        if (_dataAccessManger.GetCmdMstByStoreOut(StnNo.A10, cmdSno, out var dataObject) == GetDataResult.Success)
////        {
////            cmdSno = dataObject[0].CmdSno;
////            string source = dataObject[0].Loc;
////            string dest = $"{CranePortNo.A9}";

////            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Get Command");
////            log.CmdSno = cmdSno;
////            _loggerManager.WriteLogTrace(log);

////            using (var db = _dataAccessManger.GetDB())
////            {
////                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    return;
////                }
////                if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Insert EquCmd Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                {
////                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
////                    log.CmdSno = cmdSno;
////                    _loggerManager.WriteLogTrace(log);
////                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                    return;
////                }
////            }
////        }
////    }
////}
////#endregion

////private void StoreOut_EquCmdFinish()
////{
////    try
////    {
////    var stn = new List<string>()
////    {
////        StnNo.A3,
////        StnNo.A6,
////        StnNo.A8,
////        StnNo.A10,
////    };
////        if (_dataAccessManger.GetCmdMstByStoreOutFinish(stn, out var dataObject) == GetDataResult.Success)
////        {
////            foreach (var cmdMst in dataObject.Data)
////            {
////                if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
////                {
////                    if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
////                    {
////                        if (equCmd[0].CompleteCode == "92")
////                        {
////                            using (var db = _dataAccessManger.GetDB())
////                            {
////                                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                                {
////                                    return;
////                                }
////                                if (cmdMst.IOType == "2")
////                                {
////                                    if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.StoreOutCraneCmdFinish) == ExecuteSQLResult.Success)
////                                    {
////                                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                                        return;
////                                    }
////                                }
////                                if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) == ExecuteSQLResult.Success)
////                                {
////                                    db.TransactionCtrl2(TransactionTypes.Rollback);
////                                    return;
////                                }
////                                if (db.TransactionCtrl2(TransactionTypes.Commit) == TransactionCtrlResult.Success)
////                                {
////                                }
////                            }
////                        }
////                        else if (equCmd[0].CompleteCode.StartsWith("W"))
////                        {
////                            using (var db = _dataAccessManger.GetDB())
////                            {
////                                if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) == ExecuteSQLResult.Success)
////                                {
////                                    return;
////                                }
////                            }
////                        }
////                    }
////                }
////            }
////        }
////    }
////    catch (Exception ex)
////    {
////        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
////        var log = new StoreOutLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
////        _loggerManager.WriteLogTrace(log);
////    }
////}

//#region//判斷function:當檢查命令是最後一個以及一樓buffer沒有貨物，便要直接路徑到A3，狀態正確寫入1
//private int LastCargoOrNot()
//{
//    if (_dataAccessManger.GetCmdMstByStoreOutcheck(StnNo.A3, out var dataObject) == GetDataResult.Success)
//    {
//        int COUNT = Convert.ToInt32(dataObject[0].COUNT);

//        if (_conveyor.GetBuffer(2).A2LV2 == 0 && COUNT == '0' && _conveyor.GetBuffer(2).CommandId == 0 && _conveyor.GetBuffer(1).CommandId == 0)
//        {
//            return 1;
//        }
//        else
//        {
//            return 0;
//        }
//    }
//    else
//    {
//        return 99;//異常連不到資料庫
//    }

//}
//#endregion

//#region//根據判斷去決定一樓空棧板總數是否滿了，去擋下出庫命令
//private bool CheckEmptyWillBefullOrNot()
//{

//    if ((_conveyor.GetBuffer(4).EmptyINReady == 8 && _conveyor.GetBuffer(1).CommandId != 0) || (_conveyor.GetBuffer(4).EmptyINReady == 8 && _conveyor.GetBuffer(2).CommandId != 0))
//    {
//        return true;
//    }
//    else
//    {
//        return false;
//    }

//}
//#endregion
#endregion StoreOut

#region StoreIn
////private void StoreIn_A1_WriteCV()
////{
////    try
////    {
////        int bufferIndex = 3;
////        using (var db = _dataAccessManger.GetDB())
////        {

////            if (_dataAccessManger.GetCmdMstByStoreInstart(StnNo.A3, out var dataObject) == GetDataResult.Success) //讀取CMD_MST
////            {
////                string cmdSno = dataObject[0].CmdSno;
////                int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
////                int IOType = Convert.ToInt32(dataObject[0].IOType);

////                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get StoreIn Command");
////                log.CmdSno = cmdSno;
////                _loggerManager.WriteLogTrace(log);

////                //確認目前模式，是否可以切換模式，可以就寫入切換成入庫的請求
////                if (_conveyor.GetBuffer(bufferIndex - 2).Ready != Ready.StoreInReady
////                    && _conveyor.GetBuffer(bufferIndex - 2).Switch_Ack == 1)
////                {
////                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreIn Ready, Can Switchmode");
////                    _loggerManager.WriteLogTrace(log);

////                    var WritePlccheck = _conveyor.GetBuffer(bufferIndex - 2).Switch_Mode(1).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
////                    bool Result = WritePlccheck.Item1;
////                    string exmessage = WritePlccheck.Item2;
////                    if (Result != true)
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Normal-StoreIn Switchmode fail:{exmessage}");
////                        _loggerManager.WriteLogTrace(log);
////                        return;
////                    }
////                    else
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Switchmode Complete");
////                        _loggerManager.WriteLogTrace(log);
////                    }
////                }

////                if (IOType == IOtype.NormalstorIn
////                 && _conveyor.GetBuffer(bufferIndex).Auto
////                && _conveyor.GetBuffer(bufferIndex).InMode
////                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
////                && _conveyor.GetBuffer(bufferIndex).Presence == false
////                && _conveyor.GetBuffer(bufferIndex).Error == false
////                && _conveyor.GetBuffer(bufferIndex - 2).Ready == Ready.StoreInReady
////                && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
////                && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
////                && _conveyor.GetBuffer(bufferIndex - 2).CmdMode != 3   //為了不跟盤點命令衝突的條件
////                 && _conveyor.GetBuffer(bufferIndex + 1).Presence == true) //在一般入庫時要確認A4是否有空棧板，沒有則不寫入命令
////                {
////                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
////                    _loggerManager.WriteLogTrace(log);

////                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
////                        _loggerManager.WriteLogTrace(log);
////                        return;
////                    }
////                    if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd succeess");
////                        log.CmdSno = cmdSno;

////                        _loggerManager.WriteLogTrace(log);
////                    }
////                    else
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
////                        log.CmdSno = cmdSno;
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }

////                    var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
////                    bool Result = WritePlccheck.Item1;
////                    string exmessage = WritePlccheck.Item2;
////                    if (Result != true)//寫入命令和模式
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }
////                    if (IOType == IOtype.NormalstorIn)
////                    {
////                        WritePlccheck = _conveyor.GetBuffer(4).A4EmptysupplyOn().Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
////                        Result = WritePlccheck.Item1;
////                        exmessage = WritePlccheck.Item2;
////                        if (Result != true)//請A4補充母托一版
////                        {
////                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC A4EmptySupply Fail:{exmessage}");
////                            _loggerManager.WriteLogTrace(log);
////                            db.TransactionCtrl2(TransactionTypes.Rollback);
////                            return;
////                        }
////                    }
////                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }

////                }
////                else if (IOType == IOtype.NormalstorIn //待確認類別，目前尚未確定
////                && _conveyor.GetBuffer(bufferIndex).Auto
////                && _conveyor.GetBuffer(bufferIndex).InMode
////                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
////                && _conveyor.GetBuffer(bufferIndex).Presence == false
////                && _conveyor.GetBuffer(bufferIndex).Error == false
////                && _conveyor.GetBuffer(bufferIndex - 2).Ready == Ready.StoreInReady
////                && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
////                && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
////                && _conveyor.GetBuffer(bufferIndex - 2).CmdMode != 3)  //為了不跟盤點命令衝突的條件
////                {
////                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
////                    _loggerManager.WriteLogTrace(log);

////                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
////                        _loggerManager.WriteLogTrace(log);
////                        return;
////                    }
////                    if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Update cmd Success");
////                        log.CmdSno = cmdSno;
////                        _loggerManager.WriteLogTrace(log);
////                    }
////                    else
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
////                        log.CmdSno = cmdSno;
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }
////                    var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
////                    bool Result = WritePlccheck.Item1;
////                    string exmessage = WritePlccheck.Item2;
////                    if (Result != true)//寫入命令和模式
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }
////                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
////                    {
////                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
////                        _loggerManager.WriteLogTrace(log);
////                        db.TransactionCtrl2(TransactionTypes.Rollback);
////                        return;
////                    }

////                }
////                #region 站口狀態自動確認-Update-CMD-Remark
////                else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
////                {
////                    _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotInMode);
////                    return;
////                }
////                else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
////                {
////                    _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotAutoMode);
////                    return;
////                }
////                else if (_conveyor.GetBuffer(bufferIndex).Error == true)
////                {
////                    _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.BufferError);
////                    return;
////                }
////                else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 1).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 2).CmdMode == 3)
////                {
////                    _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CycleOperating);
////                    return;
////                }
////                else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
////                {
////                    _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.PresenceExist);
////                    return;
////                }
////                else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
////                {
////                    _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CmdLeftOver);
////                    return;
////                }
////                else if (_conveyor.GetBuffer(bufferIndex + 1).Presence == false)
////                {
////                    _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.A4EmptyisEmpty);
////                    return;
////                }
////                else if (_conveyor.GetBuffer(bufferIndex - 2).Ready != Ready.StoreInReady)
////                {
////                    _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotStoreInReady);
////                    return;
////                }
////                #endregion
////            }
////        }
////    }
////    catch (Exception ex)
////    {
////        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
////        var log = new StoreOutLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
////        _loggerManager.WriteLogTrace(log);
////    }
////}

//private void StoreIn_A2ToA4_WriteCV()//A2toA4寫入buffer
//{
//    string stn = "";
//    try
//    {
//        for (int bufferIndex = 6; bufferIndex <= 10; bufferIndex += 2)
//        {
//            using (var db = _dataAccessManger.GetDB())
//            {
//                if (bufferIndex == 6)
//                {
//                    stn = StnNo.A6;
//                }
//                else if (bufferIndex == 8)
//                {
//                    stn = StnNo.A8;
//                }
//                else if (bufferIndex == 10)
//                {
//                    stn = StnNo.A10;
//                }

//                if (_dataAccessManger.GetCmdMstByStoreInstart(stn, out var dataObject) == GetDataResult.Success) //讀取CMD_MST
//                {
//                    string cmdSno = dataObject[0].CmdSno;
//                    int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

//                    var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get StoreIn Command");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);


//                    if (_conveyor.GetBuffer(bufferIndex).Auto
//                    && _conveyor.GetBuffer(bufferIndex).InMode
//                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
//                    && _conveyor.GetBuffer(bufferIndex).Presence == false
//                    && _conveyor.GetBuffer(bufferIndex).Error == false
//                    && _conveyor.GetBuffer(bufferIndex - 1).Ready == Ready.StoreInReady
//                    && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
//                    && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3) //為了不跟盤點命令衝突的條件)
//                    {
//                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
//                        _loggerManager.WriteLogTrace(log);

//                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
//                            _loggerManager.WriteLogTrace(log);
//                            return;
//                        }
//                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd success");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                        }
//                        else
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
//                        bool Result = WritePlccheck.Item1;
//                        string exmessage = WritePlccheck.Item2;
//                        if (Result != true)//寫入命令和模式
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                    }
//                    #region 站口狀態自動確認-Update-CMD-Remark
//                    else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotInMode);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotAutoMode);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Error == true)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.BufferError);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 1).CmdMode == 3)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CycleOperating);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.PresenceExist);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CmdLeftOver);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex - 1).Ready != Ready.StoreInReady)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotStoreInReady);
//                        return;
//                    }
//                    #endregion

//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

//private void StoreIn_A1_CreateEquCmd()
//{
//    int bufferIndex = 1;
//    if (_conveyor.GetBuffer(bufferIndex).Auto
//        && _conveyor.GetBuffer(bufferIndex).InMode
//        && _conveyor.GetBuffer(bufferIndex).CommandId > 0
//        && _conveyor.GetBuffer(bufferIndex).Presence
//        && _conveyor.GetBuffer(bufferIndex).Error == false)
//    {
//        var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
//        log.CmdSno = string.Empty;
//        _loggerManager.WriteLogTrace(log);

//        string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
//        if (_dataAccessManger.GetCmdMstByStoreInCrane(cmdSno, out var dataObject) == GetDataResult.Success)
//        {

//            string source = $"{CranePortNo.A1}";
//            string IOType = dataObject[0].IOType;
//            string dest = "";
//            if (IOType == IOtype.Cycle.ToString())//如果是盤點，入庫儲位欄位是LOC，一般出庫是NewLoc
//            {
//                dest = $"{dataObject[0].Loc}";
//            }
//            else
//            {
//                dest = $"{dataObject[0].NewLoc}";
//            }

//            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreIn Get Command");
//            log.CmdSno = cmdSno;
//            _loggerManager.WriteLogTrace(log);

//            using (var db = _dataAccessManger.GetDB())
//            {
//                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                {
//                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);
//                    return;
//                }
//                if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
//                {
//                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);
//                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                    return;
//                }
//                if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
//                {
//                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Insert EquCmd Fail");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);
//                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                    return;
//                }
//                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                {
//                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);
//                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                    return;
//                }
//            }
//        }
//    }
//}

//private void StoreIn_A2ToA4_CreateEquCmd()//A2ToA4建立Crane命令
//{
//    try
//    {
//        for (int bufferIndex = 5; bufferIndex <= 9; bufferIndex += 2)
//        {
//            if (_conveyor.GetBuffer(bufferIndex).Auto
//                && _conveyor.GetBuffer(bufferIndex).InMode
//                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
//                && _conveyor.GetBuffer(bufferIndex).Presence
//                && _conveyor.GetBuffer(bufferIndex).Error == false)
//            {

//                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
//                log.CmdSno = string.Empty;
//                _loggerManager.WriteLogTrace(log);

//                string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
//                if (_dataAccessManger.GetCmdMstByStoreInCrane(cmdSno, out var dataObject) == GetDataResult.Success)
//                {
//                    string source = "";
//                    if (bufferIndex == 5)
//                    {
//                        source = $"{CranePortNo.A5}";
//                    }
//                    else if (bufferIndex == 7)
//                    {
//                        source = $"{CranePortNo.A7}";
//                    }
//                    else if (bufferIndex == 9)
//                    {
//                        source = $"{CranePortNo.A9}";
//                    }
//                    string IOType = dataObject[0].IOType;
//                    string dest = "";
//                    if (IOType == IOtype.Cycle.ToString()) //如果是盤點，入庫儲位欄位是LOC，一般出庫是NewLoc
//                    {
//                        dest = $"{dataObject[0].Loc}";
//                    }
//                    else
//                    {
//                        dest = $"{dataObject[0].NewLoc}";
//                    }

//                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreIn Get Command");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);

//                    using (var db = _dataAccessManger.GetDB())
//                    {
//                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                            return;
//                        }
//                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Insert EquCmd Fail");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                    }
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

//private void StoreIn_EquCmdFinish()
//{
//    try
//    {
//        var stn1 = new List<string>()
//    {
//        StnNo.A3,
//        StnNo.A6,
//        StnNo.A8,
//        StnNo.A10,
//    };
//        if (_dataAccessManger.GetCmdMstByStoreInFinish(stn1, out var dataObject) == GetDataResult.Success)
//        {
//            foreach (var cmdMst in dataObject.Data)
//            {
//                if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
//                {
//                    if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
//                    {
//                        if (equCmd[0].CompleteCode == "92")
//                        {
//                            using (var db = _dataAccessManger.GetDB())
//                            {
//                                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                                {
//                                    return;
//                                }
//                                if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.StoreInCraneCmdFinish) != ExecuteSQLResult.Success)
//                                {
//                                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                                    return;
//                                }
//                                if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
//                                {
//                                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                                    return;
//                                }
//                                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                                {
//                                    return;
//                                }
//                            }
//                        }
//                        else if (equCmd[0].CompleteCode.StartsWith("W"))
//                        {
//                            using (var db = _dataAccessManger.GetDB())
//                            {
//                                if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
//                                {
//                                    return;
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

#endregion StoreIn

#region Other
//private void EmptyStoreIn_A1_WriteCV()
//{
//    try
//    {
//        int bufferIndex = 4;
//        using (var db = _dataAccessManger.GetDB())
//        {
//            int CmdMode = 1;//待確認
//            if (_conveyor.GetBuffer(bufferIndex).EmptyINReady == 9) //滿九版,滿版訊號為9
//            {
//                if (_dataAccessManger.GetCmdMstByStoreInstart(StnNo.A4, out var dataObject) == GetDataResult.Success) //讀取CMD_MST
//                {
//                    string cmdSno = dataObject[0].CmdSno;

//                    var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get EmptyStoreIn Command");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);

//                    if (_conveyor.GetBuffer(bufferIndex).Auto
//                    && _conveyor.GetBuffer(bufferIndex).InMode
//                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
//                    && _conveyor.GetBuffer(bufferIndex).Presence == true
//                    && _conveyor.GetBuffer(bufferIndex).Error == false
//                    && _conveyor.GetBuffer(bufferIndex - 3).Ready == Ready.StoreInReady
//                    && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
//                    && _conveyor.GetBuffer(bufferIndex - 2).CmdMode != 3  //為了不跟盤點命令衝突的條件
//                    && _conveyor.GetBuffer(bufferIndex - 3).CmdMode != 3) //為了不跟盤點命令衝突的條件)
//                    {
//                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive EmptyStoreIn Command");
//                        _loggerManager.WriteLogTrace(log);

//                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "begin fail");
//                            _loggerManager.WriteLogTrace(log);
//                            return;
//                        }
//                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.EmptyStoreInWriteCraneCmdToCV) == ExecuteSQLResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Update cmd suceess");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                        }
//                        else
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
//                        bool Result = WritePlccheck.Item1;
//                        string exmessage = WritePlccheck.Item2;
//                        if (Result != true)//寫入命令和模式
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }

//                    }
//                    #region 站口狀態自動確認-Update-CMD-Remark
//                    else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotInMode);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotAutoMode);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Error == true)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.BufferError);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex - 3).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 1).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 2).CmdMode == 3)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CycleOperating);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CmdLeftOver);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex - 1).Ready != Ready.StoreInReady)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotStoreInReady);
//                        return;
//                    }
//                    #endregion
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

//private void EmptyStoreIn_A1_CreateEquCmd()
//{
//    try
//    {
//        int bufferIndex = 1;
//        if (_conveyor.GetBuffer(bufferIndex).Auto
//            && _conveyor.GetBuffer(bufferIndex).InMode
//            && _conveyor.GetBuffer(bufferIndex).CommandId > 0
//            && _conveyor.GetBuffer(bufferIndex).Presence
//            && _conveyor.GetBuffer(bufferIndex).Error == false)
//        {
//            var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready EmptyStoreIn");
//            log.CmdSno = string.Empty;
//            _loggerManager.WriteLogTrace(log);

//            string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
//            if (_dataAccessManger.GetEmptyCmdMstByStoreIn(cmdSno, out var dataObject) == GetDataResult.Success)
//            {

//                string source = $"{CranePortNo.A1}";
//                string dest = $"{dataObject[0].NewLoc}";

//                log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get Command");
//                log.CmdSno = cmdSno;
//                _loggerManager.WriteLogTrace(log);

//                using (var db = _dataAccessManger.GetDB())
//                {
//                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                    {
//                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane EmptyStoreIn Command, Begin Fail");
//                        log.CmdSno = cmdSno;
//                        _loggerManager.WriteLogTrace(log);
//                        return;
//                    }
//                    if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.EmptyStoreInCreateCraneCmd) != ExecuteSQLResult.Success)
//                    {
//                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane EmptyStoreIn Command, Update CmdMst Fail");
//                        log.CmdSno = cmdSno;
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }
//                    if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
//                    {
//                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane EmptyStoreIn Command, Insert EquCmd Fail");
//                        log.CmdSno = cmdSno;
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }
//                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                    {
//                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
//                        log.CmdSno = cmdSno;
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

//private void EmptyStoreIn_EquCmdFinish()
//{
//    try
//    {
//        var stn1 = new List<string>()
//        {
//            StnNo.A4,
//        };
//        if (_dataAccessManger.GetEmptyCmdMstByStoreInFinish(stn1, out var dataObject) == GetDataResult.Success)
//        {
//            foreach (var cmdMst in dataObject.Data)
//            {
//                if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
//                {
//                    if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
//                    {
//                        if (equCmd[0].CompleteCode == "92")
//                        {
//                            using (var db = _dataAccessManger.GetDB())
//                            {
//                                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                                {
//                                    return;
//                                }
//                                if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.EmptyStoreInCraneCmdFinish) != ExecuteSQLResult.Success)
//                                {
//                                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                                    return;
//                                }
//                                if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
//                                {
//                                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                                    return;
//                                }
//                                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                                {
//                                    return;
//                                }
//                            }
//                        }
//                        else if (equCmd[0].CompleteCode.StartsWith("W"))
//                        {
//                            using (var db = _dataAccessManger.GetDB())
//                            {
//                                if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
//                                {
//                                    return;
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

//private void EmptyStoreOut_A1_WriteCV()
//{
//    try
//    {
//        int bufferIndex = 1;
//        using (var db = _dataAccessManger.GetDB())
//        {
//            string cmdSno = "";
//            if (_conveyor.GetBuffer(bufferIndex + 3).Presence == false) //沒有荷有，無空棧板需要補充
//            {
//                if (_dataAccessManger.GetCmdMstByStoreOutStart(StnNo.A4, out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
//                {
//                    cmdSno = dataObject[0].CmdSno;
//                    int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

//                    var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get EmptyStoreOut Command");
//                    log.CmdSno = cmdSno;
//                    log.LoadCategory = cmdmode;
//                    _loggerManager.WriteLogTrace(log);

//                    //確認目前模式，是否可以切換模式，可以就寫入切換成出庫的請求
//                    if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreInReady
//                    && _conveyor.GetBuffer(bufferIndex).Switch_Ack == 1)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreOut Ready, Can Switchmode");
//                        _loggerManager.WriteLogTrace(log);

//                        var WritePlccheck = _conveyor.GetBuffer(bufferIndex).Switch_Mode(2).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
//                        bool Result = WritePlccheck.Item1;
//                        string exmessage = WritePlccheck.Item2;
//                        if (Result != true)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Empty StoreOut Switchmode fail:{exmessage}");
//                            _loggerManager.WriteLogTrace(log);
//                            return;
//                        }
//                        else
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Empty StoreOut Switchmode Complete");
//                            _loggerManager.WriteLogTrace(log);
//                        }
//                    }

//                    if (_conveyor.GetBuffer(bufferIndex).Auto
//                    && _conveyor.GetBuffer(bufferIndex).OutMode
//                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
//                    && _conveyor.GetBuffer(bufferIndex).Presence == false
//                    && _conveyor.GetBuffer(bufferIndex).Error == false
//                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
//                    && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
//                    && _conveyor.GetBuffer(bufferIndex + 2).CmdMode != 3  //為了不跟盤點命令衝突的條件
//                    && _conveyor.GetBuffer(bufferIndex).CmdMode != 3) //為了不跟盤點命令衝突的條件)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive EmptyStoreOut Command");
//                        _loggerManager.WriteLogTrace(log);

//                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Begin Fail");
//                            log.CmdSno = cmdSno;
//                            return;
//                        }
//                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.EmptyStoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Update cmd success");
//                            log.CmdSno = cmdSno;

//                            _loggerManager.WriteLogTrace(log);
//                        }
//                        else
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Upadte cmd fail");
//                            log.CmdSno = cmdSno;
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        var WritePlccheck = _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, cmdmode).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
//                        bool Result = WritePlccheck.Item1;
//                        string exmessage = WritePlccheck.Item2;
//                        if (Result != true)//寫入命令和模式
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Command-mode Fail:{exmessage}");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }

//                        WritePlccheck = _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path3_toA4).Result;//一樓出庫都要寫入路徑編號，確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
//                        Result = WritePlccheck.Item1;
//                        exmessage = WritePlccheck.Item2;
//                        if (Result != true)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"WritePLC Path3_toA4 Fail:{exmessage}");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                        {
//                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Commit Fail");
//                            _loggerManager.WriteLogTrace(log);
//                            db.TransactionCtrl2(TransactionTypes.Rollback);
//                            return;
//                        }
//                    }
//                    #region 站口狀態自動確認-Update-CMD-Remark
//                    else if (_conveyor.GetBuffer(bufferIndex).OutMode == false)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotOutMode);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotAutoMode);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Error == true)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.BufferError);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 1).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex + 2).CmdMode == 3)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CycleOperating);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.CmdLeftOver);
//                        return;
//                    }
//                    else if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady)
//                    {
//                        _dataAccessManger.UpdateCmdMstRemark(db, cmdSno, Remark.NotStoreOutReady);
//                        return;
//                    }
//                    #endregion


//                }

//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

//private void EmptyStoreOut_A1_CreateEquCmd()
//{
//    try
//    {
//        int bufferIndex = 1;
//        if (_conveyor.GetBuffer(bufferIndex).Auto
//            && _conveyor.GetBuffer(bufferIndex).InMode
//            && _conveyor.GetBuffer(bufferIndex).CommandId > 0
//            && _conveyor.GetBuffer(bufferIndex).Presence
//            && _conveyor.GetBuffer(bufferIndex).Error == false)
//        {
//            var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready EmptyStoreOut");
//            _loggerManager.WriteLogTrace(log);

//            string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
//            if (_dataAccessManger.GetCmdMstByEmptyStoreOutCrane(cmdSno, out var dataObject) == GetDataResult.Success)
//            {

//                string source = $"{dataObject[0].Loc}";
//                string dest = $"{CranePortNo.A1}";

//                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Get Command");
//                log.CmdSno = cmdSno;
//                _loggerManager.WriteLogTrace(log);

//                using (var db = _dataAccessManger.GetDB())
//                {
//                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
//                        log.CmdSno = cmdSno;
//                        _loggerManager.WriteLogTrace(log);
//                        return;
//                    }
//                    if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.EmptyStoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
//                        log.CmdSno = cmdSno;
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }
//                    if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Insert EquCmd Fail");
//                        log.CmdSno = cmdSno;
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }
//                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                    {
//                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
//                        log.CmdSno = cmdSno;
//                        _loggerManager.WriteLogTrace(log);
//                        db.TransactionCtrl2(TransactionTypes.Rollback);
//                        return;
//                    }
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}


//private void EmptyStoreOut_EquCmdFinish()
//{
//    try
//    {
//        var stn = new List<string>()
//        {
//            StnNo.A4,
//        };
//        if (_dataAccessManger.GetEmptyCmdMstByStoreOutFinish(stn, out var dataObject) == GetDataResult.Success)
//        {
//            foreach (var cmdMst in dataObject.Data)
//            {
//                if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
//                {
//                    if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
//                    {
//                        if (equCmd[0].CompleteCode == "92")
//                        {
//                            using (var db = _dataAccessManger.GetDB())
//                            {
//                                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                                {
//                                    return;
//                                }
//                                if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.EmptyStoreOutCraneCmdFinish) == ExecuteSQLResult.Success)
//                                {
//                                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                                    return;
//                                }
//                                if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) == ExecuteSQLResult.Success)
//                                {
//                                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                                    return;
//                                }
//                                if (db.TransactionCtrl2(TransactionTypes.Commit) == TransactionCtrlResult.Success)
//                                {
//                                }
//                            }
//                        }
//                        else if (equCmd[0].CompleteCode.StartsWith("W"))
//                        {
//                            using (var db = _dataAccessManger.GetDB())
//                            {
//                                if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) == ExecuteSQLResult.Success)
//                                {
//                                    return;
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

//private void Other_LocToLoc()
//{
//    try
//    {
//        if (_dataAccessManger.GetLocToLoc(out var dataObject) == GetDataResult.Success)
//        {

//            string source = $"{dataObject[0].Loc}";
//            string dest = $"{dataObject[0].NewLoc}";
//            string cmdSno = $"{dataObject[0].CmdSno}";

//            var log = new StoreOutLogTrace(5, "LocToLoc", "LocToLoc Command Received");
//            log.CmdSno = cmdSno;
//            _loggerManager.WriteLogTrace(log);

//            using (var db = _dataAccessManger.GetDB())
//            {
//                if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
//                {
//                    log = new StoreOutLogTrace(5, "LocToLoc", "Create Crane LocToLoc Command, Begin Fail");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);
//                    return;
//                }
//                if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.LoctoLocReady) != ExecuteSQLResult.Success)
//                {
//                    log = new StoreOutLogTrace(5, "LocToLoc", "Create Crane LocToLoc Command, Update CmdMst Fail");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);
//                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                    return;
//                }
//                if (InsertLocToLocEquCmd(db, 5, "LocToLoc", 1, cmdSno, source, dest, 1) == false)
//                {
//                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                    return;
//                }
//                if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
//                {
//                    log = new StoreOutLogTrace(5, "LocToLoc", "Create Crane LocToLoc Command Commit Fail");
//                    log.CmdSno = cmdSno;
//                    _loggerManager.WriteLogTrace(log);
//                    db.TransactionCtrl2(TransactionTypes.Rollback);
//                    return;
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Reflection.MethodBase cmet = System.Reflection.MethodBase.GetCurrentMethod();
//        var log = new StoreInLogTrace(999, cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
//        _loggerManager.WriteLogTrace(log);
//    }
//}

#endregion other

//private bool InsertStnToStnEquCmd(DataBase.DB db, int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority)
//{
//    try
//    {
//        if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.StnToStn, source, destination) == false)
//        {
//            if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.StnToStn).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
//            {
//                var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd");
//                log.CommandId = cmdSno;
//                log.CraneNo = craneNo;
//                log.Source = source;
//                log.Destination = destination;
//                _loggerManager.WriteLogTrace(log);
//                return true;
//            }
//            else
//            {
//                var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd Fail");
//                log.CommandId = cmdSno;
//                log.CraneNo = craneNo;
//                log.Source = source;
//                log.Destination = destination;
//                _loggerManager.WriteLogTrace(log);
//                return false;
//            }
//        }
//        else
//        {
//            return false;
//        }
//    }
//    catch (Exception ex)
//    {
//        _loggerManager.Error(ex);
//        return false;
//    }
//}

//private bool InsertStoreInEquCmd(DataBase.DB db, int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority)
//{
//    try
//    {
//        if (destination.Length != 7)
//        {
//            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check destination Fail, Please Check");
//            log.CommandId = cmdSno;
//            log.CraneNo = craneNo;
//            log.Source = source;
//            log.Destination = destination;
//            _loggerManager.WriteLogTrace(log);
//            return false;
//        }

//        if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.InMode, source, destination) == false)
//        {
//            if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.InMode).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
//            {
//                var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd");
//                log.CommandId = cmdSno;
//                log.CraneNo = craneNo;
//                log.Source = source;
//                log.Destination = destination;
//                _loggerManager.WriteLogTrace(log);
//                return true;
//            }
//            else
//            {
//                var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd Fail");
//                log.CommandId = cmdSno;
//                log.CraneNo = craneNo;
//                log.Source = source;
//                log.Destination = destination;
//                _loggerManager.WriteLogTrace(log);
//                return false;
//            }
//        }
//        else
//        {
//            return false;
//        }


//    }
//    catch (Exception ex)
//    {
//        _loggerManager.Error(ex);
//        return false;
//    }
//}

//private bool InsertStoreOutEquCmd(DataBase.DB db, int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority)
//{
//    try
//    {
//        if (source.Length != 7)
//        {
//            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Source Fail, Please Check");
//            log.CommandId = cmdSno;
//            log.CraneNo = craneNo;
//            log.Source = source;
//            log.Destination = destination;
//            _loggerManager.WriteLogTrace(log);
//            return false;
//        }


//        if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.OutMode, source, destination) == false)
//        {
//            if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.OutMode).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
//            {
//                var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd");
//                log.CommandId = cmdSno;
//                log.CraneNo = craneNo;
//                log.Source = source;
//                log.Destination = destination;
//                _loggerManager.WriteLogTrace(log);
//                return true;
//            }
//            else
//            {
//                var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd Fail");
//                log.CommandId = cmdSno;
//                log.CraneNo = craneNo;
//                log.Source = source;
//                log.Destination = destination;
//                _loggerManager.WriteLogTrace(log);
//                return false;
//            }
//        }
//        else
//        {
//            return false;
//        }
//    }


//    catch (Exception ex)
//    {
//        _loggerManager.Error(ex);
//        return false;
//    }
//}

//private bool InsertLocToLocEquCmd(DataBase.DB db, int CmdType, string IoType, int craneNo, string cmdSno, string source, string destination, int priority)
//{
//    try
//    {
//        if (source.Length != 7)
//        {
//            var log = new EquCmdLogTrace(CmdType, IoType, "Check Source Fail, Please Check");
//            log.CommandId = cmdSno;
//            log.CraneNo = craneNo;
//            log.Source = source;
//            log.Destination = destination;
//            _loggerManager.WriteLogTrace(log);
//            return false;
//        }

//        if (destination.Length != 7)
//        {
//            var log = new EquCmdLogTrace(CmdType, IoType, "Check Destination Fail, Please Check");
//            log.CommandId = cmdSno;
//            log.CraneNo = craneNo;
//            log.Source = source;
//            log.Destination = destination;
//            _loggerManager.WriteLogTrace(log);
//            return false;
//        }

//            if (CheckExecutionEquCmd(CmdType, IoType, craneNo, cmdSno, EquCmdMode.LocToLoc, source, destination) == false)
//            {
//                if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.LocToLoc).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
//                {
//                    var log = new EquCmdLogTrace(CmdType, IoType, "Insert Equ Cmd");
//                    log.CommandId = cmdSno;
//                    log.CraneNo = craneNo;
//                    log.Source = source;
//                    log.Destination = destination;
//                    _loggerManager.WriteLogTrace(log);
//                    return true;
//                }
//                else
//                {
//                    var log = new EquCmdLogTrace(CmdType, IoType, "Insert Equ Cmd Fail");
//                    log.CommandId = cmdSno;
//                    log.CraneNo = craneNo;
//                    log.Source = source;
//                    log.Destination = destination;
//                    _loggerManager.WriteLogTrace(log);
//                    return false;
//                }
//            }
//            else
//            {
//                return false;
//            }
//    }
//    catch (Exception ex)
//    {
//        _loggerManager.Error(ex);
//        return false;
//    }
//}

////檢查是否有重複的Crane命令，狀態為0和1的命令只能為1個
//private bool CheckExecutionEquCmd(int bufferIndex, string bufferName, int craneNo, string cmdSno, EquCmdMode equCmdMode, string source, string destination)
//{
//    if (_dataAccessManger.GetEquCmd(cmdSno, out var equCmd) == GetDataResult.Success)
//    {
//        if (equCmd[0].CmdSts == CmdSts.Queue.GetHashCode().ToString() || equCmd[0].CmdSts == CmdSts.Transferring.GetHashCode().ToString())
//        {
//            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Exists Command On Equ Execute, Please Check");
//            log.CommandId = cmdSno;
//            log.CraneNo = craneNo;
//            log.Source = source;
//            log.Destination = destination;
//            _loggerManager.WriteLogTrace(log);
//            return true;
//        }
//        else
//        {
//            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Exists Command On Equ, Please Check");
//            log.CommandId = cmdSno;
//            log.CraneNo = craneNo;
//            log.Source = source;
//            log.Destination = destination;
//            _loggerManager.WriteLogTrace(log);
//            return true;
//        }
//    }
//    else
//    {
//        if (_dataAccessManger.checkCraneNoReapeat(out var dataObject) == GetDataResult.Success)
//        {
//            int intCraneCount = 0;
//            intCraneCount = int.Parse(dataObject[0].COUNT.ToString());
//            return intCraneCount == 0 ? false : true;
//        }
//        else
//        {
//            return true;
//        }
//    }
//}


#endregion Mark