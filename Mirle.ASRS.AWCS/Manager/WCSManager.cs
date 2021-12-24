using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Mirle.ASRS.AWCS.Model.DataAccess;
using Mirle.ASRS.AWCS.Model.LogTrace;
using Mirle.ASRS.AWCS.Model.PLCDefinitions;
using Mirle.ASRS.Conveyors;
using Mirle.DataBase;

namespace Mirle.ASRS.AWCS.Manager
{
    public class WCSManager
    {
        private readonly bool _isBQA = false;
        private readonly Conveyor _conveyor;
        private readonly LoggerManager _loggerManager;
        private readonly DataAccessManger _dataAccessManger;
        private readonly Timer _storeInProcess = new Timer();
        private readonly Timer _storeOutProcess = new Timer();
        private readonly Timer _otherProcess = new Timer();

        private bool _storeOut_B02_RGVCmdCreateChange = false;

        public WCSManager(CVCSHost host, bool isBQA)
        {
            _conveyor = host.GetCVControllerr().GetConveryor();
            _loggerManager = host.GetLoggerManager();
            _dataAccessManger = host.GetDataAccessManger();
            _isBQA = isBQA;

            _storeOutProcess.Interval = 500;
            _storeInProcess.Interval = 500;
            _otherProcess.Interval = 500;

            _storeOutProcess.Elapsed += StoreOutProcess;
            _storeInProcess.Elapsed += StoreInProcess;
            _otherProcess.Elapsed += OtherProcess;
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

        #region StoreOut
        private void StoreOutProcess(object sender, ElapsedEventArgs e)
        {
            _storeOutProcess.Stop();
            if (_isBQA)
            {
                StoreOut_A08_WriteCV();//OK
                StoreOut_A08_CreateEquCmd();//OK

                StoreOut_A131_WriteCV();//OK
                StoreOut_A131_CreateEquCmd();//OK

                StoreOut_BQA_EquCmdFinish();//OK
            }
            else
            {
                StoreOut_A01_WriteCV();//OK
                StoreOut_A01_CreateEquCmd();//OK

                StoreOut_A03_WriteCV();//OK
                StoreOut_A03_CreateEquCmd();//OK

                StoreOut_B01_WriteCV();//OK
                StoreOut_B01_CreateEquCmd();//OK

                StoreOut_B013_WriteCV();//OK
                StoreOut_B013_CreateEquCmd();//OK

                StoreOut_B02_WriteCV();//OK

                StoreOut_B012_CreateEquCmd();//OK

                StoreOut_B015_CreateEquCmd();//OK

                StoreOut_MFG_EquCmdFinish();//OK
            }
            _storeOutProcess.Start();
        }

        #region BQA

        #region A08
        private void StoreOut_A08_WriteCV()
        {
            int bufferIndex = 1;
            var stn = new List<string>()
            {
                StnNo.A08,
                StnNo.A11_1,
            };
            if (_dataAccessManger.GetCmdMstByStoreOut(stn, out var dataObject) == GetDataResult.Success)
            {
                string cmdSno = dataObject[0].CmdSno;
                string trayId = dataObject[0].TrayId;
                int loadType = Convert.ToInt32(dataObject[0].LoadType);

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                log.CmdSno = cmdSno;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).OutMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
                {
                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }
        private void StoreOut_A08_CreateEquCmd()
        {
            int bufferIndex = 1;
            List<string> stn = new List<string>()
            {
                StnNo.A08,
                StnNo.A11_1,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A01}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }
        #endregion A08

        #region A13_1
        private void StoreOut_A131_WriteCV()
        {
            int bufferIndex = 15;
            var stn = new List<string>()
            {
                StnNo.A17_4,
                StnNo.A19,
            };
            if (_dataAccessManger.GetCmdMstByStoreOut(stn, out var dataObject) == GetDataResult.Success)
            {
                string cmdSno = dataObject[0].CmdSno;
                string trayId = dataObject[0].TrayId;
                int loadType = Convert.ToInt32(dataObject[0].LoadType);

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                log.CmdSno = cmdSno;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).OutMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
                {
                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }
        private void StoreOut_A131_CreateEquCmd()
        {
            int bufferIndex = 15;
            List<string> stn = new List<string>()
            {
                StnNo.A17_4,
                StnNo.A19,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A131}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }
        #endregion A13_1

        private void StoreOut_BQA_EquCmdFinish()
        {
            var stn = new List<string>()
            {
                StnNo.A08,
                StnNo.A11_1,
                StnNo.A17_4,
                StnNo.A19,
            };
            if (_dataAccessManger.GetCmdMstByStoreOutFinish(stn, out var dataObject) == GetDataResult.Success)
            {
                foreach (var cmdMst in dataObject.Data)
                {
                    if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
                    {
                        if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                        {
                            if (equCmd[0].CompleteCode == "92")
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                    if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.StoreOutCraneCmdFinish) == ExecuteSQLResult.Success)
                                    {
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return;
                                    }
                                    if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) == ExecuteSQLResult.Success)
                                    {
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return;
                                    }
                                    if (db.TransactionCtrl2(TransactionTypes.Commit) == TransactionCtrlResult.Success)
                                    {
                                    }
                                }
                            }
                            else if (equCmd[0].CompleteCode.StartsWith("W"))
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) == ExecuteSQLResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion BQA

        #region MFG

        #region A01
        private void StoreOut_A01_WriteCV()
        {
            int bufferIndex = 1;
            var stn = new List<string>()
            {
                StnNo.A01,
                StnNo.A02_1,
            };
            if (_dataAccessManger.GetCmdMstByStoreOut(stn, out var dataObject) == GetDataResult.Success)
            {
                string cmdSno = dataObject[0].CmdSno;
                string trayId = dataObject[0].TrayId;
                int loadType = Convert.ToInt32(dataObject[0].LoadType);

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                log.CmdSno = cmdSno;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).OutMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
                {
                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }
        private void StoreOut_A01_CreateEquCmd()
        {
            int bufferIndex = 1;
            List<string> stn = new List<string>()
            {
                StnNo.A01,
                StnNo.A02_1
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(1).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A01}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }
        #endregion A01

        #region A03
        private void StoreOut_A03_WriteCV()
        {
            int bufferIndex = 10;
            var stn = new List<string>()
            {
                StnNo.A03,
                StnNo.A05_1,
            };
            if (_dataAccessManger.GetCmdMstByStoreOut(stn, out var dataObject) == GetDataResult.Success)
            {
                string cmdSno = dataObject[0].CmdSno;
                string trayId = dataObject[0].TrayId;
                int loadType = Convert.ToInt32(dataObject[0].LoadType);

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                log.CmdSno = cmdSno;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).OutMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
                {
                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }
        private void StoreOut_A03_CreateEquCmd()
        {
            int bufferIndex = 10;
            List<string> stn = new List<string>()
            {
                StnNo.A03,
                StnNo.A05_1,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A03}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 2, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }
        #endregion A03

        #region B01
        private void StoreOut_B01_WriteCV()
        {
            int bufferIndex = 19;
            var stn = new List<string>()
            {
                StnNo.B12_5,
                StnNo.B14_5,
                StnNo.B15_4,
                StnNo.B16_5,
                StnNo.B18,
                StnNo.B10,
            };
            if (_dataAccessManger.GetCmdMstByStoreOut(stn, out var dataObject) == GetDataResult.Success)
            {
                string cmdSno = dataObject[0].CmdSno;
                string trayId = dataObject[0].TrayId;
                int loadType = Convert.ToInt32(dataObject[0].LoadType);

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                log.CmdSno = cmdSno;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).OutMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
                {
                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);//Wait Modify
                        }
                    }
                }
            }
        }
        private void StoreOut_B01_CreateEquCmd()
        {
            int bufferIndex = 19;
            List<string> stn = new List<string>()
            {
                    StnNo.B12_5,
                    StnNo.B14_5,
                    StnNo.B15_4,
                    StnNo.B16_5,
                    StnNo.B18,
                    StnNo.B10,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A01}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }
        #endregion B01

