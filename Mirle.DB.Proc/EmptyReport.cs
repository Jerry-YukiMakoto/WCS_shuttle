using Mirle.ASRS.WCS.Controller;
using Mirle.Def;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.CENS.U0NXMA30;
using WCS_API_Client.ReportInfo;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using System.Timers;
using System.Collections.Generic;

namespace Mirle.DB.Proc
{
    public class EmptyReport
    {
        private DB.Fun.clsCmd_Mst CMD_MST = new DB.Fun.clsCmd_Mst();
        private clsDbConfig _config = new clsDbConfig();
        public static Dictionary<string, int> EmptyFlag = new Dictionary<string, int>();
        public static Dictionary<string, int> DisplayFlag = new Dictionary<string, int>();

        public EmptyReport(clsDbConfig config)
        {
            _config = config;
            EmptyFlag.Add("1", 0);
            EmptyFlag.Add("2", 0);
            DisplayFlag.Add("1", 0);
        }

        public void EmptyInWMS()//確認空棧板入庫狀態，狀態正確才上報WMS，要求搬運命令
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            
            StackPalletsInInfo info = new StackPalletsInInfo
            {
                locationFrom = StnNo.A4
            };

            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    if (_conveyor.GetBuffer(4).EmptyINReady == 8 /*&& (_conveyor.GetBuffer(4).Ready == 1)*/ && (_conveyor.GetBuffer(1).CommandId != 0))
                    {
                        if (CMD_MST.GetCmdMstByStoreOutStartForEmpty(out var dataObject, db) != GetDataResult.NoDataSelect)
                        {

                            if (CMD_MST.EmptyInOutCheck(clsConstValue.IoType.PalletStockIn, out var dataObject1, db) == GetDataResult.NoDataSelect && EmptyFlag["1"] == 0)//沒有命令資料就上報WMS
                            {
                                //做上報WMS的動作
                                if (!clsWmsApi.GetApiProcess().GetStackPalletsIn().FunReport(info))
                                {
                                    EmptyFlag["1"] = 0;
                                    return;
                                }
                                else
                                {
                                    EmptyFlag["1"] = 1;
                                }
                            }
                        }          
                    }

                    if (_conveyor.GetBuffer(4).EmptyINReady < 8 /*&& EmptyFlag["1"] == 1*/)//可以重新上報的時機
                    {
                        EmptyFlag["1"] = 0;
                    }

                    }
                else
                {
                    string strEM = "Error: 開啟DB失敗！";
                    DB.Fun.clsWriLog.Log.FunWriTraceLog_CV(strEM);
                }
            }
        }
        


        public void EmptyOutWMS()//確認空棧板出庫狀態，狀態正確才上報WMS，要求搬運命令
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            
            StackPalletsOutInfo info = new StackPalletsOutInfo
            {
                locationTo = StnNo.A4
            };

            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    if (_conveyor.GetBuffer(4).Presence == false/* && (_conveyor.GetBuffer(4).Ready == 2 /*|| _conveyor.GetBuffer(4).Ready == 0*/ && _conveyor.GetBuffer(4).EmptyError==0)
                    {
                        if (CMD_MST.GetCmdMstByStoreInStartForEmpty( out var dataObject1, db) != GetDataResult.NoDataSelect)
                        {

                            if (CMD_MST.EmptyInOutCheck(clsConstValue.IoType.PalletStockOut, out var dataObject, db) == GetDataResult.NoDataSelect && EmptyFlag["2"] != 1)//沒有命令資料就上報WMS
                            {
                                //做上報WMS的動作
                                var StoreOutApichk = clsWmsApi.GetApiProcess().GetStackPalletsOut().FunReport(info);//上報需要空棧板補充命令
                                bool success = StoreOutApichk.success;
                                string errmesg = StoreOutApichk.errMsg;
                                if (success)
                                {
                                    EmptyFlag["2"] = 1;
                                }
                                else if (!success && errmesg == "無空棧板庫存")
                                {
                                    _conveyor.GetBuffer(4).A4ErrorOn();
                                    DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                                    {
                                        //填入回報訊息
                                        locationId = "A3",
                                        taskNo = "0",
                                        state = "2", //任務開始
                                        MerrMsg = errmesg,
                                    };
                                    clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1);//上報異常於看板
                                    DisplayFlag["1"] = 1;
                                    EmptyFlag["2"] = 1;
                                }
                                else if (!success)
                                {
                                    EmptyFlag["2"] = 0;
                                }
                            }
                        }

                        
                    }
                    else
                    {
                        if (DisplayFlag["1"] == 1)//當有上報過異常，符合解除異常的狀態，才會上報無異常
                        {
                            DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A3",
                                taskNo = "0",
                                state = "2", //任務結束
                                MerrMsg = "",
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1))//上報異常於看板
                            {
                                return;
                            }
                            DisplayFlag["1"] = 0;
                        }
                        EmptyFlag["2"] = 0;
                    }

                    if (_conveyor.GetBuffer(4).Presence != false /*&& EmptyFlag["2"] == 1*/)//重新上報的時機
                    {
                        EmptyFlag["2"] = 0;
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
}