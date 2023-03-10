using System;
using System.Collections.Generic;
using Mirle.ASRS.WCS.Controller;
using Mirle.DB.Fun;
using Mirle.Def;
using Mirle.DataBase;
using Mirle.Def.T26YGAP0;
using Mirle.Grid.T26YGAP0;
using Mirle.Structure;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.CENS.T26YGAP0;
using HslCommunicationPLC.Siemens;
using static Mirle.Def.clsConstValue;
using Mirle.IASC;
using Mirle.BarcodeReader;
using System.Text.RegularExpressions;
using static Mirle.Grid.T26YGAP0.ColumnDef;
using PLCConfigSetting.PLCsetting;
using System.Text;
using System.Threading;
using static Mirle.Def.clsEnum;

namespace Mirle.DB.Proc
{
    public class clsProc
    {
        private clsPortDef PortDef = new clsPortDef();
        private Fun.clsCmd_Mst CMD_MST = new Fun.clsCmd_Mst();
        private Fun.clsSno SNO = new Fun.clsSno();
        private Fun.clsLocMst LocMst = new Fun.clsLocMst();
        private Fun.clsProc proc;
        private Fun.clsAlarmData alarmData = new Fun.clsAlarmData();
        private Fun.clsCmd_Mst_His CMD_MST_HIS = new Fun.clsCmd_Mst_His();
        private Fun.clsTaskNo Task = new Fun.clsTaskNo(); 

        private clsDbConfig _config = new clsDbConfig();
        private clsDbConfig _config_WMS = new clsDbConfig();
        private clsDbConfig _config_Sqlite = new clsDbConfig();
        private ShuttleController? _shuttleController;
        private ShuttleCommand? _shuttleCommand;
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

        public void getshuttlecontroller(ShuttleController shuttleController)
        {
            _shuttleController = shuttleController;
        }
        public ShuttleController returnshuttlecontroller()
        {
            return _shuttleController ;
        }


        #region StoreIn

        public bool FunStoreiNwriPLCA1andA3(clsBufferData Plc1)
        {
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {


                    if (CMD_MST.GetCmdMstByStoreInBCRdataStart(out var dataObject, db).ResultCode == DBResult.Success) //讀取入庫動作流程
                    {
                        string cmdSno = dataObject[0].CmdSno;
                        int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                        string TraceBCR = dataObject[0].Trace;
                        int bufferIndex = 0;
                        if(TraceBCR== Trace.StoreInWriteCmdToCV1)
                        {
                            bufferIndex = 1;
                        }
                        else if(TraceBCR== Trace.StoreInWriteCmdToCV2)
                        {
                            bufferIndex= 3;
                        }
                        string Loc_ID = dataObject[0].Loc_ID;

                        clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"Buffer Get Command => {cmdSno}, " +
                                $"{CmdMode}" +
                                $"{Loc_ID}");

                        #region//根據buffer狀態更新命令

                        if (Plc1.oPLC.PLC[bufferIndex].CV.AllowWriteCommand != true)
                        {
                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CV_NOTallowWriteCommand, db);
                            return false;
                        }
                        #endregion

                        clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"Buffer Ready Receive StoreIn Command=> {cmdSno}");

