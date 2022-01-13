using Mirle.ASRS.WCS.Controller;
using Mirle.Def;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.CENS.U0NXMA30;
using WCS_API_Client.ReportInfo;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using System.Timers;

namespace Mirle.DB.Proc
{
    public class EmptyReport
    {
        private DB.Fun.clsCmd_Mst CMD_MST = new DB.Fun.clsCmd_Mst();
        private clsDbConfig _config = new clsDbConfig();

        public void EmptyInWMS(object sender, ElapsedEventArgs e)//確認空棧板入庫狀態，狀態正確才上報WMS，要求搬運命令
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            
            StackPalletsInInfo info = new StackPalletsInInfo
            {
                locationFrom = StnNo.A4
            };

            using var db = clsGetDB.GetDB(_config);
            int iRet = clsGetDB.FunDbOpen(db);
            if (iRet == DBResult.Success)
            {
                if (_conveyor.GetBuffer(4).EmptyINReady == 9 && _conveyor.GetBuffer(4).Ready == 1)
                {
                    if (CMD_MST.EmptyInOutCheck(IOtype.EmptyStoreIn,out var dataObject, db) == GetDataResult.NoDataSelect)//沒有命令資料就上報WMS
                    {
                        //做上報WMS的動作
                        clsWmsApi.GetApiProcess().GetStackPalletsIn().FunReport(info);
                    }
                }
            }
            else
            {
                string strEM = "Error: 開啟DB失敗！";
                DB.Fun.clsWriLog.Log.FunWriTraceLog_CV(strEM);
            }
        }
        


        public void EmptyOutWMS(object sender, ElapsedEventArgs e)//確認空棧板出庫狀態，狀態正確才上報WMS，要求搬運命令
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            
            StackPalletsOutInfo info = new StackPalletsOutInfo
            {
                locationTo = StnNo.A4
            };

            using var db = clsGetDB.GetDB(_config);
            int iRet = clsGetDB.FunDbOpen(db);
            if (iRet == DBResult.Success)
            {
                if (_conveyor.GetBuffer(4).Presence == false && _conveyor.GetBuffer(4).Ready == 2)
                {
                    if (CMD_MST.EmptyInOutCheck(IOtype.EmptyStroeOut,out var dataObject, db) == GetDataResult.NoDataSelect)//沒有命令資料就上報WMS
                    {
                        //做上報WMS的動作
                        clsWmsApi.GetApiProcess().GetStackPalletsOut().FunReport(info);
                    }
                }
            }
            else
            {
                string strEM = "Error: 開啟DB失敗！";
                DB.Fun.clsWriLog.Log.FunWriTraceLog_CV(strEM);
            }
        }
    }
}