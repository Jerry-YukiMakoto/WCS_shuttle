using System;
using Mirle.Def;
using Mirle.Micron.U2NMMA30;
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
                backupPortId = Convert.ToString(dtTmp.Rows[0]["backupPortId"]),
                BatchID = Convert.ToString(dtTmp.Rows[0]["BatchID"]),
                BoxID = Convert.ToString(dtTmp.Rows[0]["BoxId"]),
                CmdMode = Convert.ToString(dtTmp.Rows[0]["CmdMode"]),
                CmdSno = Convert.ToString(dtTmp.Rows[0]["CmdSno"]),
                CmdSts = Convert.ToString(dtTmp.Rows[0]["CmdSts"]),
                CrtDate = Convert.ToString(dtTmp.Rows[0]["CrtDate"]),
                CurDeviceID = Convert.ToString(dtTmp.Rows[0]["CurDeviceID"]),
                CurLoc = Convert.ToString(dtTmp.Rows[0]["CurLoc"]),
                EndDate = Convert.ToString(dtTmp.Rows[0]["EndDate"]),
                EquNo = Convert.ToString(dtTmp.Rows[0]["EquNO"]),
                ExpDate = Convert.ToString(dtTmp.Rows[0]["ExpDate"]),
                IoType = Convert.ToString(dtTmp.Rows[0]["Iotype"]),
                JobID = Convert.ToString(dtTmp.Rows[0]["JobID"]),
                Loc = Convert.ToString(dtTmp.Rows[0]["Loc"]),
                NeedShelfToShelf = Convert.ToString(dtTmp.Rows[0]["NeedShelfToShelf"]),
                NewLoc = Convert.ToString(dtTmp.Rows[0]["NewLoc"]),
                Prt = Convert.ToString(dtTmp.Rows[0]["PRT"]),
                Remark = Convert.ToString(dtTmp.Rows[0]["Remark"]),
                StnNo = Convert.ToString(dtTmp.Rows[0]["StnNo"]),
                Userid = Convert.ToString(dtTmp.Rows[0]["UserID"]),
                ZoneID = Convert.ToString(dtTmp.Rows[0]["ZoneID"]),
                ticketId = Convert.ToString(dtTmp.Rows[0]["ticketId"])
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
