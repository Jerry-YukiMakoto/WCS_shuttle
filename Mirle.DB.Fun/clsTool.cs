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
                taskNo = Convert.ToString(dtTmp.Rows[0]["taskNo"]),
                CmdMode = Convert.ToString(dtTmp.Rows[0]["CmdMode"]),
                CmdSts = Convert.ToString(dtTmp.Rows[0]["CmdSts"]),
                EquNo = Convert.ToString(dtTmp.Rows[0]["EquNO"]),
                IoType = Convert.ToString(dtTmp.Rows[0]["Iotype"]),
                Loc = Convert.ToString(dtTmp.Rows[0]["Loc"]),
                NewLoc = Convert.ToString(dtTmp.Rows[0]["NewLoc"]),
                Prt = Convert.ToString(dtTmp.Rows[0]["PRT"]),
                StnNo = Convert.ToString(dtTmp.Rows[0]["StnNo"]),
                Userid = Convert.ToString(dtTmp.Rows[0]["UserID"]),
                Remark = Convert.ToString(dtTmp.Rows[0]["Remark"]),
                CrtDate = Convert.ToString(dtTmp.Rows[0]["CrtDate"]),
                ExpDate = Convert.ToString(dtTmp.Rows[0]["ExpDate"]),
                EndDate = Convert.ToString(dtTmp.Rows[0]["EndDate"])
                
            };

            return cmd;
        }

        //public int CheckLocDDHasNeedL2LCmd(CmdMstInfo cmd, string sDeviceID, string sCurLoc, ref string sCmdSno_DD, SqlServer db)
        //{
        //    try
        //    {
        //        return proc.CheckLocDDHasNeedL2LCmd(cmd, sDeviceID, sCurLoc, ref sCmdSno_DD, db);
        //    }
        //    catch (Exception ex)
        //    {
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
        //        return DBResult.Exception;
        //    }
        //}
    }
}
