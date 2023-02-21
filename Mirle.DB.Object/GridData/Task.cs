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
    public class Task: IGrid
    {
        public void SubShowCmdtoGrid(ref DataGridView oGrid)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                oGrid.SuspendLayout();
                oGrid.Rows.Clear();
                int iRet = clsDB_Proc.GetDB_Object().GetCmd_Mst().FunGetTask_Grid(ref dtTmp);
                if (iRet == DBResult.Success)
                {
                    for (int i = 0; i < dtTmp.Rows.Count; i++)
                    {
                        oGrid.Rows.Add();
                        oGrid.Rows[oGrid.RowCount - 1].HeaderCell.Value = Convert.ToString(oGrid.RowCount);
                        oGrid[TaskColumnDef.Task.CommandID.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["CommandID"]);
                        oGrid[TaskColumnDef.Task.TaskNo.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["TaskNo"]);
                        oGrid[TaskColumnDef.Task.CMDState.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["CMDState"]);
                        oGrid[TaskColumnDef.Task.TaskState.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["TaskState"]);
                        oGrid[TaskColumnDef.Task.InitialDT.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["InitialDT"]);
                        oGrid[TaskColumnDef.Task.ActiveDT.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["ActiveDT"]);
                        oGrid[TaskColumnDef.Task.Source.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Source"]);
                        oGrid[TaskColumnDef.Task.Destination.Index, oGrid.Rows.Count - 1].Value = Convert.ToString(dtTmp.Rows[i]["Destination"]);
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
