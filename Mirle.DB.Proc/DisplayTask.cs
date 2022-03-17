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
        private Fun.clsEqu_Cmd EQU_CMD = new Fun.clsEqu_Cmd();
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
                locationId = sLocationID,
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

        public void CVErrorTaskStart()
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
            {
                //填入回報訊息
                locationId = "0",
                taskNo = "0",
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

        public void ErrorReportTaskStart()
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            string MerrMsg="";
            int CraneStsFirst=0;

            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    if (EQU_CMD.SubGetCraneSts(out DataObject<EquCmd> dataObject, db).ResultCode == DBResult.Success)
                    {
                        string EquMode = dataObject[0].EquMode;
                        if (EquMode != "C")
                        {
                            CraneStsFirst = 1;
                            switch (EquMode)
                            {
                                case "R":
                                    MerrMsg = "地上盤模式";
                                    break;
                                case "I":
                                    MerrMsg = "地上盤維護模式";
                                    break;
                                case "M":
                                    MerrMsg = "車上盤維護模式";
                                    break;
                                case "N":
                                    MerrMsg = "電腦離線中";
                                    break;
                                default:
                                    MerrMsg = "";
                                    break;
                            }

                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "0",
                                taskNo = "0",
                                state = "1", //任務開始
                                MerrMsg = MerrMsg,
                            };
                            clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                        }
                        else
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "0",
                                taskNo = "0",
                                state = "2", //任務結束
                                MerrMsg = "",
                            };
                            clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                        }
                    }
                    
                    if(_conveyor.GetBuffer(1).Error|| _conveyor.GetBuffer(2).Error|| _conveyor.GetBuffer(3).Error || CraneStsFirst!=1)
                    {
                        if (_conveyor.GetBuffer(1).Error) { MerrMsg = "A1異常"; }
                        if (_conveyor.GetBuffer(2).Error) { MerrMsg += "A2異常";}
                        if (_conveyor.GetBuffer(3).Error) { MerrMsg += "A3異常";}

                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "1",
                            taskNo = "0",
                            state = "1", //任務開始
                            MerrMsg = MerrMsg,
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    }
                    else
                    {
                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "1",
                            taskNo = "0",
                            state = "2", //任務結束
                            MerrMsg = "",
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    }

                    if (_conveyor.GetBuffer(5).Error || _conveyor.GetBuffer(6).Error || CraneStsFirst != 1)
                    {
                        if (_conveyor.GetBuffer(5).Error) { MerrMsg = "A5異常"; }
                        if (_conveyor.GetBuffer(6).Error) { MerrMsg += "A6異常"; }

                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "2",
                            taskNo = "0",
                            state = "1", //任務開始
                            MerrMsg = MerrMsg,
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    }
                    else
                    {
                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "2",
                            taskNo = "0",
                            state = "2", //任務結束
                            MerrMsg = "",
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    }

                    if (_conveyor.GetBuffer(7).Error || _conveyor.GetBuffer(8).Error || CraneStsFirst != 1)
                    {
                        if (_conveyor.GetBuffer(7).Error) { MerrMsg = "A7異常"; }
                        if (_conveyor.GetBuffer(8).Error) { MerrMsg += "A8異常"; }

                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "3",
                            taskNo = "0",
                            state = "1", //任務開始
                            MerrMsg = MerrMsg,
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    }
                    else
                    {
                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "3",
                            taskNo = "0",
                            state = "2", //任務結束
                            MerrMsg = "",
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    }

                    if (_conveyor.GetBuffer(9).Error || _conveyor.GetBuffer(10).Error || CraneStsFirst != 1)
                    {
                        if (_conveyor.GetBuffer(9).Error) { MerrMsg = "A9異常"; }
                        if (_conveyor.GetBuffer(10).Error) { MerrMsg += "A10異常"; }

                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "4",
                            taskNo = "0",
                            state = "1", //任務開始
                            MerrMsg = MerrMsg,
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    }
                    else
                    {
                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "4",
                            taskNo = "0",
                            state = "2", //任務結束
                            MerrMsg = "",
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                    }

                }
                else
                {
                    string strEM = "Error: 開啟DB失敗！";
                    DB.Fun.clsWriLog.Log.FunWriTraceLog_CV(strEM);
                }
            }
        }

        public void TaskEnd()
        {
            var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    for (int i = 6; i <= 10; i += 2)
                    {
                        int cmdsno = _conveyor.GetBuffer(i).CommandId;
                        bool position = _conveyor.GetBuffer(i).Position;
                        if (cmdsno != 0 && position)
                        {

                            if(CMD_MST.GetCmdMstByStoreOutFinish(cmdsno.ToString(), out var dataObject, db).ResultCode==DBResult.Success)
                            {
                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                     
                                    locationId = ((i - 2) / 2).ToString(),
                                    taskNo = cmdsno.ToString(),
                                    state = "2", //任務結束
                                };
                                if(!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    return;
                                }
                                CMD_MST.UpdateCmdMstRemark(cmdsno.ToString(), Remark.WMSReportComplete, db);
                            }
                        }
                    }

                    int cmdsno1 = _conveyor.GetBuffer(2).CommandId;
                    bool position1 = _conveyor.GetBuffer(2).Position;
                    if (cmdsno1 != 0 && position1)
                    {

                        if (CMD_MST.GetCmdMstByStoreOutFinish(cmdsno1.ToString(), out var dataObject, db).ResultCode == DBResult.Success)
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                 
                                locationId = "1",
                                taskNo = cmdsno1.ToString(),
                                state = "2", //任務結束
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            CMD_MST.UpdateCmdMstRemark(cmdsno1.ToString(), Remark.WMSReportComplete, db);
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
}
