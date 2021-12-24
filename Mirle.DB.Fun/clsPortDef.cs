using System;
using System.Collections.Generic;
using Mirle.Def;
using System.Data;
using Mirle.DataBase;

namespace Mirle.DB.Fun
{
    public class clsPortDef
    {
        private string[] sDevice = new string[0];
        private List<Element_Port>[] glstPort = new List<Element_Port>[0];

        public List<Element_Port>[] GetLstPort()
        {
            return glstPort;
        }


        public bool FunDevice(SqlServer db)
        {
            DataTable dtTmp = new DataTable();
            string strEM = "";
            try
            {
                string strSql = "select DISTINCT DeviceID from PortDef";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    sDevice = new string[dtTmp.Rows.Count];
                    for (int i = 0; i < sDevice.Length; i++)
                    {
                        sDevice[i] = Convert.ToString(dtTmp.Rows[i][0]);
                    }

                    return true;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql + " => " + strEM);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return false;
            }
            finally
            {
                dtTmp = null;
            }
        }

        public List<Element_Port> GetAllPort(string DeviceID, SqlServer db)
        {
            List<Element_Port> lstPorts = new List<Element_Port>();
            Element_Port objPort = null;
            DataTable dtTmp = new DataTable();
            string strEM = "";
            try
            {
                string strSql = "select * from PortDef where DeviceID = '" + DeviceID + "' ";
                int iRet = db.GetDataTable(strSql, ref dtTmp, ref strEM);
                if (iRet == DBResult.Success)
                {
                    for (int i = 0; i < dtTmp.Rows.Count; i++)
                    {
                        objPort = new Element_Port(Convert.ToString(dtTmp.Rows[i]["DeviceID"]), Convert.ToString(dtTmp.Rows[i]["HostPortID"]),
                            Convert.ToInt32(dtTmp.Rows[i]["PortType"]), Convert.ToInt32(dtTmp.Rows[i]["PortTypeIndex"]), Convert.ToInt32(dtTmp.Rows[i]["PLCPortID"]));
                        lstPorts.Add(objPort);
                    }

                    return lstPorts;
                }
                else
                {
                    clsWriLog.Log.FunWriTraceLog_CV(strSql + " => " + strEM);
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
                return new List<Element_Port>();
            }
            finally
            {
                dtTmp = null;
            }
        }

        public void FunGetAllPort(SqlServer db)
        {
            try
            {
                glstPort = new List<Element_Port>[sDevice.Length];
                for (int i = 0; i < sDevice.Length; i++)
                {
                    glstPort[i] = GetAllPort(sDevice[i], db);
                }
            }
            catch (Exception ex)
            {
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);
            }
        }
    }
}
