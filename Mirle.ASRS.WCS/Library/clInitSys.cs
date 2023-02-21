using System;
using System.Windows.Forms;
using Mirle.Def;
using Config.Net;
using Mirle.DataBase;
using Mirle.Structure;
using System.Text;
using System.Runtime.InteropServices;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using System.Collections.Generic;
using HslCommunicationPLC.Siemens;
using Mirle.Def.T26YGAP0;

namespace Mirle.ASRS.WCS.Library
{
    public class clInitSys
    {

        public static clsDbConfig DbConfig = new clsDbConfig();
        public static clsDbConfig DbConfig_WMS = new clsDbConfig();
        public static PlcConfig CV_Config = new PlcConfig();
        public static WebApiConfig WmsApi_Config = new WebApiConfig();
        public static WebApiConfig WcsApi_Config = new WebApiConfig();
        public static ASRSINI lcsini;
        public static string[] gsCraneID = new string[4];
        public static int L2L_MaxCount = 5;
        public static string SHC_IP;
        public static int SHC_port;
        public static string STN_NO;
        public static string BCR_IP;


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
                //FunApiConfig(lcsini);
                FunDeviceConfig(lcsini);
                FunPlcConfig(lcsini);
                FunStnNoConfig(lcsini);
            }
            catch (Exception ex)
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

        }




        //private static void FunApiConfig(ASRSINI lcsini)
        //{
        //    WmsApi_Config.IP = lcsini.WMS_API.IP;
        //    WcsApi_Config.IP = lcsini.WCS_API.IP;
        //}

        private static void FunDeviceConfig(ASRSINI lcsini)
        {
            string[] adCrane = lcsini.Device.CraneID.Split(',');
            gsCraneID = new string[adCrane.Length];
            gsCraneID = adCrane;
        }

        private static void FunPlcConfig(ASRSINI lcsini)
        {
            CV_Config.IpAddress = lcsini.CV.IP;
            SHC_IP = lcsini.CV.SHC_IP;
            SHC_port =lcsini.CV.SHC_PORT;
            ASRS_Setting.BCR_IP_1 = lcsini.CV.BCR_IP_1;
            ASRS_Setting.BCR_IP_2 = lcsini.CV.BCR_IP_2;
            ASRS_Setting.BCR_port = lcsini.CV.BCR_port;
        }

        private static void FunStnNoConfig(ASRSINI lcsini)
        {
            ASRS_Setting.STNNO_1F_left = lcsini.StnNo_Suttle.STNL;
            ASRS_Setting.STNNO_1F_right = lcsini.StnNo_Suttle.STNR;
            ASRS_Setting.STNNO_1F_L = lcsini.StnNo.STNL;
            ASRS_Setting.STNNO_1F_R=lcsini.StnNo.STNR;
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
