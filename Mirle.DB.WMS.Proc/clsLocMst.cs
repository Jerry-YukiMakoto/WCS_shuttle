using System;
using System.Collections.Generic;
using System.Linq;
using Mirle.Def;
using Mirle.DataBase;

namespace Mirle.DB.WMS.Proc
{
    public class clsLocMst
    {
        private WMS.Fun.clsLocMst LocMst = new WMS.Fun.clsLocMst();
        private clsDbConfig _config = new clsDbConfig();
        public clsLocMst(clsDbConfig config)
        {
            _config = config;
        }

        public string GetLocDD(string sLoc)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return LocMst.GetLocDD(sLoc, db);
                    }
                    else
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return string.Empty;
            }
        }

        public int CheckLocIsEmpty(string sLoc, ref bool IsEmpty, ref string BoxID)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return LocMst.CheckLocIsEmpty(sLoc, ref IsEmpty, ref BoxID, db);
                    }
                    else return iRet;
                }
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
        public int CheckLocIsOutside(string sLoc, ref bool IsOutside, ref string sLocDD, ref bool IsEmpty_DD, ref string BoxID_DD)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return LocMst.CheckLocIsOutside(sLoc, ref IsOutside, ref sLocDD, ref IsEmpty_DD, ref BoxID_DD, db);
                    }
                    else return iRet;
                }
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
        public int CheckLocIsOutside(string sLoc, ref bool IsOutside)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return LocMst.CheckLocIsOutside(sLoc, ref IsOutside, db);
                    }
                    else return iRet;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
        }

        public int CheckLocByBoxID(string sBoxID, ref int StockerID, ref string sLoc)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return LocMst.CheckLineByBoxID(sBoxID, ref StockerID, ref sLoc, db);
                    }
                    else return iRet;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return DBResult.Exception;
            }
        }

        public string FunSearchEmptyLoc(ref int EquNo, int CurStockerID)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        return LocMst.FunSearchEmptyLoc(ref EquNo, CurStockerID, db);
                    }
                    else return "";
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return "";
            }
        }
    }
}
