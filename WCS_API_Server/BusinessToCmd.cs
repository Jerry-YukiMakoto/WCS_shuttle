using Mirle.Def;
using Mirle.Def.U0NXMA30;

namespace WCS_API_Server
{
    public static class BusinessToCmd
    {
        public static string ConvertToCmd(string businessType)
        {
            string cvtCmd;
          
            switch (businessType)
            {
                case clsConstValue.IoType.NormalStockIn:
                case clsConstValue.IoType.PalletStockIn:
                    cvtCmd = clsConstValue.CmdMode.StockIn;
                    break;
                
                case clsConstValue.IoType.NormalStockOut:
                case clsConstValue.IoType.PalletStockOut:
                    cvtCmd = clsConstValue.CmdMode.StockOut;
                    break;
                
                case clsConstValue.IoType.Cycle:
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
