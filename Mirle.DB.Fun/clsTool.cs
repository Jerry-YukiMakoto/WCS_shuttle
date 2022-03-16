using System;
using Mirle.Def;
using Mirle.Structure;
using Mirle.DataBase;
using System.Data;

namespace Mirle.DB.Fun
{
    public class clsTool
    {
        public CmdMstInfo GetCommand(DataTable dtTmp)
        {
            CmdMstInfo cmd = new CmdMstInfo
            {
                CmdSno = Convert.ToString(dtTmp.Rows[0]["CmdSno"]),
                CmdMode = Convert.ToString(dtTmp.Rows[0]["CmdMode"]),
                CmdSts = Convert.ToString(dtTmp.Rows[0]["CmdSts"]),
                taskNo = Convert.ToString(dtTmp.Rows[0]["CmdSno"]),
                EquNo = Convert.ToString(dtTmp.Rows[0]["EquNO"]),
                IoType = Convert.ToString(dtTmp.Rows[0]["Iotype"]),
                Loc = Convert.ToString(dtTmp.Rows[0]["Loc"]),
                NewLoc = Convert.ToString(dtTmp.Rows[0]["NewLoc"]),
                //Prt = Convert.ToString(dtTmp.Rows[0]["PRT"]),
                StnNo = Convert.ToString(dtTmp.Rows[0]["StnNo"]),
                Userid = Convert.ToString(dtTmp.Rows[0]["UserID"]),
                Remark = Convert.ToString(dtTmp.Rows[0]["Remark"]),
                CrtDate = Convert.ToString(dtTmp.Rows[0]["CrtDate"]),
                ExpDate = Convert.ToString(dtTmp.Rows[0]["ExpDate"]),
                EndDate = Convert.ToString(dtTmp.Rows[0]["EndDate"])
                
            };

            return cmd;
        }

        
    }
}
