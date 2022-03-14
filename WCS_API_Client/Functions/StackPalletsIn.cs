using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Def;
using WCS_API_Client.ReportInfo;

namespace WCS_API_Client.Functions
{
    public class StackPalletsIn
    {
        private WebApiConfig _config = new WebApiConfig();
        public StackPalletsIn(WebApiConfig Config)
        {
            _config = Config;
        }
        public bool FunReport(StackPalletsInInfo info)
        {
            try
            {
                string strJson = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                clsWriLog.Log.FunWriTraceLog_CV(strJson);
                string sLink = $"http://{_config.IP}/StackPalletsIn";
                clsWriLog.Log.FunWriTraceLog_CV($"URL: {sLink}");
                string re = clsTool.HttpPost(sLink, strJson);
                clsWriLog.Log.FunWriTraceLog_CV(re);
                var info_wms = (ReturnMsgInfo)Newtonsoft.Json.Linq.JObject.Parse(re).ToObject(typeof(ReturnMsgInfo));

                if (info_wms.success) return true;
                else return false;
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
