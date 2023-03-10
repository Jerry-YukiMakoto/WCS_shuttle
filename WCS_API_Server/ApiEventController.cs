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
    public class WCSController : ApiController
    {
        public WCSController()
        {
        }

        [Route("api/WCS/MoveTaskAdd")]
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

                cmd.CmdSno = Body.taskNo;
                cmd.CmdMode = BusinessTypeConvert.cvtCmdMode(Body.bussinessType, Body.WhetherAllout);
                cmd.IoType = Body.bussinessType;
                cmd.taskNo = Body.taskNo;
                cmd.StnNo = BusinessTypeConvert.cvtStnNo(Body.bussinessType, Body.locationFrom, Body.locationTo);
                cmd.Loc = Body.locationFrom;
                cmd.NewLoc = Body.locationTo;
                cmd.CrtDate = Body.deliveryTime;
                cmd.Userid = "WMS";
                #endregion

                //var _conveyor = ControllerReader.GetCVControllerr().GetConveryor();

                //寫入DB
                //if (!clsDB_Proc.GetDB_Object().GetCmd_Mst().FunInsCmdMst(cmd, ref strEM))
                //    throw new Exception(strEM);

                rMsg.success = true;
                rMsg.errMsg = "";
                clsWriLog.Log.FunWriTraceLog_CV($"<{Body.taskNo}>MOVE_TASK_ADD end!");
                return Json(rMsg);

                //return Ok();
            }
            catch (Exception ex)
            {

                rMsg.success = false;
                rMsg.errMsg = ex.Message;

                var cmet = System.Reflection.MethodBase.GetCurrentMethod();
                clsWriLog.Log.subWriteExLog(cmet.DeclaringType.FullName + "." + cmet.Name, ex.Message);

                return Json(rMsg);


                //return BadRequest();
            }
    
        }
        

        
    }
}
