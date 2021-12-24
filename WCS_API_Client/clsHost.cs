using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirle.Def;
using WCS_API_Client.Functions;

namespace Mirle.WebAPI.U2NMMA30
{
    public class clsHost
    {
        private WebApiConfig _config = new WebApiConfig();
        private TaskStateUpdate taskStateUpdate;
        private WcsCancel wcsCancel;

        public clsHost(WebApiConfig Config)
        {
            _config = Config;
            taskStateUpdate = new TaskStateUpdate(_config);
            
        }
        public TaskStateUpdate GetTaskStateUpdate()
        {
            return taskStateUpdate;
        }
        public WcsCancel GetWcsCancel()
        {
            return wcsCancel;
        }
        
     }
}
