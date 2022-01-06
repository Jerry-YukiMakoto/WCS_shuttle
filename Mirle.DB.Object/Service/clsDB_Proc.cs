using System;
using Mirle.Def;
using System.Windows.Forms;
using Mirle.Structure;
using Mirle.Structure.Info;
using System.Collections.Generic;
using System.Linq;

namespace Mirle.DB.Object
{
    public class clsDB_Proc
    {
        private static Proc.clsHost wcs;
        public static void Initial(clsDbConfig dbConfig, clsDbConfig dbConfig_WMS)
        {
            wcs = new Proc.clsHost(dbConfig, dbConfig_WMS, Application.StartupPath + "\\Sqlite\\LCSCODE.DB");
        }

        public static Proc.clsHost GetDB_Object()
        {
            return wcs;
        }
        #region Mark
        /*
        public static bool FunBatchAndAnotherHasCarrior_Proc(IEnumerable<DataGridViewRow> obj_Batch, int EquNo, int Fork, TransferBatch cmd)
        {
            try
            {
                string sRemark = "";
                int fork_another = Fork == 1 ? 2 : 1;
                if (clsMicronStocker.GetCommand(EquNo, Fork).CmdMode == clsConstValue.CmdMode.L2L)
                {
                    bool bCheck = obj_Batch.Any(
                                                v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.NeedShelfToShelf.Index].Value) == clsEnum.NeedL2L.Y.ToString() &&
                                                     (Convert.ToString(v.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value) == clsConstValue.CmdSts.strCmd_Initial ||
                                                      Convert.ToString(v.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value) == LocationDef.Location.Teach.ToString())
                                               );
                    if (EquNo == 4) return false;
                    else
                    {
                        string sLoc = GetDB_Object().GetProcess().GetFunProcess().GetWMS_DBObject().GetLocMst().GetLocDD(clsMicronStocker.GetCommand(EquNo, Fork).Loc);
                        bool bOutsideCmd = obj_Batch.Any(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.Loc.Index].Value) == sLoc);
                        if (bCheck)
                        {
                            if (!bOutsideCmd) return false;
                            else
                            {
                                #region 更新NeedShelfToShelf欄位為N
                                var obj = obj_Batch.Where(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.Loc.Index].Value) == sLoc);
                                foreach (var row in obj)
                                {
                                    string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                                    string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                                    if (GetDB_Object().GetCmd_Mst().FunUpdateNeedL2L(sCmdSno, clsEnum.NeedL2L.N))
                                        return true;
                                    else
                                    {
                                        sRemark = $"Error: {sCmdSno}更新NeedShelfToShelf欄位失敗！";
                                        if (sRemark != sRemark_Pre)
                                        {
                                            GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                                        }

                                        return false;
                                    }
                                }

                                return false;
                                #endregion 更新NeedShelfToShelf欄位為N
                            }
                        }
                        else
                        {
                            if (bOutsideCmd)
                            {
                                var obj = obj_Batch.Where(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.Loc.Index].Value) == sLoc);
                                return wcs.GetProcess().FunShelfToFork_Proc(obj, fork_another);
                            }
                            else return false;
                        }
                    }
                }
                else
                {
                    bool bCheck = obj_Batch.Any(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value) ==
                                               clsMicronStocker.GetCommand(EquNo, Fork).CommandID);
                    if (bCheck)
                    {   //尋找另一筆
                        var obj_another = obj_Batch.Where(v => Convert.ToString(v.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value) !=
                                               clsMicronStocker.GetCommand(EquNo, Fork).CommandID);
                        foreach (var row_another in obj_another)
                        {
                            string sLoc = Convert.ToString(row_another.Cells[ColumnDef.CMD_MST.Loc.Index].Value);
                            string sCmdSts = Convert.ToString(row_another.Cells[ColumnDef.CMD_MST.CmdSts.Index].Value);
                            string sCurLoc = Convert.ToString(row_another.Cells[ColumnDef.CMD_MST.CurLoc.Index].Value);

                            if (sCmdSts == clsConstValue.CmdSts.strCmd_Initial ||
                               sCurLoc == LocationDef.Location.Teach.ToString())
                            {
                                if (Micron.U2NMMA30.clsTool.CheckForkCanDo(sLoc, EquNo, fork_another))
                                {
                                    if (wcs.GetProcess().FunShelfToFork_Proc(row_another, fork_another)) return true;
                                    else break;
                                }
                                else
                                {
                                    ConveyorInfo buffer = new ConveyorInfo();
                                    if (!clsMicronCV.GetStockOutBuffer(EquNo, Fork, ref buffer, ref sRemark))
                                    {
                                        if (sRemark != clsMicronStocker.GetCommand(EquNo, Fork).Remark)
                                        {
                                            GetDB_Object().GetCmd_Mst().FunUpdateRemark(clsMicronStocker.GetCommand(EquNo, Fork).CommandID, sRemark);
                                        }

                                        return false;
                                    }

                                    return wcs.GetProcess().FunDepositToCV_Proc_addCancelBatch(clsMicronStocker.GetCommand(EquNo, Fork), EquNo, Fork, buffer);
                                }
                            }
                        }

                        return false;
                    }
                    else
                    {
                        sRemark = $"Error: 等候Stocker{EquNo}執行完Fork{Fork}上的命令({clsMicronStocker.GetCommand(EquNo, Fork).CommandID})";
                        if (sRemark != cmd.Remark)
                        {
                            GetDB_Object().GetCmd_Mst().FunUpdateRemark(cmd.CommandID, sRemark);
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }

        public static bool FunStockIn_WriPlc_Proc(DataGridViewRow row)
        {
            try
            {
                string sCmdSno = Convert.ToString(row.Cells[ColumnDef.CMD_MST.CmdSno.Index].Value);
                int iEquNo = Convert.ToInt32(row.Cells[ColumnDef.CMD_MST.EquNO.Index].Value);
                string sRemark_Pre = Convert.ToString(row.Cells[ColumnDef.CMD_MST.Remark.Index].Value);
                string sRemark = "";

                if (clsMicronCV.GetConveyorController().IsConnected)
                {
                    return wcs.GetProcess().FunStockInWriPlc_Proc(row);
                }
                else
                {
                    sRemark = "Error: CV PLC連線異常！";
                    if (sRemark != sRemark_Pre)
                    {
                        GetDB_Object().GetCmd_Mst().FunUpdateRemark(sCmdSno, sRemark);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                int errorLine = new System.Diagnostics.StackTrace(ex, true).GetFrame(0).GetFileLineNumber();
                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, errorLine.ToString() + ":" + ex.Message);
                return false;
            }
        }
        */
        #endregion
    }
}
