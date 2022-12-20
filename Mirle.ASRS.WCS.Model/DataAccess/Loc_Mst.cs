using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.DataBase;

namespace Mirle.ASRS.WCS.Model.DataAccess
{
    public sealed class Loc_Mst : ValueObject
    {
        public string Loc { get; private set; }
        public string Loc_DD { get; private set; }
        public string Loc_Sts { get; private set; }
        public string LVl_Z { get; private set; }
        public string Old_Sts { get; private set; }
        public string EQU_ROWNO { get; private set; }
        public string EQU_NO { get; private set; }

        protected override ValueObject ConvaertDataRow(DataRow row)
        {
            if (row.Table.Columns.Contains("Loc"))
            {
                Loc = Convert.ToString(row["Loc"]);
            }
            if (row.Table.Columns.Contains("Loc_DD"))
            {
                Loc_DD = Convert.ToString(row["Loc_DD"]);
            }
            if (row.Table.Columns.Contains("Loc_Sts"))
            {
                Loc_Sts = Convert.ToString(row["Loc_Sts"]);
            }
            if (row.Table.Columns.Contains("Old_Sts"))
            {
                Old_Sts = Convert.ToString(row["Old_Sts"]);
            }
            if (row.Table.Columns.Contains("EQU_ROWNO"))
            {
                EQU_ROWNO = Convert.ToString(row["EQU_ROWNO"]);
            }
            if (row.Table.Columns.Contains("EQU_NO"))
            {
                EQU_NO = Convert.ToString(row["EQU_NO"]);
            }
            if (row.Table.Columns.Contains("LVl_Z "))
            {
                LVl_Z = Convert.ToString(row["LVl_Z "]);
            }
            return this;
        }
    }
}
