using Mirle.Def;
using Mirle.Def.U0NXMA30;

namespace WCS_API_Server
{
    public static class BusinessTypeConvert
    {
        public static string cvtCmdMode(string businessType, string WhetherAllout)
        {
            string cvtCmdMode;
            switch (businessType)
            {
                case clsConstValue.IoType.NormalStockIn:
                case clsConstValue.IoType.PalletStockIn:
                case clsConstValue.IoType.CycleIn:
                    cvtCmdMode = clsConstValue.CmdMode.StockIn;
                    break;

                case clsConstValue.IoType.NormalStockOut:
                    if (WhetherAllout == "0") //檢料
                        cvtCmdMode = "3";
                    else
                        cvtCmdMode = clsConstValue.CmdMode.StockOut;
                    break;

                case clsConstValue.IoType.PalletStockOut:
                    cvtCmdMode = clsConstValue.CmdMode.StockOut;
                    break;

                
                case clsConstValue.IoType.CycleOut:
                    cvtCmdMode = clsConstValue.CmdMode.StockOut;
                    break;

                default:
                    cvtCmdMode = businessType; //庫對庫
                    break;
            }
            return cvtCmdMode;
        }

        public static string cvtStnNo(string businessType, string locationFrom, string locationTo)
        {
            string cvtStnNo = "";
            switch (businessType)
            {
                case clsConstValue.IoType.NormalStockOut:
                case clsConstValue.IoType.CycleOut:
                case clsConstValue.IoType.PalletStockOut:
                    cvtStnNo = locationTo;
                    break;

                case clsConstValue.IoType.NormalStockIn:
                case clsConstValue.IoType.CycleIn:
                case clsConstValue.IoType.PalletStockIn:
                    cvtStnNo = locationFrom;
                    break;
            }
            return cvtStnNo;
        }
    }
}
