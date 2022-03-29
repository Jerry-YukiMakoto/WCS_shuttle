using Mirle.Def;
using Mirle.ASRS.WCS.Controller;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.CENS.U0NXMA30;
using WCS_API_Client.ReportInfo;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using System.Collections.Generic;

namespace Mirle.DB.Proc
{
    public class DisplayTask
    {
        private DB.Fun.clsCmd_Mst CMD_MST = new DB.Fun.clsCmd_Mst();
        private Fun.clsEqu_Cmd EQU_CMD = new Fun.clsEqu_Cmd();
        private clsDbConfig _config = new clsDbConfig();
        public static Dictionary<string, string> Errormessage = new Dictionary<string, string>();
        public static Dictionary<string, int> ErrorNormal = new Dictionary<string, int>();

        public DisplayTask(clsDbConfig config)
        {
            _config = config;
            string CraneError="";
            string BufferError1F = "";
            string BufferError2F = "";
            string BufferError3F = "";
            string BufferError4F = "";

            Errormessage.Add("0", CraneError);
            Errormessage.Add("1", BufferError1F);
            Errormessage.Add("2", BufferError2F);
            Errormessage.Add("3", BufferError3F);
            Errormessage.Add("4", BufferError4F);

            ErrorNormal.Add("0", 0);
            ErrorNormal.Add("1", 0);
            ErrorNormal.Add("2", 0);
            ErrorNormal.Add("3", 0);
            ErrorNormal.Add("4", 0);

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
            string Alarmdesc="";

            using (var db = clsGetDB.GetDB(_config))
            {
                int iRet = clsGetDB.FunDbOpen(db);
                if (iRet == DBResult.Success)
                {
                    if (EQU_CMD.SubGetCraneSts(out DataObject<EquCmd> dataObject, db).ResultCode == DBResult.Success )
                    {
                        Alarmdesc = dataObject[0].alarmdesc;
                        CraneStsFirst = 1;
                        if (Alarmdesc != Errormessage["0"])
                        {
                            MerrMsg = Alarmdesc;
                            Errormessage["0"] = Alarmdesc;

                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "0",
                                taskNo = "0",
                                state = "1", //任務開始
                                MerrMsg = MerrMsg,
                            };
                            clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                            ErrorNormal["0"] =1;
                        }
                    }
                    else if(ErrorNormal["0"] == 1 || ErrorNormal["0"]== 0)//Errornormal 0:第一次程式進來回報正常 ErrorNormal 有上報過異常加上目前資料表沒有異常了要回報正常
                    {
                        DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "0",
                            taskNo = "0",
                            state = "2", //任務結束
                            MerrMsg = "",
                        };
                        clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1);
                        ErrorNormal["0"] = 2;//讓回報正常只發生一次，值會改變就是在上報異常之後，檢查值改變加上資料表沒有異常
                        Errormessage["0"] = "";
                    }


                    if (_conveyor.GetBuffer(1).Error|| _conveyor.GetBuffer(2).Error|| _conveyor.GetBuffer(3).Error || CraneStsFirst!=1)
                    {
                        if (_conveyor.GetBuffer(1).Error) { MerrMsg = "A1異常"; ErrorNormal["1"] = 1; }
                        if (_conveyor.GetBuffer(2).Error) { MerrMsg += " A2異常"; ErrorNormal["1"] = 1; }
                        if (_conveyor.GetBuffer(3).Error) { MerrMsg += " A3異常"; ErrorNormal["1"] = 1; }



                        if (Errormessage["1"] != "一樓異常")
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "1",
                                taskNo = "0",
                                state = "1", //任務開始
                                MerrMsg = MerrMsg,
                            };
                            clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                            Errormessage["1"] = "一樓異常";
                        }
                    }
                    else if(ErrorNormal["1"] == 0 || ErrorNormal["1"] == 1)
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
                        ErrorNormal["1"] = 2;
                        Errormessage["1"] = "";
                    }

                    if (_conveyor.GetBuffer(5).Error || _conveyor.GetBuffer(6).Error || CraneStsFirst != 1)
                    {
                        if (_conveyor.GetBuffer(5).Error) { MerrMsg = "A5異常"; ErrorNormal["2"] = 1; }
                        if (_conveyor.GetBuffer(6).Error) { MerrMsg += "A6異常"; ErrorNormal["2"] = 1; }

                        if (Errormessage["2"] != "二樓異常")
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "2",
                                taskNo = "0",
                                state = "1", //任務開始
                                MerrMsg = MerrMsg,
                            };
                            clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                            Errormessage["2"] = "二樓異常";
                        }
                    }
                    else if (ErrorNormal["2"] == 0 || ErrorNormal["2"] == 1)
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
                        ErrorNormal["2"] = 2;
                        Errormessage["2"] = "";
                    }

                    if (_conveyor.GetBuffer(7).Error || _conveyor.GetBuffer(8).Error || CraneStsFirst != 1)
                    {
                        if (_conveyor.GetBuffer(7).Error) { MerrMsg = "A7異常"; ErrorNormal["3"] = 1; }
                        if (_conveyor.GetBuffer(8).Error) { MerrMsg += "A8異常"; ErrorNormal["3"] = 1; }

                        if (Errormessage["3"] != "三樓異常")
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "3",
                                taskNo = "0",
                                state = "1", //任務開始
                                MerrMsg = MerrMsg,
                            };
                            clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                            Errormessage["3"] = "三樓異常";
                        }
                    }
                    else if(ErrorNormal["3"] == 0 || ErrorNormal["3"] == 1)
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
                        ErrorNormal["3"] = 2;
                        Errormessage["3"] = "";
                    }

                    if (_conveyor.GetBuffer(9).Error || _conveyor.GetBuffer(10).Error || CraneStsFirst != 1)
                    {
                        if (_conveyor.GetBuffer(9).Error) { MerrMsg = "A9異常"; ErrorNormal["4"] = 1; }
                        if (_conveyor.GetBuffer(10).Error) { MerrMsg += "A10異常"; ErrorNormal["4"] = 1; }


                        if (Errormessage["4"] != "四樓異常")
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "4",
                                taskNo = "0",
                                state = "1", //任務開始
                                MerrMsg = MerrMsg,
                            };
                            clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info);
                            Errormessage["4"] = "四樓異常";
                        }
                    }
                    else if(ErrorNormal["4"] == 0 || ErrorNormal["4"] == 1)
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
                        ErrorNormal["4"] = 2;
                        Errormessage["4"] = "";
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
                        string cmdsno = _conveyor.GetBuffer(i).CommandId.ToString().PadLeft(5, '0'); 
                        bool position = _conveyor.GetBuffer(i).Position;
                        if (cmdsno != "0" && position)
                        {

                            if(CMD_MST.GetCmdMstByStoreOutFinish(cmdsno, out var dataObject, db).ResultCode==DBResult.Success)
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

                    string cmdsno1 = _conveyor.GetBuffer(2).CommandId.ToString().PadLeft(5, '0');
                    bool position1 = _conveyor.GetBuffer(2).Position;
                    if (cmdsno1 != "0" && position1)
                    {

                        if (CMD_MST.GetCmdMstByStoreOutFinish(cmdsno1, out var dataObject, db).ResultCode == DBResult.Success)
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
