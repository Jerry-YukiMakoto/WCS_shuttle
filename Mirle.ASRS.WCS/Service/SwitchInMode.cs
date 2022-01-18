using System;
using System.Collections.Generic;
using System.Timers;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.ASRS.WCS.Model.LogTrace;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.ASRS.Conveyors;
using Mirle.ASRS.WCS.Controller;
using Mirle.DataBase;
using Mirle.DB.Object;


namespace Mirle.ASRS.WCS.Service
{
    public class SwitchInMode 
    {

        public static void Switch_InMode(Conveyors.Conveyor _conveyor,LoggerManager _loggerManager)//自動切換入庫模式
        {
            #region//確認目前模式，是否可以切換模式，可以就寫入切換成入庫的請求
            if (_conveyor.GetBuffer(1).Ready != Ready.StoreInReady)
            {
                if (_conveyor.GetBuffer(1).CmdMode <= 1 && _conveyor.GetBuffer(2).CmdMode <= 1 && _conveyor.GetBuffer(3).CmdMode <= 1 && _conveyor.GetBuffer(4).CmdMode <= 1)
                {
                    if (_conveyor.GetBuffer(1).Switch_Ack == 1) 
                    {

                        var WritePlccheck = _conveyor.GetBuffer(1).Switch_Mode(1).Result;//確認寫入PLC的方法是否正常運作，傳回結果和有異常的時候的訊息
                        bool Result = WritePlccheck;
                        if (Result != true)
                        {
                            var log = new StoreOutLogTrace(_conveyor.GetBuffer(1).BufferIndex, _conveyor.GetBuffer(1).BufferName, $"Normal-StoreOut Switchmode fail");
                            _loggerManager.WriteLogTrace(log);   
                        }
                        else
                        {
                            var log = new StoreOutLogTrace(_conveyor.GetBuffer(1).BufferIndex, _conveyor.GetBuffer(1).BufferName, "Normal-StoreOut Switchmode Complete");
                            _loggerManager.WriteLogTrace(log);
                        }
                    }
                }
            }
            #endregion 
        }
    }
}