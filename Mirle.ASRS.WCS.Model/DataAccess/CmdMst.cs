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
        public string TaskNo { get; private set; }
        public string CmdMode { get; private set; }
        public string StnNo { get; private set; }
        public string Loc { get; private set; }
        public string Loc_ID { get; private set; }
        public string NewLoc { get; private set; }
        public string Trace { get; private set; }
        public string IOType { get; private set; }
        public string COUNT { get; internal set; }

        protected override ValueObject ConvaertDataRow(DataRow row)
        {
            if (row.Table.Columns.Contains("CMD_SNO"))
            {
                CmdSno = Convert.ToString(row["CMD_SNO"]);
            }
            if (row.Table.Columns.Contains("TaskNo"))
            {
                TaskNo = Convert.ToString(row["TaskNo"]);
            }
            if (row.Table.Columns.Contains("LOC_ID"))
            {
                Loc_ID = Convert.ToString(row["LOC_ID"]);
            }
            if (row.Table.Columns.Contains("CMD_MODE"))
            {
                CmdMode = Convert.ToString(row["CMD_MODE"]);
            }
            if (row.Table.Columns.Contains("STN_NO"))
            {
                StnNo = Convert.ToString(row["STN_NO"]);
            }
            if (row.Table.Columns.Contains("LOC"))
            {
                Loc = Convert.ToString(row["LOC"]);
            }
            if (row.Table.Columns.Contains("NEW_LOC"))
            {
                NewLoc = Convert.ToString(row["NEW_LOC"]);
            }
            if (row.Table.Columns.Contains("TRACE"))
            {
                Trace = Convert.ToString(row["TRACE"]);
            }
            if (row.Table.Columns.Contains("IO_Type"))
            {
                IOType = Convert.ToString(row["IO_Type"]);
            }
            if (row.Table.Columns.Contains("COUNT"))
            {
                COUNT = Convert.ToString(row["COUNT"]);
            }
            return this;
        }
    }
    }

    public struct struCmdDtl
    {
        public string Cmd_Txno { get; set; }
    public string LOC_Txno { get; set; }
    public string Cmd_Sno { get; set; }
        public double Plt_Qty { get; set; }
        public double ALO_Qty { get; set; }
        public string In_Date { get; set; }
        public string TRN_Date { get; set; }
        public string CYCLE_Date { get; set; }
        public string Item_No { get; set; }
        public string Lot_No { get; set; }
        public string QC_CODE { get; set; }
        public string Tkt_NO { get; set; }
        public string Tkt_SEQ { get; set; }
        public string Item_TYPE { get; set; }
        public string Store_CODE { get; set; }
        public string BANK_CODE { get; set; }

    public string expire_date { get; set; }
    }
