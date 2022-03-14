//WCS回報給WMS命令完成

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Def;
using WCS_API_Client.ReportInfo;

namespace WCS_API_Client.Functions
{
    public class TaskStateUpdate
    {
        private WebApiConfig _config = new WebApiConfig();
        public TaskStateUpdate(WebApiConfig Config)
        {
            _config = Config;
        }
        public bool FunReport(TaskStateUpdateInfo info) 
        {
            try
            {
                string strJson = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                clsWriLog.Log.FunWriTraceLog_CV(strJson);
                string sLink = $"http://{_config.IP}/TaskStatesUpdate";
                clsWriLog.Log.FunWriTraceLog_CV($"URL: {sLink}");
                string re = clsTool.HttpPost(sLink, strJson);
                clsWriLog.Log.FunWriTraceLog_CV(re);
                var info_wms = (TaskStateUpdateReturnInfo)Newtonsoft.Json.Linq.JObject.Parse(re).ToObject(typeof(TaskStateUpdateReturnInfo));

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
