using System;
using System.Collections.Generic;
using Mirle.Def;
using System.Data;
using Mirle.DataBase;
using Mirle.ASRS.WCS.Model.DataAccess;

namespace Mirle.DB.Fun
{
    public class clsLocMst
    {

        public GetDataResult GetLoc_DD(string Loc,out DataObject<Loc_Mst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM Loc_Mst ";
            sql += $"WHERE Loc IN ('{Loc}') ";
            sql += $"ORDER BY LVL_Z,BAY_Y,ROW_X desc";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetEmptyLoc_SameFloor_OutFirst(string Lvl_Z, out DataObject<Loc_Mst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM Loc_Mst ";
            sql += $"WHERE Lvl_Z IN ('{Lvl_Z}') ";
            sql += $"AND Loc_Sts='N' ";
            sql += $"ORDER BY LVL_Z,BAY_Y,ROW_X desc";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetEmptyLoc_SameFloor(string Loc, out DataObject<Loc_Mst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM Loc_Mst ";
            sql += $"WHERE Loc IN ('{Loc}') ";
            sql += $"ORDER BY LVL_Z,BAY_Y,ROW_X desc";
            return db.GetData(sql, out dataObject);
        }

        public GetDataResult GetLoc_DD_Sts(string Loc, out DataObject<Loc_Mst> dataObject, SqlServer db)
        {
            string sql = "SELECT * FROM Loc_Mst ";
            sql += $"WHERE Loc IN ('{Loc}') ";
            sql += $"ORDER BY LVL_Z,BAY_Y,ROW_X desc";
            return db.GetData(sql, out dataObject);
        }
        public ExecuteSQLResult UpdateStoreOutLocMst(string Loc, SqlServer db)
        {
            string sql = "UPDATE Loc_Mst ";
            sql += $"SET Loc_sts='O', ";
            sql += $"TRN_DATE='{DateTime.Now:yyyy-MM-dd HH:mm:ss}',";
            sql += $"TRN_USER='WCS'";
            sql += $" WHERE LOC='{Loc}' ";
            return db.ExecuteSQL2(sql);
        }
        public ExecuteSQLResult UpdateStoreInLocMst(string Loc, SqlServer db)
        {
            string sql = "UPDATE Loc_Mst ";
            sql += $"SET Loc_sts='I', ";
            sql += $"TRN_DATE='{DateTime.Now:yyyy-MM-dd HH:mm:ss}',";
            sql += $"TRN_USER='WCS'";
            sql += $" WHERE LOC='{Loc}' ";
            return db.ExecuteSQL2(sql);
        }

    }
}
