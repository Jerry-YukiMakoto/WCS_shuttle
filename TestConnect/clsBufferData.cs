#region Header

#endregion Header

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Data;
using System.Threading;
using Mirle.MPLC.DataType;
using HslCommunicationPLC.Siemens;

namespace PLCConfigSetting.PLCsetting
{
    public class clsBufferData
    {
        private System.Timers.Timer timRead = new System.Timers.Timer();
        private Thread Thread_ReConnect;
        private bool bThread_OpenPlc = false;
        public CV_Structure oPLC;
        private int[] iPlcIdx = new int[6];
        private int[] lifterIdx = new int[1];
        private int[] iPcIdx = new int[6];
        private int iCvIdx_Total = 0;
        private int buffer_count = 4;
        private string sIP = "";
        public string IP_Addr
        {
            get { return sIP; }
            set { sIP = value; }
        }

        public bool bConnectPLC;

        private bool bCheckOnline = false;
        public bool ChkOnline
        {
            get { return bCheckOnline; }
            set { bCheckOnline = value; }
        }

        public string[] PlcData = new string[2];
        private clsHost _plcHost;

        public clsBufferData()
        {
            oPLC = new CV_Structure();
        }

        public void subStart(clsHost _plcHost, PlcConfig CVConfig)
        {
            this._plcHost = _plcHost;
            sIP = CVConfig.IpAddress;
            if (bThread_OpenPlc == false)
            {
                bThread_OpenPlc = true;

                Thread_ReConnect = new Thread(SubOpenPLC);
                Thread_ReConnect.IsBackground = true;
                Thread_ReConnect.Start();

            }

            iPlcIdx[0] = 2;
            iPlcIdx[1] = 5;
            iPlcIdx[2] = 5;
            iPlcIdx[3] = 5;
            iPlcIdx[4] = 5;
            iPlcIdx[5] = 5;

            lifterIdx[0] = 8;

            iPcIdx[0] = 2;
            iPcIdx[1] = 5;
            iPcIdx[2] = 5;
            iPcIdx[3] = 5;
            iPcIdx[4] = 5;

        }



        public void FunProcess()
        {
            if (bConnectPLC == true)
            {
                // FunWriPLC_Bit("DB1.27.2", true);
                // FunWriPLC_Bit("DB1.27.4", false);
                // //FunWriPLC_Bit("DB1.26.4", true);
                // FunWriPLC_Bit("DB1.26.1", true);
                // short[] iRetData_Pc = new short[1];
                // _plcHost.ReadBlock("DB1.27", ref iRetData_Pc);


                // _plcHost.ReadBlock("DB1.25", ref iRetData_Pc);
                // bool content=false;
                // bool test = GetPlcBit(iRetData_Pc[0], 1);
                // _plcHost.ReadPLCbit("DB1.26.21", ref content);
                //test =GetPlcBit(iRetData_Pc[0],2);
                //FunWriPLC_Word("DB2.0.0", "123");
                ReadPlc();
                ReadPlc_1();

                if (bConnectPLC == true)
                {
                    #region 與周邊PLC HandShake
                    if (oPLC.PLC[0].SYSTEM_PLC.HandShake == true &&
                        oPLC.PC[0].SYSTEM_PC.HandShake == true)
                    {
                        if (WriPlcPC_HandShaking(false) == true)
                        {
                            oPLC.PC[0].SYSTEM_PC.HandShake = false;
                        }
                        else
                        {
                            bConnectPLC = false;
                        }
                    }
                    else if (oPLC.PLC[0].SYSTEM_PLC.HandShake == false &&
                        oPLC.PC[0].SYSTEM_PC.HandShake == false)
                    {
                        if (WriPlcPC_HandShaking(true) == true)
                        {
                            oPLC.PC[0].SYSTEM_PC.HandShake = true;
                        }
                        else
                        {
                            bConnectPLC = false;
                        }
                    }

                    FunSetTime();
                    #endregion 與周邊PLC HandShake
                    FunClearPlc();
                }
            }
            else
            {


                if (bThread_OpenPlc == false)
                {
                    bThread_OpenPlc = true;

                    Thread_ReConnect = new Thread(SubOpenPLC);
                    Thread_ReConnect.IsBackground = true;
                    Thread_ReConnect.Start();
                }
            }


        }

