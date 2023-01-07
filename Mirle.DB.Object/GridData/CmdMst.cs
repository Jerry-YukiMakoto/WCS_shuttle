using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Mirle.DataBase;
using Mirle.Grid;
using Mirle.Grid.T26YGAP0;

namespace Mirle.DB.Object.GridData
{
    public class CmdMst: IGrid
    {
        public void SubShowCmdtoGrid(ref DataGridView oGrid)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                oGrid.SuspendLayout();
                oGrid.Rows.Clear();
                int iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunGetCmdMst_Grid(ref dtTmp);
                if (iRet == DBResult.Success)
                {
                    for (int i = 0; i < dtTmp.Rows.Count; i++)
                    {
                        oGrid.Rows.Add();
                        oGrid.Rows[oGrid.RowCount - 1].HeaderCell.Value = Convert.ToString(oGrid.RowCount);
                        oGrid[ColumnDef.CMD_MST.CmdSno.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Cmd_Sno"]);
                        oGrid[ColumnDef.CMD_MST.LOC_ID.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Loc_ID"]);
                        oGrid[ColumnDef.CMD_MST.CmdSts.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Cmd_Sts"]);
                        oGrid[ColumnDef.CMD_MST.CmdMode.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Cmd_Mode"]);
                        oGrid[ColumnDef.CMD_MST.Trace.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Trace"]);
                        oGrid[ColumnDef.CMD_MST.EquNO.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Equ_No"]);
                        oGrid[ColumnDef.CMD_MST.StnNo.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Stn_No"]);
                        //oGrid[ColumnDef.CMD_MST.palletNo.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["palletNo"]);
                        //oGrid[ColumnDef.CMD_MST.whetherAllout.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["whetherAllout"]);
                        //oGrid[ColumnDef.CMD_MST.lastPallet.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["lastPallet"]);
                        oGrid[ColumnDef.CMD_MST.Loc.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Loc"]);
                        oGrid[ColumnDef.CMD_MST.NewLoc.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["New_Loc"]);
                        oGrid[ColumnDef.CMD_MST.Remark.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Remark"]);
                        oGrid[ColumnDef.CMD_MST.CrtDate.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Crt_Date"]);
                        oGrid[ColumnDef.CMD_MST.ExpDate.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Exp_Date"]);
                        oGrid[ColumnDef.CMD_MST.EndDate.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["End_Date"]);
                    }
                }
                oGrid.ResumeLayout();
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
            finally
            {
                dtTmp = null;
            }
        }
    }
}
