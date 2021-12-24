using Mirle.DataBase;
using Mirle.Def;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.DB.Proc
{
    public class clsAlarmData
    {
        private Fun.clsAlarmData alarmData = new Fun.clsAlarmData();
        private clsDbConfig _config = new clsDbConfig();
        public clsAlarmData(clsDbConfig config)
        {
            _config = config;
        }

        public bool FunGetAlarmSts(DateTime startTime, DateTime endTime, string eqpId, ref DataTable dtTmp)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        alarmData.FunGetAlarmSts(startTime, endTime, eqpId, ref dtTmp, db);
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch(Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
        }
        public bool FunGetAlarmBetweenTime(DateTime startTime, DateTime endTime, string eqpId, ref DataTable dtTmp)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        alarmData.FunGetAlarmBetweenTime(startTime, endTime, eqpId, ref dtTmp, db);
                        return true;
                    }
                    else
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
