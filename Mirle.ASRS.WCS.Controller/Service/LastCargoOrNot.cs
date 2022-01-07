using System;
using System.Collections.Generic;
using System.Timers;
using Mirle.ASRS.WCS.Model.DataAccess;
using Mirle.ASRS.WCS.Model.LogTrace;
using Mirle.ASRS.WCS.Model.PLCDefinitions;
using Mirle.ASRS.Conveyors;
using Mirle.DataBase;
using Mirle.DB.Object;



namespace Mirle.ASRS.WCS.Controller.Service
{
    public class LastCargoOrNot 
    {
        private readonly Conveyor _conveyor;
        private readonly DataAccessManger _dataAccessManger;

        public LastCargoOrNot()
        {
            _conveyor = ControllerReader.GetCVControllerr().GetConveryor();
            _dataAccessManger = ControllerReader.GetDataAccessManger();
            CheckLastCargo();
        }

        public bool CheckLastCargo()
        {
            if (_dataAccessManger.GetCmdMstByStoreOutcheck(StnNo.A3, out var dataObject) == GetDataResult.Success)
            {
                int COUNT = Convert.ToInt32(dataObject[0].COUNT);

                if (_conveyor.GetBuffer(2).A2LV2 == 0 && COUNT == '0' && _conveyor.GetBuffer(2).CommandId == 0 && _conveyor.GetBuffer(1).CommandId == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;//異常連不到資料庫
            }

        }
    }
}