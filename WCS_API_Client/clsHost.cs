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
        private DisplayTaskStatus displayTaskStatus;
        private StackPalletsIn StackPalletsIn;
        private StackPalletsOut StackPalletsOut;
        

        public clsHost(WebApiConfig Config)
        {
            _config = Config;
            taskStateUpdate = new TaskStateUpdate(_config);
            displayTaskStatus = new DisplayTaskStatus(_config);
            StackPalletsIn = new StackPalletsIn(_config);
            StackPalletsOut = new StackPalletsOut(_config);
        }
        public TaskStateUpdate GetTaskStateUpdate()
        {
            return taskStateUpdate;
        }
        public DisplayTaskStatus GetDisplayTaskStatus()
        {
            return displayTaskStatus;
        }

        public StackPalletsIn GetStackPalletsIn()
        {
            return StackPalletsIn;
        }

        public StackPalletsOut GetPalletsOut()
        {
            return StackPalletsOut;
        }

    }
}
