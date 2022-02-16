using Mirle.Def;
using Mirle.ASRS.WCS.Controller;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.CENS.U0NXMA30;
using WCS_API_Client.ReportInfo;
using Mirle.ASRS.WCS.Model.PLCDefinitions;

namespace Mirle.DB.Proc
{
    public class DisplayTask
    {
        private DB.Fun.clsCmd_Mst CMD_MST = new DB.Fun.clsCmd_Mst();
        private clsDbConfig _config = new clsDbConfig();

        public DisplayTask(clsDbConfig config)
        {
            _config = config;
        }

        public void TaskStart(string sCmdSno, string sLocationID)
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
            {
                //填入回報訊息
                locationID = sLocationID,
                taskNo = sCmdSno,
                state = "1", //任務開始
            };


            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    //if (填入條件)
                    //{
                        //做上報WMS的動作
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    //}
                }
                else
                {
                    string strEM = "Error: 開啟DB失敗！";
                    DB.Fun.clsWriLog.Log.FunWriTraceLog_CV(strEM);
                }
            }
        }

        public void TaskEnd(string sCmdSno, string sLocationID)
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
            {
                //填入回報訊息
                locationID = sLocationID,
                taskNo = sCmdSno,
                state = "2", //任務結束
            };


            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    //if (填入條件)
                    //{
                        //做上報WMS的動作
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    //}
                }
                else
                {
                    string strEM = "Error: 開啟DB失敗！";
                    DB.Fun.clsWriLog.Log.FunWriTraceLog_CV(strEM);
                }
            }
        }

    }
}
