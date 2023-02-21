using System;
using System.Collections.Generic;
using Mirle.Structure;
using System.Data;
using Mirle.Def;
using Mirle.DataBase;

namespace Mirle.DB.Fun
{
    public class clsSno
    {
        private clsCmd_Mst cmd_Mst = new clsCmd_Mst();
        public string FunGetSeqNo(clsEnum.enuSnoType objType, SqlServer db)
        {
            int intGetCnt = 0;
            long lngSeq1 = 0;
            long lngSeq2 = 0;
            string strSql = string.Empty;
            string strEM = string.Empty;
            string strMonthFlag = string.Empty;     //月異動 Y/N
            //string strDayFlag = string.Empty;       //v1.0 日異動 Y/N by Ian
            int intSnoLen = 0;                      //Sno_Len 序號長度

            DataTable dtSno = new DataTable();
            int intRtn;

            try
            {

            ProNext:

                intGetCnt = intGetCnt + 1;

                strSql = "SELECT C.SNO_TYP,C.Trn_Date,C.SNO,M.MONTH_FLAG,M.INIT_SNO,M.MAX_SNO,M.SNO_LEN";
                strSql += " FROM SNO_CTL C LEFT JOIN SNO_MAX M ON C.SNO_TYP=M.SNO_TYPE";
                strSql += " WHERE C.SNO_TYP='" + objType.ToString() + "'";

                intRtn = db.GetDataTable(strSql, ref dtSno, ref strEM);
                if (intRtn == DBResult.Success)
                {
                    lngSeq2 = int.Parse(dtSno.Rows[0]["SNO"].ToString());
                    strMonthFlag = dtSno.Rows[0]["MONTH_FLAG"].ToString();

                    if (dtSno.Rows[0]["SNO_LEN"].ToString() == "")
                        intSnoLen = 0;
                    else
                    {
                        intSnoLen = int.Parse(dtSno.Rows[0]["SNO_LEN"].ToString());
                    }

                    if (lngSeq2 >= int.Parse(dtSno.Rows[0]["MAX_SNO"].ToString()))
                    {
                        lngSeq1 = int.Parse(dtSno.Rows[0]["INIT_SNO"].ToString());
                    }
                    else
                    {
                        lngSeq1 = lngSeq2 + 1;
                    }

                    strSql = "UPDATE SNO_CTL SET SNO = " + lngSeq1;
                    //if (strMonthFlag == "Y")
                    //{
                    //    strSql += ",TRN_MONTH = '" + strGetYearMonth + "'";
                    //}
                    strSql += " WHERE SNO_TYP = '" + objType.ToString() + "'";
                    strSql += " AND SNO = " + lngSeq2;
                }
                else if (intRtn == DBResult.NoDataSelect)
                {
                    #region v1.3 找尋序號長度 by Ian
                    strSql = "select Sno_Len, Init_Sno from Sno_Max where Sno_Type='" + objType.ToString() + "' ";
                    dtSno = new DataTable();
                    if (db.GetDataTable(strSql, ref dtSno, ref strEM) != DBResult.Success)
                        throw new Exception();
                    intSnoLen = int.Parse(dtSno.Rows[0]["Sno_Len"].ToString());
                    int iInitial = int.Parse(dtSno.Rows[0]["Init_Sno"].ToString());
                    #endregion v1.3 找尋序號長度 by Ian

                    strSql = "INSERT INTO SNO_CTL (SNO_TYP,Trn_Date,SNO) VALUES ('" + objType.ToString() + "','" +
                        DateTime.Now.ToString("yyyyMMdd") + "'," + iInitial.ToString() + ")";

                    lngSeq1 = iInitial;
                }
                else
                {
                    return "";
                }

                if (db.ExecuteSQL(strSql, ref strEM) != DBResult.Success)
                {
                    //讀取Count
                    if (intGetCnt >= 5)
                    {
                        return "";
                    }
                    else
                    {
                        goto ProNext;
                    }
                }

                switch (objType)
                {
                    case clsEnum.enuSnoType.TaskNO:
                        return lngSeq1.ToString().PadLeft(intSnoLen, '0');
                    case clsEnum.enuSnoType.CMDSNO:
                    case clsEnum.enuSnoType.CMDSUO:
                        string sCmdSno = lngSeq1.ToString().PadLeft(intSnoLen, '0');
                        CmdMstInfo cmd = new CmdMstInfo(); int iRet = DBResult.Initial;
                        if (cmd_Mst.FunGetCommand(sCmdSno, ref cmd, ref iRet, db)) goto ProNext;
                        else return sCmdSno;
                    case clsEnum.enuSnoType.WCSTrxNo:
                        return DateTime.Now.ToString("HHmmss") + lngSeq1.ToString().PadLeft(intSnoLen, '0');
                    case clsEnum.enuSnoType.LOCTXNO:
                        return DateTime.Now.ToString("yyMMdd") + lngSeq1.ToString().PadLeft(intSnoLen, '0');
                    default:
                        return lngSeq1.ToString().PadLeft(intSnoLen, '0');
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
                dtSno.Dispose();
            }
        }
    }
}
