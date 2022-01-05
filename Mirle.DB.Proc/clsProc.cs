using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
        private static OEEParamConfig _config_OEEParam = new OEEParamConfig();
        public clsProc(clsDbConfig config, clsDbConfig config_WMS, clsDbConfig config_Sqlite, OEEParamConfig config_OEEParam)
        {
            _config = config;
            _config_WMS = config_WMS;
            _config_Sqlite = config_Sqlite;
            _config_OEEParam = config_OEEParam;
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

    }
}
