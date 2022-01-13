using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirle.Def;
using WCS_API_Client.Functions;

namespace WCS_API_Client
{
    public class clsHost
    {
        private WebApiConfig _config = new WebApiConfig();
        private TaskStateUpdate taskStateUpdate;
        private DisplayTaskStatus displayTaskStatus;
        private StackPalletsIn stackPalletsIn;
        private StackPalletsOut stackPalletsOut;
        

        public clsHost(WebApiConfig Config)
        {
            _config = Config;
            taskStateUpdate = new TaskStateUpdate(_config);
            displayTaskStatus = new DisplayTaskStatus(_config);
            stackPalletsIn = new StackPalletsIn(_config);
            stackPalletsOut = new StackPalletsOut(_config);
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
            return stackPalletsIn;
        }

        public StackPalletsOut GetStackPalletsOut()
        {
            return stackPalletsOut;
        }

    }
}
