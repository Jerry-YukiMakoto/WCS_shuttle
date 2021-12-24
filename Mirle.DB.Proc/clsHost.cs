using System.Collections.Generic;
using Mirle.DataBase;
using Mirle.Def;

namespace Mirle.DB.Proc
{
    public class clsHost
    {
        private readonly clsProc Process;
        private readonly clsCmd_Mst CMD_MST;
        private readonly clsSno SNO;
        private readonly clsLocMst LocMst;
        //private readonly clsTask task;
        private readonly clsAlarmData ALARMDATA;
        private readonly clsCmd_Mst_His CMD_MST_HIS;
        private readonly clsUnitStsLog unitStsLog;
        private readonly clsUnitModeDef unitModeDef;
        private readonly clsL2LCount L2LCount;
        private static object _Lock = new object();
        private static bool _IsConn = false;
        public static bool IsConn
        {
            get { return _IsConn; }
            set
            {
                lock(_Lock)
                {
                    _IsConn = value;
                }
            }
        }

        private static clsDbConfig _config_Sqlite = new clsDbConfig
        {
            DBType = DBTypes.SQLite
        };

        public clsHost(clsDbConfig config, clsDbConfig config_WMS, string DB_Name_Sqlite, OEEParamConfig config_OEEParam)
        {
            _config_Sqlite.DbName = DB_Name_Sqlite;
            Process = new clsProc(config, config_WMS, _config_Sqlite, config_OEEParam);
            CMD_MST = new clsCmd_Mst(config);
            SNO = new clsSno(config);
            LocMst = new clsLocMst(config);
            //task = new clsTask(config, config_WMS, _config_Sqlite);
            ALARMDATA = new clsAlarmData(config);
            CMD_MST_HIS = new clsCmd_Mst_His(config);
            unitStsLog = new clsUnitStsLog(config);
            unitModeDef = new clsUnitModeDef(config);
            L2LCount = new clsL2LCount(config);
        }

        public clsProc GetProcess()
        {
            return Process;
        }

        public clsCmd_Mst GetCmd_Mst()
        {
            return CMD_MST;
        }

        public clsLocMst GetLocMst()
        {
            return LocMst;
        }
        /*
        public clsTask GetTask()
        {
            return task;
        }
        */
        public clsSno GetSNO()
        {
            return SNO;
        }

        public clsAlarmData GetAlarmData()
        {
            return ALARMDATA;
        }
        public clsCmd_Mst_His GetCmd_Mst_His()
        {
            return CMD_MST_HIS;
        }

        public clsUnitStsLog GetUnitStsLog()
        {
            return unitStsLog;
        }

        public clsUnitModeDef GetUnitModeDef()
        {
            return unitModeDef;
        }

        public clsL2LCount GetL2LCount()
        {
            return L2LCount;
        }

        public List<Element_Port>[] GetLstPort()
        {
            return Process.GetLstPort();
        }
    }
}