        /// <summary>
        /// 寫入PLC時間
        /// </summary>
        private void FunSetTime()
        {
            try
            {
                var dt = DateTime.Now;
                string BCDyear = Convert.ToInt32(dt.Year.ToString().Substring(2, 2)).ConvertBase10ToBCD().ToString().PadLeft(2, '0'); ;
                string BCDmonth = dt.Month.ConvertBase10ToBCD().ToString().PadLeft(2, '0');
                string YM = BCDyear + BCDmonth;
                string BCDday = dt.Day.ConvertBase10ToBCD().ToString().PadLeft(2, '0'); ;
                string BCDhour = dt.Hour.ConvertBase10ToBCD().ToString().PadLeft(2, '0'); ;
                string DH = BCDday + BCDhour;
                string BCDminute = dt.Minute.ConvertBase10ToBCD().ToString().PadLeft(2, '0'); ;
                string BCDsecond = dt.Second.ConvertBase10ToBCD().ToString().PadLeft(2, '0'); ;
                string ms = BCDminute + BCDsecond;
                //FunWriPLC_Bit("DB1.2.0",true);
                FunWriPLC_Word("DB1.4", YM);
                FunWriPLC_Word("DB1.6", DH);
                FunWriPLC_Word("DB1.8", ms);


            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
            }
        }

