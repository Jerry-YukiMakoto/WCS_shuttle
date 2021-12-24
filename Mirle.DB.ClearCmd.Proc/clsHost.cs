using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.DB.ClearCmd.Proc
{
    public class clsHost
    {
        private clsMoveCmdInHistory_Proc moveCmdInHistory_Proc = new clsMoveCmdInHistory_Proc();
        private clsDelCmdMst_His_Proc delCmdMst_His_Proc = new clsDelCmdMst_His_Proc();
        private clsDelHisTask_Proc delHisTask_Proc = new clsDelHisTask_Proc();
        public clsHost()
        {
            moveCmdInHistory_Proc.subStart();
            delCmdMst_His_Proc.subStart();
            delHisTask_Proc.subStart();
        }
    }
}
