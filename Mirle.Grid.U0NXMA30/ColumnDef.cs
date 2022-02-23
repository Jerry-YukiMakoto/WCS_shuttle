using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Mirle.Grid;

namespace Mirle.Grid.U0NXMA30
{
    public class ColumnDef
    {
        public class CMD_MST
        {
            public static readonly ColumnInfo CmdSno = new ColumnInfo { Index = 0, Name = "任務號", Width = 68 };
            public static readonly ColumnInfo CmdSts = new ColumnInfo { Index = 1, Name = "命令狀態", Width = 40 };
            public static readonly ColumnInfo CmdMode = new ColumnInfo { Index = 2, Name = "模式", Width = 40 };
            public static readonly ColumnInfo Trace = new ColumnInfo { Index = 3, Name = "Trace", Width = 40 };
            public static readonly ColumnInfo EquNO = new ColumnInfo { Index = 4, Name = "設備編號", Width = 60 };
            public static readonly ColumnInfo StnNo = new ColumnInfo { Index = 5, Name = "站口", Width = 60 };
            public static readonly ColumnInfo whetherAllout = new ColumnInfo { Index = 6, Name = "整板出庫", Width = 40 };
            public static readonly ColumnInfo lastPallet = new ColumnInfo { Index = 7, Name = "是否尾板", Width = 40 };
            public static readonly ColumnInfo Loc = new ColumnInfo { Index = 8, Name = "儲位", Width = 68 };
            public static readonly ColumnInfo NewLoc = new ColumnInfo { Index = 9, Name = "新儲位", Width = 68 };
            public static readonly ColumnInfo Remark = new ColumnInfo { Index = 10, Name = "說明", Width = 200 };
            public static readonly ColumnInfo CrtDate = new ColumnInfo { Index = 11, Name = "產生時間", Width = 200 };
            public static readonly ColumnInfo ExpDate = new ColumnInfo { Index = 12, Name = "執行時間", Width = 200 };
            public static readonly ColumnInfo EndDate = new ColumnInfo { Index = 13, Name = "結束時間", Width = 200 };

            public static void GridSetLocRange(ref DataGridView oGrid)
            {
                oGrid.ColumnCount = 14;
                oGrid.RowCount = 0;
                clInitSys.SetGridColumnInit(CmdSno, ref oGrid);
                clInitSys.SetGridColumnInit(CmdSts, ref oGrid);
                clInitSys.SetGridColumnInit(CmdMode, ref oGrid);
                clInitSys.SetGridColumnInit(Trace, ref oGrid);
                clInitSys.SetGridColumnInit(EquNO, ref oGrid);
                clInitSys.SetGridColumnInit(StnNo, ref oGrid);
                clInitSys.SetGridColumnInit(whetherAllout, ref oGrid);
                clInitSys.SetGridColumnInit(lastPallet, ref oGrid);
                clInitSys.SetGridColumnInit(Loc, ref oGrid);
                clInitSys.SetGridColumnInit(NewLoc, ref oGrid);
                clInitSys.SetGridColumnInit(Remark, ref oGrid);
                clInitSys.SetGridColumnInit(CrtDate, ref oGrid);
                clInitSys.SetGridColumnInit(ExpDate, ref oGrid);
                clInitSys.SetGridColumnInit(EndDate, ref oGrid);
            }
        }
    }
}
