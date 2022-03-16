using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mirle.DataBase;

namespace Mirle.ASRS.WCS.Model.DataAccess
{
    public sealed class CmdMst : ValueObject
    {
        public string CmdSno { get; private set; }
        public string CmdMode { get; private set; }
        public string StnNo { get; private set; }
        public string Loc { get; private set; }
        public string NewLoc { get; private set; }
        public string LoadType { get; private set; }
        public string TrayId { get; private set; }
        public string Trace { get; private set; }
        public string IOType { get; private set; }
        public string COUNT { get; internal set; }
        public string whetherAllOut { get; internal set; }
        public string lastpallet { get; internal set; }

        protected override ValueObject ConvaertDataRow(DataRow row)
        {
            if (row.Table.Columns.Contains("CMDSNO"))
            {
                CmdSno = Convert.ToString(row["CMDSNO"]);
            }
            if (row.Table.Columns.Contains("CMDMODE"))
            {
                CmdMode = Convert.ToString(row["CMDMODE"]);
            }
            if (row.Table.Columns.Contains("STNNO"))
            {
                StnNo = Convert.ToString(row["STNNO"]);
            }
            if (row.Table.Columns.Contains("LOC"))
            {
                Loc = Convert.ToString(row["LOC"]);
            }
            if (row.Table.Columns.Contains("NEWLOC"))
            {
                NewLoc = Convert.ToString(row["NEWLOC"]);
            }
            if (row.Table.Columns.Contains("LOADTYPE"))
            {
                LoadType = Convert.ToString(row["LOADTYPE"]);
            }
            if (row.Table.Columns.Contains("TRAYID"))
            {
                TrayId = Convert.ToString(row["TRAYID"]);
            }
            if (row.Table.Columns.Contains("TRACE"))
            {
                Trace = Convert.ToString(row["TRACE"]);
            }
            if (row.Table.Columns.Contains("IOType"))
            {
                IOType = Convert.ToString(row["IOType"]);
            }
            if (row.Table.Columns.Contains("COUNT"))
            {
                COUNT = Convert.ToString(row["COUNT"]);
            }
            if (row.Table.Columns.Contains("whetherAllOut"))
            {
                whetherAllOut = Convert.ToString(row["whetherAllOut"]);
            }
            if (row.Table.Columns.Contains("lastpallet"))
            {
                lastpallet = Convert.ToString(row["lastpallet"]);
            }
            return this;
        }
    }
}
