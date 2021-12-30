using System;
using Mirle.Def;
using System.Linq;
using System.Windows.Forms;
using Mirle.Def.U0NXMA30;
using Mirle.Grid.U0NXMA30;

using System.Data;
using Mirle.DB.Object;
using Mirle.DataBase;

namespace Mirle.ASRS.WCS.Library
{
    public class clsTool
    {
        //public static bool FunCheckIsCanGo(string sStnNo, DataGridView oGrid)
        //{
        //    try
        //    {
        //        string[] sLocIn = Enum.GetNames(typeof(LocationDef.LocationIn));

        //        var buffer = clsMicronCV.GetBufferByStnNo(sStnNo);
        //        int iCount = (from DataGridViewRow drv in oGrid.Rows
        //                      where Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CmdMode.Index].Value) == clsConstValue.CmdMode.StockOut &&
        //                            Convert.ToString(drv.Cells[ColumnDef.CMD_MST.StnNo.Index].Value) == sStnNo &&
        //                            Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value) == clsConstValue.CmdSts.strCmd_Running &&
        //                            !(sLocIn.Contains(Convert.ToString(drv.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value)))
        //                      select drv).Count();
        //        if (iCount >= buffer.WaterLevel) return false;
        //        else return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
        //        var cmet = System.Reflection.MethodBase.GetCurrentMethod();
        //        clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
        //        return false;
        //    }
        //}
    }
}
