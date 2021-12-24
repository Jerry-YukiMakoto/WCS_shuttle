using System;
using System.Data;
using Mirle.Def;
using Mirle.Structure;
using Mirle.DataBase;

namespace Mirle.DB.Proc
{
    public class clsCmd_Mst_His
    {
        private Fun.clsCmd_Mst_His CMD_MST_HIS = new Fun.clsCmd_Mst_His();
        private clsDbConfig _config = new clsDbConfig();
        public clsCmd_Mst_His(clsDbConfig config)
        {
            _config = config;
        }

        public bool FunGetCmdBetweenTime(DateTime startTime, DateTime endTime, string eqpId, ref DataTable dtTmp)
        {
            try
            {
                using (var db = clsGetDB.GetDB(_config))
                {
                    int iRet = clsGetDB.FunDbOpen(db);
                    if (iRet == DBResult.Success)
                    {
                        CMD_MST_HIS.FunGetCmdBetweenTime(startTime, endTime, eqpId, ref dtTmp, db);
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
