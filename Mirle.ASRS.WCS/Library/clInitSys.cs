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

            //DbConfig_WMS.CommandTimeOut = lcsini.Database_WMS.CommandTimeOut;
            //DbConfig_WMS.ConnectTimeOut = lcsini.Database_WMS.ConnectTimeOut;
            //DbConfig_WMS.DbName = lcsini.Database_WMS.DataBase;
            //DbConfig_WMS.DbPassword = lcsini.Database_WMS.DbPswd;
            //DbConfig_WMS.DbServer = lcsini.Database_WMS.DbServer;
            //DbConfig_WMS.DBType = (DBTypes)Enum.Parse(typeof(DBTypes), lcsini.Database_WMS.DBMS);
            //DbConfig_WMS.DbUser = lcsini.Database_WMS.DbUser;
            //DbConfig_WMS.FODBServer = lcsini.Database_WMS.FODbServer;
            //DbConfig_WMS.WriteLog = true;
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
