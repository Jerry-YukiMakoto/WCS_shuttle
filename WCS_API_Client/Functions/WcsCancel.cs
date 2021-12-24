using System;
using System.Threading.Tasks;
using Mirle.Def;
using WCS_API_Client.ReportInfo;

namespace WCS_API_Client.Functions
{
    public class WcsCancel
    {
        private WebApiConfig _config = new WebApiConfig();
        public WcsCancel(WebApiConfig Config)
        {
            _config = Config;
        }
        public bool FunReport(WcsCancelInfo info)
        {
            try
            {
                string strJson = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                clsWriLog.Log.FunWriTraceLog_CV(strJson);
                string sLink = $"http://{_config.IP}/WCS_CANCEL";
                clsWriLog.Log.FunWriTraceLog_CV($"URL: {sLink}");
                string re = clsTool.HttpPost(sLink, strJson);
                clsWriLog.Log.FunWriTraceLog_CV(re);

                return true;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
    }
}
