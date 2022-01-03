using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.ASRS.WCS.Model.LogTrace;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.ASRS.Conveyors;
using Mirle.DataBase;

namespace Mirle.ASRS.WCS.Controller
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
        
        public WCSManager(bool isBQA)
        {
            _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            _loggerManager = ControllerReader.GetLoggerManager();
            _dataAccessManger = ControllerReader.GetDataAccessManger();
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

            StoreOut_S101_WriteCV();
            StoreOut_S101_CreateEquCmd();

            StoreOut_S201_WriteCV();
            StoreOut_S201_CreateEquCmd();

            StoreOut_S301_WriteCV();
            StoreOut_S301_CreateEquCmd();

            StoreOut_S401_WriteCV();
            StoreOut_S401_CreateEquCmd();

            StoreOut_EquCmdFinish();

            _storeOutProcess.Start();
        }



        private void StoreOut_S101_WriteCV()
        {
            int bufferIndex = 1; //A1
            int CmdMode = 0;
            using (var db = _dataAccessManger.GetDB())
            {

                if (_dataAccessManger.GetCmdMstByStoreOut("A1", out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
                {
                    string CmdSno = dataObject[0].CmdSno;
                    int IOType = Convert.ToInt32(dataObject[0].IOType);
                    CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

                    var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                    log.CmdSno = CmdSno;
                    log.LoadCategory = CmdMode;
                    _loggerManager.WriteLogTrace(log);

                    //確認目前模式，是否可以切換模式，可以就寫入切換成出庫的請求
                    if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreOutReady
                        && _conveyor.GetBuffer(bufferIndex).Switch_Ack == 1)
                    {
                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreIn Ready, Can Switchmode");
                        _loggerManager.WriteLogTrace(log);

                        _conveyor.GetBuffer(bufferIndex).Switch_Mode(2);
                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Switchmode Complete");
                        _loggerManager.WriteLogTrace(log);
                    }

                    if (_conveyor.GetBuffer(bufferIndex).Auto
                        && _conveyor.GetBuffer(bufferIndex).OutMode
                        && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                        && _conveyor.GetBuffer(bufferIndex).Presence == false
                        && _conveyor.GetBuffer(bufferIndex).Error == false
                        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
                        && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex + 2).CmdMode != 3//為了不跟盤點命令衝突的條件
                        && CheckEmptyWillBefullOrNot() == false) //檢查一樓buffer是否整體滿九版空棧板了
                    {
                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                        log.CmdSno = CmdSno;
                        log.LoadCategory = CmdMode;
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, CmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = CmdSno;
                            log.LoadCategory = CmdMode;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(CmdSno, CmdMode);

                            //出庫都要寫入路徑編號，編號1為堆疊，編號2為直接出庫，編號3為補充母棧板

                            if (IOType == 7 || IOType == 8 || LastCargoOrNot() == 1)//Iotype如果是盤點或是空棧板整版出，直接到A3
                            {
                                _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path2_toA3);
                            }
                            else
                            {
                                _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(PathNotice.Path1_toA2);
                            }
                        }

                    }
                }
            }
        }

        //判斷function:當檢查命令是最後一個以及一樓buffer沒有貨物，便要直接路徑到A3，狀態正確寫入1
        private int LastCargoOrNot()
        {
            if (_dataAccessManger.GetCmdMstByStoreOutcheck("A1", out var dataObject) == GetDataResult.Success)
            {
                int COUNT = Convert.ToInt32(dataObject[0].COUNT);

                if (_conveyor.GetBuffer(2).A2LV2 == 0 && COUNT == '0' && _conveyor.GetBuffer(2).CommandId == 0 && _conveyor.GetBuffer(1).CommandId == 0)
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

        //根據判斷去決定一樓空棧板總數是否滿了，去擋下出庫命令
        private bool CheckEmptyWillBefullOrNot()
        {

            if ((_conveyor.GetBuffer(4).EmptyINReady == 8 && _conveyor.GetBuffer(1).CommandId != 0) || (_conveyor.GetBuffer(4).EmptyINReady == 8 && _conveyor.GetBuffer(2).CommandId != 0))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void StoreOut_S201_WriteCV()
        {
            int bufferIndex = 5; //A5
            using (var db = _dataAccessManger.GetDB())
            {

                if (_dataAccessManger.GetCmdMstByStoreOut("A5", out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

                    var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.LoadCategory = cmdmode;
                    _loggerManager.WriteLogTrace(log);



                    if (_conveyor.GetBuffer(bufferIndex).Auto
                        && _conveyor.GetBuffer(bufferIndex).OutMode
                        && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                        && _conveyor.GetBuffer(bufferIndex).Presence == false
                        && _conveyor.GetBuffer(bufferIndex).Error == false
                        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
                        && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3)  //為了不跟盤點命令衝突的條件
                    {
                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                        log.CmdSno = cmdSno;
                        log.LoadCategory = cmdmode;
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.LoadCategory = cmdmode;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, cmdmode);
                        }

                    }
                }
            }
        }

        private void StoreOut_S301_WriteCV()
        {
            int bufferIndex = 7; //A7
            using (var db = _dataAccessManger.GetDB())
            {

                if (_dataAccessManger.GetCmdMstByStoreOut("A7", out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

                    var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.LoadCategory = cmdmode;
                    _loggerManager.WriteLogTrace(log);



                    if (_conveyor.GetBuffer(bufferIndex).Auto
                        && _conveyor.GetBuffer(bufferIndex).OutMode
                        && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                        && _conveyor.GetBuffer(bufferIndex).Presence == false
                        && _conveyor.GetBuffer(bufferIndex).Error == false
                        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
                        && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3)  //為了不跟盤點命令衝突的條件
                    {
                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                        log.CmdSno = cmdSno;
                        log.LoadCategory = cmdmode;
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.LoadCategory = cmdmode;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, cmdmode);
                        }

                    }
                }
            }
        }

        private void StoreOut_S401_WriteCV()
        {
            int bufferIndex = 9; //A9
            using (var db = _dataAccessManger.GetDB())
            {

                if (_dataAccessManger.GetCmdMstByStoreOut("A9", out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

                    var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get StoreOut Command");
                    log.CmdSno = cmdSno;
                    log.LoadCategory = cmdmode;
                    _loggerManager.WriteLogTrace(log);


                    if (_conveyor.GetBuffer(bufferIndex).Auto
                        && _conveyor.GetBuffer(bufferIndex).OutMode
                        && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                        && _conveyor.GetBuffer(bufferIndex).Presence == false
                        && _conveyor.GetBuffer(bufferIndex).Error == false
                        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
                        && _conveyor.GetBuffer(bufferIndex).CmdMode != 3    //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex + 1).CmdMode != 3)  //為了不跟盤點命令衝突的條件
                    {
                        log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreOut Command");
                        log.CmdSno = cmdSno;
                        log.LoadCategory = cmdmode;
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreOut Command To Buffer");
                            log.CmdSno = cmdSno;
                            log.LoadCategory = cmdmode;
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, cmdmode);
                        }

                    }
                }
            }
        }
        private void StoreOut_S101_CreateEquCmd()
        {
            int bufferIndex = 1;
            List<string> stn = new List<string>()
            {
                StnNo.A1,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
                int CmdMode = _conveyor.GetBuffer(bufferIndex).CmdMode;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = cmdSno;
                log.LoadCategory = CmdMode;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, cmdSno, out var dataObject) == GetDataResult.Success)
                {
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A1}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }


        private void StoreOut_S201_CreateEquCmd()
        {
            int bufferIndex = 5;
            List<string> stn = new List<string>()
            {
                StnNo.A5,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
                int LoadCategory = _conveyor.GetBuffer(bufferIndex).CmdMode;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = cmdSno;
                log.LoadCategory = LoadCategory;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, cmdSno, out var dataObject) == GetDataResult.Success)
                {
                    cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A5}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreOut_S301_CreateEquCmd()
        {
            int bufferIndex = 7;
            List<string> stn = new List<string>()
            {
                StnNo.A7,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
                int LoadCategory = _conveyor.GetBuffer(bufferIndex).CmdMode;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = cmdSno;
                log.LoadCategory = LoadCategory;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, cmdSno, out var dataObject) == GetDataResult.Success)
                {
                    cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A7}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreOut_S401_CreateEquCmd()
        {
            int bufferIndex = 1;
            List<string> stn = new List<string>()
            {
                StnNo.A9,
            };
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).OutMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence == false
                && _conveyor.GetBuffer(bufferIndex).Error == false
                && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady)
            {
                string cmdSno = _conveyor.GetBuffer(bufferIndex).CommandId.ToString();
                int LoadCategory = _conveyor.GetBuffer(bufferIndex).CmdMode;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer StoreOut Command Receive Completed");
                log.CmdSno = cmdSno;
                log.LoadCategory = LoadCategory;
                _loggerManager.WriteLogTrace(log);

                if (_dataAccessManger.GetCmdMstByStoreOut(stn, cmdSno, out var dataObject) == GetDataResult.Success)
                {
                    cmdSno = dataObject[0].CmdSno;
                    string source = dataObject[0].Loc;
                    string dest = $"{CranePortNo.A9}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreOut Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreOut_EquCmdFinish()
        {
            var stn = new List<string>()
            {
                StnNo.A1,
                StnNo.A5,
                StnNo.A7,
                StnNo.A9,
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

        #endregion StoreOut



        #region StoreIn
        private void StoreInProcess(object sender, ElapsedEventArgs e)
        {
            _storeInProcess.Stop();

            StoreIn_S101_WriteCV();

            StoreIn_S101_CreateEquCmd();//OK

            StoreIn_S201_WriteCV();//OK

            StoreIn_S201_CreateEquCmd();//OK

            StoreIn_S301_WriteCV();//OK

            StoreIn_S301_CreateEquCmd();//OK

            StoreIn_S401_WriteCV();//OK

            StoreIn_S401_CreateEquCmd();//OK

            StoreIn_EquCmdFinish();//OK


            _storeInProcess.Start();
        }


        private void StoreIn_S101_WriteCV()
        {
            int bufferIndex = 3;
            using (var db = _dataAccessManger.GetDB())
            {

                if (_dataAccessManger.GetCmdMstByStoreInstart("A3", out var dataObject) == GetDataResult.Success) //讀取CMD_MST
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                    int IOType = Convert.ToInt32(dataObject[0].IOType);

                    var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Write StoreIn Command");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    //確認目前模式，是否可以切換模式，可以就寫入切換成入庫的請求
                    if (_conveyor.GetBuffer(bufferIndex - 2).Ready != Ready.StoreInReady
                        && _conveyor.GetBuffer(bufferIndex - 2).Switch_Ack == 1)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreIn Ready, Can Switchmode");
                        _loggerManager.WriteLogTrace(log);

                        _conveyor.GetBuffer(bufferIndex - 2).Switch_Mode(1);

                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Switchmode Complete");
                        _loggerManager.WriteLogTrace(log);
                    }

                    if (IOType == 1
                     && _conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).InMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex - 2).Ready == Ready.StoreInReady
                    && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
                    && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
                    && _conveyor.GetBuffer(bufferIndex - 2).CmdMode != 3   //為了不跟盤點命令衝突的條件
                     && _conveyor.GetBuffer(bufferIndex +1).Presence ==true) //在一般入庫時要確認A4是否有空棧板，沒有則不寫入命令
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;

                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode);//寫入命令和模式

                            if (IOType == 1)
                            {
                                _conveyor.GetBuffer(4).A4EmptysupplyOn();//請A4補充母托一版
                            }
                        }

                    }
                    else if (IOType == 6
                     && _conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).InMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex - 2).Ready == Ready.StoreInReady
                    && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
                    && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3  //為了不跟盤點命令衝突的條件
                    && _conveyor.GetBuffer(bufferIndex - 2).CmdMode != 3 )  //為了不跟盤點命令衝突的條件
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive Normal-EmptyStoreIn Command");
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte Normal-EmptyStoreIn Command To Buffer");
                            log.CmdSno = cmdSno;

                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode);//寫入命令和模式
                        }

                    }
                    #region 站口狀態自動確認寫log
                    else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口不是入庫模式");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口不是自動模式");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Error == true)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口buffer異常中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3 || _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3 || _conveyor.GetBuffer(bufferIndex - 2).CmdMode != 3)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口執行盤點中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Presence == true)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口有荷有訊號，請移除荷有");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).CommandId > 0)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口有命令號，請確認命令號是否異常殘留");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    #endregion
                }
            }
        }

        private void StoreIn_S201_WriteCV()
        {
            int bufferIndex = 6;
            using (var db = _dataAccessManger.GetDB())
            {

                if (_dataAccessManger.GetCmdMstByStoreInstart("A6", out var dataObject) == GetDataResult.Success) //讀取CMD_MST
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

                    var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Write StoreIn Command");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);


                    if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).InMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex - 1).Ready == Ready.StoreInReady
                    && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
                    && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3) //為了不跟盤點命令衝突的條件)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;

                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode);//寫入命令和模式
                        }

                    }
                    else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口不是入庫模式");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口不是自動模式");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Error == true)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口異常中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口執行盤點中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex - 1).CmdMode == 3)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口執行盤點中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }

                }
            }
        }


        private void StoreIn_S301_WriteCV()
        {
            int bufferIndex = 8;
            using (var db = _dataAccessManger.GetDB())
            {

                if (_dataAccessManger.GetCmdMstByStoreInstart("A8", out var dataObject) == GetDataResult.Success) //讀取CMD_MST
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);


                    var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Write StoreIn Command");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);



                    if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).InMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex - 1).Ready == Ready.StoreInReady
                    && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
                    && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3) //為了不跟盤點命令衝突的條件
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;

                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode);//寫入命令和模式
                        }

                    }
                    else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口不是入庫模式");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口不是自動模式");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Error == true)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口異常中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口執行盤點中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口執行盤點中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                }
            }
        }

        private void StoreIn_S401_WriteCV()
        {
            int bufferIndex = 10;
            using (var db = _dataAccessManger.GetDB())
            {


                if (_dataAccessManger.GetCmdMstByStoreInstart("A10", out var dataObject) == GetDataResult.Success) //讀取CMD_MST
                {
                    string cmdSno = dataObject[0].CmdSno;
                    int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

                    var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Write StoreIn Command");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);


                    if (_conveyor.GetBuffer(bufferIndex).Auto
                    && _conveyor.GetBuffer(bufferIndex).InMode
                    && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                    && _conveyor.GetBuffer(bufferIndex).Presence == false
                    && _conveyor.GetBuffer(bufferIndex).Error == false
                    && _conveyor.GetBuffer(bufferIndex - 1).Ready == Ready.StoreInReady
                    && _conveyor.GetBuffer(bufferIndex).CmdMode != 3      //為了不跟盤點命令衝突的條件
                    && _conveyor.GetBuffer(bufferIndex - 1).CmdMode != 3) //為了不跟盤點命令衝突的條件
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive StoreIn Command");
                        _loggerManager.WriteLogTrace(log);


                        if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.StoreInWriteCmdToCV) == ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte StoreIn Command To Buffer");
                            log.CmdSno = cmdSno;

                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, CmdMode);//寫入命令和模式
                        }

                    }
                    else if (_conveyor.GetBuffer(bufferIndex).InMode == false)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口不是入庫模式");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Auto == false)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口不是自動模式");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).Error == true)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口異常中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口執行盤點中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    else if (_conveyor.GetBuffer(bufferIndex).CmdMode == 3)
                    {
                        log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "站口執行盤點中");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                }
            }
        }

        private void StoreIn_S101_CreateEquCmd()
        {
            int bufferIndex = 1;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Error == false)
            {


                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                _loggerManager.WriteLogTrace(log);

                string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
                if (_dataAccessManger.GetCmdMstByStoreIn(cmdSno, out var dataObject) == GetDataResult.Success)
                {

                    string source = $"{CranePortNo.A1}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_S201_CreateEquCmd()
        {
            int bufferIndex = 5;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Error == false)
            {

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                _loggerManager.WriteLogTrace(log);

                string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
                if (_dataAccessManger.GetCmdMstByStoreIn(cmdSno, out var dataObject) == GetDataResult.Success)
                {
                    string source = $"{CranePortNo.A5}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_S301_CreateEquCmd()
        {
            int bufferIndex = 7;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Error == false)
            {
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                _loggerManager.WriteLogTrace(log);

                string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
                if (_dataAccessManger.GetCmdMstByStoreIn(cmdSno, out var dataObject) == GetDataResult.Success)
                {
                    string source = $"{CranePortNo.A7}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void StoreIn_S401_CreateEquCmd()
        {
            int bufferIndex = 9;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Error == false)
            {
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                _loggerManager.WriteLogTrace(log);

                string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
                if (_dataAccessManger.GetCmdMstByStoreIn(cmdSno, out var dataObject) == GetDataResult.Success)
                {
                    string source = $"{CranePortNo.A9}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.StoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }





        private void StoreIn_EquCmdFinish()
        {
            var stn1 = new List<string>()
            {
                StnNo.A1,
                StnNo.A5,
                StnNo.A7,
                StnNo.A9,
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
        #endregion StoreIn

        #region Other
        private void OtherProcess(object sender, ElapsedEventArgs e)
        {
            _otherProcess.Stop();

            EmptyStoreIn_S101_WriteCV();

            EmptyStoreIn_S101_CreateEquCmd();

            EmptyStoreIn_EquCmdFinish();

            EmptyStoreOut_S101_WriteCV();

            EmptyStoreOut_S101_CreateEquCmd();

            EmptyStoreOut_EquCmdFinish();

            Other_LocToLoc();

            _otherProcess.Start();
        }



        private void EmptyStoreIn_S101_WriteCV()
        {
            int bufferIndex = 4;
            List<string> stn = new List<string>()
            {
                StnNo.A1,
            };
            using (var db = _dataAccessManger.GetDB())
            {
                int loadType = 9;
                if (_conveyor.GetBuffer(bufferIndex).EmptyINReady == 9) //滿九版,滿版訊號為9
                {
                    if (_dataAccessManger.GetCmdMstByStoreInstart("A4", out var dataObject) == GetDataResult.Success) //讀取CMD_MST
                    {
                        string cmdSno = dataObject[0].CmdSno;

                        var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Write StoreIn Command");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);

                        //確認目前模式，是否可以切換模式，可以就寫入切換成入庫的請求
                        if (_conveyor.GetBuffer(bufferIndex - 3).Ready != Ready.StoreInReady
                        && _conveyor.GetBuffer(bufferIndex - 3).Switch_Ack == 1)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreIn Ready, Can Switchmode");
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex - 3).Switch_Mode(1);
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Switchmode Complete");
                            _loggerManager.WriteLogTrace(log);
                        }

                        if (_conveyor.GetBuffer(bufferIndex).Auto
                        && _conveyor.GetBuffer(bufferIndex).InMode
                        && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                        && _conveyor.GetBuffer(bufferIndex).Presence == true
                        && _conveyor.GetBuffer(bufferIndex).Error == false
                        && _conveyor.GetBuffer(bufferIndex - 3).Ready == Ready.StoreInReady
                        && _conveyor.GetBuffer(bufferIndex - 1).TrayType != 3  //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex - 2).TrayType != 3  //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex - 3).TrayType != 3) //為了不跟盤點命令衝突的條件)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive EmptyStoreIn Command");
                            _loggerManager.WriteLogTrace(log);


                            if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.EmptyStoreInWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                            {
                                log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte EmptyStoreIn Command To Buffer");
                                log.CmdSno = cmdSno;

                                _loggerManager.WriteLogTrace(log);

                                _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, loadType);//寫入命令和模式
                            }

                        }
                    }
                }
            }
        }

        private void EmptyStoreIn_S101_CreateEquCmd()
        {
            int bufferIndex = 1;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Error == false)
            {
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready EmptyStoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                _loggerManager.WriteLogTrace(log);

                string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
                if (_dataAccessManger.GetEmptyCmdMstByStoreIn(cmdSno, out var dataObject) == GetDataResult.Success)
                {

                    string source = $"{CranePortNo.A1}";
                    string dest = $"{dataObject[0].NewLoc}";

                    log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.EmptyStoreInCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreInLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }

        private void EmptyStoreIn_EquCmdFinish()
        {
            var stn1 = new List<string>()
            {
                StnNo.A1,
            };
            if (_dataAccessManger.GetEmptyCmdMstByStoreInFinish(stn1, out var dataObject) == GetDataResult.Success)
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
                                    if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.EmptyStoreInCraneCmdFinish) != ExecuteSQLResult.Success)
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

        private void EmptyStoreOut_S101_WriteCV()
        {
            int bufferIndex = 1;
            List<string> stn = new List<string>()
            {
                StnNo.A1,
            };
            using (var db = _dataAccessManger.GetDB())
            {
                string cmdSno = "";
                int loadType = 4;
                string LOC = "";
                if (_conveyor.GetBuffer(bufferIndex+3).Presence == false) //沒有荷有，無空棧板需要補充
                {
                    if (_dataAccessManger.GetCmdMstByStoreOut("A1", out var dataObject) == GetDataResult.Success) //讀取CMD_MST 
                    {
                        cmdSno = dataObject[0].CmdSno;
                        int cmdmode = Convert.ToInt32(dataObject[0].CmdMode);

                        var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Get EmptyStoreOut Command");
                        log.CmdSno = cmdSno;
                        log.LoadCategory = cmdmode;
                        _loggerManager.WriteLogTrace(log);

                        //確認目前模式，是否可以切換模式，可以就寫入切換成入庫的請求
                        if (_conveyor.GetBuffer(bufferIndex).Ready != Ready.StoreInReady
                        && _conveyor.GetBuffer(bufferIndex).Switch_Ack == 1)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Not StoreOut Ready, Can Switchmode");
                            _loggerManager.WriteLogTrace(log);

                            _conveyor.GetBuffer(bufferIndex - 3).Switch_Mode(2);
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Empty StoreOut Switchmode Complete");
                            _loggerManager.WriteLogTrace(log);
                        }

                        if (_conveyor.GetBuffer(bufferIndex).Auto
                        && _conveyor.GetBuffer(bufferIndex).OutMode
                        && _conveyor.GetBuffer(bufferIndex).CommandId == 0
                        && _conveyor.GetBuffer(bufferIndex).Presence == false
                        && _conveyor.GetBuffer(bufferIndex).Error == false
                        && _conveyor.GetBuffer(bufferIndex).Ready == Ready.StoreOutReady
                        && _conveyor.GetBuffer(bufferIndex+1).TrayType != 3  //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex+2).TrayType != 3  //為了不跟盤點命令衝突的條件
                        && _conveyor.GetBuffer(bufferIndex).TrayType != 3) //為了不跟盤點命令衝突的條件)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready Receive EmptyStoreOut Command");
                            _loggerManager.WriteLogTrace(log);


                            if (_dataAccessManger.UpdateCmdMstTransferring(db, cmdSno, Trace.EmptyStoreOutWriteCraneCmdToCV) == ExecuteSQLResult.Success)
                            {
                                log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Wirte EmptyStoreOut Command To Buffer");
                                log.CmdSno = cmdSno;

                                _loggerManager.WriteLogTrace(log);

                                _conveyor.GetBuffer(bufferIndex).WriteCommandIdAsync(cmdSno, loadType);//寫入命令和模式
                                int path = 3;
                                _conveyor.GetBuffer(bufferIndex).WritePathChabgeNotice(path);//出庫都要寫入路徑編號，寫入路徑編號根據WMS寫入的棧口作為依據
                            }

                        }

                    }

                }
            }
        }

        private void EmptyStoreOut_S101_CreateEquCmd()
        {
            int bufferIndex = 1;
            if (_conveyor.GetBuffer(bufferIndex).Auto
                && _conveyor.GetBuffer(bufferIndex).InMode
                && _conveyor.GetBuffer(bufferIndex).CommandId > 0
                && _conveyor.GetBuffer(bufferIndex).Presence
                && _conveyor.GetBuffer(bufferIndex).Error == false)
            {
                int loadType = _conveyor.GetBuffer(bufferIndex).LoadCategory;

                var log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready EmptyStoreIn, Tray Ready On Position");
                log.CmdSno = string.Empty;
                _loggerManager.WriteLogTrace(log);

                string cmdSno = (_conveyor.GetBuffer(bufferIndex).CommandId).ToString();
                if (_dataAccessManger.GetCmdMstByStoreOut(cmdSno, out var dataObject) == GetDataResult.Success)
                {

                    string source = $"{dataObject[0].NewLoc}";
                    string dest =  $"{CranePortNo.A1}";

                    log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Buffer Ready StoreIn");
                    log.CmdSno = cmdSno;
                    _loggerManager.WriteLogTrace(log);

                    using (var db = _dataAccessManger.GetDB())
                    {
                        if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Begin Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            return;
                        }
                        if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.EmptyStoreOutCreateCraneCmd) != ExecuteSQLResult.Success)
                        {
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Update CmdMst Fail");
                            log.CmdSno = cmdSno;
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
                            log = new StoreOutLogTrace(_conveyor.GetBuffer(bufferIndex).BufferIndex, _conveyor.GetBuffer(bufferIndex).BufferName, "Create Crane StoreIn Command, Commit Fail");
                            log.CmdSno = cmdSno;
                            _loggerManager.WriteLogTrace(log);
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return;
                        }
                    }
                }
            }
        }


        private void EmptyStoreOut_EquCmdFinish()
        {
            var stn = new List<string>()
            {
                StnNo.A1,
            };
            if (_dataAccessManger.GetEmptyCmdMstByStoreOutFinish(stn, out var dataObject) == GetDataResult.Success)
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
                                    if (_dataAccessManger.UpdateCmdMst(db, equCmd[0].CmdSno, $"{CmdSts.CompleteWaitUpdate}", Trace.EmptyStoreOutCraneCmdFinish) == ExecuteSQLResult.Success)
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

        private void Other_LocToLoc()
        {
           

            if (_dataAccessManger.GetLocToLoc(out var dataObject) == GetDataResult.Success)
            {

                string source = $"{dataObject[0].Loc}";
                string dest = $"{dataObject[0].NewLoc}";
                string cmdSno = $"{dataObject[0].CmdSno}";

                var log = new StoreOutLogTrace(5, "LocToLoc", "Buffer Ready StoreIn");
                log.CmdSno = cmdSno;
                _loggerManager.WriteLogTrace(log);

                using (var db = _dataAccessManger.GetDB())
                {
                    if (db.TransactionCtrl2(TransactionTypes.Begin) != TransactionCtrlResult.Success)
                    {
                        log = new StoreOutLogTrace(5, "LocToLoc", "Create Crane LocToLoc Command, Begin Fail");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        return;
                    }
                    if (_dataAccessManger.UpdateCmdMst(db, cmdSno, Trace.LoctoLocReady) != ExecuteSQLResult.Success)
                    {
                        log = new StoreOutLogTrace(5, "LocToLoc", "Create Crane LocToLoc Command, Update CmdMst Fail");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        db.TransactionCtrl2(TransactionTypes.Rollback);
                        return;
                    }
                    if (InsertLocToLocEquCmd(db, 5, "LocToLoc", 1, cmdSno, source, dest, 1) == false)
                    {
                        db.TransactionCtrl2(TransactionTypes.Rollback);
                        return;
                    }
                    if (db.TransactionCtrl2(TransactionTypes.Commit) != TransactionCtrlResult.Success)
                    {
                        log = new StoreOutLogTrace(5, "LocToLoc",  "Create Crane LocToLoc Command Commit Fail");
                        log.CmdSno = cmdSno;
                        _loggerManager.WriteLogTrace(log);
                        db.TransactionCtrl2(TransactionTypes.Rollback);
                        return;
                    }
                }
            }
        }

        #endregion other

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


            catch (Exception ex)
            {
                _loggerManager.Error(ex);
                return false;
            }
        }

        private bool InsertLocToLocEquCmd(DB db, int CmdType, string IoType, int craneNo, string cmdSno, string source, string destination, int priority)
        {
            try
            {
                if (source.Length != 7)
                {
                    var log = new EquCmdLogTrace(CmdType, IoType, "Check Source Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }

                if (destination.Length != 7)
                {
                    var log = new EquCmdLogTrace(CmdType, IoType, "Check Destination Fail, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return false;
                }

                    if (CheckExecutionEquCmd(CmdType, IoType, craneNo, cmdSno, EquCmdMode.LocToLoc, source, destination) == false)
                    {
                        if (_dataAccessManger.InsertEquCmd(db, craneNo, cmdSno, ((int)EquCmdMode.LocToLoc).ToString(), source, destination, priority) == ExecuteSQLResult.Success)
                        {
                            var log = new EquCmdLogTrace(CmdType, IoType, "Insert Equ Cmd");
                            log.CommandId = cmdSno;
                            log.CraneNo = craneNo;
                            log.Source = source;
                            log.Destination = destination;
                            _loggerManager.WriteLogTrace(log);
                            return true;
                        }
                        else
                        {
                            var log = new EquCmdLogTrace(CmdType, IoType, "Insert Equ Cmd Fail");
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

        //檢查是否有重複的Crane命令，狀態為0和1的命令只能為1個
        private bool CheckExecutionEquCmd(int bufferIndex, string bufferName, int craneNo, string cmdSno, EquCmdMode equCmdMode, string source, string destination)
        {
            if (_dataAccessManger.GetEquCmd(cmdSno, out var equCmd) == GetDataResult.Success)
            {
                if (equCmd[0].CmdSts == CmdSts.Queue.GetHashCode().ToString() || equCmd[0].CmdSts == CmdSts.Transferring.GetHashCode().ToString())
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Exists Command On Equ Execute, Please Check");
                    log.CommandId = cmdSno;
                    log.CraneNo = craneNo;
                    log.Source = source;
                    log.Destination = destination;
                    _loggerManager.WriteLogTrace(log);
                    return true;
                }
                else
                {
                    var log = new EquCmdLogTrace(bufferIndex, bufferName, "Exists Command On Equ, Please Check");
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
                if (_dataAccessManger.checkCraneNoReapeat(out var dataObject) == GetDataResult.Success)
                {
                    int intCraneCount = 0;
                    intCraneCount = int.Parse(dataObject[0].COUNT.ToString());
                    return intCraneCount == 0 ? false : true;
                }
                else
                {
                    return true;
                }
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
