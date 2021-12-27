using System;
using Mirle.DataBase;
using Mirle.Structure.Info;
using Mirle.Structure;
using System.Windows.Forms;
using Mirle.Def;



namespace Mirle.DB.Fun
{
    public class clsProc
    {
        //private clsCmd_Mst CMD_MST = new clsCmd_Mst();
        //private clsTask TaskTable = new clsTask();
        //private clsSno SNO = new clsSno();
        //private clsLocMst locMst = new clsLocMst();
        private WMS.Proc.clsHost wms;

        public clsProc(clsDbConfig dbConfig_WMS)
        {
            wms = new WMS.Proc.clsHost(dbConfig_WMS);
        }

        public WMS.Proc.clsHost GetWMS_DBObject()
        {
            return wms;
        }

       

        
    }
}