                        if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                        {
                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", "begin fail");
                            return false;
                        }
                        if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInWriteCmdToCV, db).ResultCode == DBResult.Success)
                        {
                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"Upadte cmd succeess => {cmdSno}");
                        }
                        else
                        {
                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"Upadte cmd fail => {cmdSno}");

                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        string sdevice = "";
                        if (bufferIndex == 1)
                        {
                            sdevice = "2";
                        }
                        else if (bufferIndex == 3)
                        {
                            sdevice = "4";
                        }


                        bool Result = Plc1.FunWriPLC_Word("DB" + sdevice + ".0", cmdSno);//確認寫入PLC命令命令序號

                        if (Result != true)
                        {
                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"WritePLC CV_Command Fail");
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        Result = Plc1.FunWriPLC_Word("DB" + sdevice + ".2", CmdMode.ToString());//確認寫入PLC模式的方法是否正常運作命令模式

                        if (Result != true)
                        {
                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"WritePLC CV_mode Fail");
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        Result = Plc1.FunWriPLC_Bit("DB" + sdevice + ".6.1", true);//確認寫入PLC命令完成的方法是否正常運作//寫入命令完成(CV) 

                        if (Result != true)
                        {
                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"WritePLC CV_writeCommandComplete Fail");
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        Result = Plc1.FunWriPLC_Bit("DB" + sdevice + ".6.2", true);//確認寫入PLC讀取完成的方法是否正常運作//放行許可 

                        if (Result != true)
                        {
                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"WritePLC CV_BCRReadComplete Fail");
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                        {
                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", "Commit Fail");

                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }
                        else return true;

                    }


                    if (CMD_MST.GetCmdMstByCycleBCRDATAINstart(out dataObject, db).ResultCode == DBResult.Success) //讀取撿料動作
                    {
                        string cmdSno = dataObject[0].CmdSno;
                        int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                        string TraceBCR = dataObject[0].Trace;
                        int bufferIndex = 0;
                        if (TraceBCR == Trace.StoreInWriteCmdToCV1)
                        {
                            bufferIndex = 1;
                        }
                        else if (TraceBCR == Trace.StoreInWriteCmdToCV2)
                        {
                            bufferIndex = 3;
                        }
                        string Loc_ID = dataObject[0].Loc_ID;

                        clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Buffer Get Command => {cmdSno}, " +
                                $"{CmdMode}" + $"{Loc_ID}");


                        #region//根據buffer狀態更新命令

                        if (Plc1.oPLC.PLC[bufferIndex].CV.AllowWriteCommand != true)
                        {
                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.CV_NOTallowWriteCommand, db);
                            return false;
                        }
                        #endregion

                        clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Buffer Ready Receive StoreIn Command=> {cmdSno}");

                        if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                        {
                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", "begin fail");
                            return false;
                        }
                        if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInWriteCmdToCV, db).ResultCode == DBResult.Success)
                        {
                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd succeess => {cmdSno}");
                        }
                        else
                        {
                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd fail => {cmdSno}");

                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        string sdevice = "";
                        if (bufferIndex == 1)
                        {
                            sdevice = "2";
                        }
                        else if (bufferIndex == 3)
                        {
                            sdevice = "4";
                        }


                        bool Result = Plc1.FunWriPLC_Word("DB" + sdevice + ".0", cmdSno);//確認寫入PLC命令的方法是否正常運作

                        if (Result != true)
                        {
                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"WritePLC CV_Command Fail");
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        Result = Plc1.FunWriPLC_Word("DB" + sdevice + ".2", CmdMode.ToString());//確認寫入PLC模式的方法是否正常運作

                        if (Result != true)
                        {
                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"WritePLC CV_mode Fail");
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        Result = Plc1.FunWriPLC_Bit("DB" + sdevice + ".6.1", true);//確認寫入PLC命令完成的方法是否正常運作

                        if (Result != true)
                        {
                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"WritePLC CV_writeCommandComplete Fail");
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        Result = Plc1.FunWriPLC_Bit("DB" + sdevice + ".6.2", true);//確認寫入PLC讀取完成的方法是否正常運作

                        if (Result != true)
                        {
                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"WritePLC CV_BCRReadComplete Fail");
                            db.TransactionCtrl2(TransactionTypes.Rollback);
                            return false;
                        }

                        if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                        {
                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", "Commit Fail");

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


        public bool FunStoreInWriPlc1FBCR(clsBufferData Plc1, SocketDataReceiveEventArgs e)//由BCR入庫觸發function
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        string Loc_ID= Regex.Replace(e.Content, @"[\r-\r\n]", string.Empty); ;
                        int bufferIndex = 0;
                        string TraceBCR = "";
                        string PLCDB = "";

                        if (e.RemoteEndPoint.ToString().Substring(0,14)==ASRS_Setting.BCR_IP_1)
                        {
                            bufferIndex = 1;
                            TraceBCR = Trace.StoreInWriteCmdToCV1;
                            PLCDB = "2";
                        }
                        else if(e.RemoteEndPoint.ToString().Substring(0,14)==ASRS_Setting.BCR_IP_2)
                        {
                            bufferIndex = 3;
                            TraceBCR = Trace.StoreInWriteCmdToCV2;
                            PLCDB = "4";
                        }
                        else
                        {
                            clsWriLog.StoreInLogTrace(0, "1F", $"BCR Trigger=>can't find IP");
                            return false;
                        }


                        if (CMD_MST.GetCmdMstByStoreInStart(Loc_ID, out var dataObject, db).ResultCode == DBResult.Success) //讀取入庫動作流程
                        {
                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"Buffer Get Command => {cmdSno}, " +
                                    $"{CmdMode}" +
                                    $"{Loc_ID}");



                            clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"Buffer Ready Receive StoreIn Command=> {cmdSno}");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, "1F", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferringstart(cmdSno, TraceBCR, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, "1F", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else return true;

                        }
                        else if (CMD_MST.GetCmdMstByCycleIN(Loc_ID, out dataObject, db).ResultCode == DBResult.Success) //讀取撿料動作
                        {
                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);

                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Buffer Get Command => {cmdSno}, " +
                                    $"{CmdMode}" + $"{Loc_ID}");



                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Buffer Ready Receive StoreIn Command=> {cmdSno}");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, TraceBCR, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }


                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else return true;

                        }
                        else {


                            bool Result = Plc1.FunWriPLC_Bit("DB"+PLCDB+".6.3", true);//確認寫入PLC命令完成的方法是否正常運作//寫入命令完成(CV) 

                            if (Result != true)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"WritePLC NoCommand Fail");
                                return false;
                            }
                            else
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, "1F", $"WritePLC NoCommand Succeess");
                                return false;
                            }

                        };

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

        public bool FunStoreInFA1andA3CallLifterAndSHC(clsBufferData Plc1, string sStnNo, int bufferIndex)
        {

            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        if (CMD_MST.CheckCMDLifterOnlyONECMDatAtime(out var dataObject1, db).ResultCode == DBResult.Success)//對於此案電梯的流程一次只會執行一筆命令，所以要做卡控的動作
                        {
                            return false;
                        }
                        string Sno = Plc1.oPLC.PLC[bufferIndex].CV.Sno.PadLeft(5, '0');


                        //Sno = "00123";

                        if (CMD_MST.GetCmdMstByStoreInLifterStart(Sno, out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string Loc = dataObject[0].Loc;
                            string Loc_ID = dataObject[0].Loc_ID;

                            clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Buffer Get StoreIn Command => {cmdSno}, " + $"{CmdMode}");

                            #region//根據buffer狀態更新命令
                            if (Plc1.oPLC.PLC[bufferIndex].CV.AutoManual != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[bufferIndex].CV.Presence != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceNotExist, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[bufferIndex].CV.idle != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotIdle, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[bufferIndex].CV.StoreInInfo != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NoStoreInInfo, db);
                                return false;
                            }
                            #endregion

                            clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Buffer Ready Call Shuttle and Lifter=> {cmdSno}");

                            //這邊需要作流程為與shuttle交握，需要告知SHC有入庫命令去得知車子在哪一層

                            string TaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.TaskNO, db); //尋找最新不重複的命令號

                            if (TaskNo == "" || TaskNo == "0000")
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex,sStnNo
                                        , $"Find taskNo fail");
                                return false;
                            }
                            string StoreInstart = "";
                            if(bufferIndex==2)
                            {
                                StoreInstart = ASRS_Setting.STNNO_1F_left;
                            }
                            else
                            {
                                StoreInstart = ASRS_Setting.STNNO_1F_right;
                            }

                            //要insert到TaskNo


                            //要insert到TaskNo


                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInWCScommandReportSHC, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (Task.CheckTaskInsert(cmdSno, out var dataObject2, db).ResultCode != DBResult.Success)//避免重複insert設備命令
                            {


                                if (Task.StorIninsertTaskstart("0000", 1, cmdSno, TaskNo, Trace.StoreInWCScommandReportSHC, StoreInstart, Loc, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Insert Taskcmd succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Insert cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }

                            }
                            if (CMD_MST.UpdateCmdMstTaskNo(cmdSno,TaskNo, db).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"CMDmst Upadte TaskNo fail => {cmdSno}");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            
                            else {
                                _shuttleCommand = new ShuttleCommand(TaskNo, "H", 1, StoreInstart, Loc, Loc_ID, "0000");//入庫待修改參數 儲位由WMS給 箱子號為掃bCR得到，vehicle固定0000
                                _shuttleController?.CreateShuttleCommand(_shuttleCommand);
                                return true;
                            } 

                        }
                        else
                        {
                            CMD_MST.UpdateCmdMstTransferring10sec(Sno,Trace.StoreInWriteCmdToCV,db);
                            //clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, "10 sec PASS no SHC reply,return trace to last step");
                            return true;
                        } 
                            

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

        public bool FunStoreIn_OUT_A1andA3CallLifterAndSHCheckcommand(clsBufferData Plc1, string sStnNo, int bufferIndex, CommandReceiveEventArgs e)
        {

            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        string Sno = e.CommandId;


                        if (CMD_MST.GetCmdMstByStoreInLifterStartSHCcheck(Sno, out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string Loc = dataObject[0].Loc;
                            string Loc_ID = dataObject[0].Loc_ID;  
                            string CmdTrace= dataObject[0].Trace;



                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.StoreInCallSHCMoveCar, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (Task.UpdateTaskState(cmdSno, Trace.StoreInCallSHCMoveCar, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else return true;

                        }

                        if (CMD_MST.GetCmdMstByLoctoLoc(Sno, out var dataObject3, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject3[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject3[0].CmdMode);
                            string Loc = dataObject3[0].Loc;
                            string Loc_ID = dataObject3[0].Loc_ID;
                            string CmdTrace = dataObject3[0].Trace;

                            clsWriLog.L2LLogTrace(bufferIndex, "", $"{cmdSno}=>TaskNo:{Sno}Receive SHC command Report");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.LocToLocStartSHCreceive, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (Task.UpdateTaskState(cmdSno, Trace.LocToLocStartSHCreceive, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else return true;

                        }

                        if (CMD_MST.GetCmdMstByDouble(Sno, out var dataObject5, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject5[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject5[0].CmdMode);
                            string Loc = dataObject5[0].Loc;
                            string Loc_ID = dataObject5[0].Loc_ID;
                            string CmdTrace = dataObject5[0].Trace;

                            clsWriLog.L2LLogTrace(bufferIndex, "", $"{cmdSno}=>TaskNo:{Sno}Receive SHC command Report");

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.DoubleStorageSHCreceive, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (Task.UpdateTaskState(cmdSno, Trace.DoubleStorageSHCreceive, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(bufferIndex, "", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else return true;

                        }

                        if (CMD_MST.GetCmdMstByStoreOutLifterStart(Sno, out var dataObject1, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject1[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject1[0].CmdMode);
                            string Loc = dataObject1[0].Loc;
                            string Loc_ID = dataObject1[0].Loc_ID;


                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.PickUpStartSHCreport, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, "Commit Fail");

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

        public bool FunCommandChangeStatus(clsBufferData Plc1, CommandStatusEventArgs e)//接收SHC確認車子命令最後有完成
        {
            string TaskNo = e.CommandId;
            string CommandStatus = e.CommandStatus;
            string ResultCode = e.ResultCode;
            string vehicleID = e.VehicleId;
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        if (CMD_MST.GetCmdMstBySno(TaskNo, out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string Loc = dataObject[0].Loc;
                            string Lvl_Z = Loc.Substring(7, 2);
                            string Loc_ID = dataObject[0].Loc_ID;
                            string newLoc="";
                            string CmdTrace = dataObject[0].Trace;
                            string strfloor = "";
                            string Complete_Floor="";
                            bool StoreIn = false;
                            string newTaskNo = "";

                            if(CommandStatus== "COFAIL" && ResultCode=="0001")//Double Storage處理
                            {
                                string newTrace = Trace.DoubleStorageStart;

                                if (LocMst.GetEmptyLoc_SameFloor_OutFirst(Lvl_Z, out var dataObject3, db).ResultCode == DBResult.Success)//找尋空儲位放貨物
                                {
                                    newLoc = dataObject3[0].Loc;
                                    clsWriLog.LifterLogTrace(0, "Lifter", $"LOCtoLOC=>Find Same Floor EmptyLoc succeess");
                                }
                                else
                                {
                                    clsWriLog.LifterLogTrace(0, "Lifter", $"Command Find EmptyLoc Fail => {cmdSno}, " +
                                    $"{CmdMode}");
                                    return false;
                                }
                                newTaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.TaskNO, db); //尋找最新不重複的命令號

                                if (newTaskNo == "" || newTaskNo == "0000")
                                {
                                    clsWriLog.LifterLogTrace(0, ""
                                            , $"Find taskNo fail");
                                    return false;
                                }


                                if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange_Double_Storage", "begin fail");
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMststsfor_DoubleStorage(cmdSno, newLoc,newTrace,newTaskNo,CmdMode, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange_Double_Storage", $"Upadte cmd succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange_Double_Storage", $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (Task.UpdateTaskforDouble_storage(cmdSno,newTrace ,newLoc,newTaskNo, vehicleID, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange_Double_Storage", $"Upadte Task succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange_Double_Storage", $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (LocMst.UpdateStoreInLocMst(newLoc, db).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "Lifter", $"LocToLoc_LocMSTIN_update Fail!");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange_Double_Storage", "Commit Fail");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                else
                                {
                                    _shuttleController?.S62("0", TaskNo, CommandStatus);//Result_Code:0000=Succeess
                                    return true;
                                };
                            }

                            if (CommandStatus == "COFAIL" && ResultCode == "0002")
                            {
                                if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", "begin fail");
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMststsEndforabnormal(cmdSno, true, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", $"Upadte cmd succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (Task.UpdateTaskStateSHCEnd(cmdSno, true, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", $"Upadte Task succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", "Commit Fail");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                else
                                {
                                    _shuttleController?.S62("0", TaskNo, CommandStatus);//Result_Code:0000=Succeess
                                    return true;
                                };
                            }



                            

                            if (CommandStatus == "TCOMPT" && ResultCode == "0000")
                            {
                                if (Task.CheckTaskGolevel(cmdSno, out var dataObject1, db).ResultCode == DBResult.Success)
                                {
                                    strfloor = dataObject1[0].GoLevel.PadLeft(2, '0');
                                }
                                else
                                {
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }

                                clsWriLog.LifterLogTrace(0, "SHCcommandChange", "Finish Start");

                                if (CmdTrace == ASRS.WCS.Model.DataAccess.Trace.StoreInLiftToLocLevel)
                                {
                                    //CMD_MST.UpdateCmdMstEnd(cmdSno, ASRS.WCS.Model.DataAccess.CmdSts.CompleteWaitUpdate, NewTrace, db);
                                    //Task.UpdateTaskStateEnd(cmdSno, NewTrace, db);
                                    #region 車子移動完成樓層

                                    if (strfloor == "01")
                                    {
                                        Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor1;
                                    }
                                    else if (strfloor == "02")
                                    {
                                        Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor2;
                                    }
                                    else if (strfloor == "03")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor3);
                                    }
                                    else if (strfloor == "04")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor4);
                                    }
                                    else if (strfloor == "05")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor5);
                                    }
                                    else if (strfloor == "06")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor6);
                                    }
                                    else if (strfloor == "07")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor7);
                                    }
                                    else if (strfloor == "08")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor8);
                                    }
                                    else if (strfloor == "09")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor9);
                                    }
                                    else if (strfloor == "10")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor10);
                                    }
                                    #endregion

                                    bool Result;

                                    Result = Plc1.FunWriPLC_Bit("DB1." + Complete_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作


                                    StoreIn = true;
                                }

                                if (CmdTrace == ASRS.WCS.Model.DataAccess.Trace.PickUpCarToReturnCarLevel)
                                {
                                    //CMD_MST.UpdateCmdMstEnd(cmdSno, ASRS.WCS.Model.DataAccess.CmdSts.CompleteWaitUpdate, NewTrace, db);
                                    //Task.UpdateTaskStateEnd(cmdSno, NewTrace, db);
                                    bool Result;

                                    #region 車子移動完成樓層
                                    if (strfloor == "01")
                                    {
                                        Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor1;
                                    }
                                    else if (strfloor == "02")
                                    {
                                        Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor2;
                                    }
                                    else if (strfloor == "03")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor3);
                                    }
                                    else if (strfloor == "04")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor4);
                                    }
                                    else if (strfloor == "05")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor5);
                                    }
                                    else if (strfloor == "06")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor6);
                                    }
                                    else if (strfloor == "07")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor7);
                                    }
                                    else if (strfloor == "08")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor8);
                                    }
                                    else if (strfloor == "09")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor9);
                                    }
                                    else if (strfloor == "10")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor10);
                                    }
                                    #endregion


                                    Result = Plc1.FunWriPLC_Bit("DB1." + Complete_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作
                                    StoreIn = false;
                                }

                                if(CmdTrace==Trace.LocToLocStartSHCreceive)
                                {
                                    StoreIn = true;
                                }

                                if (CmdTrace == Trace.DoubleStorageSHCreceive)
                                {
                                    //CMD_MST.UpdateCmdMstEnd(cmdSno, ASRS.WCS.Model.DataAccess.CmdSts.CompleteWaitUpdate, NewTrace, db);
                                    //Task.UpdateTaskStateEnd(cmdSno, NewTrace, db);
                                    #region 車子移動完成樓層

                                    if (strfloor == "01")
                                    {
                                        Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor1;
                                    }
                                    else if (strfloor == "02")
                                    {
                                        Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor2;
                                    }
                                    else if (strfloor == "03")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor3);
                                    }
                                    else if (strfloor == "04")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor4);
                                    }
                                    else if (strfloor == "05")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor5);
                                    }
                                    else if (strfloor == "06")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor6);
                                    }
                                    else if (strfloor == "07")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor7);
                                    }
                                    else if (strfloor == "08")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor8);
                                    }
                                    else if (strfloor == "09")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor9);
                                    }
                                    else if (strfloor == "10")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor10);
                                    }
                                    #endregion

                                    bool Result;

                                    Result = Plc1.FunWriPLC_Bit("DB1." + Complete_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作


                                    StoreIn = true;
                                }

                                if (CmdTrace == Trace.LocToLocCallSHCtoL2Lfloor)
                                {
                                    StoreIn = true;
                                    #region 車子移動完成樓層
                                    if (strfloor == "01")
                                    {
                                        Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor1;
                                    }
                                    else if (strfloor == "02")
                                    {
                                        Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor2;
                                    }
                                    else if (strfloor == "03")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor3);
                                    }
                                    else if (strfloor == "04")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor4);
                                    }
                                    else if (strfloor == "05")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor5);
                                    }
                                    else if (strfloor == "06")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor6);
                                    }
                                    else if (strfloor == "07")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor7);
                                    }
                                    else if (strfloor == "08")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor8);
                                    }
                                    else if (strfloor == "09")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor9);
                                    }
                                    else if (strfloor == "10")
                                    {
                                        Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor10);
                                    }
                                    #endregion

                                    bool Result;
                                    Result = Plc1.FunWriPLC_Bit("DB1." + Complete_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作
                                }

                                if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", "begin fail");
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (CMD_MST.UpdateCmdMststsEnd(cmdSno, StoreIn, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", $"Upadte cmd succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (Task.UpdateTaskStateSHCEnd(cmdSno, StoreIn, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", $"Upadte Task succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", $"Upadte cmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                {
                                    clsWriLog.LifterLogTrace(0, "SHCcommandChange", "Commit Fail");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    _shuttleController?.S62("1", TaskNo, CommandStatus);
                                    return false;
                                }
                                else
                                {
                                    _shuttleController?.S62("0", TaskNo, CommandStatus);//Result_Code:0000=Succeess
                                    return true;
                                };
                            }
                            else
                            {
                                _shuttleController?.S62("0", TaskNo, CommandStatus);//Result_Code:0000=Succeess
                                return true;
                            }
                            
                        }
                        else
                        {
                            _shuttleController?.S62("1", TaskNo, CommandStatus);
                            return false; };
                        }
                    else
                    {
                        _shuttleController?.S62("1", TaskNo, CommandStatus);
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

        public bool FunWriPlcCompletefloor_CMD(clsBufferData Plc1)//與SHC_CALL切開，當接收到命令為SHC_CALL換層，此function才寫入PLC訊號
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        if (CMD_MST.CheckCMDMovePLCwrite(out var dataObject, db).ResultCode == DBResult.Success) //讀取入庫動作流程
                        {
                            bool Result = false;
                            string trace = dataObject[0].Trace;
                          
                            bool WriteCMD = false;
                            string CarmoveCompleteFloor = "";
                            string cmdSno = dataObject[0].CmdSno;
                            string Loc_ID = dataObject[0].Loc_ID;
                            string NewTrace = "";
                            string CmdMode = dataObject[0].CmdMode;
                            string TaskNo = dataObject[0].TaskNo;
                            string DestinationLayer = "";

                            if (Task.CheckTaskGolevel(cmdSno, out var dataObject4, db).ResultCode == DBResult.Success)
                            {
                                DestinationLayer = dataObject4[0].GoLevel;
                            }
                            string CallLift_Floor = DestinationLayer;




                            #region 根據Trace去做動作分類
                            if (trace == Trace.StoreInLiftOK_CallSHCCarGoinLifter)//寫入命令以及PLC=>寫入樓層以及車子移動完成
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Start Write CMD TO Lifter=> {cmdSno}");

                                if (Plc1.oPLC.PLC[0].Lifter.CMDno.PadLeft(1, '0') == "0")//確認命令符合狀態也確認lifter內沒有命令才寫入
                                {

                                    Result = Plc1.FunWriPLC_Word("DB1.20", Loc_ID);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter Loc_ID  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Word("DB1.22", cmdSno);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter CmdSno(Big)  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Word("DB1.24", "1");//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter TaskNo(small)  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Bit("DB1.26.1", true);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter WriteCMD_Complete  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                }
                                else return false;

                                CarmoveCompleteFloor = Plc1.oPLC.PLC[0].Lifter.LiftPosition.ToString().PadLeft(2, '0');
                                CallLift_Floor = DestinationLayer;//到裝卸層
                                NewTrace = Trace.StoreInSHCCallWCS_CarinLift_ChangeLayer;
                                WriteCMD = true;
                            }
                            else if (trace == Trace.StoreInCallSHCMoveCar)//入庫換成車子所在的那層
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.StoreInSHCcallWCSChangeLifter;
                            }
                            else if (trace == Trace.StoreInLiftAtStorinLevel)//入庫換成入庫儲位的那層
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.StoreInLiftToLoc;
                                CarmoveCompleteFloor = Plc1.oPLC.PLC[0].Lifter.LiftPosition.ToString().PadLeft(2, '0');
                                WriteCMD = true;
                            }
                            else if (trace == Trace.PickUpLifterLevelCorret || (DestinationLayer == "11" && (CmdMode == "2" || CmdMode == "3")))
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Start Write CMD TO Lifter=> {cmdSno}");


                                    Result = Plc1.FunWriPLC_Word("DB1.20", Loc_ID);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter Loc_ID  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Word("DB1.22", cmdSno);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter CmdSno(Big)  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Word("DB1.24", "3");//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter TaskNo(small)  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Bit("DB1.26.1", true);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter WriteCMD_Complete  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                
                                CarmoveCompleteFloor = Plc1.oPLC.PLC[0].Lifter.LiftPosition.ToString().PadLeft(2, '0');
                                CallLift_Floor = DestinationLayer;//到裝卸層
                                NewTrace = Trace.PickUpSHCCallWcsChangeLifterToStorinLevel;
                                WriteCMD = true;

                            }
                            else if (trace == Trace.PickUpStartSHCreport || trace == Trace.PickUpStartCallSHC)//撿料換成車子所在的那層
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.PickUpSHCcallWCSChangeLifter;
                            }
                            else if(trace==Trace.PickUpLifterToStartLevel)
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.PickUpSHCCallGetlotOK_ChangeLayer;
                            }
                            else if (trace == Trace.PickUpLifterToStorinLevel)//撿料換成車子回收的那層
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.PickUpCarReturn;
                                CarmoveCompleteFloor = Plc1.oPLC.PLC[0].Lifter.LiftPosition.ToString().PadLeft(2, '0');
                                WriteCMD = true;
                            }
                            else if (trace == Trace.PickUpLiftOK_CallSHCCarGoinLifter)
                            {
                                if (Plc1.oPLC.PLC[0].Lifter.CMDno.PadLeft(1, '0') == "0")//確認命令符合狀態也確認lifter內沒有命令才寫入
                                {

                                    Result = Plc1.FunWriPLC_Word("DB1.20", Loc_ID);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter Loc_ID  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Word("DB1.22", cmdSno);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter CmdSno(Big)  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Word("DB1.24", "3");//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter TaskNo(small)  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Bit("DB1.26.1", true);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter WriteCMD_Complete  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                }
                                else
                                {

                                }
                                CarmoveCompleteFloor = Plc1.oPLC.PLC[0].Lifter.LiftPosition.ToString().PadLeft(2, '0');
                                WriteCMD = true;
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.PickUpSHCcallWCSChangeLifterToStartLevel;
                            }
                            else if (trace == Trace.LocToLocStartSHCreceive)
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.LocToLocStartSHCcall;
                            }
                            else if (trace == Trace.Loc2LocCallShcLifterSafe)//寫入命令以及PLC=>寫入樓層以及車子移動完成
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Start Write CMD TO Lifter=> {cmdSno}");

                                if (Plc1.oPLC.PLC[0].Lifter.CMDno == "0")//確認命令符合狀態也確認lifter內沒有命令才寫入
                                {

                                    Result = Plc1.FunWriPLC_Word("DB1.20", Loc_ID);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter Loc_ID  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Word("DB1.22", cmdSno);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter CmdSno(Big)  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Word("DB1.24", "5");//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter TaskNo(small)  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    Result = Plc1.FunWriPLC_Bit("DB1.26.1", true);//確認寫入PLC讀取完成的方法是否正常運作

                                    if (Result != true)
                                    {
                                        clsWriLog.LifterLogTrace(0, "Lifter", $"WriteLifter WriteCMD_Complete  Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                }
                                else return false;

                                CarmoveCompleteFloor = Plc1.oPLC.PLC[0].Lifter.LiftPosition.ToString().PadLeft(2, '0');
                                CallLift_Floor = DestinationLayer;//到裝卸層
                                NewTrace = Trace.LocToLocStartSHCcalltoL2Lfloor;
                                WriteCMD = true;
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"{trace}:Can't find Trace,please check{CallLift_Floor}=> {cmdSno},{TaskNo}");
                                return false;
                            }
                            #endregion

                            //此案有許多一樣Change Layer的功能，可以共用只要照SHC的REQ以及命令的類別做區分即可

                            clsWriLog.LifterLogTrace(0, "Lifter", $"Trace:{NewTrace}:Call Lifter Change Layer To{CallLift_Floor}=> {cmdSno},{TaskNo}");
                            CallLift_Floor.PadLeft(2, '0');

                            #region 車子移動完成樓層

                            if (WriteCMD || CallLift_Floor == "11")
                            {
                                if (CarmoveCompleteFloor == "01")
                                {
                                    CarmoveCompleteFloor = CallLifterFloorCarMoveCompleteBits.Floor1;
                                }
                                else if (CarmoveCompleteFloor == "02")
                                {
                                    CarmoveCompleteFloor = CallLifterFloorCarMoveCompleteBits.Floor2;
                                }
                                else if (CarmoveCompleteFloor == "03")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor3);
                                }
                                else if (CarmoveCompleteFloor == "04")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor4);
                                }
                                else if (CarmoveCompleteFloor == "05")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor5);
                                }
                                else if (CarmoveCompleteFloor == "06")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor6);
                                }
                                else if (CarmoveCompleteFloor == "07")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor7);
                                }
                                else if (CarmoveCompleteFloor == "08")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor8);
                                }
                                else if (CarmoveCompleteFloor == "09")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor9);
                                }
                                else if (CarmoveCompleteFloor == "10")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor10);
                                }
                                else if (CarmoveCompleteFloor == "11")
                                {
                                    CarmoveCompleteFloor = (CallLifterFloorCarMoveCompleteBits.Floor11);
                                }
                            }
                            #endregion

                            #region Call車子樓層
                            if (CallLift_Floor == "01")
                            {
                                CallLift_Floor = CallLifterFloorBits.Floor1;
                            }
                            else if (CallLift_Floor == "02")
                            {
                                CallLift_Floor = CallLifterFloorBits.Floor2;
                            }
                            else if (CallLift_Floor == "03")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor3);
                            }
                            else if (CallLift_Floor == "04")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor4);
                            }
                            else if (CallLift_Floor == "05")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor5);
                            }
                            else if (CallLift_Floor == "06")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor6);
                            }
                            else if (CallLift_Floor == "07")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor7);
                            }
                            else if (CallLift_Floor == "08")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor8);
                            }
                            else if (CallLift_Floor == "09")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor9);
                            }
                            else if (CallLift_Floor == "10")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor10);
                            }
                            else if (CallLift_Floor == "11")
                            {
                                CallLift_Floor = (CallLifterFloorBits.Floor11);
                            }
                            else
                            {
                                return false;
                            }
                            #endregion

                            

                            if (WriteCMD || CallLift_Floor == "11")//只有在此類型下才需要做CAR完成移動的PLC交握
                            {
                                Result = Plc1.FunWriPLC_Bit("DB1." + CarmoveCompleteFloor, true);//確認寫入PLC讀取完成的方法是否正常運作

                                if (Result != true)
                                {
                                    clsWriLog.LifterLogTrace(0, "Lifter", $"WritePLC CarmoveCompleteFloor Fail");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                                else
                                {
                                    clsWriLog.LifterLogTrace(0, "Lifter", $"WritePLC " + CarmoveCompleteFloor + " CarmoveCompleteFloor Success");
                                }
                            }

                            Thread.Sleep(500);


                            Result = Plc1.FunWriPLC_Bit("DB1." + CallLift_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作

                            if (Result != true)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"WritePLC CallLifter Fail");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferringFormoveComplete(cmdSno, NewTrace, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (Task.UpdateTaskState(cmdSno, NewTrace, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "lifter", $"Upadte Task succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "lifter", $"Upadte Task fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (Task.UpdateTaskStateGolevel(cmdSno, DestinationLayer, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "lifter", $"Upadte Task level succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "lifter", $"Upadte Task level fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else
                            {
                                _shuttleController?.P85("1", "S", "0000");//Result_Code:0000=Succeess
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

        public bool FunSHC_ChangeLayerReq(clsBufferData Plc1, ChangeLayerEventArgsLayer e)//SHC協定P83呼叫_修改成只更新命令狀態為"需要寫入PLC的訊號，以及接下要去的樓層"
        {
            try
            {

                //這邊需要作流程為與shuttle交握，流程起始點為SHC呼叫WCS=>Change_layer request 
                //WCS呼叫lifter換層 
                string cmdSno="";
                string DestinationLayer = "";
                DestinationLayer = e.DestinationLayer;


                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        if (CMD_MST.CheckCMDLifterOnlyONECMDatAtimeWith_SHCCALL(out var dataObject, db).ResultCode == DBResult.Success) //檢查會唯一在執行的一筆電梯命令
                        {

                            cmdSno = dataObject[0].CmdSno;
                            string Loc = dataObject[0].Loc;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string trace = dataObject[0].Trace;
                            string TaskNo = dataObject[0].CmdSno;
                            string Loc_ID = dataObject[0].Loc_ID;
                            string CallLift_Floor = "";

                            clsWriLog.LifterLogTrace(0, "Lifter", $"Start_Get SHC Chanege Layer Req=> {cmdSno}");
                            CallLift_Floor = DestinationLayer;

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstSHCmovelevel(cmdSno, "True", db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (Task.UpdateTaskStateGolevel(cmdSno, DestinationLayer, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "lifter", $"Upadte Task level succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "lifter", $"Upadte Task level fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            _shuttleController?.S84("0", "0000");//Result_Code:0000=Succeess
                            _shuttleController?.P85("1", "Q", "0000");//Result_Code:0000=Succeess
                           
                            return true;

                        }
                        else {
                            return false;
                        }

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
                //_shuttleController?.S84("1", "0001");//Result_Code:0000=Succeess
                return false;
            }
        }


        public bool FunStoreCarInLifter_ReportSHC(clsBufferData Plc1, int floor)//電梯訊號正確回報SHC電梯安全
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        #region lifter狀態符不符合狀態
                        //if (Plc1.oPLC.PLC[0].Lifter.AllowWriteCommand != true)
                        //{
                        //    return false;
                        //}

                        if (floor == 1)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor1_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor1LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 2)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor2_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor2LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 3)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor3_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor3LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 4)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor4_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor4LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 5)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor5_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor5LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 6)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor6_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor6LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 7)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor7_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor7LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 8)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor8_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor8LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 9)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor9_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor9LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 10)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor10_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.Floor10LocationCheck)//
                            {
                                return false;
                            }
                        }
                        else if (floor == 11)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.Floor11_SafetyCheck != true && !Plc1.oPLC.PLC[0].Lifter.UnloadingLocationCheck)//
                            {
                                return false;
                            }
                        }


                        if (floor == 1)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor1 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 2)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor2 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 3)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor3 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 4)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor4 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 5)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor5 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 6)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor6 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 7)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor7 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 8)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor8 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 9)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor9 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 10)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor10 != false)
                            {
                                return false;
                            }
                        }
                        else if (floor == 11)
                        {
                            if (Plc1.oPLC.PLC[0].Lifter.MoveToFloor11 != false)
                            {
                                return false;
                            }
                        }
                        #endregion

                        if (CMD_MST.CheckCMDLifterOnlyONECMDatAtime(out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {
                            
                            string cmdSno = dataObject[0].CmdSno;
                            string TaskNo = dataObject[0].CmdSno;
                            string Loc_ID = dataObject[0].Loc_ID;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string trace = dataObject[0].Trace;
                            string level = "";

                            if (Task.CheckTaskGolevel(cmdSno,out var dataObject4,db).ResultCode==DBResult.Success)
                            {
                                level = dataObject4[0].GoLevel;

                                if (level != floor.ToString().PadLeft(2,'0'))
                                {
                                    clsWriLog.LifterLogTrace(0, "Lifter", $"Data_Golevel:{level} != Real_LifterLevel:{floor.ToString().PadLeft(2, '0')} => {cmdSno} return false");
                                    return false;
                                }
                                else
                                {

                                }
                            }

                            string NewTrace = "";

                            if (trace == Trace.StoreInSHCcallWCSChangeLifter)
                            {
                                NewTrace = Trace.StoreInLiftOK_CallSHCCarGoinLifter;
                            }
                            else if (trace == Trace.StoreInSHCCallWCS_CarinLift_ChangeLayer)
                            {
                                NewTrace = Trace.StoreInLiftAtStorinLevel;
                            }
                            else if (trace == Trace.StoreInLiftToLoc)
                            {
                                NewTrace = Trace.StoreInLiftToLocLevel;
                            }
                            else if (trace == Trace.PickUpSHCcallWCSChangeLifter)
                            {
                                NewTrace = Trace.PickUpLiftOK_CallSHCCarGoinLifter;
                            }
                            else if (trace == Trace.PickUpSHCcallWCSChangeLifterToStartLevel)
                            {
                                NewTrace = Trace.PickUpLifterToStartLevel;
                            }
                            else if (trace == Trace.PickUpSHCCallWcsChangeLifterToStorinLevel)
                            {
                                NewTrace = Trace.PickUpLifterToStorinLevel;
                                if(floor!=11)
                                {
                                    return false;
                                }
                            }
                            else if(trace== Trace.PickUpSHCCallGetlotOK_ChangeLayer)
                            {
                                NewTrace = Trace.PickUpLifterLevelCorret;
                            }
                            else if (trace == Trace.PickUpCarReturn)
                            {
                                NewTrace = Trace.PickUpCarToReturnCarLevel;
                            }
                            else if(trace==Trace.LocToLocStartSHCcall)
                            {
                                NewTrace=Trace.Loc2LocCallShcLifterSafe;
                            }
                            else if (trace == Trace.LocToLocStartSHCcalltoL2Lfloor)
                            {
                                NewTrace = Trace.LocToLocCallSHCtoL2Lfloor;
                            }
                            else
                            {
                                return false;
                            }


                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, NewTrace, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (Task.UpdateTaskState(cmdSno, NewTrace, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "lifter", $"Upadte Task succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "lifter", $"Upadte Task fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            string strfloor = floor.ToString();

                            
                            

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            //回報shuttle樓層
                            _shuttleController?.P85("1", "C", "0000");//Result_Code:0000=Succeess
                            clsWriLog.LifterLogTrace(0, "Lifter", $"Trace:{NewTrace} => {cmdSno}");



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

        #endregion StoreIn

        #region StoreOut
        public bool FunStoreOutWriPlc1FA2andA4(clsBufferData Plc1)//初始命令，先預約buffer

        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        struCmdDtl stuCmdDtl = new struCmdDtl();

                        if (CMD_MST.GetCmdMstByPickUpStart(out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string Loc_ID = dataObject[0].Loc_ID;
                            string Loc = dataObject[0].Loc;
                            string Loc_DD = "";
                            string Loc_Sts = "";
                            string Lvl_Z = "";
                            string newLoc = "";
                            string TaskNo = "";
                            //先告知shuttle有撿料命令
                            string Stn_no = dataObject[0].StnNo;
                            string sdevice = "";
                            int bufferIndex;
                            bool Result = false;
                            if (Stn_no == ASRS_Setting.STNNO_1F_L)
                            {
                                bufferIndex = 2;
                                sdevice = "3";
                            }
                            else
                            {
                                bufferIndex = 4;
                                sdevice = "5";
                            }

                            if (Loc.Substring(2, 1) == "7")//此案子外儲位為第三位數為7的儲位
                            {
                                if (LocMst.GetLoc_DD(Loc, out var dataObject1, db).ResultCode == DBResult.Success)
                                {
                                    Loc_DD = dataObject1[0].Loc_DD;
                                    clsWriLog.L2LLogTrace(0, "Lifter", $"LOCtoLOC=>Find Loc_DD succeess");
                                }
                                else
                                {
                                    clsWriLog.PickUpLogTrace(0, "Lifter", $"Command Get Loc_DD Fail => {cmdSno}, " +
                                    $"{CmdMode}");
                                    return false;
                                }

                                if (LocMst.GetLoc_DD_Sts(Loc_DD, out var dataObject6, db).ResultCode == DBResult.Success)
                                {
                                    Loc_Sts = dataObject6[0].Loc_Sts;
                                    Lvl_Z = dataObject6[0].LVl_Z;
                                    clsWriLog.L2LLogTrace(0, "Lifter", $"LOCtoLOC=>Find Loc_DD_STS succeess");
                                }
                                else
                                {
                                    clsWriLog.L2LLogTrace(0, "Lifter", $"Command Get Loc_DD_STS Fail => {cmdSno}, " +
                                    $"{CmdMode}");
                                    return false;
                                }
                                if (Loc_Sts == "S")
                                {
                                    if (LocMst.GetEmptyLoc_SameFloor_OutFirst(Lvl_Z, out var dataObject3, db).ResultCode == DBResult.Success)//找尋空儲位放貨物
                                    {
                                        newLoc = dataObject3[0].Loc;
                                        clsWriLog.L2LLogTrace(0, "Lifter", $"LOCtoLOC=>Find Same Floor EmptyLoc succeess");
                                    }
                                    else
                                    {
                                        clsWriLog.L2LLogTrace(0, "Lifter", $"Command Find EmptyLoc Fail => {cmdSno}, " +
                                        $"{CmdMode}");
                                        return false;
                                    }

                                    CmdMstInfo stuCmdMst = new CmdMstInfo();
                                    cmdSno = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSNO, db); //尋找最新不重複的命令號 //dtl記得補充
                                    if (cmdSno == "" || cmdSno == "00000")
                                    {
                                        clsWriLog.L2LLogTrace(0, "Lifter", $"LOCtoLOC=>Find cmdSno fail");
                                        return false;
                                    }
                                    TaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.TaskNO, db); //尋找最新不重複的命令號

                                    if (TaskNo == "" || TaskNo == "0000")
                                    {
                                        clsWriLog.L2LLogTrace(bufferIndex, ""
                                                , $"Find taskNo fail");
                                        return false;
                                    }

                                    
                                    if (LocMst.GetLocdtlDATA(Loc_DD, out var dataObject5, db).ResultCode == DBResult.Success) //讀取_loc_dtl
                                    {
                                        string loctxno = SNO.FunGetSeqNo(clsEnum.enuSnoType.LOCTXNO, db); //尋找最新不重複的命令號 //dtl記得補充
                                        if (loctxno == "" || loctxno == "00000")
                                        {
                                            clsWriLog.L2LLogTrace(0, "Lifter", $"LOCtoLOC=>Find loctxno fail");
                                            return false;
                                        }

                                        stuCmdMst.CmdSno = cmdSno;
                                        stuCmdMst.CmdSts = "0";
                                        stuCmdMst.CmdMode = "5";
                                        stuCmdMst.Prt = "1";
                                        stuCmdMst.IoType = "51";
                                        stuCmdMst.Loc = Loc_DD;
                                        stuCmdMst.NewLoc = newLoc;
                                        stuCmdMst.taskNo = TaskNo;
                                        stuCmdMst.StnNo = "5";
                                        stuCmdMst.CrtDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        stuCmdMst.TrnUser = "WCS";
                                        stuCmdMst.Loc_ID = dataObject5[0].Loc_ID;



                                        stuCmdDtl.LOC_Txno = dataObject5[0].Loc_Txno;
                                        stuCmdDtl.Cmd_Sno = cmdSno;
                                        stuCmdDtl.Cmd_Txno = dataObject5[0].Loc_Txno;
                                        stuCmdDtl.In_Date = dataObject5[0].IN_DATE;
                                        stuCmdDtl.TRN_Date = dataObject5[0].TRN_DATE;
                                        stuCmdDtl.CYCLE_Date = dataObject5[0].CYCLE_DATE;
                                        stuCmdDtl.Plt_Qty = Convert.ToDouble(dataObject5[0].PLT_QTY);
                                        stuCmdDtl.ALO_Qty = Convert.ToDouble(dataObject5[0].ALO_QTY);
                                        stuCmdDtl.Tkt_NO = dataObject5[0].IN_TKT_NO;
                                        stuCmdDtl.Lot_No = dataObject5[0].LOT_NO;
                                        stuCmdDtl.Store_CODE = dataObject5[0].STORE_CODE;
                                        stuCmdDtl.BANK_CODE = dataObject5[0].BANK_CODE;
                                        stuCmdDtl.QC_CODE = dataObject5[0].QC_CODE;
                                        stuCmdDtl.Item_TYPE = dataObject5[0].ITEM_TYPE;
                                        stuCmdDtl.expire_date = dataObject5[0].EXPIRE_DATE;
                                    }

                                    if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.L2LLogTrace(0, "Lifter", "loctoloc_begin fail");
                                        return false;
                                    }
                                    if (CMD_MST.FunInsCmdMst(stuCmdMst, db).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.L2LLogTrace(0, "Lifter", $"LocToLocCMDInsert Fail!");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if (CMD_MST.FunInsCmdDtl(stuCmdDtl, db).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.L2LLogTrace(0, "Lifter", $"LocToLocCMD_DTLInsert Fail!");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                    if (Task.StorIninsertTaskstart("0000", 1, cmdSno, TaskNo, Trace.LocToLocStart, Loc_DD, newLoc, db).ResultCode == DBResult.Success)
                                    {
                                        clsWriLog.L2LLogTrace(bufferIndex, "", $"Insert Taskcmd succeess => {cmdSno}");
                                    }
                                    else
                                    {
                                        clsWriLog.L2LLogTrace(bufferIndex, "", $"Insert Taskcmd fail => {cmdSno}");

                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if (LocMst.UpdateStoreOutLocMst(Loc_DD, db).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.L2LLogTrace(0, "Lifter", $"LocToLoc_LocMSTOutupdate Fail!");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if (LocMst.UpdateStoreInLocMst(newLoc, db).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.L2LLogTrace(0, "Lifter", $"LocToLoc_LocMSTInupdate Fail!");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.L2LLogTrace(0, "Lifter", "Loctoloc_Commit Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }

                                }
                                else if (Loc_Sts != "N")
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.Loc_DDexistSTS_S, db);
                                    return false;
                                }
                            }

                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Buffer Get PickUp Command => {cmdSno}, " +
                                    $"{CmdMode}");

                            #region//根據buffer狀態更新命令
                            if (Plc1.oPLC.PLC[bufferIndex].CV.CV_Status != 2)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotPickUpMode, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[bufferIndex].CV.Sno != "0")
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.ExistCmdSno, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[bufferIndex].CV.AutoManual != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotAutoMode, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[bufferIndex].CV.Presence != false)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceExist, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[bufferIndex].CV.idle != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotIdle, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[0].Lifter.LiftMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.LifterNotAutoMode, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[0].Lifter.presence_shuttle == true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.PresenceShuttle, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[0].Lifter.LiftIdle != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.LifterNotIdleMode, db);
                                return false;
                            }
                            #endregion

                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Buffer Ready Receive PickUp Command=> {cmdSno}");

                            //接收到撿料命令，以及Buffer符合狀態，，也同時可先預約buffer



                            TaskNo = SNO.FunGetSeqNo(clsEnum.enuSnoType.TaskNO, db); //尋找最新不重複的命令號

                            if (TaskNo == "" || TaskNo == "0000")
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, Stn_no
                                        , $"Find taskNo fail");
                                return false;
                            }

                            //預約buffer
                            Result = Plc1.FunWriPLC_Word("DB" + sdevice + ".0.0", cmdSno);//確認寫入PLC命令的方法是否正常運作

                            if (Result != true)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"WritePLC CV_Command Fail");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            Result = Plc1.FunWriPLC_Word("DB" + sdevice + ".2.0", CmdMode.ToString());//確認寫入PLC模式的方法是否正常運作

                            if (Result != true)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"WritePLC CV_mode Fail");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            Result = Plc1.FunWriPLC_Bit("DB" + sdevice + ".6.1.0", true);//確認寫入PLC模式的方法是否正常運作

                            if (Result != true)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"WritePLC CV_mode Fail");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }



                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "begin fail");
                                return false;
                            }

                            if (CMD_MST.UpdateCmdMstTransferringstart(cmdSno, Trace.PickUpStart, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (Task.CheckTaskInsert(cmdSno, out var dataObject2, db).ResultCode != DBResult.Success)//避免重複insert設備命令
                            {

                                if (Task.StorIninsertTaskstart("0000", 1, cmdSno, TaskNo, Trace.PickUpStart, Loc, Stn_no, db).ResultCode == DBResult.Success)
                                {
                                    clsWriLog.PickUpLogTrace(bufferIndex, Stn_no, $"Insert Taskcmd succeess => {cmdSno}");
                                }
                                else
                                {
                                    clsWriLog.PickUpLogTrace(bufferIndex, Stn_no, $"Insert Taskcmd fail => {cmdSno}");

                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
                            }

                            if (CMD_MST.UpdateCmdMstTaskNo(cmdSno, TaskNo, db).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, Stn_no, $"CMDmst Upadte TaskNo fail => {cmdSno}");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else return true;

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

        public bool FunStoreOutStart_GiveSHCCommand(clsBufferData Plc1)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.CheckCMDLifterOnlyONECMDAtime_StoreOut(out var dataObject4, db).ResultCode == DBResult.Success)//對於此案電梯的流程一次只會執行一筆命令，所以要做卡控的動作
                        {
                            return false;
                        }

                        if (CMD_MST.GetCmdMstPickUpStart(out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string Loc_ID = dataObject[0].Loc_ID;
                            string Loc = dataObject[0].Loc;
                            string Stn_no = dataObject[0].StnNo;
                            string TaskNo = dataObject[0].TaskNo;
                            int bufferIndex = 0;

                            //先告知shuttle有撿料命令
                            string BCRstnno = "";
                            if (Stn_no == ASRS_Setting.STNNO_1F_L)
                            {
                                BCRstnno = ASRS_Setting.STNNO_1F_left;                              
                                bufferIndex = 2;
                            }
                            else
                            {
                                BCRstnno = ASRS_Setting.STNNO_1F_right;
                                bufferIndex = 4;
                            }


                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "begin fail");
                                return false;
                            }

                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.PickUpStartCallSHC, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else
                            {
                                _shuttleCommand = new ShuttleCommand(TaskNo, "H", 1, Loc, BCRstnno, Loc_ID, "0000");//撿料命令 待修改參數 儲位由WMS給 箱子號為原本資料表得到，vehicle固定0000
                                _shuttleController?.CreateShuttleCommand(_shuttleCommand);
                                return true;
                            };

                        }
                        else
                        {
                            CMD_MST.UpdateCmdMstTransferringforStoreOut10sec(Trace.PickUpStart, db);
                            //clsWriLog.StoreOutLogTrace(0, "", "10 sec PASS no SHC reply,return trace to last step");
                            return true;
                        }

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


        public bool FunDoubleStorage_GiveSHCCommand(clsBufferData Plc1)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        if (CMD_MST.GetDouble_StorageCMD(out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string Loc_ID = dataObject[0].Loc_ID;
                            string Loc = dataObject[0].Loc;
                            string newLoc = dataObject[0].NewLoc;
                            string Stn_no = dataObject[0].StnNo;
                            string TaskNo = dataObject[0].TaskNo;
                            int bufferIndex = 0;
                            string vehicle_ID = "";

                            if(CmdMode==5)
                            {
                                Loc = newLoc;
                            }
                                

                            Task.CheckTaskInsert(cmdSno,out var dataObject2, db);

                            vehicle_ID=dataObject2[0].vehicle_ID;


                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "begin fail");
                                return false;
                            }

                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.DoubleStorageSHCcall, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Upadte cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else
                            {
                                _shuttleCommand = new ShuttleCommand(TaskNo, "H", 1, "", Loc , Loc_ID, vehicle_ID);//撿料命令 待修改參數 儲位由WMS給 箱子號為原本資料表得到，vehicle固定0000
                                _shuttleController?.CreateShuttleCommand(_shuttleCommand);
                                return true;
                            };

                        }
                        else
                        {
                            CMD_MST.UpdateCmdMstTransferringforDouble10sec(Trace.DoubleStorageStart, db);
                            //clsWriLog.StoreOutLogTrace(0, "", "10 sec PASS no SHC reply,return trace to last step");
                            return true;
                        }

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

        public bool FunL2L(clsBufferData Plc1)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        //if (CMD_MST.UpdateCmdFortesting(db).ResultCode == DBResult.Success)
                        //{
                        //    CMD_MST.UpdateCmdLOCFortesting(db);
                        //}


                        if (CMD_MST.GetLocToLoc(out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;

                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string Loc = dataObject[0].Loc;
                            string NewLoc = dataObject[0].NewLoc;
                            string Loc_ID = dataObject[0].Loc_ID;
                            string TaskNo= dataObject[0].TaskNo;

                            clsWriLog.L2LLogTrace(5, "L2L", $"Get L2L Command => {cmdSno}, " +
                                       $"{CmdMode}");


                            #region//根據buffer狀態更新命令
                            if (Plc1.oPLC.PLC[0].Lifter.LiftMode != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.LifterNotAutoMode, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[0].Lifter.LiftIdle != true)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.LifterNotIdleMode, db);
                                return false;
                            }
                            #endregion

                            clsWriLog.L2LLogTrace(5, "Lifter", $"Ready Receive L2L Command=> {cmdSno}");

                            //接收到庫對庫命令

                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(5, "1F", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferringLocstart(cmdSno, Trace.LocToLocStart, db).ResultCode == DBResult.Success)//庫對庫流程
                            {
                                clsWriLog.L2LLogTrace(5, "1F", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.L2LLogTrace(5, "1F", $"Upadte cmd fail => {cmdSno}");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(0, "Lifter", "Loctoloc_Commit Fail");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            else
                            {
                                _shuttleCommand = new ShuttleCommand(TaskNo, "H", 1, Loc, NewLoc, Loc_ID, "0000");//庫對庫命令 待修改參數 儲位由WMS給 箱子號為原本資料表得到，vehicle固定0000
                                _shuttleController?.CreateShuttleCommand(_shuttleCommand);
                                return true; 
                            }

                        }
                        else 
                        {
                             CMD_MST.UpdateCmdMstTransferringforL2L10sec("0", db);
                             //clsWriLog.L2LLogTrace(0, "", "10 sec PASS no SHC reply,return trace to last step");
                             return true;
                            
                        };

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
