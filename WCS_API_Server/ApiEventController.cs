using System;
using System.Data;
using System.Web.Http;
using Newtonsoft.Json;
using WCS_API_Server.Models;
using Mirle.Structure;
using Mirle.Def;
using Mirle.DataBase;
using Mirle.DB.Object;

namespace WCS_API_Server
{
    public class WMSWCSController : ApiController
    {
        public WMSWCSController()
        {
        }

        [Route("WMSWCS/MOVE_TASK_ADD")]
        [HttpPost]
        public IHttpActionResult MOVE_TASK_ADD([FromBody] MoveTaskAddInfo Body)
        {
            clsWriLog.Log.FunWriTraceLog_CV($"<MOVE_TASK_ADD> <WMS Send>\n{JsonConvert.SerializeObject(Body)}");
            ReturnMessage rMsg = new ReturnMessage
            {
                lineId = Body.lineId,
                taskNo = Body.taskNo,
            };
            clsWriLog.Log.FunWriTraceLog_CV($"<{Body.taskNo}>MOVE_TASK_ADD start!");
            
            
            try
            {
                #region 填入cmd資訊
                CmdMstInfo cmd = new CmdMstInfo();
                string strEM = "";

                cmd.CmdSno = clsDB_Proc.GetDB_Object().GetSNO().FunGetSeqNo(clsEnum.enuSnoType.CMDSNO);
                if (string.IsNullOrWhiteSpace(cmd.CmdSno))
                {
                    throw new Exception($"<{Body.taskNo}>取得序號失敗！");
                }

                cmd.CmdMode = BusinessToCmd.ConvertToCmd(Body.bussinessType);
                cmd.IoType = Body.bussinessType;
                cmd.taskNo = Body.taskNo;
                cmd.Loc = Body.locationFrom;
                cmd.NewLoc = Body.locationTo;
                cmd.Prt = Body.priority;
                cmd.CrtDate = Body.deliveryTime;
                cmd.Userid = "WMS";
                #endregion

                //寫入DB
                if (!clsDB_Proc.GetDB_Object().GetCmd_Mst().FunInsCmdMst(cmd, ref strEM))
                    throw new Exception(strEM);
                
                rMsg.success = true;
                rMsg.errMsg = "";
                clsWriLog.Log.FunWriTraceLog_CV($"<{Body.taskNo}>MOVE_TASK_ADD end!");
                return Json(rMsg);
                
                //return Ok();
            }
            catch (Exception ex)
            {
               
                rMsg.success=false;
                rMsg.errMsg = ex.Message;

                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName+"."+cmet.Name, ex.Message);

                return Json(rMsg);
                
                
                //return BadRequest();
            }
    
        }
        
        [Route("WMSWCS/MOVE_TASK_FORCE_CLEAR")]
        [HttpPost]
        public IHttpActionResult MOVE_TASK_FORCE_CLEAR([FromBody] MoveTaskAddInfo Body)
        {
            clsWriLog.Log.FunWriTraceLog_CV($"<MOVE_TASK_FORCE_CLEAR> <WMS Send>\n{JsonConvert.SerializeObject(Body)}");
            ReturnMessage rMsg = new ReturnMessage
            {
                lineId = Body.lineId,
                taskNo = Body.taskNo
            };
            clsWriLog.Log.FunWriTraceLog_CV($"<{Body.taskNo}>MOVE_TASK_FORCE_CLEAR start!");
            try
            {
                string strEM = "";
                if (!clsDB_Proc.GetDB_Object().GetProcess().FunMoveTaskForceClear(Body.taskNo, ref strEM))
                    throw new Exception(strEM);
                rMsg.success = true;
                rMsg.errMsg = "";
                clsWriLog.Log.FunWriTraceLog_CV($"<{Body.taskNo}>MOVE_TASK_FORCE_CLEAR end!");
                return Json(rMsg);
                
                //return Ok();
            }
            catch(Exception ex)
            {
                
                rMsg.success = false;
                rMsg.errMsg = ex.Message;

                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);

                return Json(rMsg);
                
                //return NotFound();
            }
            
        }
        
    }
}
