using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Mirle.Grid;

namespace Mirle.Grid.T26YGAP0
{
    public class TaskColumnDef
    {
        public class Task
        {
            public static readonly ColumnInfo CommandID = new ColumnInfo { Index = 0, Name = "命令號", Width = 75 };
            public static readonly ColumnInfo TaskNo = new ColumnInfo { Index = 1, Name = "任務號", Width = 120 };
            public static readonly ColumnInfo CMDState = new ColumnInfo { Index = 2, Name = "任務狀態", Width = 60 };
            public static readonly ColumnInfo TaskState = new ColumnInfo { Index = 3, Name = "模式", Width = 50 };
            public static readonly ColumnInfo InitialDT = new ColumnInfo { Index = 4, Name = "開始時間", Width = 50 };
            public static readonly ColumnInfo ActiveDT = new ColumnInfo { Index = 5, Name = "執行時間", Width = 75 };
            public static readonly ColumnInfo Source = new ColumnInfo { Index = 6, Name = "來源", Width = 60 };
            public static readonly ColumnInfo Destination = new ColumnInfo { Index = 7, Name = "目的", Width = 68 };

            public static void GridSetLocRange(ref DataGridView oGrid)
            {
                oGrid.ColumnCount = 8;
                oGrid.RowCount = 0;
                clInitSys.SetGridColumnInit(CommandID, ref oGrid);
                clInitSys.SetGridColumnInit(TaskNo, ref oGrid);
                //clInitSys.SetGridColumnInit(palletNo, ref oGrid);
                clInitSys.SetGridColumnInit(CMDState, ref oGrid);
                clInitSys.SetGridColumnInit(TaskState, ref oGrid);
                clInitSys.SetGridColumnInit(InitialDT, ref oGrid);
                clInitSys.SetGridColumnInit(ActiveDT, ref oGrid);
                clInitSys.SetGridColumnInit(Source, ref oGrid);
                //clInitSys.SetGridColumnInit(whetherAllout, ref oGrid);
                //clInitSys.SetGridColumnInit(lastPallet, ref oGrid);
                clInitSys.SetGridColumnInit(Destination, ref oGrid);
            }
        }
    }
}
