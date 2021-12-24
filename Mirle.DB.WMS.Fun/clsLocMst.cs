using System;
using System.Collections.Generic;
using Mirle.Def;
using System.Data;
using Mirle.DataBase;

namespace Mirle.DB.WMS.Fun
{
    public class clsLocMst
    {
        /// <summary>
        /// 確認儲位是否是外儲位
        /// </summary>
        /// <param name="sLoc"></param>
        /// <param name="IsOutside"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public int CheckLocIsOutside(string sLoc, ref bool IsOutside, ref string sLocDD, ref bool IsEmpty_DD, ref string BoxID_DD, SqlServer db)
        {
            try
            {
                int iRet = CheckLocIsOutside(sLoc, ref IsOutside, db);
                if (iRet == DBResult.Success)
                {
                    sLocDD = GetLocDD(sLoc, db);
                    if (string.IsNullOrWhiteSpace(sLocDD))
                        throw new Exception($"找不到{sLoc}的對照儲位！");
                    return CheckLocIsEmpty(sLocDD, ref IsEmpty_DD, ref BoxID_DD, db);
                }

                return iRet;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
        }

        /// <summary>
        /// 確認儲位是否是外儲位
        /// </summary>
        /// <param name="sLoc"></param>
        /// <param name="IsOutside"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public int CheckLocIsOutside(string sLoc, ref bool IsOutside, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = $"select IS_INSIDE from r_wms_location where LOCATION_CODE = '{sLoc}' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    IsOutside = Convert.ToString(dtTmp.Rows[0][0]) == "N" ? true : false;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                }

                return iRet;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
            finally
            {
                dtTmp = null;
            }
        }

        public string GetLocDD(string sLoc, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = $"select BROTHER_LOCATION_CODE from r_wms_location where LOCATION_CODE = '{sLoc}' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    return Convert.ToString(dtTmp.Rows[0][0]);
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return string.Empty;
            }
            finally
            {
                dtTmp = null;
            }
        }

        public int CheckLocIsEmpty(string sLoc, ref bool IsEmpty, ref string BoxID, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strEM = "";
                string strSql = $"select STORAGE_STATUS, CARRIER_CODE from r_wms_location where LOCATION_CODE = '{sLoc}' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    IsEmpty = Convert.ToString(dtTmp.Rows[0]["STORAGE_STATUS"]).Trim().ToUpper() ==
                        clsConstValue.LocSts.Empty;
                    BoxID = Convert.ToString(dtTmp.Rows[0]["CARRIER_CODE"]);
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                }

