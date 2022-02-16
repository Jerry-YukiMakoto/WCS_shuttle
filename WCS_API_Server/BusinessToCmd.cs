using Mirle.Def;
using Mirle.Def.U0NXMA30;

namespace WCS_API_Server
{
    public static class BusinessToCmd
    {
        public static string ConvertToCmd(string businessType, string WhetherAllout)
        {
            string cvtCmd;
          
            switch (businessType)
            {
                case clsConstValue.IoType.NormalStockIn:
                case clsConstValue.IoType.PalletStockIn:
                    cvtCmd = clsConstValue.CmdMode.StockIn;
                    break;
                
                case clsConstValue.IoType.NormalStockOut:
                    if(WhetherAllout == "0") //檢料
                        cvtCmd = "6";
                    else
                        cvtCmd = clsConstValue.CmdMode.StockOut;
                    break;

                case clsConstValue.IoType.PalletStockOut:
                    cvtCmd = clsConstValue.CmdMode.StockOut;
                    break;

                case clsConstValue.IoType.CycleIn:
                case clsConstValue.IoType.CycleOut:
                    cvtCmd = clsConstValue.CmdMode.Cycle;
                    break;

                default:
                    cvtCmd = businessType; //庫對庫
                    break;
            }
            
            return cvtCmd;
        }
    }
}
