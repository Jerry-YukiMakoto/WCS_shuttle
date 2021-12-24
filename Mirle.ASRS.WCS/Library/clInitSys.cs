using System;
using System.Windows.Forms;
using Mirle.Def;
using Config.Net;
using Mirle.Def.U0NXMA30;
using Mirle.DataBase;
using Mirle.Structure;
using System.Text;
using System.Runtime.InteropServices;
using Mirle.Micron.U2NMMA30;

namespace Mirle.ASRS.WCS
{
    public class clInitSys
    {
        
        public static clsDbConfig DbConfig = new clsDbConfig();
        public static clsDbConfig DbConfig_WMS = new clsDbConfig();
        public static clsPlcConfig CV_Config = new clsPlcConfig();
        public static WebApiConfig WmsApi_Config = new WebApiConfig();
        public static WebApiConfig WcsApi_Config = new WebApiConfig();
        public static ASRSINI lcsini;
        public static string[] gsStockerID = new string[4];
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
                FunSysConfig(lcsini);
                FunApiConfig(lcsini);
                FunDeviceConfig(lcsini);
                FunPlcConfig(lcsini);
                FunLoadCVAlarmIni();
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

        private static void FunSysConfig(ASRSINI lcsini)
        {
            L2L_MaxCount = lcsini.System_Info.L2L_MaxCount;
        }
        

        private static void FunApiConfig(ASRSINI lcsini)
        {
            WmsApi_Config.IP = lcsini.WMS_API.IP;
            WcsApi_Config.IP = lcsini.WCS_API.IP;
        }

        private static void FunDeviceConfig(ASRSINI lcsini)
        {
            string[] adStocker = lcsini.Device.StockerID.Split(',');
            gsStockerID = new string[adStocker.Length];
            gsStockerID = adStocker;
        }

        private static void FunPlcConfig(ASRSINI lcsini)
        {
            switch (lcsini.CV.InMemorySimulator)
            {
                case 1:
                    CV_Config.InMemorySimulator = true;
                    break;
                default:
                    CV_Config.InMemorySimulator = false;
                    break;
            }

            CV_Config.MPLCIP = lcsini.CV.MPLCIP;
            CV_Config.MPLCNo = lcsini.CV.MPLCNo;
            CV_Config.MPLCPort = lcsini.CV.MPLCPort;
            CV_Config.MPLCTimeout = lcsini.CV.MPLCTimeout;

            switch (lcsini.CV.UseMCProtocol)
            {
                case 1:
                    CV_Config.UseMCProtocol = true;
                    break;
                default:
                    CV_Config.UseMCProtocol = false;
                    break;
            }

            switch (lcsini.CV.WritePLCRawData)
            {
                case 1:
                    CV_Config.WritePLCRawData = true;
                    break;
                default:
                    CV_Config.WritePLCRawData = false;
                    break;
            }

            CV_Config.CycleCount_Max = lcsini.CV.CycleCount_Max;
        }

        
        public static void FunLoadCVAlarmIni()
        {
            try
            {
                string strFileName = null;
                strFileName = Application.StartupPath + "\\Config\\CVAlarm.ini";

                if (!System.IO.File.Exists(strFileName))
                {
                    MessageBox.Show("找不到.ini資料，請洽系統管理人員 !!", "MIRLE", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Environment.Exit(0);
                }

                string strAppName = "", strKeyName = "";
                string[] bit = new string[32];
                for (int i=1; i <= clsMicronCV.GetBufferCount(); i++)
                {
                    strAppName = "A1-" + i.ToString().PadLeft(2, '0');
                    
                    for (int j = 0; j< 32; j++)
                    {
                        strKeyName = "bit" + j.ToString();
                        try
                        {
                            bit[j] = funReadParam(strFileName, strAppName, strKeyName);   
                        }
                        catch
                        {
                            bit[j] = "";
                        }
                                            
                    }
                    clsMicronCV.CV_Alarm.Add(i, bit);
                }
                 
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
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
