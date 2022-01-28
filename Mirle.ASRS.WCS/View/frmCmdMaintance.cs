using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Mirle.DataBase;
using Mirle.DB.Object;
using System.Windows.Forms;
using Mirle.Grid.U0NXMA30;

namespace Mirle.ASRS.WCS.View
{
    public partial class frmCmdMaintance : Form
    {
        public frmCmdMaintance()
        {
            InitializeComponent();
        }

        private void frmCmdMaintance_Load(object sender, EventArgs e)
        {
            GridInit();
            btnQuery.PerformClick();
        }

        #region Grid顯示
        private void GridInit()
        {
            Grid.clInitSys.GridSysInit(ref GridCmd);
            ColumnDef.CMD_MST.GridSetLocRange(ref GridCmd);
        }

        delegate void degShowCmdtoGrid(ref DataGridView oGrid);
        private void SubShowCmdtoGrid(ref DataGridView oGrid)
        {
            degShowCmdtoGrid obj;
            try
            {
                if (InvokeRequired)
                {
                    obj = new degShowCmdtoGrid(SubShowCmdtoGrid);
                    Invoke(obj, oGrid);
                }
                else
                {
                    Grid.IGrid grid;
                    grid = new DB.Object.GridData.CmdMst();
                    grid.SubShowCmdtoGrid(ref oGrid);
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                Library.clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
        }


        #endregion Grid顯示

        private void btnQuery_Click(object sender, EventArgs e)
        {
            btnQuery.Enabled = false;
            try
            {
                SubShowCmdtoGrid(ref GridCmd);
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
            }
            finally
            {
                btnQuery.Enabled = true;
            }
        }

    }
}