                return iRet;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
            finally
            {
                dtTmp = null;
            }
        }

        public int CheckLineByBoxID(string sBoxID, ref int StockerID, ref string sLoc, SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            try
            {
                string strSql = $"select * from r_wms_location where CARRIER_CODE = '{sBoxID}' ";
                string strEM = "";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    StockerID = Convert.ToInt32(dtTmp.Rows[0]["CRANE"]);
                    sLoc = Convert.ToString(dtTmp.Rows[0]["LOCATION_CODE"]);
                }
                else clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");

                return iRet;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
            finally
            {
                dtTmp = null;
            }
        }

        public string FunSearchEmptyLoc(ref int EquNo, int CurStockerID, SqlServer db)
        {
            try
            {
                EquNo = CurStockerID;
                int iCount = 0;
                do
                {
                    if (Micron.U2NMMA30.clsMicronStocker.GetStockerById(EquNo).GetCraneById(1).IsInService)
                    {
                        string sNewLoc = "";
                        if (EquNo == 4)
                        {   //Single Deep
                            sNewLoc = funSearchEmptyLoc(EquNo.ToString(), clsEnum.LocSts_Double.None, db);
                        }
                        else
                        {
                            sNewLoc = funSearchEmptyLoc(EquNo.ToString(), clsEnum.LocSts_Double.NNNN, db);
                            if (string.IsNullOrWhiteSpace(sNewLoc))
                            {
                                sNewLoc = funSearchEmptyLoc(EquNo.ToString(), clsEnum.LocSts_Double.SNNS, db);
                                if (string.IsNullOrWhiteSpace(sNewLoc))
                                {
                                    sNewLoc = funSearchEmptyLoc(EquNo.ToString(), clsEnum.LocSts_Double.ENNE, db);
                                    if (string.IsNullOrWhiteSpace(sNewLoc))
                                    {
                                        sNewLoc = funSearchEmptyLoc(EquNo.ToString(), clsEnum.LocSts_Double.XNNX, db);
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(sNewLoc)) return sNewLoc;
                    }
                    else clsWriLog.Log.FunWriTraceLog_CV($"Error: Stocker{EquNo}並非InService！");

                    if (EquNo == 4) EquNo = 1;
                    else EquNo++;

                    iCount++;
                } while (iCount < 4);

                return "";
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return "";
            }
        }

        public string funSearchEmptyLoc(string Equ_No, clsEnum.LocSts_Double locSts, SqlServer db)
        {
            string strEM = "";
            DataTable dtTmp = new DataTable();
            try
            {
                string sSQL = "SELECT TOP 1 LOCATION_CODE FROM r_wms_location WHERE STORAGE_STATUS = '" + clsConstValue.LocSts.Empty + "' ";
                sSQL += $" AND OPERATE_STATUS = '{clsConstValue.LocSts.Normal}' and CRANE = '" + Equ_No + "' ";

                if (locSts == clsEnum.LocSts_Double.NNNN) //找外側空庫位
                {
                    sSQL += " AND LOCATION_CODE IN (SELECT BROTHER_LOCATION_CODE FROM r_wms_location" +
                        $" WHERE STORAGE_STATUS='{clsConstValue.LocSts.Empty}' AND OPERATE_STATUS = '{clsConstValue.LocSts.Normal}'" +
                        $" AND IS_INSIDE = 'Y') ";
                }
                else if (locSts == clsEnum.LocSts_Double.SNNS)
                {
                    sSQL += " AND LOCATION_CODE IN (SELECT BROTHER_LOCATION_CODE FROM r_wms_location" +
                        $" WHERE STORAGE_STATUS in ('{clsConstValue.LocSts.Full}','{clsConstValue.LocSts.NotFull}')" +
                        $" AND OPERATE_STATUS = '{clsConstValue.LocSts.Normal}' AND IS_INSIDE = 'N') ";
                }
                else if (locSts == clsEnum.LocSts_Double.ENNE)
                {
                    sSQL += " AND LOCATION_CODE IN (SELECT BROTHER_LOCATION_CODE FROM r_wms_location" +
                        $" WHERE STORAGE_STATUS = '{clsConstValue.LocSts.EmptyBox}' AND OPERATE_STATUS = '{clsConstValue.LocSts.Normal}'" +
                        " AND IS_INSIDE = 'N') ";
                }
                else if (locSts == clsEnum.LocSts_Double.XNNX)
                {
                    sSQL += " AND LOCATION_CODE IN (SELECT BROTHER_LOCATION_CODE FROM r_wms_location" +
                        $" WHERE OPERATE_STATUS = '{clsConstValue.LocSts.Block}' AND IS_INSIDE = 'N') ";
                }
                else { } //Single Deep

                sSQL += " ORDER BY BAY, LEVEL, ROW DESC";

                dtTmp = new DataTable();
                string sNewLoc;
                if (db.GetDataTable(sSQL, ref dtTmp, ref strEM) == DBResult.Success)
                {
                    sNewLoc = dtTmp.Rows[0]["LOCATION_CODE"].ToString();
                }
                else
                {
                    sNewLoc = "";
                }

                return sNewLoc;
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return "";
            }
            finally
            {
                dtTmp = null;
            }
        }
    }
}
