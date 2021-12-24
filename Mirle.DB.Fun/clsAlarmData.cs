using Mirle.DataBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.DB.Fun
{
    public class clsAlarmData
    {
        
        public bool FunGetAlarmSts(DateTime startTime, DateTime endTime, string eqpId, ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                string strSql = $"SELECT a.DeviceID, a.ALARMCODE, a.StartDT, a.ENDDT, a.ALARMTIME,b.AlarmDesc , b.AlarmDesc_EN FROM ALARMDATA as a LEFT JOIN AlarmDef as b ON a.ALARMCODE = b.AlarmCode" +
                    $" WHERE ((StartDT between '{startTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}' AND '{endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}') OR (ENDDT between '{startTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}' AND '{endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}'))";
                if (eqpId != "")
                {
                    strSql += $" AND DeviceID = '{eqpId}'" + $" AND UnitID != 'C2'" + $" ORDER BY StartDT DESC";
                }
                else
                {
                    strSql += $" AND UnitID != 'C2'" + $" ORDER BY StartDT DESC";
                }

                string strEM = "";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    return true;
                }
                else if (iRet == DBResult.NoDataSelect)
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                    return true;
                }
                else 
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{strSql} => {strEM}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        public bool FunGetAlarmBetweenTime(DateTime startTime, DateTime endTime, string eqpId, ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                var sql = $"select * from ALARMDATA where ((StartDT between '{startTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}' and '{endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}')" +
                    $" or (ENDDT between '{startTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}' and '{endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}'))" +
                    $" and DeviceID = '{eqpId}'" + $" AND UnitID = 'C1'";
                string strEM = "";
                int iRet = db.GetDataTable(sql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success) return true;
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{sql} => {strEM}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }

        }
        public bool FunGetPortAlarm(DateTime startTime, DateTime endTime, string portId, ref DataTable dtTmp, SqlServer db)
        {
            try
            {
                var sql = $"select * from ALARMDATA where ((StartDT between '{startTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}' and '{endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}')" +
                    $" or (ENDDT between '{startTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}' and '{endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}'))" +
                    $" and DeviceID = '{portId}'" + $" AND UnitID = '{portId}'";
                string strEM = "";
                int iRet = db.GetDataTable(sql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success) return true;
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV($"{sql} => {strEM}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }

        }
    }
}
