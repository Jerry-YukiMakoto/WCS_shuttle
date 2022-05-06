using System;
using System.Windows.Forms;
using Mirle.Def;
using Config.Net;
using Mirle.Def.U0NXMA30;
using Mirle.DataBase;
using Mirle.Structure;
using System.Text;
using System.Runtime.InteropServices;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using System.Collections.Generic;

namespace Mirle.ASRS.WCS.Library
{
    public class clInitSys
    {
        
        public static clsDbConfig DbConfig = new clsDbConfig();
        public static clsDbConfig DbConfig_WMS = new clsDbConfig();
        public static clsPlcConfig CV_Config = new clsPlcConfig();
        public static WebApiConfig WmsApi_Config = new WebApiConfig();
        public static WebApiConfig WcsApi_Config = new WebApiConfig();
        public static ASRSINI lcsini;
        public static string[] gsCraneID = new string[4];
        public static int L2L_MaxCount = 5;
        

        //API
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString
            (string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static void FunLoadIniSys()
        {
            try
            {
                lcsini = new ConfigurationBuilder<ASRSINI>()
                    .UseIniFile("Config\\ASRS.ini")
                    .Build();

                FunDbConfig(lcsini);
                FunApiConfig(lcsini);
                FunDeviceConfig(lcsini);
                FunPlcConfig(lcsini);
                FunStnNoConfig(lcsini);
                FunCVErrorConfig(lcsini);
            }
            catch(Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                MessageBox.Show("找不到.ini資料，請洽系統管理人員 !!", "MIRLE", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Environment.Exit(0);
            }
        }

        private static void FunDbConfig(ASRSINI lcsini)
        {
            DbConfig.CommandTimeOut = lcsini.Database.CommandTimeOut;
            DbConfig.ConnectTimeOut = lcsini.Database.ConnectTimeOut;
            DbConfig.DbName = lcsini.Database.DataBase;
            DbConfig.DbPassword = lcsini.Database.DbPswd;
            DbConfig.DbServer = lcsini.Database.DbServer;
            DbConfig.DBType = (DBTypes)Enum.Parse(typeof(DBTypes), lcsini.Database.DBMS);
            DbConfig.DbUser = lcsini.Database.DbUser;
            DbConfig.FODBServer = lcsini.Database.FODbServer;
            DbConfig.WriteLog = true;

            DbConfig_WMS.CommandTimeOut = lcsini.Database_WMS.CommandTimeOut;
            DbConfig_WMS.ConnectTimeOut = lcsini.Database_WMS.ConnectTimeOut;
            DbConfig_WMS.DbName = lcsini.Database_WMS.DataBase;
            DbConfig_WMS.DbPassword = lcsini.Database_WMS.DbPswd;
            DbConfig_WMS.DbServer = lcsini.Database_WMS.DbServer;
            DbConfig_WMS.DBType = (DBTypes)Enum.Parse(typeof(DBTypes), lcsini.Database_WMS.DBMS);
            DbConfig_WMS.DbUser = lcsini.Database_WMS.DbUser;
            DbConfig_WMS.FODBServer = lcsini.Database_WMS.FODbServer;
            DbConfig_WMS.WriteLog = true;
        }

       
        

        private static void FunApiConfig(ASRSINI lcsini)
        {
            WmsApi_Config.IP = lcsini.WMS_API.IP;
            WcsApi_Config.IP = lcsini.WCS_API.IP;
        }

        private static void FunDeviceConfig(ASRSINI lcsini)
        {
            string[] adCrane = lcsini.Device.CraneID.Split(',');
            gsCraneID = new string[adCrane.Length];
            gsCraneID = adCrane;
        }

        private static void FunPlcConfig(ASRSINI lcsini)
        {
            CV_Config.InMemorySimulator = lcsini.CV.InMemorySimulator == 1 ? true : false;
            CV_Config.MPLCIP = lcsini.CV.MPLCIP;
            CV_Config.MPLCNo = lcsini.CV.MPLCNo;
            CV_Config.MPLCPort = lcsini.CV.MPLCPort;
            CV_Config.MPLCTimeout = lcsini.CV.MPLCTimeout;
            CV_Config.UseMCProtocol = lcsini.CV.UseMCProtocol == 1 ? true : false;
        }

        private static void FunStnNoConfig(ASRSINI lcsini) 
        {
            string str = "000000";
            CranePortNo.A1 = str + lcsini.StnNo.A1;
            CranePortNo.A5 = str + lcsini.StnNo.A5;
            CranePortNo.A7 = str + lcsini.StnNo.A7;
            CranePortNo.A9 = str + lcsini.StnNo.A9;
        }

        private static void FunCVErrorConfig(ASRSINI lcsini)
        {
            Dictionary<int, string> bitError = new Dictionary<int, string>();
            Dictionary<int, string> A2bitError = new Dictionary<int, string>();
            Dictionary<int, string> A4bitError = new Dictionary<int, string>();
            Dictionary<int, string> PortStnbitError = new Dictionary<int, string>();

            bitError.Add(0, lcsini.CVError.bit0);
            bitError.Add(1, lcsini.CVError.bit1);
            bitError.Add(2, lcsini.CVError.bit2);
            bitError.Add(3, lcsini.CVError.bit3);
            bitError.Add(4, lcsini.CVError.bit4);
            bitError.Add(5, lcsini.CVError.bit5);
            bitError.Add(6, lcsini.CVError.bit6);
            bitError.Add(7, lcsini.CVError.bit7);
            bitError.Add(8, lcsini.CVError.bit8);
            bitError.Add(9, lcsini.CVError.bit9);
            bitError.Add(10, lcsini.CVError.bitA);
            bitError.Add(11, lcsini.CVError.bitB);
            bitError.Add(12, lcsini.CVError.bitC);

            CVErrorDefine.bitError=bitError;

            A2bitError.Add(0, lcsini.CVError.bit0);
            A2bitError.Add(1, lcsini.CVError.A2bit1);
            A2bitError.Add(2, lcsini.CVError.A2bit2);
            A2bitError.Add(3, lcsini.CVError.A2bit3);
            A2bitError.Add(4, lcsini.CVError.A2bit4);
            A2bitError.Add(5, lcsini.CVError.A2bit5);
            A2bitError.Add(6, lcsini.CVError.A2bit6);
            A2bitError.Add(7, lcsini.CVError.A2bit7);
            A2bitError.Add(8, lcsini.CVError.A2bit8);
            A2bitError.Add(9, lcsini.CVError.bit9);
            A2bitError.Add(10, lcsini.CVError.bitA);
            A2bitError.Add(11, lcsini.CVError.bitB);
            A2bitError.Add(12, lcsini.CVError.bitC);

            CVErrorDefine.A2bitError = A2bitError;

            A4bitError.Add(0, lcsini.CVError.bit0);
            A4bitError.Add(1, lcsini.CVError.A4bit1);
            A4bitError.Add(2, lcsini.CVError.bit2);
            A4bitError.Add(3, lcsini.CVError.A4bit3);
            A4bitError.Add(4, lcsini.CVError.A4bit4);
            A4bitError.Add(5, lcsini.CVError.A4bit5);
            A4bitError.Add(6, lcsini.CVError.bit6);
            A4bitError.Add(7, lcsini.CVError.bit7);
            A4bitError.Add(8, lcsini.CVError.bit8);
            A4bitError.Add(9, lcsini.CVError.bit9);
            A4bitError.Add(10, lcsini.CVError.bitA);
            A4bitError.Add(11, lcsini.CVError.bitB);
            A4bitError.Add(12, lcsini.CVError.bitC);

            CVErrorDefine.A4bitError = A4bitError;


            PortStnbitError.Add(0, lcsini.CVError.UPLVbit0); 
            PortStnbitError.Add(1, lcsini.CVError.bit1);
            PortStnbitError.Add(2, lcsini.CVError.bit2);
            PortStnbitError.Add(3, lcsini.CVError.bit3);
            PortStnbitError.Add(4, lcsini.CVError.bit4);
            PortStnbitError.Add(5, lcsini.CVError.bit5);
            PortStnbitError.Add(6, lcsini.CVError.bit6);
            PortStnbitError.Add(7, lcsini.CVError.bit7);
            PortStnbitError.Add(8, lcsini.CVError.bit8);
            PortStnbitError.Add(9, lcsini.CVError.bit9);
            PortStnbitError.Add(10, lcsini.CVError.bitA);
            PortStnbitError.Add(11, lcsini.CVError.bitB);
            PortStnbitError.Add(12, lcsini.CVError.bitC);

            CVErrorDefine.PortStnbitError = PortStnbitError;
        }

        /// <summary>
        /// 讀取ini檔的單一欄位
        /// </summary>
        /// <param name="sFileName">INI檔檔名</param>
        /// <param name="sAppName">區段名</param>
        /// <param name="sKeyName">KEY名稱</param>
        /// <param name="strDefault">Default</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string funReadParam(string sFileName, string sAppName, string sKeyName, string strDefault = "")
        {
            StringBuilder sResult = new StringBuilder(255);
            int intResult;
            try
            {
                intResult = GetPrivateProfileString(sAppName, sKeyName, strDefault, sResult, sResult.Capacity, sFileName);
                string R = sResult.ToString().Trim();
                return R;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return strDefault;
            }
        }
    }
}
