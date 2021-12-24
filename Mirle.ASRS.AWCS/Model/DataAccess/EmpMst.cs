using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.DataBase;

namespace Mirle.ASRS.AWCS.Model.DataAccess
{
    public sealed class EmpMst : ValueObject
    {
        public string EmpSno { get; private set; }
        public string EmpNo { get; private set; }
        public string EmpName { get; private set; }
        public bool OpenFlag { get; private set; }

        protected override ValueObject ConvaertDataRow(DataRow row)
        {
            if (row.Table.Columns.Contains("EMPSNO"))
            {
                EmpSno = Convert.ToString(row["EMPSNO"]);
            }
            if (row.Table.Columns.Contains("EMPNO"))
            {
                EmpNo = Convert.ToString(row["EMPNO"]);
            }
            if (row.Table.Columns.Contains("EMPNAME"))
            {
                EmpName = Convert.ToString(row["EMPNAME"]);
            }
            if (row.Table.Columns.Contains("OPENFLAG"))
            {
                OpenFlag = Convert.ToString(row["OPENFLAG"]) == "1";
            }
            return this;
        }
    }
}