        #region B01_3
        private void StoreOut_B013_WriteCV()
        {
            int bufferIndex = 22;
            var stn = new List<string>()
            {
                StnNo.B12_5,
                StnNo.B14_5,
                StnNo.B15_4,
                StnNo.B16_5,
                StnNo.B18,
                StnNo.B10,
            };
            if (_dataAccessManger.GetCmdMstByStoreOut(stn, out var dataObject) == GetDataResult.Success)
            {
                string cmdSno = dataObject[0].CmdSno;
                string trayId = dataObject[0].TrayId;
                int loadType = Convert.ToInt32(dataObject[0].LoadType);

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                log.CmdSno = cmdSno;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).OutMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
                {
                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);
                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);//Wait Modify
                        }
                    }
                }
            }
        }
        private void StoreOut_B013_CreateEquCmd()
        {
            int bufferIndex = 22;
            List<string> stn = new List<string>()
            {
                    StnNo.B12_5,
                    StnNo.B14_5,
                    StnNo.B15_4,
                    StnNo.B16_5,
                    StnNo.B18,
                    StnNo.B10,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                log.LoadCategory = loadType;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A01}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreOutEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }
        #endregion B01_3

        private void StoreOut_B02_WriteCV()
        {
            int bufferIndex_B012 = 22;
            int bufferIndex_B015 = 24;
            int bufferIndex = 25;
            List<string> stn = new List<string>()
            {
                    StnNo.B12_5,
                    StnNo.B14_5,
                    StnNo.B15_4,
                    StnNo.B16_5,
                    StnNo.B18,
                    StnNo.B10,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                 && _conveyor.GetBuffer(bufferIndex).OutMode
                 && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                 && _conveyor.GetBuffer(bufferIndex).Presence == false
                 && _conveyor.GetBuffer(bufferIndex).Error == false
                 && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                //B01-2、B01-5輪流搬送
                if (_storeOut_B02_RGVCmdCreateChange)
                {
                    if (_conveyor.GetBuffer(bufferIndex_B012).Auto
                        && _conveyor.GetBuffer(bufferIndex_B012).InMode
                        && _conveyor.GetBuffer(bufferIndex_B012).CommandId > 0
                        && _conveyor.GetBuffer(bufferIndex_B012).Error == false
                        && _conveyor.GetBuffer(bufferIndex_B012).Ready == Ready.StoreInReady)
                    {
                        string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                        int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                        var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut, Tray Not Ready On Position");
                        log.TrayId = trayId;
                        log.LoadCategory = loadType;
                        _loggerManager.WriteLogTrace(log);

                        if (_conveyor.GetBuffer(bufferIndex_B012).Presence && _conveyor.GetBuffer(bufferIndex_B012).Position)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut, Tray Ready On Position");
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);

                            if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                            {
                                string cmdSno = dataObject[0].CmdSno;

                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                log.LoadCategory = loadType;
                                _loggerManager.WriteLogTrace(log);

                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutWriteCraneCmdToRGV) == ExecuteSQLResult.Success)
                                    {
                                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut, Wirte StoreOut Command To Next Buffer");
                                        log.CmdSno = cmdSno;
                                        log.TrayId = trayId;
                                        log.LoadCategory = loadType;
                                        _loggerManager.WriteLogTrace(log);

                                        _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);//Wait Modify
                                        _storeOut_B02_RGVCmdCreateChange = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _storeOut_B02_RGVCmdCreateChange = false;
                    }
                }
                else
                {
                    if (_conveyor.GetBuffer(bufferIndex_B015).Auto
                             && _conveyor.GetBuffer(bufferIndex_B015).InMode
                             && _conveyor.GetBuffer(bufferIndex_B015).CommandId > 0
                             && _conveyor.GetBuffer(bufferIndex_B015).Error == false
                             && _conveyor.GetBuffer(bufferIndex_B015).Ready == Ready.StoreInReady)
                    {
                        string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                        int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                        var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut, Tray Not Ready On Position");
                        log.TrayId = trayId;
                        log.LoadCategory = loadType;
                        _loggerManager.WriteLogTrace(log);

                        if (_conveyor.GetBuffer(bufferIndex_B015).Presence && _conveyor.GetBuffer(bufferIndex_B015).Position)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut, Tray Ready On Position");
                            log.TrayId = trayId;
                            log.LoadCategory = loadType;
                            _loggerManager.WriteLogTrace(log);

                            if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                            {
                                string cmdSno = dataObject[0].CmdSno;

                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                log.LoadCategory = loadType;
                                _loggerManager.WriteLogTrace(log);

                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutWriteCraneCmdToRGV) == ExecuteSQLResult.Success)
                                    {
                                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreOut, Wirte StoreOut Command To Next Buffer");
                                        log.CmdSno = cmdSno;
                                        log.TrayId = trayId;
                                        log.LoadCategory = loadType;
                                        _loggerManager.WriteLogTrace(log);

                                        _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);//Wait Modify
                                        _storeOut_B02_RGVCmdCreateChange = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _storeOut_B02_RGVCmdCreateChange = true;
                    }
                }
            }
        }

        private void StoreOut_B012_CreateEquCmd()
        {
            int bufferIndex_B02 = 25;
            int bufferIndex = 21;
            List<string> stn = new List<string>()
            {
                    StnNo.B12_5,
                    StnNo.B14_5,
                    StnNo.B15_4,
                    StnNo.B16_5,
                    StnNo.B18,
                    StnNo.B10,
            };
            if (_conveyor.GetBuffer(bufferIndex_B02).Auto
                && _conveyor.GetBuffer(bufferIndex_B02).InMode
                && _conveyor.GetBuffer(bufferIndex_B02).Presence == false
                && _conveyor.GetBuffer(bufferIndex_B02).Error == false
                && _conveyor.GetBuffer(bufferIndex_B02).Ready == Ready.StoreInReady)
            {
                if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).OutMode
                    && _conveyor.GetBuffer(bufferIndex).Presence
                    && _conveyor.GetBuffer(bufferIndex).Position
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
                    && _conveyor.GetBuffer(bufferIndex).CommandId == _conveyor.GetBuffer(bufferIndex_B02).CommandId)
                {
                    string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                    int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                    var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                    log.CmdSno = string.Empty;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                    {
                        string cmdSno = dataObject[0].CmdSno;
                        string source = $"{CranePortNo.B01_2}";
                        string dest = $"{CranePortNo.B02}";

                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreOut Command");
                        log.CmdSno = cmdSno;
                        log.TrayId = trayId;
                        log.LoadCategory = loadType;
                        _loggerManager.WriteLogTrace(log);

                        using (var db = _dataAccessManger.GetDB())
                        {
                            if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                            {
                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreOut Command, Begin Fail");
                                log.TrayId = trayId;
                                log.LoadCategory = loadType;
                                _loggerManager.WriteLogTrace(log);
                                return;
                            }
                            if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateEGVCmd) != ExecuteSQLResult.Success)
                            {
                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreOut Command, Update CmdMst Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                log.LoadCategory = loadType;
                                _loggerManager.WriteLogTrace(log);
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                            if (InsertStnToStnEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 5, cmdSno, source, dest, 5) == false)
                            {
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                            {
                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreOut Command, Commit Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                log.LoadCategory = loadType;
                                _loggerManager.WriteLogTrace(log);
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void StoreOut_B015_CreateEquCmd()
        {
            int bufferIndex_B02 = 25;
            int bufferIndex = 21;
            List<string> stn = new List<string>()
            {
                    StnNo.B12_5,
                    StnNo.B14_5,
                    StnNo.B15_4,
                    StnNo.B16_5,
                    StnNo.B18,
                    StnNo.B10,
            };
            if (_conveyor.GetBuffer(bufferIndex_B02).Auto
                && _conveyor.GetBuffer(bufferIndex_B02).InMode
                && _conveyor.GetBuffer(bufferIndex_B02).Presence == false
                && _conveyor.GetBuffer(bufferIndex_B02).Error == false
                && _conveyor.GetBuffer(bufferIndex_B02).Ready == Ready.StoreInReady)
            {
                if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).OutMode
                    && _conveyor.GetBuffer(bufferIndex).Presence
                    && _conveyor.GetBuffer(bufferIndex).Position
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
                    && _conveyor.GetBuffer(bufferIndex).CommandId == _conveyor.GetBuffer(bufferIndex_B02).CommandId)
                {
                    string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                    int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                    var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                    log.CmdSno = string.Empty;
                    log.TrayId = trayId;
                    log.LoadCategory = loadType;
                    _loggerManager.WriteLogTrace(log);

                    if (_dataAccessManger.GetCmdMstByStoreOut(stn, trayId, out var dataObject) == GetDataResult.Success)
                    {
                        string cmdSno = dataObject[0].CmdSno;
                        string source = $"{CranePortNo.B01_5}";
                        string dest = $"{CranePortNo.B02}";

                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreOut Command");
                        log.CmdSno = cmdSno;
                        log.TrayId = trayId;
                        log.LoadCategory = loadType;
                        _loggerManager.WriteLogTrace(log);

                        using (var db = _dataAccessManger.GetDB())
                        {
                            if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                            {
                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreOut Command, Begin Fail");
                                log.TrayId = trayId;
                                log.LoadCategory = loadType;
                                _loggerManager.WriteLogTrace(log);
                                return;
                            }
                            if (_dataAccessManger.UpdateCmdMst(db, cmdSno, "15") != ExecuteSQLResult.Success)
                            {
                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreOut Command, Update CmdMst Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                log.LoadCategory = loadType;
                                _loggerManager.WriteLogTrace(log);
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                            if (InsertStnToStnEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 6, cmdSno, source, dest, 5) == false)
                            {
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                            {
                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreOut Command, Commit Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                log.LoadCategory = loadType;
                                _loggerManager.WriteLogTrace(log);
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void StoreOut_MFG_EquCmdFinish()
        {
            var stn1 = new List<string>()
            {
                StnNo.A01,
                StnNo.A02_1,
                StnNo.A03,
                StnNo.A05_1,
            };
            if (_dataAccessManger.GetCmdMstByStoreOutFinish(stn1, out var dataObject) == GetDataResult.Success)
            {
                foreach (var cmdMst in dataObject.Data)
                {
                    if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
                    {
                        if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                        {
                            if (equCmd[0].CompleteCode == "92")
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                    if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", "13") != ExecuteSQLResult.Success)
                                    {
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return;
                                    }
                                    if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
                                    {
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return;
                                    }
                                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                            else if (equCmd[0].CompleteCode.StartsWith("W"))
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var stn2 = new List<string>()
            {
                StnNo.B12_5,
                StnNo.B14_5,
                StnNo.B15_4,
                StnNo.B16_5,
                StnNo.B18,
                StnNo.B10,
            };
            if (_dataAccessManger.GetCmdMstByStoreOutFinish(stn1, out dataObject) == GetDataResult.Success)
            {
                foreach (var cmdMst in dataObject.Data)
                {
                    if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
                    {
                        if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                        {
                            if (equCmd[0].CompleteCode == "92")
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                    if (cmdMst.Trace == "12")
                                    {
                                        if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, "13") != ExecuteSQLResult.Success)
                                        {
                                            db.TransactionCtrl2(TransactionTypes.Rollback);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", "16") != ExecuteSQLResult.Success)
                                        {
                                            db.TransactionCtrl2(TransactionTypes.Rollback);
                                            return;
                                        }
                                    }
                                    if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
                                    {
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return;
                                    }
                                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                            else if (equCmd[0].CompleteCode.StartsWith("W"))
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion MFG

        #endregion StoreOut

        #region StoreIn
        private void StoreInProcess(object sender, ElapsedEventArgs e)
        {
            _storeInProcess.Stop();
            if (_isBQA)
            {
                StoreIn_A112_WriteCV();//OK

                StoreIn_A12_CreateEquCmd();//OK

                StoreIn_A173_WriteCV();//OK

                StoreIn_A19_WriteCV();//OK

                StoreIn_A13_CreateEquCmd();//OK

                StoreIn_BQA_EquCmdFinish();//OK
            }
            else
            {
                StoreIn_A022_WriteCV();//OK

                StoreIn_A013_CreateEquCmd();//OK

                StoreIn_A07_WriteCV();//OK

                StoreIn_A032_CreateEquCmd();//OK

                StoreIn_B10_WriteCV();//OK

                StoreIn_B121_WriteCV();//OK

                StoreIn_B141_WriteCV();//OK

                StoreIn_B154_WriteCV();

                StoreIn_B161_WriteCV();//OK

                StoreIn_B18_WriteCV();//OK

                StoreIn_C02_WriteCV();//OK
                StoreIn_C02_CreateEquCmd();//OK

                StoreIn_C01_CreateEquCmd();//OK

                StoreIn_C013_CreateEquCmd();//OK

                StoreIn_MFG_EquCmdFinish();//OK
            }
            _storeInProcess.Start();
        }

        #region BQA
        private void StoreIn_A112_WriteCV()
        {
            int bufferIndex = 11;
            List<string> stn = new List<string>()
            {
                StnNo.A11_1,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string fodbId = _conveyor.GetBuffer(bufferIndex).FosbId;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId = fodbId;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId = fodbId;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV, weight) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId = fodbId;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.ASRS, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_A12_CreateEquCmd()
        {
            int bufferIndex = 14;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = $"{CranePortNo.A12}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_A173_WriteCV()
        {
            int bufferIndex = 21;
            List<string> stn = new List<string>()
            {
                StnNo.A17_4,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                string plant = _conveyor.GetBuffer(bufferIndex).Plant;
                string fodbId = _conveyor.GetBuffer(bufferIndex).FosbId;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId = fodbId;
                log.Plant = plant;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId = fodbId;
                    log.Plant = plant;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId = fodbId;
                            log.Plant = plant;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.ASRS, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_A19_WriteCV()
        {
            int bufferIndex = 24;
            List<string> stn = new List<string>()
            {
                StnNo.A19,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                string plant = _conveyor.GetBuffer(bufferIndex).Plant;
                string fodbId = _conveyor.GetBuffer(bufferIndex).FosbId;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId = fodbId;
                log.Plant = plant;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId = fodbId;
                    log.Plant = plant;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId = fodbId;
                            log.Plant = plant;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.ASRS, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_A13_CreateEquCmd()
        {
            int bufferIndex = 30;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = $"{_conveyor.GetBuffer(bufferIndex).CommandId:0000}";
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = $"{CranePortNo.A13}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_BQA_EquCmdFinish()
        {
            var stn1 = new List<string>()
            {
                StnNo.A17_4,
                StnNo.A19,
                StnNo.A08,
                StnNo.A11_1,
            };
            if (_dataAccessManger.GetCmdMstByStoreInFinish(stn1, out var dataObject) == GetDataResult.Success)
            {
                foreach (var cmdMst in dataObject.Data)
                {
                    if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
                    {
                        if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                        {
                            if (equCmd[0].CompleteCode == "92")
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                    if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.StoreInCraneCmdFinish) != ExecuteSQLResult.Success)
                                    {
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return;
                                    }
                                    if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
                                    {
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return;
                                    }
                                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                            else if (equCmd[0].CompleteCode.StartsWith("W"))
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion BQA

        #region MFG

        private void StoreIn_A022_WriteCV()
        {
            int bufferIndex = 6;
            List<string> stn = new List<string>()
            {
                StnNo.A02_1,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string fodbId_Left = _conveyor.GetBuffer(bufferIndex).FosbId_Left;
                string fodbId_Right = _conveyor.GetBuffer(bufferIndex).FosbId_Right;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId_Left = fodbId_Left;
                log.FosbId_Right = fodbId_Right;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId_Left = fodbId_Left;
                    log.FosbId_Right = fodbId_Right;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId_Left = fodbId_Left;
                            log.FosbId_Right = fodbId_Right;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_A013_CreateEquCmd()
        {
            int bufferIndex = 9;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = $"{CranePortNo.A01_3}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_A07_WriteCV()
        {
            int bufferIndex = 15;
            List<string> stn = new List<string>()
            {
                StnNo.A05_1,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string fodbId_Left = _conveyor.GetBuffer(bufferIndex).FosbId_Left;
                string fodbId_Right = _conveyor.GetBuffer(bufferIndex).FosbId_Right;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId_Left = fodbId_Left;
                log.FosbId_Right = fodbId_Right;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId_Left = fodbId_Left;
                    log.FosbId_Right = fodbId_Right;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId_Left = fodbId_Left;
                            log.FosbId_Right = fodbId_Right;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_A032_CreateEquCmd()
        {
            int bufferIndex = 18;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = $"{CranePortNo.A03_2}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 2, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_B10_WriteCV()
        {
            int bufferIndex = 79;
            List<string> stn = new List<string>()
            {
                StnNo.B10,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string fodbId_Left = _conveyor.GetBuffer(bufferIndex).FosbId_Left;
                string fodbId_Right = _conveyor.GetBuffer(bufferIndex).FosbId_Right;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId_Left = fodbId_Left;
                log.FosbId_Right = fodbId_Right;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId_Left = fodbId_Left;
                    log.FosbId_Right = fodbId_Right;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId_Left = fodbId_Left;
                            log.FosbId_Right = fodbId_Right;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_B121_WriteCV()
        {
            int bufferIndex = 48;
            List<string> stn = new List<string>()
            {
                StnNo.B12_5,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string fodbId_Left = _conveyor.GetBuffer(bufferIndex).FosbId_Left;
                string fodbId_Right = _conveyor.GetBuffer(bufferIndex).FosbId_Right;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId_Left = fodbId_Left;
                log.FosbId_Right = fodbId_Right;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId_Left = fodbId_Left;
                    log.FosbId_Right = fodbId_Right;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId_Left = fodbId_Left;
                            log.FosbId_Right = fodbId_Right;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_B141_WriteCV()
        {
            int bufferIndex = 55;
            List<string> stn = new List<string>()
            {
                StnNo.B14_5,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string fodbId_Left = _conveyor.GetBuffer(bufferIndex).FosbId_Left;
                string fodbId_Right = _conveyor.GetBuffer(bufferIndex).FosbId_Right;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId_Left = fodbId_Left;
                log.FosbId_Right = fodbId_Right;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId_Left = fodbId_Left;
                    log.FosbId_Right = fodbId_Right;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId_Left = fodbId_Left;
                            log.FosbId_Right = fodbId_Right;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_B154_WriteCV()
        {
        }

        private void StoreIn_B161_WriteCV()
        {
            int bufferIndex = 62;
            List<string> stn = new List<string>()
            {
                StnNo.B16_5,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string fodbId_Left = _conveyor.GetBuffer(bufferIndex).FosbId_Left;
                string fodbId_Right = _conveyor.GetBuffer(bufferIndex).FosbId_Right;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId_Left = fodbId_Left;
                log.FosbId_Right = fodbId_Right;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId_Left = fodbId_Left;
                    log.FosbId_Right = fodbId_Right;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId_Left = fodbId_Left;
                            log.FosbId_Right = fodbId_Right;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }

        private void StoreIn_B18_WriteCV()
        {
            int bufferIndex = 66;
            List<string> stn = new List<string>()
            {
                StnNo.B18,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).ReadNotice > 0
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = _conveyor.GetBuffer(bufferIndex).TrayId;
                int weight = _conveyor.GetBuffer(bufferIndex).Weight;
                string fodbId_Left = _conveyor.GetBuffer(bufferIndex).FosbId_Left;
                string fodbId_Right = _conveyor.GetBuffer(bufferIndex).FosbId_Right;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                log.TrayId = trayId;
                log.TrayWeight = weight;
                log.FosbId_Left = fodbId_Left;
                log.FosbId_Right = fodbId_Right;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(stn, trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int loadType = Convert.ToInt32(dataObject[0].LoadType);

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreIn Command");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    log.TrayWeight = weight;
                    log.FosbId_Left = fodbId_Left;
                    log.FosbId_Right = fodbId_Right;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            log.TrayWeight = weight;
                            log.FosbId_Left = fodbId_Left;
                            log.FosbId_Right = fodbId_Right;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(trayId, Path.NoPath, loadType);
                        }
                    }
                }
            }
        }

        #region C02
        private void StoreIn_C02_WriteCV()
        {
            int bufferIndex = 37;
            int bufferIndex_C012 = 39;
            int bufferIndex_C015 = 41;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.TrayId = trayId;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    string rowString = dataObject[0].NewLoc.Substring(0, 2);
                    if (int.TryParse(rowString, out var row))
                    {
                        int craneNo = (row + 1) / 2;

                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, $"Buffer Ready StoreIn, Got to Crane{craneNo}");
                        log.CmdSno = cmdSno;
                        log.TrayId = trayId;
                        _loggerManager.WriteLogTrace(log);

                        if (craneNo == 1)
                        {
                            if (_conveyor.GetBuffer(bufferIndex_C012).Auto
                                && _conveyor.GetBuffer(bufferIndex_C012).OutMode
                                && _conveyor.GetBuffer(bufferIndex_C012).CommandId == 0
                                && _conveyor.GetBuffer(bufferIndex_C012).Presence == false
                                && _conveyor.GetBuffer(bufferIndex_C012).Error == false
                                && _conveyor.GetBuffer(bufferIndex_C012).Ready == Ready.StoreOutReady)
                            {
                                string source = $"{CranePortNo.C02}";
                                string dest = $"{CranePortNo.C01_2}";

                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInTrayOnStation) == ExecuteSQLResult.Success)
                                    {
                                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Wirte StoreIn Command To Next Buffer");
                                        log.CmdSno = cmdSno;
                                        log.TrayId = trayId;
                                        log.Source = source;
                                        log.Dest = dest;
                                        _loggerManager.WriteLogTrace(log);

                                        _conveyor.GetBuffer(bufferIndex_C012).WriteCommandIdAsync(trayId, loadType);
                                    }
                                }
                            }
                            else
                            {
                            }
                        }
                        else if (craneNo == 2)
                        {
                            if (_conveyor.GetBuffer(bufferIndex_C015).Auto
                                  && _conveyor.GetBuffer(bufferIndex_C015).OutMode
                                  && _conveyor.GetBuffer(bufferIndex_C015).CommandId == 0
                                  && _conveyor.GetBuffer(bufferIndex_C015).Presence == false
                                  && _conveyor.GetBuffer(bufferIndex_C015).Error == false
                                  && _conveyor.GetBuffer(bufferIndex_C015).Ready == Ready.StoreOutReady)
                            {
                                string source = $"{CranePortNo.C02}";
                                string dest = $"{CranePortNo.C01_5}";

                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInTrayOnStation) == ExecuteSQLResult.Success)
                                    {
                                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Wirte StoreIn Command To Next Buffer");
                                        log.CmdSno = cmdSno;
                                        log.TrayId = trayId;
                                        log.Source = source;
                                        log.Dest = dest;
                                        _loggerManager.WriteLogTrace(log);

                                        _conveyor.GetBuffer(bufferIndex_C015).WriteCommandIdAsync(trayId, loadType);
                                    }
                                }
                            }
                            else
                            {
                            }
                        }
                        else if (craneNo == 3)
                        {
                        }
                        else if (craneNo == 4)
                        {
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Please Check Dest Loc");
                        log.CmdSno = cmdSno;
                        log.TrayId = trayId;
                        log.Source = $"{CranePortNo.C02}";
                        log.Dest = dataObject[0].NewLoc;
                        _loggerManager.WriteLogTrace(log);
                    }
                }
            }

        }
        private void StoreIn_C02_CreateEquCmd()
        {
            int bufferIndex = 37;
            int bufferIndex_C012 = 39;
            int bufferIndex_C015 = 41;

            if (_conveyor.GetBuffer(bufferIndex).Auto
               && _conveyor.GetBuffer(bufferIndex).InMode
               && _conveyor.GetBuffer(bufferIndex).Presence
               && _conveyor.GetBuffer(bufferIndex).Position
               && _conveyor.GetBuffer(bufferIndex).Error == false
               && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                if (_conveyor.GetBuffer(bufferIndex_C012).Auto
                    && _conveyor.GetBuffer(bufferIndex_C012).OutMode
                    && _conveyor.GetBuffer(bufferIndex_C012).CommandId == _conveyor.GetBuffer(bufferIndex).CommandId
                    && _conveyor.GetBuffer(bufferIndex_C012).Presence == false
                    && _conveyor.GetBuffer(bufferIndex_C012).Error == false
                    && _conveyor.GetBuffer(bufferIndex_C012).Ready == Ready.StoreOutReady)
                {
                    string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);

                    var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn Receive Completed");
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                    {
                        string cmdSno = dataObject[0].CmdSno;
                        string source = $"{CranePortNo.C02}";
                        string dest = $"{CranePortNo.C01_2}";

                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreIn Command");
                        log.CmdSno = cmdSno;
                        log.TrayId = trayId;
                        _loggerManager.WriteLogTrace(log);

                        using (var db = _dataAccessManger.GetDB())
                        {
                            if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                            {
                                log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreIn Command, Begin Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                _loggerManager.WriteLogTrace(log);
                                return;
                            }
                            if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateEGVCmd) != ExecuteSQLResult.Success)
                            {
                                log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreIn Command, Update CmdMst Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                _loggerManager.WriteLogTrace(log);
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                            if (InsertStnToStnEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 6, cmdSno, source, dest, 5) == false)
                            {
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                            {
                                log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreIn Command, Commit Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                _loggerManager.WriteLogTrace(log);
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                        }
                    }
                }
                else if (_conveyor.GetBuffer(bufferIndex_C015).Auto
                    && _conveyor.GetBuffer(bufferIndex_C015).OutMode
                    && _conveyor.GetBuffer(bufferIndex_C015).CommandId == _conveyor.GetBuffer(bufferIndex).CommandId
                    && _conveyor.GetBuffer(bufferIndex_C015).Presence == false
                    && _conveyor.GetBuffer(bufferIndex_C015).Error == false
                    && _conveyor.GetBuffer(bufferIndex_C015).Ready == Ready.StoreOutReady)
                {
                    string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);

                    var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn Receive Completed");
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                    {
                        string cmdSno = dataObject[0].CmdSno;
                        string source = $"{CranePortNo.C02}";
                        string dest = $"{CranePortNo.C01_5}";

                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreIn Command");
                        log.CmdSno = cmdSno;
                        log.TrayId = trayId;
                        _loggerManager.WriteLogTrace(log);

                        using (var db = _dataAccessManger.GetDB())
                        {
                            if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                            {
                                log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreIn Command, Begin Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                _loggerManager.WriteLogTrace(log);
                                return;
                            }
                            if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateEGVCmd) != ExecuteSQLResult.Success)
                            {
                                log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreIn Command, Update CmdMst Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                _loggerManager.WriteLogTrace(log);
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                            if (InsertStnToStnEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 6, cmdSno, source, dest, 5) == false)
                            {
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                            {
                                log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create RGV StoreIn Command, Commit Fail");
                                log.CmdSno = cmdSno;
                                log.TrayId = trayId;
                                _loggerManager.WriteLogTrace(log);
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return;
                            }
                        }
                    }
                }
            }
        }
        #endregion C02

        private void StoreIn_C01_CreateEquCmd()
        {
            int bufferIndex = 40;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = $"{CranePortNo.C01}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInTrayOnStation) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 1, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_C013_CreateEquCmd()
        {
            int bufferIndex = 43;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Position
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreInReady)
            {
                string trayId = PlcCommandIdToTrayId(_conveyor.GetBuffer(bufferIndex).CommandId);
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                log.TrayId = trayId;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreIn(trayId, out var dataObject) == GetDataResult.Success)
                {
                    string cmdSno = dataObject[0].CmdSno;
                    string source = $"{CranePortNo.A03_2}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    log.TrayId = trayId;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInTrayOnStation) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (InsertStoreInEquCmd(db, _conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, 2, cmdSno, source, dest, 5) == false)
                        {
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                        if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            log.TrayId = trayId;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_MFG_EquCmdFinish()
        {
            var stn1 = new List<string>()
            {
                StnNo.B10,
                StnNo.B18,
                StnNo.B12_5,
                StnNo.B14_5,
                StnNo.B15_4,
                StnNo.B16_5,
            };
            if (_dataAccessManger.GetCmdMstByStoreInFinish(stn1, out var dataObject) == GetDataResult.Success)
            {
                foreach (var cmdMst in dataObject.Data)
                {
                    if (_dataAccessManger.GetEquCmd(cmdMst.CmdSno, out var equCmd) == GetDataResult.Success)
                    {
                        if (equCmd[0].ReNeqFlag != "F" && equCmd[0].CmdSts == "9")
                        {
                            if (equCmd[0].CompleteCode == "92")
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                    if (cmdMst.Trace == "23")
                                    {
                                        if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", "14") != ExecuteSQLResult.Success)
                                        {
                                            db.TransactionCtrl2(TransactionTypes.Rollback);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, Trace.StoreInRGVCmdFinish) != ExecuteSQLResult.Success)
                                        {
                                            db.TransactionCtrl2(TransactionTypes.Rollback);
                                            return;
                                        }
                                    }
                                    if (_dataAccessManger.DeleteEquCmd(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
                                    {
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return;
                                    }
                                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                            else if (equCmd[0].CompleteCode.StartsWith("W"))
                            {
                                using (var db = _dataAccessManger.GetDB())
                                {
                                    if (_dataAccessManger.UpdateEquCmdRetry(db, equCmd[0].CmdSno) != ExecuteSQLResult.Success)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion MFG

        #endregion StoreIn

        #region Other
        private void OtherProcess(object sender, ElapsedEventArgs e)
        {
            _otherProcess.Stop();
            if (_isBQA)
            {
                Other_A10_AutomaticDoor();//OK

                Other_A172_WriteCV();
            }
            else
            {
                Other_A012_AutomaticDoor();//OK

                Other_A4_AutomaticDoor();//OK

                Other_B12_WriteCV();

                Other_B14_WriteCV();

                Other_B142_WriteCV();

                Other_B16_WriteCV();
            }

            Other_LocToLoc();

            _otherProcess.Start();
        }

        #region BQA

        #region A10
        private void Other_A10_AutomaticDoor()
        {
            int bufferIndex = 8;
            if (_dataAccessManger.GetEmpMst("A10", out var dataObject) == GetDataResult.Success)
            {
                if (dataObject[0].OpenFlag)
                {
                    if (_conveyor.GetBuffer(bufferIndex).AutomaticDoor == false)
                    {
                        if (_conveyor.GetBuffer(bufferIndex).Presence
                            && _conveyor.GetBuffer(bufferIndex).Position
                            && (_conveyor.GetBuffer(bufferIndex - 1).Presence
                                || _conveyor.GetBuffer(bufferIndex - 2).Presence
                                || _conveyor.GetBuffer(bufferIndex - 3).Presence
                                || _conveyor.GetBuffer(bufferIndex - 4).Presence))
                        {
                            _conveyor.GetBuffer(bufferIndex).AutomaticDoorOpendAsync();
                        }
                    }
                    else
                    {
                        if (_conveyor.GetBuffer(bufferIndex).Presence == false || _conveyor.GetBuffer(bufferIndex - 1).Presence || _conveyor.GetBuffer(bufferIndex - 2).Presence || _conveyor.GetBuffer(bufferIndex - 3).Presence)
                        {
                            if (true)//3Min
                            {
                                _conveyor.GetBuffer(bufferIndex).AutomaticDoorClosedAsync();
                            }

                        }

                    }
                }
            }
        }
        #endregion A10

        #region A17-2
        private void Other_A172_WriteCV()
        {
        }
        #endregion A17-2

        #endregion BQA

        #region MFG

        #region A12
        private void Other_A012_AutomaticDoor()
        {
            if (_dataAccessManger.GetEmpMst("A01-2", out var dataObject) == GetDataResult.Success)
            {
                if (dataObject[0].OpenFlag)
                {
                    if (_conveyor.GetBuffer(3).AutomaticDoor == false)
                    {
                        if ((_conveyor.GetBuffer(3).Presence && _conveyor.GetBuffer(3).Position) || _conveyor.GetBuffer(2).Presence || _conveyor.GetBuffer(2).Presence || _conveyor.GetBuffer(1).Presence)
                        {
                            _conveyor.GetBuffer(3).AutomaticDoorOpendAsync();
                        }
                    }
                }
                else
                {
                    if (_conveyor.GetBuffer(3).AutomaticDoor)
                    {
                        _conveyor.GetBuffer(3).AutomaticDoorClosedAsync();
                    }
                }
            }
        }
        #endregion A12

        #region A4
        private void Other_A4_AutomaticDoor()
        {
            if (_dataAccessManger.GetEmpMst("A4", out var dataObject) == GetDataResult.Success)
            {
                if (dataObject[0].OpenFlag)
                {
                    if (_conveyor.GetBuffer(12).AutomaticDoor == false)
                    {
                        if ((_conveyor.GetBuffer(12).Presence && _conveyor.GetBuffer(12).Position) || _conveyor.GetBuffer(11).Presence || _conveyor.GetBuffer(10).Presence || _conveyor.GetBuffer(9).Presence)
                        {
                            _conveyor.GetBuffer(12).AutomaticDoorOpendAsync();
                        }
                    }
                }
                else
                {
                    if (_conveyor.GetBuffer(12).AutomaticDoor)
                    {
                        _conveyor.GetBuffer(12).AutomaticDoorClosedAsync();
                    }
                }
            }
        }
        #endregion A4

        #region B12
        private void Other_B12_WriteCV()
        {
        }
        #endregion B12

        #region B14-2
        private void Other_B142_WriteCV()
        {
        }
        #endregion B14-2

        #region B14
        private void Other_B14_WriteCV()
        {
        }
        #endregion B14

        #region B16
        private void Other_B16_WriteCV()
        {
        }
        #endregion B16

        #endregion MFG

        private void Other_LocToLoc()
        {
        }

        #endregion Other

        private bool InsertStnToStnEquCmd(DB db, int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority)
        {
            try
            {
                if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.StnToStn, source, destination) == false)
                {
                    if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.StnToStn).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
                    {
                        var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd");
                        log.CommandId = cmdSno;
                        log.CraneNo = craneNo;
                        log.Source = source;
                        log.Destination = destination;
                        _loggerManager.WriteLogTrace(log);
                        return true;
                    }
                    else
                    {
                        var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd Fail");
                        log.CommandId = cmdSno;
                        log.CraneNo = craneNo;
                        log.Source = source;
                        log.Destination = destination;
                        _loggerManager.WriteLogTrace(log);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _loggerManager.Error(ex);
                return false;
            }
        }

        private bool InsertStoreInEquCmd(DB db, int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority)
        {
            try
            {
                if (destination.Length != 7)
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check destination Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }

                if (int.TryParse(destination.Substring(0, 2), out var tmp))
                {
                    if (craneNo != (tmp + 1) / 2)
                    {
                        var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Crane No Fail, Please Check");
                        log.CommandId = cmdSno;
                        log.CraneNo = craneNo;
                        log.Source = source;
                        log.Destination = destination;
                        _loggerManager.WriteLogTrace(log);
                        return false;
                    }

                    if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.InMode, source, destination) == false)
                    {
                        if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.InMode).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return true;
                        }
                        else
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd Fail");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Get Crane No Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _loggerManager.Error(ex);
                return false;
            }
        }

        private bool InsertStoreOutEquCmd(DB db, int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority)
        {
            try
            {
                if (source.Length != 7)
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Source Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }

                if (int.TryParse(source.Substring(0, 2), out var tmp))
                {
                    if (craneNo != (tmp + 1) / 2)
                    {
                        var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Crane No Fail, Please Check");
                        log.CommandId = cmdSno;
                        log.CraneNo = craneNo;
                        log.Source = source;
                        log.Destination = destination;
                        _loggerManager.WriteLogTrace(log);
                        return false;
                    }

                    if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.OutMode, source, destination) == false)
                    {
                        if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.OutMode).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return true;
                        }
                        else
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd Fail");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Get Crane No Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _loggerManager.Error(ex);
                return false;
            }
        }

        private bool InsertLocToLocEquCmd(DB db, int bufferIndex, string bufferName, int craneNo, string cmdSno, string source, string destination, int priority)
        {
            try
            {
                if (destination.Length != 7)
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Source Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }

                if (destination.Length != 7)
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Destination Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }

                if (int.TryParse(source.Substring(0, 2), out var tmp1) && int.TryParse(destination.Substring(0, 2), out var tmp2))
                {
                    if (craneNo != (tmp1 + 1) / 2)
                    {
                        var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Source Crane No Fail, Please Check");
                        log.CommandId = cmdSno;
                        log.CraneNo = craneNo;
                        log.Source = source;
                        log.Destination = destination;
                        _loggerManager.WriteLogTrace(log);
                        return false;
                    }
                    if (craneNo != (tmp2 + 1) / 2)
                    {
                        var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Destination Crane No Fail, Please Check");
                        log.CommandId = cmdSno;
                        log.CraneNo = craneNo;
                        log.Source = source;
                        log.Destination = destination;
                        _loggerManager.WriteLogTrace(log);
                        return false;
                    }
                    if ((tmp1 + 1) / 2 != (tmp2 + 1) / 2)
                    {
                        var log = new EquCmdLogTrace(bufferIndex, bufferName, "Check Crane No Fail, Please Check");
                        log.CommandId = cmdSno;
                        log.CraneNo = craneNo;
                        log.Source = source;
                        log.Destination = destination;
                        _loggerManager.WriteLogTrace(log);
                        return false;
                    }

                    if (CheckExecutionEquCmd(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.LocToLoc, source, destination) == false)
                    {
                        if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.LocToLoc).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return true;
                        }
                        else
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Insert Equ Cmd Fail");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Get Crane No Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _loggerManager.Error(ex);
                return false;
            }
        }

        private bool CheckExecutionEquCmd(int bufferIndex, string bufferName, int craneNo, string cmdSno, EquCmdMode equCmdMode, string source, string destination)
        {
            if (_dataAccessManger.GetEquCmd(cmdSno, out var equCmd) == GetDataResult.Success)
            {
                if (equCmd[0].CmdSts == CmdSts.Queue.GetHashCode().ToString() || equCmd[0].CmdSts == CmdSts.Transferring.GetHashCode().ToString())
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Exists Command On Other Equ Execute, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return true;
                }
                else
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Exists Command On Other Equ, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return true;
                }
            }
            else
            {
                bool bolFlag = true;
                switch (equCmdMode)
                {
                    case EquCmdMode.InMode:
                        bolFlag = CheckExecutionEquCmdByCrane(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.InMode, source, destination);
                        break;
                    case EquCmdMode.OutMode:
                        bolFlag = CheckExecutionEquCmdByCrane(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.OutMode, source, destination);
                        break;
                    case EquCmdMode.LocToLoc:
                        if (bolFlag)
                        {
                            bolFlag = CheckExecutionEquCmdByCrane(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.InMode, source, destination);
                        }
                        if (bolFlag)
                        {
                            bolFlag = CheckExecutionEquCmdByCrane(bufferIndex, bufferName, craneNo, cmdSno, EquCmdMode.OutMode, source, destination);
                        }
                        break;
                    case EquCmdMode.StnToStn:
                        if (_dataAccessManger.GetEquCmdByLocToLoc(craneNo, out equCmd) == GetDataResult.Success)
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, $"Exists Loc To Loc Execute, Count:{equCmd.Count}");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                }
                return bolFlag;
            }
        }
        private bool CheckExecutionEquCmdByCrane(int bufferIndex, string bufferName, int craneNo, string cmdSno, EquCmdMode equCmdMode, string source, string destination)
        {
            if (equCmdMode == EquCmdMode.InMode)
            {
                if (_dataAccessManger.GetEquCmdByInMode(craneNo, source, out var equCmd) == GetDataResult.Success)
                {
                    foreach (var cmd in equCmd.Data)
                    {
                        if (cmd.CmdSts == $"{CmdSts.Queue}")
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Source Exists Command Execute, Please Check");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return false;
                        }
                        else if (cmd.CmdSts == $"{CmdSts.Transferring}")
                        {
                            if (string.IsNullOrWhiteSpace(cmd.CompleteCode))
                            {
                                var log = new EquCmdLogTrace(bufferIndex, bufferName, "Source Exists Command Execute And No CompleteCode, Please Check");
                                log.CommandId = cmdSno;
                                log.CraneNo = craneNo;
                                log.Source = source;
                                log.Destination = destination;
                                _loggerManager.WriteLogTrace(log);
                                return false;
                            }
                        }
                    }
                }
            }
            else if (equCmdMode == EquCmdMode.OutMode)
            {
                if (_dataAccessManger.GetEquCmdByOutMode(craneNo, destination, out var equCmd) == GetDataResult.Success)
                {
                    foreach (var cmd in equCmd.Data)
                    {
                        if (cmd.CmdSts == $"{CmdSts.Queue}")
                        {
                            var log = new EquCmdLogTrace(bufferIndex, bufferName, "Destination Exists Command Execute, Please Check");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return false;
                        }
                        else if (cmd.CmdSts == $"{CmdSts.Transferring}")
                        {
                            if (string.IsNullOrWhiteSpace(cmd.CompleteCode))
                            {
                                var log = new EquCmdLogTrace(bufferIndex, bufferName, "Destination Exists Command Execute And No CompleteCode, Please Check");
                                log.CommandId = cmdSno;
                                log.CraneNo = craneNo;
                                log.Source = source;
                                log.Destination = destination;
                                _loggerManager.WriteLogTrace(log);
                                return false;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private string PlcCommandIdToTrayId(int commandId)
        {
            if (_isBQA)
            {
                return $"{commandId:D4}";
            }
            else
            {
                return $"T{commandId:D5}";
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
