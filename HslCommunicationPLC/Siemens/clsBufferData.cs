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

namespace HslCommunicationPLC.Siemens
{
    public class clsBufferData
    {
        private System.Timers.Timer timRead = new System.Timers.Timer();
        private Thread Thread_ReConnect;
        private bool bThread_OpenPlc = false;
        public CV_Structure oPLC;
        private int[] iPlcIdx = new int[6];
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

        public void subStart(clsHost _plcHost,PlcConfig CVConfig)
        {
            this._plcHost = _plcHost;
            sIP = CVConfig.IpAddress;
            if(bThread_OpenPlc == false)
            {
                bThread_OpenPlc = true;

                Thread_ReConnect = new Thread(SubOpenPLC);
                Thread_ReConnect.IsBackground = true;
                Thread_ReConnect.Start();
                
            }

            iPlcIdx[0] = 45;
            iPlcIdx[1] = 15;
            iPlcIdx[2] = 15;
            iPlcIdx[3] = 15;
            iPlcIdx[4] = 15;
            iPlcIdx[5] = 15;

            iPcIdx[0] = 30;
            iPcIdx[1] = 15;
            iPcIdx[2] = 15;
            iPcIdx[3] = 15;
            iPcIdx[4] = 15;

        }

    

        public void FunProcess()
        {
            if(bConnectPLC == true)
            {
                ReadPlc();
                ReadPlc_1();

                if(bConnectPLC == true)
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


                if(bThread_OpenPlc == false)
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
                string BCDyear = dt.Year.ConvertBase10ToBCD().ToString();
                string BCDmonth = dt.Month.ConvertBase10ToBCD().ToString();
                string YM = BCDyear + BCDmonth;
                string BCDday = dt.Day.ConvertBase10ToBCD().ToString();
                string BCDhour = dt.Hour.ConvertBase10ToBCD().ToString();
                string DH= BCDday+ BCDhour;
                string BCDminute = dt.Minute.ConvertBase10ToBCD().ToString();
                string BCDsecond = dt.Second.ConvertBase10ToBCD().ToString();
                string ms=BCDminute+ BCDsecond;
                //FunWriPLC_Bit("DB1.2.0",true);
                FunWriPLC_Word("DB1.4", YM);
                FunWriPLC_Word("DB1.6", DH);
                FunWriPLC_Word("DB1.8", ms);

            }
            catch(Exception ex)
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
                for(int i = 1; i <= buffer_count; i++)
                {
                    #region BUFFER_PC->PLC清值
                    if(oPLC.PLC[i].CV.Sno!=""&& oPLC.PLC[i].CV.Sno== oPLC.PC[i].CV.Sno)
                    {
                        FunWriPLC_Word("DB"+i+1+"0.0", "");
                       
                        //oPLC.PC[i].CV.Sno = "";
                    }
                    if (oPLC.PLC[i].CV.Mode != 0 && oPLC.PLC[i].CV.Mode == oPLC.PC[i].CV.Mode)
                    {
                        FunWriPLC_Word("DB" + i + 1 + "2.0", "");
                        //oPLC.PC[i].CV.Mode = 0;
                    }
                    if(oPLC.PLC[i].CV.WriteCommandComplete==true&&oPLC.PC[i].CV.WriteCommandComplete==true)
                    {
                        FunWriPLC_Bit("DB" + i + 1 + "6.1", false);
                    }
                    
                    if(oPLC.PLC[i].CV.ReadBCR==false&& oPLC.PC[i].CV.ReadComplete==true)
                    {
                        FunWriPLC_Bit("DB" + i + 1 + "6.2", false);
                    }
                    #endregion buffer_PC->PLC清值
                }

                #region Lifter PC->PLC清值
                if (oPLC.PLC[0].Lifter.Vehicle_ID != "" && oPLC.PLC[0].Lifter.Vehicle_ID == oPLC.PC[0].Lifter.Vehicle_ID)
                {
                    FunWriPLC_Word("DB120.0", "");
                }
                if (oPLC.PLC[0].Lifter.CMDno != "" && oPLC.PLC[0].Lifter.CMDno == oPLC.PC[0].Lifter.CMDno)
                {
                    FunWriPLC_Word("DB122.0", "");
                    FunWriPLC_Bit("DB126.1", false);//寫入命令完成點位
                }
                if (oPLC.PLC[0].Lifter.Taskno != "" && oPLC.PLC[0].Lifter.Taskno == oPLC.PC[0].Lifter.Taskno)
                {
                    FunWriPLC_Word("DB124.0", "");
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor1 == true && oPLC.PC[0].Lifter.CallToFloor1 == true)
                {
                    FunWriPLC_Bit("DB126.2", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor2 == true && oPLC.PC[0].Lifter.CallToFloor2 == true)
                {
                    FunWriPLC_Bit("DB126.4", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor3 == true && oPLC.PC[0].Lifter.CallToFloor3 == true)
                {
                    FunWriPLC_Bit("DB126.6", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor4 == true && oPLC.PC[0].Lifter.CallToFloor4 == true)
                {
                    FunWriPLC_Bit("DB127.0", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor5 == true && oPLC.PC[0].Lifter.CallToFloor5 == true)
                {
                    FunWriPLC_Bit("DB127.2", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor6 == true && oPLC.PC[0].Lifter.CallToFloor6 == true)
                {
                    FunWriPLC_Bit("DB127.4", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor7 == true && oPLC.PC[0].Lifter.CallToFloor7 == true)
                {
                    FunWriPLC_Bit("DB127.6", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor8 == true && oPLC.PC[0].Lifter.CallToFloor8 == true)
                {
                    FunWriPLC_Bit("DB128.0", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor9 == true && oPLC.PC[0].Lifter.CallToFloor9 == true)
                {
                    FunWriPLC_Bit("DB128.2", false);
                }
                if (oPLC.PLC[0].Lifter.MoveToFloor10 == true && oPLC.PC[0].Lifter.CallToFloor10 == true)
                {
                    FunWriPLC_Bit("DB128.4", false);
                }
                if (oPLC.PLC[0].Lifter.Floor1_SafetyCheck == false && oPLC.PC[0].Lifter.Floor1_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB126.3", false);
                }
                if (oPLC.PLC[0].Lifter.Floor2_SafetyCheck == false && oPLC.PC[0].Lifter.Floor2_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB126.5", false);
                }
                if (oPLC.PLC[0].Lifter.Floor3_SafetyCheck == false && oPLC.PC[0].Lifter.Floor3_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB126.7", false);
                }
                if (oPLC.PLC[0].Lifter.Floor4_SafetyCheck == false && oPLC.PC[0].Lifter.Floor4_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB127.1", false);
                }
                if (oPLC.PLC[0].Lifter.Floor5_SafetyCheck == false && oPLC.PC[0].Lifter.Floor5_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB127.3", false);
                }
                if (oPLC.PLC[0].Lifter.Floor6_SafetyCheck == false && oPLC.PC[0].Lifter.Floor6_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB127.5", false);
                }
                if (oPLC.PLC[0].Lifter.Floor7_SafetyCheck == false && oPLC.PC[0].Lifter.Floor7_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB127.7", false);
                }
                if (oPLC.PLC[0].Lifter.Floor8_SafetyCheck == false && oPLC.PC[0].Lifter.Floor8_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB128.1", false);
                }
                if (oPLC.PLC[0].Lifter.Floor9_SafetyCheck == false && oPLC.PC[0].Lifter.Floor9_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB128.3", false);
                }
                if (oPLC.PLC[0].Lifter.Floor10_SafetyCheck == false && oPLC.PC[0].Lifter.Floor10_CarMoveComplete == true)
                {
                    FunWriPLC_Bit("DB128.5", false);
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
                if (_plcHost.ReadBlock("DB1.0", ref iRetData_Plc))
                {
                    oPLC.PLC[0].SYSTEM_PLC.HandShake = GetPlcBit(iRetData_Plc[10], 0);
                    #region Lifter PLC -> PC
                    oPLC.PLC[0].Lifter.Vehicle_ID = iRetData_Plc[30].ToString();
                    oPLC.PLC[0].Lifter.CMDno = iRetData_Plc[32].ToString();
                    oPLC.PLC[0].Lifter.Taskno = iRetData_Plc[34].ToString();
                    oPLC.PLC[0].Lifter.AllowWriteCommand = GetPlcBit(iRetData_Plc[36], 0);
                    oPLC.PLC[0].Lifter.WriteCommandComplete = GetPlcBit(iRetData_Plc[36], 1);
                    oPLC.PLC[0].Lifter.MoveToFloor1 = GetPlcBit(iRetData_Plc[36], 2);
                    oPLC.PLC[0].Lifter.Floor1_SafetyCheck = GetPlcBit(iRetData_Plc[36], 3);
                    oPLC.PLC[0].Lifter.MoveToFloor2 = GetPlcBit(iRetData_Plc[36], 4);
                    oPLC.PLC[0].Lifter.Floor2_SafetyCheck = GetPlcBit(iRetData_Plc[36], 5);
                    oPLC.PLC[0].Lifter.MoveToFloor3 = GetPlcBit(iRetData_Plc[36], 6);
                    oPLC.PLC[0].Lifter.Floor3_SafetyCheck = GetPlcBit(iRetData_Plc[36], 7);
                    oPLC.PLC[0].Lifter.MoveToFloor4 = GetPlcBit(iRetData_Plc[37], 0);
                    oPLC.PLC[0].Lifter.Floor4_SafetyCheck = GetPlcBit(iRetData_Plc[37], 1);
                    oPLC.PLC[0].Lifter.MoveToFloor5 = GetPlcBit(iRetData_Plc[37], 2);
                    oPLC.PLC[0].Lifter.Floor5_SafetyCheck = GetPlcBit(iRetData_Plc[37], 3);
                    oPLC.PLC[0].Lifter.MoveToFloor6 = GetPlcBit(iRetData_Plc[37], 4);
                    oPLC.PLC[0].Lifter.Floor6_SafetyCheck = GetPlcBit(iRetData_Plc[37], 5);
                    oPLC.PLC[0].Lifter.MoveToFloor7 = GetPlcBit(iRetData_Plc[37], 6);
                    oPLC.PLC[0].Lifter.Floor7_SafetyCheck = GetPlcBit(iRetData_Plc[37], 7);
                    oPLC.PLC[0].Lifter.MoveToFloor8 = GetPlcBit(iRetData_Plc[38], 0);
                    oPLC.PLC[0].Lifter.Floor8_SafetyCheck = GetPlcBit(iRetData_Plc[38], 1);
                    oPLC.PLC[0].Lifter.MoveToFloor9 = GetPlcBit(iRetData_Plc[38], 2);
                    oPLC.PLC[0].Lifter.Floor9_SafetyCheck = GetPlcBit(iRetData_Plc[38], 3);
                    oPLC.PLC[0].Lifter.MoveToFloor10 = GetPlcBit(iRetData_Plc[38], 4);
                    oPLC.PLC[0].Lifter.Floor10_SafetyCheck = GetPlcBit(iRetData_Plc[38], 5);
                    oPLC.PLC[0].Lifter.LiftMode = GetPlcBit(iRetData_Plc[38], 6);
                    oPLC.PLC[0].Lifter.LiftRun = GetPlcBit(iRetData_Plc[38], 7);
                    oPLC.PLC[0].Lifter.LiftDown = GetPlcBit(iRetData_Plc[39], 0);
                    oPLC.PLC[0].Lifter.LiftIdle = GetPlcBit(iRetData_Plc[39], 1);
                    oPLC.PLC[0].Lifter.LiftPosition = iRetData_Plc[40];
                    oPLC.PLC[0].Lifter.presence_shuttle = GetPlcBit(iRetData_Plc[42], 0);
                    oPLC.PLC[0].Lifter.UnloadingLocationCheck = GetPlcBit(iRetData_Plc[42], 1);
                    oPLC.PLC[0].Lifter.Floor1LocationCheck = GetPlcBit(iRetData_Plc[42], 2);
                    oPLC.PLC[0].Lifter.Floor2LocationCheck = GetPlcBit(iRetData_Plc[42], 3);
                    oPLC.PLC[0].Lifter.Floor3LocationCheck = GetPlcBit(iRetData_Plc[42], 4);
                    oPLC.PLC[0].Lifter.Floor4LocationCheck = GetPlcBit(iRetData_Plc[42], 5);
                    oPLC.PLC[0].Lifter.Floor5LocationCheck = GetPlcBit(iRetData_Plc[42], 6);
                    oPLC.PLC[0].Lifter.Floor6LocationCheck = GetPlcBit(iRetData_Plc[42], 7);
                    oPLC.PLC[0].Lifter.Floor7LocationCheck = GetPlcBit(iRetData_Plc[43], 0);
                    oPLC.PLC[0].Lifter.Floor8LocationCheck = GetPlcBit(iRetData_Plc[43], 1);
                    oPLC.PLC[0].Lifter.Floor9LocationCheck = GetPlcBit(iRetData_Plc[43], 2);
                    oPLC.PLC[0].Lifter.Floor10LocationCheck = GetPlcBit(iRetData_Plc[43], 3);
                    #endregion PLC -> PC
                }
                else
                {
                    bConnectPLC = false;
                }

              

                for (int i = 1; i <= buffer_count; i++)
                {
                    iRetData_Plc = new short[iPlcIdx[i]];

                    if (_plcHost.ReadBlock("DB"+(i+1)+".0", ref iRetData_Plc))
                    {
                        #region Conveyor PLC -> PC

                        oPLC.PLC[i].CV.Sno = iRetData_Plc[8].ToString();
                        oPLC.PLC[i].CV.Mode= iRetData_Plc[10];
                        oPLC.PLC[i].CV.CV_Status= iRetData_Plc[12];
                        oPLC.PLC[i].CV.AllowWriteCommand=GetPlcBit(iRetData_Plc[14], 0);
                        oPLC.PLC[i].CV.WriteCommandComplete = GetPlcBit(iRetData_Plc[14], 1);
                        oPLC.PLC[i].CV.ReadBCR = GetPlcBit(iRetData_Plc[14], 2);
                        oPLC.PLC[i].CV.WaitToRelease = GetPlcBit(iRetData_Plc[14], 3);
                        oPLC.PLC[i].CV.Presence = GetPlcBit(iRetData_Plc[14], 4);
                        oPLC.PLC[i].CV.StoreInInfo = GetPlcBit(iRetData_Plc[14],5);
                        oPLC.PLC[i].CV.AutoManual = GetPlcBit(iRetData_Plc[14],6);
                        oPLC.PLC[i].CV.Run = GetPlcBit(iRetData_Plc[14], 7);
                        oPLC.PLC[i].CV.Down = GetPlcBit(iRetData_Plc[15], 0);
                        oPLC.PLC[i].CV.idle = GetPlcBit(iRetData_Plc[15], 1);
                        oPLC.PLC[i].CV.Spare = GetPlcBit(iRetData_Plc[15], 2);
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
            if(bConnectPLC == false)
                return;
            short[] iRetData_Pc = new short[iPcIdx[0]];
            try
            {
                if(_plcHost.ReadBlock("DB1.0", ref iRetData_Pc))
                {
                    oPLC.PC[0].SYSTEM_PC.HandShake=GetPlcBit(iRetData_Pc[0], 0);
                    #region PC -> PLC
                    oPLC.PC[0].Lifter.Vehicle_ID = iRetData_Pc[20].ToString();
                    oPLC.PC[0].Lifter.CMDno=iRetData_Pc[22].ToString();
                    oPLC.PC[0].Lifter.Taskno=iRetData_Pc[24].ToString();
                    oPLC.PC[0].Lifter.WriteCommandComplete=GetPlcBit(iRetData_Pc[26], 1);
                    oPLC.PC[0].Lifter.CallToFloor1 = GetPlcBit(iRetData_Pc[26], 2);
                    oPLC.PC[0].Lifter.Floor1_CarMoveComplete = GetPlcBit(iRetData_Pc[26], 3);
                    oPLC.PC[0].Lifter.CallToFloor2 = GetPlcBit(iRetData_Pc[26], 4);
                    oPLC.PC[0].Lifter.Floor2_CarMoveComplete = GetPlcBit(iRetData_Pc[26], 5);
                    oPLC.PC[0].Lifter.CallToFloor3 = GetPlcBit(iRetData_Pc[26], 6);
                    oPLC.PC[0].Lifter.Floor3_CarMoveComplete = GetPlcBit(iRetData_Pc[26], 7);
                    oPLC.PC[0].Lifter.CallToFloor4 = GetPlcBit(iRetData_Pc[27], 0);
                    oPLC.PC[0].Lifter.Floor4_CarMoveComplete = GetPlcBit(iRetData_Pc[27], 1);
                    oPLC.PC[0].Lifter.CallToFloor5 = GetPlcBit(iRetData_Pc[27], 2);
                    oPLC.PC[0].Lifter.Floor5_CarMoveComplete = GetPlcBit(iRetData_Pc[27], 3);
                    oPLC.PC[0].Lifter.CallToFloor6 = GetPlcBit(iRetData_Pc[27], 4);
                    oPLC.PC[0].Lifter.Floor6_CarMoveComplete = GetPlcBit(iRetData_Pc[27], 5);
                    oPLC.PC[0].Lifter.CallToFloor7 = GetPlcBit(iRetData_Pc[27], 6);
                    oPLC.PC[0].Lifter.Floor7_CarMoveComplete = GetPlcBit(iRetData_Pc[27], 7);
                    oPLC.PC[0].Lifter.CallToFloor8 = GetPlcBit(iRetData_Pc[28], 0);
                    oPLC.PC[0].Lifter.Floor8_CarMoveComplete = GetPlcBit(iRetData_Pc[28], 1);
                    oPLC.PC[0].Lifter.CallToFloor9 = GetPlcBit(iRetData_Pc[28], 2);
                    oPLC.PC[0].Lifter.Floor9_CarMoveComplete = GetPlcBit(iRetData_Pc[28], 3);
                    oPLC.PC[0].Lifter.CallToFloor10 = GetPlcBit(iRetData_Pc[28], 4);
                    oPLC.PC[0].Lifter.Floor10_CarMoveComplete = GetPlcBit(iRetData_Pc[28], 5);

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
                        oPLC.PC[i].CV.Mode = iRetData_Pc[2];
                        oPLC.PC[i].CV.WriteCommandComplete =GetPlcBit(iRetData_Pc[6],1);
                        oPLC.PC[i].CV.ReadComplete = GetPlcBit(iRetData_Pc[6], 2);
                        oPLC.PC[i].CV.NoCommand = GetPlcBit(iRetData_Pc[6], 3);

                        #endregion

                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                }


            }
            catch(Exception ex)
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
                    if (_plcHost.Connect()==true)
                    {
                        bConnectPLC = true;
                    }
                    else
                    {
                        bConnectPLC = false;
                    }
                }
                else
                {
                    bConnectPLC = false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
               
            }
            finally
            {
                bThread_OpenPlc = false;
            }
        }

        public bool WriPlcPC_HandShaking(bool sValue)
        {
            bool bRet;
            string sAddr = "DB1.10.0";
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
                bool bRet = _plcHost.Write(sAddr,shortSetData);
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
                if(pingresult.Status.ToString() == "Success")
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
            bFlag = (iData & ((short)(Math.Pow(2, iBit)))) == 0 ? false : true;
            return bFlag;
        }

        private string GetAscii_LowByte(int iData)
        {
            int iTmp = int.Parse(Convert.ToString((iData % 256), 10));
            if(Convert.ToChar(iTmp) == '\0' || Convert.ToByte(Convert.ToChar(iTmp)) == 3)
                return "";
            else
            {
                return Convert.ToChar(iTmp).ToString();
            }
        }

        private string GetAscii_HiByte(int iData)
        {
            int iTmp = int.Parse(Convert.ToString((iData / 256), 10));
            if(Convert.ToChar(iTmp) == '\0' || Convert.ToByte(Convert.ToChar(iTmp)) == 3)
                return "";
            else
            {
                return Convert.ToChar(iTmp).ToString();
            }
        }
    }
}
