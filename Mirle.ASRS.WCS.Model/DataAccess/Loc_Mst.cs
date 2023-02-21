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

        public string Loc_Txno { get; private set; }
        public string EQU_NO { get; private set; }
        public string PLT_QTY { get; private set; }
        public string ALO_QTY { get; private set; }
        public string WH_ID { get; private set; }
        public string IN_DATE { get; private set; }
        public string LOT_NO { get; private set; }
        public string ITEM_NO { get; private set; }
        public string IN_TKT_NO { get; private set; }
        public string EXPIRE_DATE { get; private set; }
        public string TRN_DATE { get; private set; }
        public string STORE_CODE { get; private set; }

        public string CYCLE_DATE { get; private set; }

        public string BANK_CODE { get; private set; }
        public string QC_CODE { get; private set; }
        public string ITEM_TYPE { get; private set; }

        public string Loc_ID { get; private set; }


        protected override ValueObject ConvaertDataRow(DataRow row)
        {
            if (row.Table.Columns.Contains("Loc"))
            {
                Loc = Convert.ToString(row["Loc"]);
            }
            if (row.Table.Columns.Contains("Loc_Txno"))
            {
                Loc_Txno = Convert.ToString(row["Loc_Txno"]);
            }
            if (row.Table.Columns.Contains("Loc_ID"))
            {
                Loc_ID = Convert.ToString(row["Loc_ID"]);
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
            if (row.Table.Columns.Contains("LVl_Z"))
            {
                LVl_Z = Convert.ToString(row["LVl_Z"]);
            }
            if (row.Table.Columns.Contains("WH_ID"))
            {
                WH_ID = Convert.ToString(row["WH_ID"]);
            }
            if (row.Table.Columns.Contains("IN_DATE"))
            {
                IN_DATE = Convert.ToString(row["IN_DATE"]);
            }
            if (row.Table.Columns.Contains("LOT_NO"))
            {
                LOT_NO = Convert.ToString(row["LOT_NO"]);
            }
            if (row.Table.Columns.Contains("ITEM_NO"))
            {
                ITEM_NO = Convert.ToString(row["ITEM_NO"]);
            }
            if (row.Table.Columns.Contains("IN_TKT_NO"))
            {
                IN_TKT_NO = Convert.ToString(row["IN_TKT_NO"]);
            }
            if (row.Table.Columns.Contains("TRN_DATE"))
            {
                TRN_DATE = Convert.ToString(row["TRN_DATE"]);
            }
            if (row.Table.Columns.Contains("CYCLE_DATE"))
            {
                CYCLE_DATE = Convert.ToString(row["CYCLE_DATE"]);
            }
            if (row.Table.Columns.Contains("EXPIRE_DATE"))
            {
                EXPIRE_DATE = Convert.ToString(row["EXPIRE_DATE"]);
            }
            if (row.Table.Columns.Contains("STORE_CODE"))
            {
                STORE_CODE = Convert.ToString(row["STORE_CODE"]);
            }
            if (row.Table.Columns.Contains("BANK_CODE"))
            {
                BANK_CODE = Convert.ToString(row["BANK_CODE"]);
            }
            if (row.Table.Columns.Contains("QC_CODE"))
            {
                QC_CODE = Convert.ToString(row["QC_CODE"]);
            }
            if (row.Table.Columns.Contains("ITEM_TYPE"))
            {
                ITEM_TYPE = Convert.ToString(row["ITEM_TYPE"]);
            }
            if (row.Table.Columns.Contains("PLT_QTY"))
            {
                PLT_QTY = Convert.ToString(row["PLT_QTY"]);
            }
            if (row.Table.Columns.Contains("ALO_QTY"))
            {
                ALO_QTY = Convert.ToString(row["ALO_QTY"]);
            }
            return this;
        }
    }
}