        /// <summary>
        /// PC -> PLC清值
        /// </summary>
        private void FunClearPlc()
        {
            string sDevice = "";
            DataTable dtTmp = new DataTable();

            try
            {
                for (int i = 1; i <= buffer_count; i++)
                {
                    #region BUFFER_PC->PLC清值
                    if (oPLC.PLC[i].CV.Sno != "0" && oPLC.PLC[i].CV.Sno == oPLC.PC[i].CV.Sno)
                    {
                        FunWriPLC_Word("DB" + (i + 1) + ".00", "0");

                        //oPLC.PC[i].CV.Sno = "";
                    }
                    if (oPLC.PLC[i].CV.Mode != 0 && oPLC.PLC[i].CV.Mode == oPLC.PC[i].CV.Mode)
                    {
                        FunWriPLC_Word("DB" + (i + 1) + ".2", "0");
                        //oPLC.PC[i].CV.Mode = 0;
                    }
                    if (oPLC.PLC[i].CV.WriteCommandComplete == true && oPLC.PC[i].CV.WriteCommandComplete == true)
                    {
                        FunWriPLC_Bit("DB" + (i + 1) + ".6.1", false);
                    }

                    //if (oPLC.PLC[i].CV.ReadBCR == false && oPLC.PC[i].CV.ReadComplete == true)
                    //{
                    //    FunWriPLC_Bit("DB" + i + 1 + ".6.2", false);
                    //}
                    #endregion buffer_PC->PLC清值
                }

                #region Lifter PC->PLC清值
                if (oPLC.PLC[0].Lifter.Vehicle_ID != "" && oPLC.PLC[0].Lifter.Vehicle_ID == oPLC.PC[0].Lifter.Vehicle_ID)
                {
                    FunWriPLC_Word("DB1.20.0", "");
                }
                if (oPLC.PLC[0].Lifter.CMDno != "" && oPLC.PLC[0].Lifter.CMDno == oPLC.PC[0].Lifter.CMDno)
                {
                    FunWriPLC_Word("DB1.22.0", "");
                    FunWriPLC_Bit("DB1.26.1", false);//寫入命令完成點位
                }
                if (oPLC.PLC[0].Lifter.Taskno != "" && oPLC.PLC[0].Lifter.Taskno == oPLC.PC[0].Lifter.Taskno)
                {
                    FunWriPLC_Word("DB1.24.0", "");
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor1 == true && oPLC.PC[0].Lifter.CallToFloor1 == true)
                {
                    FunWriPLC_Bit("DB1.27.0", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor2 == true && oPLC.PC[0].Lifter.CallToFloor2 == true)
                {
                    FunWriPLC_Bit("DB1.27.2", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor3 == true && oPLC.PC[0].Lifter.CallToFloor3 == true)
                {
                    FunWriPLC_Bit("DB1.27.4", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor4 == true && oPLC.PC[0].Lifter.CallToFloor4 == true)
                {
                    FunWriPLC_Bit("DB1.27.6", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor5 == true && oPLC.PC[0].Lifter.CallToFloor5 == true)
                {
                    FunWriPLC_Bit("DB1.28.0", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor6 == true && oPLC.PC[0].Lifter.CallToFloor6 == true)
                {
                    FunWriPLC_Bit("DB1.28.2", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor7 == true && oPLC.PC[0].Lifter.CallToFloor7 == true)
                {
                    FunWriPLC_Bit("DB1.28.4", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor8 == true && oPLC.PC[0].Lifter.CallToFloor8 == true)
                {
                    FunWriPLC_Bit("DB1.28.6", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor9 == true && oPLC.PC[0].Lifter.CallToFloor9 == true)
                {
                    FunWriPLC_Bit("DB1.29.0", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor10 == true && oPLC.PC[0].Lifter.CallToFloor10 == true)
                {
                    FunWriPLC_Bit("DB1.29.2", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor11 == true && oPLC.PC[0].Lifter.CallToFloor11 == true)
                {
                    FunWriPLC_Bit("DB1.29.4", false);
                }
                if (oPLC.PLC[0].Lifter.Floor1_SafetyCheck == false && oPLC.PC[0].Lifter.Floor1_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.27.1", false);
                }
                if (oPLC.PLC[0].Lifter.Floor2_SafetyCheck == false && oPLC.PC[0].Lifter.Floor2_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.27.3", false);
                }
                if (oPLC.PLC[0].Lifter.Floor3_SafetyCheck == false && oPLC.PC[0].Lifter.Floor3_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.27.5", false);
                }
                if (oPLC.PLC[0].Lifter.Floor4_SafetyCheck == false && oPLC.PC[0].Lifter.Floor4_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.27.7", false);
                }
                if (oPLC.PLC[0].Lifter.Floor5_SafetyCheck == false && oPLC.PC[0].Lifter.Floor5_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.28.1", false);
                }
                if (oPLC.PLC[0].Lifter.Floor6_SafetyCheck == false && oPLC.PC[0].Lifter.Floor6_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.28.3", false);
                }
                if (oPLC.PLC[0].Lifter.Floor7_SafetyCheck == false && oPLC.PC[0].Lifter.Floor7_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.28.5", false);
                }
                if (oPLC.PLC[0].Lifter.Floor8_SafetyCheck == false && oPLC.PC[0].Lifter.Floor8_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.28.7", false);
                }
                if (oPLC.PLC[0].Lifter.Floor9_SafetyCheck == false && oPLC.PC[0].Lifter.Floor9_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.29.1", false);
                }
                if (oPLC.PLC[0].Lifter.Floor10_SafetyCheck == false && oPLC.PC[0].Lifter.Floor10_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.29.3", false);
                }
                if (oPLC.PLC[0].Lifter.Floor11_SafetyCheck == false && oPLC.PC[0].Lifter.Floor11_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB1.29.5", false);
                }

                #endregion
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();

            }
            finally
            {
                dtTmp = null;
            }
        }

        /// <summary>
        /// PLC -> PC
        /// </summary>
        private void ReadPlc()
        {
            short[] iRetData_Plc = new short[iPlcIdx[0]];

            try
            {
                bool content = false;

                if (_plcHost.ReadPLCbit("DB1.10.0", ref content))
                {
                    oPLC.PLC[0].SYSTEM_PLC.HandShake = content;

                }
                else
                {
                    bConnectPLC = false;
                }

                iRetData_Plc = new short[lifterIdx[0]];



                if (_plcHost.ReadPLCbit("DB1.26.0", ref content))
                {
                    oPLC.PLC[0].SYSTEM_PLC.PLCmode = content;
                }
                else
                {
                    bConnectPLC = false;
                }

                if (_plcHost.ReadPLCbit("DB1.36.0", ref content))
                {
                    oPLC.PLC[0].Lifter.AllowWriteCommand = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.37.0", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor1 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.37.1", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor1_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.37.2", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor2 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.37.3", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor2_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.37.4", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor3 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.37.5", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor3_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.37.6", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor4 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.37.7", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor4_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.38.0", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor5 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.38.1", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor5_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.38.2", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor6 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.38.3", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor6_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.38.4", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor7 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.38.5", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor7_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.38.6", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor8 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.38.7", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor8_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.39.0", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor9 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.39.1", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor9_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.39.2", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor10 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.39.3", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor10_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.39.4", ref content))
                {
                    oPLC.PLC[0].Lifter.MoveToFloor11 = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.39.5", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor11_SafetyCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.39.6", ref content))
                {
                    oPLC.PLC[0].Lifter.LiftMode = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.39.7", ref content))
                {
                    oPLC.PLC[0].Lifter.LiftRun = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.40.0", ref content))
                {
                    oPLC.PLC[0].Lifter.LiftDown = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.40.1", ref content))
                {
                    oPLC.PLC[0].Lifter.LiftIdle = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.44.0", ref content))
                {
                    oPLC.PLC[0].Lifter.presence_shuttle = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.44.1", ref content))
                {
                    oPLC.PLC[0].Lifter.UnloadingLocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.44.2", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor1LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.44.3", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor2LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.44.4", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor3LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.44.5", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor4LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.44.6", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor5LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.44.7", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor7LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.45.0", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor7LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.45.1", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor8LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.45.2", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor9LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }
                if (_plcHost.ReadPLCbit("DB1.45.3", ref content))
                {
                    oPLC.PLC[0].Lifter.Floor10LocationCheck = content;
                }
                else
                {
                    bConnectPLC = false;
                }



                if (_plcHost.ReadBlock("DB1.30", ref iRetData_Plc))
                {
                    #region Lifter PLC -> PC
                    oPLC.PLC[0].Lifter.Vehicle_ID = iRetData_Plc[0].ToString();
                    oPLC.PLC[0].Lifter.CMDno = iRetData_Plc[1].ToString();
                    oPLC.PLC[0].Lifter.Taskno = iRetData_Plc[2].ToString();
                    oPLC.PLC[0].Lifter.LiftPosition = iRetData_Plc[6];

                    #endregion PLC -> PC
                }
                else
                {
                    bConnectPLC = false;
                }

                for (int i = 1; i <= buffer_count; i++)
                {
                    iRetData_Plc = new short[iPlcIdx[i]];

                    if (_plcHost.ReadBlock("DB" + (i + 1) + ".10", ref iRetData_Plc))
                    {
                        #region Conveyor PLC -> PC
                        oPLC.PLC[i].CV.Sno = iRetData_Plc[0].ToString();
                        oPLC.PLC[i].CV.Mode = iRetData_Plc[1];
                        oPLC.PLC[i].CV.CV_Status = iRetData_Plc[2];
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".16.0", ref content))
                        {
                            oPLC.PLC[i].CV.AllowWriteCommand = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".16.2", ref content))
                        {
                            oPLC.PLC[i].CV.WriteCommandComplete = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".16.3", ref content))
                        {
                            oPLC.PLC[i].CV.WaitToRelease = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".16.4", ref content))
                        {
                            oPLC.PLC[i].CV.Presence = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".16.5", ref content))
                        {
                            oPLC.PLC[i].CV.StoreInInfo = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".18.0", ref content))
                        {
                            oPLC.PLC[i].CV.AutoManual = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".18.1", ref content))
                        {
                            oPLC.PLC[i].CV.Run = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".18.2", ref content))
                        {
                            oPLC.PLC[i].CV.Down = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".18.3", ref content))
                        {
                            oPLC.PLC[i].CV.idle = content;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".18.4", ref content))
                        {
                            oPLC.PLC[i].CV.Spare = content;
                        }
                        #endregion

                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
            }
            finally
            {
                iRetData_Plc = null;
            }
        }

        /// <summary>
        /// PC -> PLC
        /// </summary>
        private void ReadPlc_1()
        {
            if (bConnectPLC == false)
                return;
            short[] iRetData_Pc = new short[iPcIdx[0]];
            try
            {
                bool content = false;
                if (_plcHost.ReadPLCbit("DB1.0.0", ref content))
                {
                    oPLC.PC[0].SYSTEM_PC.HandShake = content;

                }
                else
                {
                    bConnectPLC = false;
                }

                iRetData_Pc = new short[lifterIdx[0]];

                if (_plcHost.ReadBlock("DB1.20", ref iRetData_Pc))
                {
                    #region PC -> PLC
                    oPLC.PC[0].Lifter.Vehicle_ID = iRetData_Pc[0].ToString();
                    oPLC.PC[0].Lifter.CMDno = iRetData_Pc[1].ToString();
                    oPLC.PC[0].Lifter.Taskno = iRetData_Pc[2].ToString();


                    if (_plcHost.ReadPLCbit("DB1.26.1", ref content))
                    {
                        oPLC.PC[0].Lifter.WriteCommandComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.27.0", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor1 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.27.1", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor1_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.27.2", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor2 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.27.3", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor2_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.27.4", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor3 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.27.5", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor3_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.27.6", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor4 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.27.7", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor4_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.28.0", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor5 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.28.1", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor5_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.28.2", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor6 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.28.3", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor6_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.28.4", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor7 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.28.5", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor7_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.28.6", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor8 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.28.7", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor8_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.29.0", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor9 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.29.1", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor9_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.29.2", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor10 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.29.3", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor10_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.29.4", ref content))
                    {
                        oPLC.PC[0].Lifter.CallToFloor11 = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    if (_plcHost.ReadPLCbit("DB1.29.5", ref content))
                    {
                        oPLC.PC[0].Lifter.Floor11_CarMoveComplete = content;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                    #endregion PC -> PLC

                }
                else
                {
                    //objPLC.Close();
                    bConnectPLC = false;
                }

                for (int i = 1; i <= buffer_count; i++)
                {
                    iRetData_Pc = new short[iPcIdx[i]];

                    if (_plcHost.ReadBlock("DB" + (i + 1) + ".0", ref iRetData_Pc))
                    {
                        #region Conveyor PC -> PLC

                        oPLC.PC[i].CV.Sno = iRetData_Pc[0].ToString();
                        oPLC.PC[i].CV.Mode = iRetData_Pc[1];

                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".6.1", ref content))
                        {
                            oPLC.PC[i].CV.WriteCommandComplete = content;
                        }
                        else
                        {
                            bConnectPLC = false;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".6.2", ref content))
                        {
                            oPLC.PC[i].CV.ReadComplete = content;
                        }
                        else
                        {
                            bConnectPLC = false;
                        }
                        if (_plcHost.ReadPLCbit("DB" + (i + 1) + ".6.3", ref content))
                        {
                            oPLC.PC[i].CV.NoCommand = content;
                        }
                        else
                        {
                            bConnectPLC = false;
                        }
                        #endregion

                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                }


            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();

            }
            finally
            {
                iRetData_Pc = null;
            }
        }

        private void SubOpenPLC()
        {
            try
            {
                if (ByPing(sIP) == true)
                {
                    string strEM = "";
                    if (_plcHost.Connect(ref strEM) == true)
                    {
                        bConnectPLC = true;
                    }
                    else
                    {
                        bConnectPLC = false;
                        clsWriLog.Log.FunWriTraceLog_CV($"NG: PLC Connect Fail => {strEM}");
                    }
                }
                else
                {
                    bConnectPLC = false;
                    clsWriLog.Log.FunWriTraceLog_CV($"NG: Ping PLC IP失敗 => {sIP}");
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
            finally
            {
                bThread_OpenPlc = false;
            }
        }

        public bool WriPlcPC_HandShaking(bool sValue)
        {
            bool bRet;
            string sAddr = "DB1.0.0";
            bRet = FunWriPLC_Bit(sAddr, sValue);
            return bRet;
        }

        public bool FunWriPLC_Bit(string sAddr, bool sSetData)
        {
            try
            {
                bool bRet = _plcHost.WriteBit(sAddr, sSetData);
                return bRet;
            }
            catch
            {
                return false;
            }
        }

        public bool FunWriPLC_Word(string sAddr, string sSetData)
        {
            try
            {
                short shortSetData = short.Parse(sSetData);
                bool bRet = _plcHost.Write(sAddr, shortSetData);
                return bRet;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ping IP
        /// </summary>
        /// <param name="strIP"></param>
        /// <returns></returns>
        private bool ByPing(string strIP)
        {
            Ping ping;
            try
            {
                ping = new Ping();
                PingReply pingresult = ping.Send(strIP);
                if (pingresult.Status.ToString() == "Success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                ping = null;
            }
        }

        public bool GetPlcBit(short iData, int iBit)
        {
            bool bFlag = false;
            bFlag = (iData & (short)Math.Pow(2, iBit)) == 0 ? false : true;
            return bFlag;
        }

        private string GetAscii_LowByte(int iData)
        {
            int iTmp = int.Parse(Convert.ToString(iData % 256, 10));
            if (Convert.ToChar(iTmp) == '\0' || Convert.ToByte(Convert.ToChar(iTmp)) == 3)
                return "";
            else
            {
                return Convert.ToChar(iTmp).ToString();
            }
        }

        private string GetAscii_HiByte(int iData)
        {
            int iTmp = int.Parse(Convert.ToString(iData / 256, 10));
            if (Convert.ToChar(iTmp) == '\0' || Convert.ToByte(Convert.ToChar(iTmp)) == 3)
                return "";
            else
            {
                return Convert.ToChar(iTmp).ToString();
            }
        }
    }
}
