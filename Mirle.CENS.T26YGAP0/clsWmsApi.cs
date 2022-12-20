﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCS_API_Client;
using WCS_API_Client.ReportInfo;
using Mirle.Def;
using Mirle.Structure;
using Mirle.ASRS.Conveyors.Signal;
using Mirle.Def.T26YGAP0;

namespace Mirle.CENS.T26YGAP0
{
    public class clsWmsApi
    {
        private static clsHost report;

        public static void FunInit(WebApiConfig config)
        {
            report = new clsHost(config);
        }

        public static TaskStateUpdateInfo GetTaskStateUpdateInfo(CmdMstInfo cmd, BufferDefine buffer)
        {
            TaskStateUpdateInfo info = new TaskStateUpdateInfo
            {
                taskNo = cmd.CmdSno,
                bussinessType = cmd.IoType,
                state = StatusDef.moveTaskFinished, 
            };

            return info;
        }

        public static clsHost GetApiProcess()
        {
            return report;
        }

    }
}