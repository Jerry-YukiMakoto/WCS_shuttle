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
        public static Dictionary<string, int> ErrorSystem = new Dictionary<string, int>();
        public static Dictionary<string, string> CVErrormessage = new Dictionary<string, string>();

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

            ErrorSystem.Add("1", 0);
            ErrorSystem.Add("2", 0);
            ErrorSystem.Add("3", 0);
            ErrorSystem.Add("4", 0);

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
                            string cmdsno="0";
                            string sStnNo="";
                            string state1 = "2";
                            string state2 = "2";
                            string state3 = "2";
                            string state4 = "2";
                            string cmdsno1 = "0";
                            string cmdsno2 = "0";
                            string cmdsno3 = "0";
                            string cmdsno4 = "0";

                            if (CMD_MST.GetCmdMstByStartforDisplay(out DataObject<CmdMst> dataobject1,db).ResultCode == DBResult.Success)
                            {
                                cmdsno = dataobject1[0].CmdSno;
                                sStnNo = dataobject1[0].StnNo;
                                
                            }
                            if(sStnNo==StnNo.A3)
                            {
                                cmdsno1=cmdsno;
                                state1 = "1";
                            }
                            if (sStnNo == StnNo.A6)
                            {
                                cmdsno2 = cmdsno;
                                state2 = "1";
                            }
                            if (sStnNo == StnNo.A8)
                            {
                                cmdsno3 = cmdsno;
                                state3 = "1";
                            }
                            if (sStnNo == StnNo.A10)
                            {
                                cmdsno4 = cmdsno;
                                state4 = "1";
                            }


                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A3",
                                taskNo = cmdsno1,
                                state = state1, 
                                MerrMsg = MerrMsg,
                            };
                            if(!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            };
                            DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A6",
                                taskNo = cmdsno2,
                                state = state2, 
                                MerrMsg = MerrMsg,
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1))
                            {
                                return;
                            };
                            DisplayTaskStatusInfo info2 = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A8",
                                taskNo = cmdsno3,
                                state = state3, 
                                MerrMsg = MerrMsg,
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info2))
                            {
                                return;
                            };
                            DisplayTaskStatusInfo info3 = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A10",
                                taskNo = cmdsno4,
                                state = state4,
                                MerrMsg = MerrMsg,
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info3))
                            {
                                return;
                            };
                            Errormessage["0"] = Alarmdesc;
                            ErrorNormal["0"] =1;
                        }
                    }
                    else if(ErrorNormal["0"] == 1 || ErrorNormal["0"]== 0)//Errornormal 0:第一次程式進來回報正常 ErrorNormal 有上報過異常加上目前資料表沒有異常了要回報正常
                    {
                        string cmdsno = "0";
                        string sStnNo = "";
                        string state1 = "2";
                        string state2 = "2";
                        string state3 = "2";
                        string state4 = "2";
                        string cmdsno1 = "0";
                        string cmdsno2 = "0";
                        string cmdsno3 = "0";
                        string cmdsno4 = "0";

                        if (CMD_MST.GetCmdMstByStartforDisplay(out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            sStnNo = dataobject1[0].StnNo;
                        }
                        if (sStnNo == StnNo.A3)
                        {
                            cmdsno1 = cmdsno;
                            state1 = "1";
                        }
                        if (sStnNo == StnNo.A6)
                        {
                            cmdsno2 = cmdsno;
                            state2 = "1";
                        }
                        if (sStnNo == StnNo.A8)
                        {
                            cmdsno3 = cmdsno;
                            state3 = "1";
                        }
                        if (sStnNo == StnNo.A10)
                        {
                            cmdsno4 = cmdsno;
                            state4 = "1";
                        }


                        DisplayTaskStatusInfo info1 = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "A3",
                            taskNo = cmdsno1,
                            state = state1,
                            MerrMsg = "",
                        };
                        if(!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info1))
                        {
                            return;
                        }
                        DisplayTaskStatusInfo info2 = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "A6",
                            taskNo = cmdsno2,
                            state = state2, 
                            MerrMsg = "",
                        };
                        if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info2))
                        {
                            return;
                        }
                        DisplayTaskStatusInfo info3 = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "A8",
                            taskNo = cmdsno3,
                            state = state3, 
                            MerrMsg = "",
                        };
                        if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info3))
                        {
                            return;
                        }
                        DisplayTaskStatusInfo info4 = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "A10",
                            taskNo = cmdsno4,
                            state = state4, 
                            MerrMsg = "",
                        };
                        if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info4))
                        {
                            return;
                        }
                        ErrorNormal["0"] = 2;//讓回報正常只發生一次，值會改變就是在上報異常之後，檢查值改變加上資料表沒有異常
                        Errormessage["0"] = "";
                    }

                    for (int i = 1; i < 5; i++)//系統異常告知，對於電控定義的系統異常訊息秀在看板上，各樓層
                    {
                        string cmdsno = "0";
                        string state = "2";

                            if (i == 1 && _conveyor._alarmBit[i] && ErrorSystem["1"] != 1)
                            {
                                if (CMD_MST.GetCmdMstByStartforDisplayStn("A3", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                                {
                                    cmdsno = dataobject1[0].CmdSno;
                                    state = "1";
                                }

                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    locationId = "A3",
                                    taskNo = cmdsno,
                                    state = state,
                                    MerrMsg = "1F 緊急停止",
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    return;
                                }
                                ErrorSystem["1"] = 1;
                            }
                            else if (!_conveyor._alarmBit[i] && ErrorSystem["1"] == 1)
                            {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A3",
                                taskNo = cmdsno,
                                state = state,
                                MerrMsg = "",
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            ErrorSystem["1"] = 2;
                            }

                        cmdsno = "0";
                        state = "2";

                        if (i == 2 && _conveyor._alarmBit[i] && ErrorSystem["2"] != 1)
                            {
                                if (CMD_MST.GetCmdMstByStartforDisplayStn("A6", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                                {
                                    cmdsno = dataobject1[0].CmdSno;
                                    state = "1";
                                }

                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    locationId = "A6",
                                    taskNo = cmdsno,
                                    state = state,
                                    MerrMsg = "2F 緊急停止",
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    return;
                                }
                                ErrorSystem["2"] = 1;
                            }
                        else if (!_conveyor._alarmBit[i] && ErrorSystem["2"] == 1)
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A6",
                                taskNo = cmdsno,
                                state = state,
                                MerrMsg = "",
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            ErrorSystem["2"] = 2;
                        }

                        cmdsno = "0";
                        state = "2";

                        if (i == 3 && _conveyor._alarmBit[i] && ErrorSystem["3"] != 1)
                            {
                                if (CMD_MST.GetCmdMstByStartforDisplayStn("A8", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                                {
                                    cmdsno = dataobject1[0].CmdSno;
                                    state = "1";
                                }

                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    locationId = "A8",
                                    taskNo = cmdsno,
                                    state = state,
                                    MerrMsg = "3F 緊急停止",
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    return;
                                }
                                ErrorSystem["3"] = 1;
                            }
                            else if (!_conveyor._alarmBit[i] && ErrorSystem["3"] == 1)
                            {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A8",
                                taskNo = cmdsno,
                                state = state,
                                MerrMsg = "",
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            ErrorSystem["3"] = 2;
                            }

                        cmdsno = "0";
                        state = "2";

                        if (i == 4 && _conveyor._alarmBit[i] && ErrorSystem["4"] != 1)
                            {
                                if (CMD_MST.GetCmdMstByStartforDisplayStn("A10", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                                {
                                    cmdsno = dataobject1[0].CmdSno;
                                    state = "1";
                                }

                                DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                                {
                                    //填入回報訊息
                                    locationId = "A10",
                                    taskNo = cmdsno,
                                    state = state,
                                    MerrMsg = "4F 緊急停止",
                                };
                                if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                                {
                                    return;
                                }
                                ErrorSystem["4"] = 1;
                            }
                        else if (!_conveyor._alarmBit[i] && ErrorSystem["4"] == 1)
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A10",
                                taskNo = cmdsno,
                                state = state,
                                MerrMsg = "",
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            ErrorSystem["4"] = 2;
                        }

                    }


                    if (_conveyor.GetBuffer(1).Error|| _conveyor.GetBuffer(2).Error|| _conveyor.GetBuffer(3).Error || _conveyor.GetBuffer(4).Error /*|| CraneStsFirst!=1*/)
                    {
                        string cmdsno = "0";
                        string state = "2";

                        if(CraneStsFirst==1)
                        {
                            return;//以Crane異常上報為主
                        }

                        if (CMD_MST.GetCmdMstByStartforDisplayStn("A3",out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            state = "1";
                        }

                        if (_conveyor.GetBuffer(1).Error)
                        {
                            MerrMsg = "A1異常:";/* ErrorNormal["1"] = 1;*/

                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(1)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.PortStnbitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }

                        }

                        if (_conveyor.GetBuffer(2).Error) 
                        {
                            MerrMsg += " A2異常:"; /*ErrorNormal["1"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(2)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.A2bitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }

                        if (_conveyor.GetBuffer(3).Error) 
                        { 
                            MerrMsg += " A3異常:"; /*ErrorNormal["1"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(3)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.bitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }

                        if (_conveyor.GetBuffer(4).Error)
                        {
                            MerrMsg += " A4異常:"; /*ErrorNormal["1"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(4)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.A4bitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }


                        if (Errormessage["1"] != "一樓異常")
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A3",
                                taskNo = cmdsno,
                                state = state, 
                                MerrMsg = MerrMsg,
                            };
                            if(!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            Errormessage["1"] = "一樓異常";
                            ErrorNormal["1"] = 1;
                        }
                    }
                    else if(ErrorNormal["1"] == 0 || ErrorNormal["1"] == 1)
                    {
                        string cmdsno = "0";
                        string state = "2";

                        if (CMD_MST.GetCmdMstByStartforDisplayStn("A3", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            state = "1";
                        }

                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "A3",
                            taskNo = cmdsno,
                            state = state, //任務結束
                            MerrMsg = "",
                        };
                        if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                        {
                            return;
                        }
                        ErrorNormal["1"] = 2;
                        Errormessage["1"] = "";
                    }

                    if (_conveyor.GetBuffer(5).Error || _conveyor.GetBuffer(6).Error )
                    {
                        string cmdsno = "0";
                        string state = "2";

                        if (CraneStsFirst == 1)
                        {
                            return;//以Crane異常上報為主
                        }

                        if (CMD_MST.GetCmdMstByStartforDisplayStn("A6", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            state = "1";
                        }

                        if (_conveyor.GetBuffer(5).Error)
                        { 
                            MerrMsg = " A5異常:"; /*ErrorNormal["2"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(5)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.PortStnbitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }
                        if (_conveyor.GetBuffer(6).Error) 
                        {
                            MerrMsg += " A6異常:"; /*ErrorNormal["2"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(6)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.bitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }

                        if (Errormessage["2"] != "二樓異常")
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A6",
                                taskNo = cmdsno,
                                state = state, 
                                MerrMsg = MerrMsg,
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            Errormessage["2"] = "二樓異常";
                            ErrorNormal["2"] = 1;
                        }
                    }
                    else if (ErrorNormal["2"] == 0 || ErrorNormal["2"] == 1)
                    {
                        string cmdsno = "0";
                        string state = "2";

                        if (CMD_MST.GetCmdMstByStartforDisplayStn("A6", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            state = "1";
                        }

                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "A6",
                            taskNo = cmdsno,
                            state = state, 
                            MerrMsg = "",
                        };
                        if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                        {
                            return;
                        }
                        ErrorNormal["2"] = 2;
                        Errormessage["2"] = "";
                    }

                    if (_conveyor.GetBuffer(7).Error || _conveyor.GetBuffer(8).Error /*|| CraneStsFirst != 1*/)
                    {
                        string cmdsno = "0";
                        string state = "2";

                        if (CraneStsFirst == 1)
                        {
                            return;//以Crane異常上報為主
                        }

                        if (CMD_MST.GetCmdMstByStartforDisplayStn("A8", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            state = "1";
                        }

                        if (_conveyor.GetBuffer(7).Error)
                        { 
                            MerrMsg = " A7異常:"; /*ErrorNormal["3"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(7)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.PortStnbitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }
                        if (_conveyor.GetBuffer(8).Error)
                        {
                            MerrMsg += " A8異常:"; /*ErrorNormal["3"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(8)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.bitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }

                        if (Errormessage["3"] != "三樓異常")
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A8",
                                taskNo = cmdsno,
                                state = state, 
                                MerrMsg = MerrMsg,
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            Errormessage["3"] = "三樓異常";
                            ErrorNormal["3"] = 1;
                        }
                    }
                    else if(ErrorNormal["3"] == 0 || ErrorNormal["3"] == 1)
                    {
                        string cmdsno = "0";
                        string state = "2";

                        if (CMD_MST.GetCmdMstByStartforDisplayStn("A8", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            state = "1";
                        }

                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "A8",
                            taskNo = cmdsno,
                            state = state,
                            MerrMsg = "",
                        };
                        if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                        {
                            return;
                        }
                        ErrorNormal["3"] = 2;
                        Errormessage["3"] = "";
                    }

                    if (_conveyor.GetBuffer(9).Error || _conveyor.GetBuffer(10).Error )
                    {
                        string cmdsno = "0";
                        string state = "2";

                        if (CraneStsFirst == 1)
                        {
                            return;//以Crane異常上報為主
                        }

                        if (CMD_MST.GetCmdMstByStartforDisplayStn("A10", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            state = "1";
                        }

                        if (_conveyor.GetBuffer(9).Error) 
                        { 
                            MerrMsg = " A9異常:"; /*ErrorNormal["4"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(9)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.PortStnbitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }

                        if (_conveyor.GetBuffer(10).Error)
                        {
                            MerrMsg += " A10異常:"; /*ErrorNormal["4"] = 1;*/
                            for (int i = 0; i < 13; i++)
                            {
                                if (_conveyor.GetBuffer(10)._alarmBit[i] == true)
                                {
                                    string CVerror = CVErrorDefine.bitError[i];
                                    MerrMsg += CVerror;
                                    break;
                                }
                            }
                        }

                        if (Errormessage["4"] != "四樓異常")
                        {
                            DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                            {
                                //填入回報訊息
                                locationId = "A10",
                                taskNo = cmdsno,
                                state = state, 
                                MerrMsg = MerrMsg,
                            };
                            if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                            {
                                return;
                            }
                            Errormessage["4"] = "四樓異常";
                            ErrorNormal["4"] = 1;
                        }
                    }
                    else if(ErrorNormal["4"] == 0 || ErrorNormal["4"] == 1)
                    {
                        string cmdsno = "0";
                        string state = "2";

                        if (CMD_MST.GetCmdMstByStartforDisplayStn("A10", out DataObject<CmdMst> dataobject1, db).ResultCode == DBResult.Success)
                        {
                            cmdsno = dataobject1[0].CmdSno;
                            state = "1";
                        }

                        DisplayTaskStatusInfo info = new DisplayTaskStatusInfo
                        {
                            //填入回報訊息
                            locationId = "A10",
                            taskNo = cmdsno,
                            state = state, 
                            MerrMsg = "",
                        };
                        if (!clsWmsApi.GetApiProcess().GetDisplayTaskStatus().FunReport(info))
                        {
                            return;
                        }
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

        public void TaskEnd() //出庫都在到了站口buffer口才做任務結束的上報動作目前放棄此方案
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
                        if (cmdsno != "0" && position && cmdsno!="00000")
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
                    if (cmdsno1 != "0" && position1 && cmdsno1 != "00000")
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
