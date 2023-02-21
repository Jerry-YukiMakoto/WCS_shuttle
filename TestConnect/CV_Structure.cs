using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLCConfigSetting.PLCsetting
{
    public class CV_Structure
    {
        public Pc[] PC = new Pc[5];
        public Plc[] PLC = new Plc[5];
        public CV_Structure()
        {
            for (int i = 0; i <= PLC.Length - 1; i++)
            {
                PLC[i] = new Plc();
            }
            for (int i = 0; i <= PC.Length - 1; i++)
            {
                PC[i] = new Pc();
            }

        }

        #region PLC Structure
        public class Plc
        {
            public SystemInfo_Plc SYSTEM_PLC = new SystemInfo_Plc();
            public PLC_CV CV = new PLC_CV();
            public PLC_Lifter Lifter = new PLC_Lifter();

        }

        public class SystemInfo_Plc
        {

            public bool HandShake = false;
            public bool PLCmode = true;

        }

        /// <summary>
        /// 设备区异常訊號
        /// </summary>
        public class ErrorCode_Bit
        {
            public bool[] BufferError = new bool[16];
        }

        /// <summary>
        /// 系统区异常訊號
        /// </summary>
        public class SystemError_Bit
        {
            public bool[] SysError = new bool[16];
        }


        #endregion PLC Structure

        public class Pc
        {
            public SystemInfo_Pc SYSTEM_PC = new SystemInfo_Pc();
            public PC_CV CV = new PC_CV();
            public PC_Lifter Lifter = new PC_Lifter();
        }

        public class SystemInfo_Pc
        {
            public bool HandShake = false;
        }

        public class PLC_CV
        {
            public string Sno = "";
            /// <summary>
            ///模式, (1=入庫 2=出庫 3=揀貨 4=盤點)
            /// </summary>
            public int Mode = 0;
            /// <summary>
            ///模式, (1=入庫 2=出庫 5=空料盒入庫)
            /// </summary>
            public int CV_Status = 0;
            /// <summary>
            ///允許寫入命令
            /// </summary>
            public bool AllowWriteCommand = false;
            /// <summary>
            ///寫入命令完成
            /// </summary>
            public bool WriteCommandComplete = false;
            public bool ReadBCR = false;
            /// <summary>
            ///等待放行
            /// </summary>
            public bool WaitToRelease = false;
            public bool Presence = false;
            /// <summary>
            ///入庫資訊(料盒)
            /// </summary>
            public bool StoreInInfo = false;
            /// <summary>
            ///狀態(ON：自動，OFF：手動)
            /// </summary>
            public bool AutoManual = false;
            public bool Run = false;
            public bool Down = false;
            public bool idle = false;
            public bool Spare = false;
        }

        public class PC_CV
        {
            public string Sno = "";
            /// <summary>
            ///模式, (1=入庫 2=出庫 3=揀貨 4=盤點)
            /// </summary>
            public int Mode = 0;
            /// <summary>
            ///寫入命令完成
            /// </summary>
            public bool WriteCommandComplete = false;
            /// <summary>
            ///放行許可(讀取完成)
            /// </summary>
            public bool ReadComplete = false;
            /// <summary>
            ///無命令
            /// </summary>
            public bool NoCommand = false;
        }

        public class PC_Lifter
        {
            public string Vehicle_ID = "";
            public string CMDno = "";
            public string Taskno = "";
            public bool WriteCommandComplete = false;

            public bool CallToFloor1 = false;
            public bool Floor1_CarMoveComplete = false;
            public bool CallToFloor2 = false;
            public bool Floor2_CarMoveComplete = false;
            public bool CallToFloor3 = false;
            public bool Floor3_CarMoveComplete = false;
            public bool CallToFloor4 = false;
            public bool Floor4_CarMoveComplete = false;
            public bool CallToFloor5 = false;
            public bool Floor5_CarMoveComplete = false;
            public bool CallToFloor6 = false;
            public bool Floor6_CarMoveComplete = false;
            public bool CallToFloor7 = false;
            public bool Floor7_CarMoveComplete = false;
            public bool CallToFloor8 = false;
            public bool Floor8_CarMoveComplete = false;
            public bool CallToFloor9 = false;
            public bool Floor9_CarMoveComplete = false;
            public bool CallToFloor10 = false;
            public bool Floor10_CarMoveComplete = false;
            public bool CallToFloor11 = false;
            public bool Floor11_CarMoveComplete = false;

        }

        public class PLC_Lifter
        {

            public string Vehicle_ID = "";
            public string CMDno = "";
            public string Taskno = "";
            public bool AllowWriteCommand = false;
            public bool WriteCommandComplete = false;

            public bool MoveToFloor1 = false;
            public bool Floor1_SafetyCheck = false;
            public bool MoveToFloor2 = false;
            public bool Floor2_SafetyCheck = false;
            public bool MoveToFloor3 = false;
            public bool Floor3_SafetyCheck = false;
            public bool MoveToFloor4 = false;
            public bool Floor4_SafetyCheck = false;
            public bool MoveToFloor5 = false;
            public bool Floor5_SafetyCheck = false;
            public bool MoveToFloor6 = false;
            public bool Floor6_SafetyCheck = false;
            public bool MoveToFloor7 = false;
            public bool Floor7_SafetyCheck = false;
            public bool MoveToFloor8 = false;
            public bool Floor8_SafetyCheck = false;
            public bool MoveToFloor9 = false;
            public bool Floor9_SafetyCheck = false;
            public bool MoveToFloor10 = false;
            public bool Floor10_SafetyCheck = false;
            public bool MoveToFloor11 = false;//裝卸層
            public bool Floor11_SafetyCheck = false;//裝卸層

            /// <summary>
            /// LIFT自動模式(ON：自動，OFF：手動)
            /// </summary>
            public bool LiftMode = false;
            public bool LiftRun = false;
            public bool LiftDown = false;
            public bool LiftIdle = false;
            /// <summary>
            ///Elevator Position(1~11, 11為裝卸層)
            /// </summary>
            public int LiftPosition = 0;
            public bool presence_shuttle = false;
            /// <summary>
            ///裝卸層位置確認
            /// </summary>
            public bool UnloadingLocationCheck = false;
            public bool Floor1LocationCheck = false;
            public bool Floor2LocationCheck = false;
            public bool Floor3LocationCheck = false;
            public bool Floor4LocationCheck = false;
            public bool Floor5LocationCheck = false;
            public bool Floor6LocationCheck = false;
            public bool Floor7LocationCheck = false;
            public bool Floor8LocationCheck = false;
            public bool Floor9LocationCheck = false;
            public bool Floor10LocationCheck = false;
        }
    }
}
