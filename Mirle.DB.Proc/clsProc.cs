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
                        if(TraceBCR=="A")
                        {
                            bufferIndex = 1;
                        }
                        else if(TraceBCR=="B")
                        {
                            bufferIndex= 2;
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
                        if (TraceBCR == "A")
                        {
                            bufferIndex = 1;
                        }
                        else if (TraceBCR == "B")
                        {
                            bufferIndex = 2;
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
                        if (Plc1.oPLC.PLC[bufferIndex].CV.ReadBCR != true)
                        {
                            CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.BCRReadNotON, db);
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
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, TraceBCR, db).ResultCode == DBResult.Success)
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

                            return false;
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
                            if (Task.StorIninsertCMDstart("0000",cmdSno,TaskNo, Trace.StoreInWCScommandReportSHC, ASRS_Setting.STNNO_1F_left, Loc, db).ResultCode == DBResult.Success)
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Insert Taskcmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.StoreInLogTrace(bufferIndex, sStnNo, $"Insert cmd fail => {cmdSno}");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
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
                                _shuttleCommand = new ShuttleCommand(TaskNo, "H", 1, ASRS_Setting.STNNO_1F_left, Loc, Loc_ID, "0000");//入庫待修改參數 儲位由WMS給 箱子號為掃bCR得到，vehicle固定0000
                                _shuttleController?.CreateShuttleCommand(_shuttleCommand);
                                return true;
                            } 

                        }
                        else
                        {
                            CMD_MST.UpdateCmdMstTransferring10sec(Sno,Trace.StoreInWriteCmdToCV,db);
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

        public bool FunStoreInFA1andA3CallLifterAndSHCheckcommand(clsBufferData Plc1, string sStnNo, int bufferIndex, CommandReceiveEventArgs e)
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

        public bool FunSHC_ChangeLayerReq(clsBufferData Plc1, ChangeLayerEventArgsLayer e)//SHC協定P83呼叫
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
                            bool Result;
                            string CallLift_Floor = "";
                            string CarmoveCompleteFloor = "";
                            string NewTrace = "";
                            string loc_destination = Loc.Substring(8, 1);
                            bool WriteCMD = false;

                            clsWriLog.LifterLogTrace(0, "Lifter", $"Start_Get SHC Chanege Layer Req=> {cmdSno}");

                            #region 根據Trace去做動作分類
                            if (trace == Trace.StoreInLiftOK_CallSHCCarGoinLifter)//寫入命令以及PLC=>寫入樓層以及車子移動完成
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

                                    Result = Plc1.FunWriPLC_Word("DB1.24", TaskNo);//確認寫入PLC讀取完成的方法是否正常運作

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
                            }
                            else if (trace == Trace.PickUpLifterToStartLevel || (DestinationLayer == "11" && (CmdMode == 2 || CmdMode == 3)))
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"Start Write CMD TO Lifter=> {cmdSno}");

                                if (Plc1.oPLC.PLC[0].Lifter.CMDno == "")//確認命令符合狀態也確認lifter內沒有命令才寫入
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

                                    Result = Plc1.FunWriPLC_Word("DB1.24", TaskNo);//確認寫入PLC讀取完成的方法是否正常運作

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
                                NewTrace = Trace.PickUpSHCCallWcsChangeLifterToStorinLevel;
                                WriteCMD = true;

                            }
                            else if (trace == Trace.PickUpStart)//撿料換成車子所在的那層
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.PickUpSHCcallWCSChangeLifter;
                            }
                            else if (trace == Trace.PickUpLifterToStorinLevel)//撿料換成車子回收的那層
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.PickUpCarReturn;
                            }
                            else if (trace == Trace.PickUpLiftOK_CallSHCCarGoinLifter)
                            {
                                CallLift_Floor = DestinationLayer;
                                NewTrace = Trace.PickUpSHCcallWCSChangeLifterToStartLevel;
                            }
                            else
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"{trace}:Can't find Trace,please check{CallLift_Floor}=> {cmdSno},{TaskNo}");
                                return false;
                            }
                            #endregion

                            //此案有許多一樣Change Layer的功能，可以共用只要照SHC的REQ以及命令的類別做區分即可

                            clsWriLog.LifterLogTrace(0, "Lifter", $"{trace}:Call Lifter Change Layer To{CallLift_Floor}=> {cmdSno},{TaskNo}");
                            CallLift_Floor.PadLeft(2, '0');

                            #region 車子移動完成樓層

                            if (WriteCMD || CallLift_Floor == "01")
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
                            #endregion

                            Result = Plc1.FunWriPLC_Bit("DB1." + CallLift_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作

                            if (Result != true)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", $"WritePLC CallLifter Fail");
                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            if (WriteCMD || CallLift_Floor == "11")//只有在此類型下才需要做CAR完成移動的PLC交握
                            {
                                Result = Plc1.FunWriPLC_Bit("DB1." + CarmoveCompleteFloor, true);//確認寫入PLC讀取完成的方法是否正常運作

                                if (Result != true)
                                {
                                    clsWriLog.LifterLogTrace(0, "Lifter", $"WritePLC CarmoveCompleteFloor Fail");
                                    db.TransactionCtrl2(TransactionTypes.Rollback);
                                    return false;
                                }
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

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }
                            _shuttleController?.S84("0", "0000");//Result_Code:0000=Succeess
                            _shuttleController?.P85("1", "Q", "0000");//Result_Code:0000=Succeess
                            _shuttleController?.P85("1", "S", "0000");//Result_Code:0000=Succeess
                            return true;

                        }
                        else {
                            //_shuttleController?.S84("1", "0001");//Result_Code:0000=Succeess
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


      

        public bool FunStoreCarInLifter_ReportSHC(clsBufferData Plc1, int floor)//這邊寫入命令跟入庫跟出庫一樣的觸發條件所以可以共用，去找符合命令符合條件的
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        #region lifter狀態符不符合狀態
                        if (Plc1.oPLC.PLC[0].Lifter.AllowWriteCommand != true)
                        {
                            return false;
                        }

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

                           

                            if (CmdMode == 1)
                            {
                                clsWriLog.StoreInLogTrace(0, "Lifter", $"Start Respone Shuttle Car Lifter Floor=> {cmdSno}, " + $"{CmdMode}");
                            }
                            else if (CmdMode == 2)
                            {
                                clsWriLog.StoreOutLogTrace(0, "Lifter", $"Start Respone Shuttle Car Lifter Floor=> {cmdSno}, " + $"{CmdMode}");
                            }
                            else if (CmdMode == 3)
                            {
                                clsWriLog.PickUpLogTrace(0, "Lifter", $"Start Respone Shuttle Car Lifter Floor=> {cmdSno}, " + $"{CmdMode}");
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
                            }
                            else if (trace == Trace.PickUpCarReturn)
                            {
                                NewTrace = Trace.PickUpCarToReturnCarLevel;
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
                            string CallLift_Floor = "";
                            string Complete_Floor = "";
                            string strfloor = floor.ToString();

                            if (NewTrace == Trace.StoreInLiftToLocLevel)
                            {
                                //命令結束要做特殊結尾，更新為九以及要多寫觸發PLC的點位讓電梯PLC清直
                                CMD_MST.UpdateCmdMstEnd(cmdSno, ASRS.WCS.Model.DataAccess.CmdSts.CompleteWaitUpdate, NewTrace, db);
                                Task.UpdateTaskStateEnd(cmdSno, NewTrace, db);
                                #region 車子移動完成樓層

                                if (strfloor == "1")
                                {
                                    Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor1;
                                }
                                else if (strfloor == "2")
                                {
                                    Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor2;
                                }
                                else if (strfloor == "3")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor3);
                                }
                                else if (strfloor == "4")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor4);
                                }
                                else if (strfloor == "5")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor5);
                                }
                                else if (strfloor == "6")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor6);
                                }
                                else if (strfloor == "7")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor7);
                                }
                                else if (strfloor == "8")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor8);
                                }
                                else if (strfloor == "9")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor9);
                                }
                                else if (strfloor == "10")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor10);
                                }
                                #endregion

                                #region Call車子樓層
                                if (strfloor == "1")
                                {
                                    CallLift_Floor = CallLifterFloorBits.Floor1;
                                }
                                else if (strfloor == "2")
                                {
                                    CallLift_Floor = CallLifterFloorBits.Floor2;
                                }
                                else if (strfloor == "3")
                                {
                                    CallLift_Floor = (CallLifterFloorBits.Floor3);
                                }
                                else if (strfloor == "4")
                                {
                                    CallLift_Floor = (CallLifterFloorBits.Floor4);
                                }
                                else if (strfloor == "5")
                                {
                                    CallLift_Floor = (CallLifterFloorBits.Floor5);
                                }
                                else if (strfloor == "6")
                                {
                                    CallLift_Floor = (CallLifterFloorBits.Floor6);
                                }
                                else if (strfloor == "7")
                                {
                                    CallLift_Floor = (CallLifterFloorBits.Floor7);
                                }
                                else if (strfloor == "8")
                                {
                                    CallLift_Floor = (CallLifterFloorBits.Floor8);
                                }
                                else if (strfloor == "9")
                                {
                                    CallLift_Floor = (CallLifterFloorBits.Floor9);
                                }
                                else if (strfloor == "10")
                                {
                                    CallLift_Floor = (CallLifterFloorBits.Floor10);
                                }
                                #endregion

                                bool Result;

                                Result = Plc1.FunWriPLC_Bit("DB" + Complete_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作

                                Result = Plc1.FunWriPLC_Bit("DB" + CallLift_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作

                            }

                            if (NewTrace == Trace.PickUpCarToReturnCarLevel)
                            {
                                //命令結束要做特殊結尾，更新為七以及要多寫觸發PLC的點位讓電梯PLC清直
                                CMD_MST.UpdateCmdMstEnd(cmdSno, ASRS.WCS.Model.DataAccess.CmdSts.CompleteWaitUpdate, NewTrace, db);
                                Task.UpdateTaskStateEnd(cmdSno, NewTrace, db);
                                bool Result;

                                #region 車子移動完成樓層
                               

                                if (strfloor == "1")
                                {
                                    Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor1;
                                }
                                else if (strfloor == "2")
                                {
                                    Complete_Floor = CallLifterFloorCarMoveCompleteBits.Floor2;
                                }
                                else if (strfloor == "3")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor3);
                                }
                                else if (strfloor == "4")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor4);
                                }
                                else if (strfloor == "5")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor5);
                                }
                                else if (strfloor == "6")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor6);
                                }
                                else if (strfloor == "7")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor7);
                                }
                                else if (strfloor == "8")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor8);
                                }
                                else if (strfloor == "9")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor9);
                                }
                                else if (strfloor == "10")
                                {
                                    Complete_Floor = (CallLifterFloorCarMoveCompleteBits.Floor10);
                                }
                                #endregion
                                

                                Result = Plc1.FunWriPLC_Bit("DB" + Complete_Floor, true);//確認寫入PLC讀取完成的方法是否正常運作

                            }
                            

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "Commit Fail");

                                db.TransactionCtrl2(TransactionTypes.Rollback);
                                return false;
                            }

                            //回報shuttle樓層
                            _shuttleController?.P85("1", "C", "0000");//Result_Code:0000=Succeess

                            


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
        public bool FunStoreOutWriPlc1FA2andA4(clsBufferData Plc1, int bufferIndex)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        if (CMD_MST.CheckCMDLifterOnlyONECMDAtime(out var dataObject4, db).ResultCode == DBResult.Success)//對於此案電梯的流程一次只會執行一筆命令，所以要做卡控的動作
                        {
                            return false;
                        }

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

                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Buffer Get Pick Up Command => {cmdSno}, " +
                                       $"{CmdMode}");

                            if (Loc.Substring(6, 1) == "1")//此案子外儲位為第六位數為一的儲位
                            {
                                if (LocMst.GetLoc_DD(Loc, out var dataObject1, db).ResultCode == DBResult.Success)
                                {
                                    Loc_DD = dataObject1[0].Loc_DD;
                                    clsWriLog.PickUpLogTrace(0, "Lifter", $"LOCtoLOC=>Find Loc_DD succeess");
                                }
                                else
                                {
                                    clsWriLog.PickUpLogTrace(0, "Lifter", $"Command Get Loc_DD Fail => {cmdSno}, " +
                                    $"{CmdMode}");
                                    return false; }

                                if(LocMst.GetLoc_DD_Sts(Loc_DD,out var dataObject2,db).ResultCode==DBResult.Success)
                                {
                                    Loc_Sts = dataObject2[0].Loc_Sts;
                                    Lvl_Z = dataObject2[0].LVl_Z;
                                    clsWriLog.PickUpLogTrace(0, "Lifter", $"LOCtoLOC=>Find Loc_DD_STS succeess");
                                }
                                else
                                {
                                    clsWriLog.PickUpLogTrace(0, "Lifter", $"Command Get Loc_DD_STS Fail => {cmdSno}, " +
                                    $"{CmdMode}");
                                    return false;
                                }
                                if (Loc_Sts=="S")
                                {
                                    if (LocMst.GetEmptyLoc_SameFloor_OutFirst(Lvl_Z, out var dataObject3, db).ResultCode == DBResult.Success)//找尋空儲位放貨物
                                    {
                                        newLoc = dataObject3[0].Loc;
                                        clsWriLog.PickUpLogTrace(0, "Lifter", $"LOCtoLOC=>Find Same Floor EmptyLoc succeess");
                                    }
                                    else
                                    {
                                        clsWriLog.PickUpLogTrace(0, "Lifter", $"Command Find EmptyLoc Fail => {cmdSno}, " +
                                        $"{CmdMode}");
                                        return false;
                                    }

                                    CmdMstInfo stuCmdMst = new CmdMstInfo();
                                    cmdSno = SNO.FunGetSeqNo(clsEnum.enuSnoType.CMDSNO, db); //尋找最新不重複的命令號 //dtl記得補充
                                    if (cmdSno == "" || cmdSno == "00000")
                                    {
                                        clsWriLog.PickUpLogTrace(0,"Lifter" , $"LOCtoLOC=>Find cmdSno fail");
                                        return false;
                                    }
                                    stuCmdMst.CmdSno = cmdSno;
                                    stuCmdMst.CmdSts = "0";
                                    stuCmdMst.CmdMode = "5";
                                    stuCmdMst.Prt = "1";
                                    stuCmdMst.IoType = "51";
                                    stuCmdMst.Loc = Loc_DD;
                                    stuCmdMst.NewLoc = newLoc;
                                    stuCmdMst.CrtDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    stuCmdMst.TrnUser = "WCS";

                                    if (LocMst.GetLocdtlDATA(Loc,out var dataObject5, db).ResultCode == DBResult.Success) //讀取_loc_dtl
                                    {
                                        string loctxno = SNO.FunGetSeqNo(clsEnum.enuSnoType.LOCTXNO, db); //尋找最新不重複的命令號 //dtl記得補充
                                        if (loctxno == "" || loctxno == "00000")
                                        {
                                            clsWriLog.PickUpLogTrace(0, "Lifter", $"LOCtoLOC=>Find loctxno fail");
                                            return false;
                                        }
                                        string CMD_TXNO = SNO.FunGetSeqNo(clsEnum.enuSnoType.WCSTrxNo, db); //尋找最新不重複的命令號 //dtl記得補充
                                        if (CMD_TXNO == "" || CMD_TXNO == "00000")
                                        {
                                            clsWriLog.PickUpLogTrace(0, "Lifter", $"LOCtoLOC=>Find CMD_TXNO fail");
                                            return false;
                                        }

                                        
                                        stuCmdDtl.LOC_Txno = loctxno;
                                        stuCmdDtl.Cmd_Sno = cmdSno;
                                        stuCmdDtl.Cmd_Txno = CMD_TXNO;
                                        stuCmdDtl.In_Date = dataObject5[0].IN_DATE;
                                        stuCmdDtl.TRN_Date= dataObject5[0].TRN_DATE;
                                        stuCmdDtl.CYCLE_Date= dataObject5[0].CYCLE_DATE;
                                        stuCmdDtl.Plt_Qty = Convert.ToDouble(dataObject5[0].PLT_QTY);
                                        stuCmdDtl.ALO_Qty = Convert.ToDouble(dataObject5[0].ALO_QTY);
                                        stuCmdDtl.Tkt_NO = dataObject5[0].IN_TKT_NO;
                                        stuCmdDtl.Lot_No = dataObject5[0].LOT_NO;
                                        stuCmdDtl.Store_CODE= dataObject5[0].STORE_CODE;
                                        stuCmdDtl.BANK_CODE= dataObject5[0].BANK_CODE;
                                        stuCmdDtl.QC_CODE= dataObject5[0].QC_CODE;
                                        stuCmdDtl.Item_TYPE = dataObject5[0].ITEM_TYPE;  
                                        stuCmdDtl.expire_date = dataObject5[0].EXPIRE_DATE;
                                    }

                                        if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.PickUpLogTrace(0, "Lifter", "loctoloc_begin fail");
                                        return false;
                                    }
                                    if (CMD_MST.FunInsCmdMst(stuCmdMst, db).ResultCode!=DBResult.Success)
                                    {
                                        clsWriLog.PickUpLogTrace(0, "Lifter", $"LocToLocCMDInsert Fail!");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if (CMD_MST.FunInsCmdDtl(stuCmdDtl, db).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.PickUpLogTrace(0, "Lifter", $"LocToLocCMD_DTLInsert Fail!");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if (LocMst.UpdateStoreOutLocMst(Loc_DD, db).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.PickUpLogTrace(0, "Lifter", $"LocToLoc_LocMSTOutupdate Fail!");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if (LocMst.UpdateStoreInLocMst(newLoc, db).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.PickUpLogTrace(0, "Lifter", $"LocToLoc_LocMSTInupdate Fail!");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    if(db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                                    {
                                        clsWriLog.PickUpLogTrace(0, "Lifter", "Loctoloc_Commit Fail");
                                        db.TransactionCtrl2(TransactionTypes.Rollback);
                                        return false;
                                    }
                                    //_shuttleCommand = new ShuttleCommand(cmdSno, "A", 1, Loc_DD, newLoc, "BOX_ID", "0000");//庫對褲命令 待修改參數 箱子號為原本資料表得到，vehicle固定0000
                                    //_shuttleController?.CreateShuttleCommand(_shuttleCommand);

                                }
                                if(Loc_Sts!="N")
                                {
                                    CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.Loc_DDexistSTS_S, db);
                                    return false;
                                }
                            }

                            clsWriLog.PickUpLogTrace(bufferIndex, "1F", $"Buffer Get PickUp Command => {cmdSno}, " +
                                    $"{CmdMode}");

                            #region//根據buffer狀態更新命令
                            if (Plc1.oPLC.PLC[bufferIndex].CV.Mode != 2)
                            {
                                CMD_MST.UpdateCmdMstRemark(cmdSno, Remark.NotPickUpMode, db);
                                return false;
                            }
                            if (Plc1.oPLC.PLC[bufferIndex].CV.Sno != "")
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


                            //先告知shuttle有撿料命令
                            
                            _shuttleCommand = new ShuttleCommand(cmdSno, "H", 1, Loc, ASRS_Setting.STNNO_1F_left, Loc_ID, "0000");//撿料命令 待修改參數 儲位由WMS給 箱子號為原本資料表得到，vehicle固定0000
                            _shuttleController?.CreateShuttleCommand(_shuttleCommand);


                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.PickUpLogTrace(bufferIndex, "1F", "begin fail");
                                return false;
                            }

                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, Trace.PickUpStart, db).ResultCode == DBResult.Success)
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

        public bool FunPickUpCmdWritePLC(clsBufferData Plc1,int bufferIndex)//SHCrecevie完成，開始寫入PLC程序
        {
            try
            {

                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        if (CMD_MST.GetCMDBYtrace(Trace.PickUpStartSHCreport,out var dataObject, db).ResultCode == DBResult.Success) //檢查會唯一在執行的一筆電梯命令
                        {

                            string cmdSno = dataObject[0].CmdSno;
                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            bool Result;
                            string NewTrace = "";
                            NewTrace = Trace.PickUpStartSHCreportcmplete_WritePLC;

                            string sdevice = "";
                            if (bufferIndex == 2)
                            {
                                sdevice = "3";
                            }
                            else if (bufferIndex == 4)
                            {
                                sdevice = "5";
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

                            if (db.TransactionCtrl2(TransactionTypes.Commit).ResultCode != DBResult.Success)
                            {
                                clsWriLog.LifterLogTrace(0, "Lifter", "Commit Fail");

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


        #endregion StoreOut


        public bool FunL2L(clsBufferData Plc1)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {

                        if (CMD_MST.GetLocToLoc(out var dataObject, db).ResultCode == DBResult.Success) //讀取CMD_MST
                        {

                            string cmdSno = dataObject[0].CmdSno;

                            int CmdMode = Convert.ToInt32(dataObject[0].CmdMode);
                            string Loc = dataObject[0].Loc;
                            string NewLoc = dataObject[0].NewLoc;
                            string Loc_ID = dataObject[0].Loc_ID;

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


                            //先告知shuttle有庫對庫命令


                            _shuttleCommand = new ShuttleCommand(cmdSno, "A", 1, Loc, NewLoc, Loc_ID, "0000");//庫對庫命令 待修改參數 儲位由WMS給 箱子號為原本資料表得到，vehicle固定0000
                            _shuttleController?.CreateShuttleCommand(_shuttleCommand);


                            if (db.TransactionCtrl2(TransactionTypes.Begin).ResultCode != DBResult.Success)
                            {
                                clsWriLog.L2LLogTrace(5, "1F", "begin fail");
                                return false;
                            }
                            if (CMD_MST.UpdateCmdMstTransferring(cmdSno, "", db).ResultCode == DBResult.Success)//庫對庫流程
                            {
                                clsWriLog.L2LLogTrace(5, "1F", $"Upadte cmd succeess => {cmdSno}");
                            }
                            else
                            {
                                clsWriLog.L2LLogTrace(5, "1F", $"Upadte cmd fail => {cmdSno}");

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
    }
}
