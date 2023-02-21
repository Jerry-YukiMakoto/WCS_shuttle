namespace Mirle.ASRS.WCS.Model.DataAccess
{
    public struct Trace
    {

        /// <summary>
        /// 讀取BCR寫入命令到左Buffer
        /// </summary>
        public const string StoreInWriteCmdToCV1 = "20A";
        /// <summary>
        /// 讀取BCR寫入命令到右Buffer
        /// </summary>
        public const string StoreInWriteCmdToCV2 = "20B";

        /// <summary>
        /// 讀取BCR寫入命令到Buffer
        /// </summary>
        public const string StoreInWriteCmdToCV = "21";

        /// <summary>
        /// WCS_commandreportSHCCMD
        /// </summary>
        public const string StoreInWCScommandReportSHC = "21A";
        /// <summary>
        /// 料盒於裝卸點，CV符合入庫狀態，呼叫SHC移動車子，SHC回報成功
        /// </summary>
        public const string StoreInCallSHCMoveCar = "22";
        /// <summary>
        /// 收到SHC換Lifter到車層通知，寫入PLC觸發(SHC_CALL)
        /// </summary>
        public const string StoreInSHCcallWCSChangeLifter = "23";
        /// <summary>
        /// Lift到車輛層，lift符合狀態，回報SHC車子可以進入LIFT
        /// </summary>
        public const string StoreInLiftOK_CallSHCCarGoinLifter = "24";
        /// <summary>
        /// 車子進入Lift，呼叫WCS換層到裝卸層，同時也要寫入命令到LIFTER裡面(SHC_CALL)
        /// </summary>//
        public const string StoreInSHCCallWCS_CarinLift_ChangeLayer = "25";
        /// <summary>
        /// 電梯至裝卸層，回報SHC到達目的地
        /// </summary>//
        public const string StoreInLiftAtStorinLevel = "26";
        /// <summary>
        /// 車子抓料完成，SHC提出換層需求到目的地(SHC_CALL)
        /// </summary>//
        public const string StoreInLiftToLoc = "27";
        /// <summary>
        /// Lift到入庫那層入庫Ending
        /// </summary>//
        public const string StoreInLiftToLocLevel = "28";

        /// <summary>
        /// 拿到撿料命令，預約buffer
        /// </summary>
        public const string PickUpStart = "300";
        /// <summary>
        /// 拿到撿料命令，Call_SHC移動車子
        /// </summary>
        public const string PickUpStartCallSHC = "301";
        /// <summary>
        /// 拿到撿料命令，SHC回報
        /// </summary>
        public const string PickUpStartSHCreport = "30A";
        /// <summary>
        /// SHC回報完成，寫入PLC
        /// </summary>
        public const string PickUpStartSHCreportcmplete_WritePLC = "31B";
        /// <summary>
        /// 收到SHC換Lifter到車輛層通知，寫入PLC觸發(SHC_CALL)
        /// </summary>
        public const string PickUpSHCcallWCSChangeLifter = "302";
        /// <summary>
        /// Lift到車輛層，lift符合狀態，回報SHC車子可以進入LIFT
        /// </summary>
        public const string PickUpLiftOK_CallSHCCarGoinLifter = "303";
        /// <summary>
        /// 收到SHC換Lifter到出發層通知，寫入PLC觸發(SHC_CALL)
        /// </summary>
        public const string PickUpSHCcallWCSChangeLifterToStartLevel = "304";
        /// <summary>
        /// Lift到出發層，lift符合狀態，回報SHC車子可以進入貨梯拿貨再出來
        /// </summary>
        public const string PickUpLifterToStartLevel = "305";
        /// <summary>
        /// 車子拿到貨物，回報WCS換電梯層的訊號(SHC_call)
        /// </summary>
        public const string PickUpSHCCallGetlotOK_ChangeLayer = "306";
        /// <summary>
        /// 回報SHC電梯層的訊號正確，可以進入電梯
        /// </summary>
        public const string PickUpLifterLevelCorret = "307";
        /// <summary>
        /// 收到SHC換Lifter到裝卸層通知，寫入PLC觸發(SHC_CALL)
        /// </summary>
        public const string PickUpSHCCallWcsChangeLifterToStorinLevel = "308";
        /// <summary>
        /// Lift到裝卸層，WCS回報SHC樓層
        /// </summary>
        public const string PickUpLifterToStorinLevel = "309";
        /// <summary>
        /// 出庫貨物完成車子回收，收到SHC換層需求(SHC_CALL)
        /// </summary>
        public const string PickUpCarReturn = "310";
        /// <summary>
        /// 出庫Lifter到車子放置層，回報SHC到達車子放置層(Ending)
        /// </summary>
        public const string PickUpCarToReturnCarLevel = "311";
        /// <summary>
        /// 庫對庫開始，命令資訊上報給SHC
        /// </summary>
        public const string LocToLocStart = "51";
        /// <summary>
        /// 庫對庫，命令資訊SHC回報收到
        /// </summary>
        public const string LocToLocStartSHCreceive = "51A";
        /// <summary>
        /// 庫對庫，收到SHC換Lifter到車輛層通知，寫入PLC觸發(SHC_CALL)
        /// </summary>
        public const string LocToLocStartSHCcall = "52";
        /// <summary>
        /// Lift到車輛層，lift符合狀態，回報SHC車子可以進入LIFT
        /// </summary>
        public const string Loc2LocCallShcLifterSafe = "53";
        /// <summary>
        /// 庫對庫，收到SHC換Lifter到庫對庫層通知，寫入PLC觸發(SHC_CALL)
        /// </summary>
        public const string LocToLocStartSHCcalltoL2Lfloor = "54";
        /// <summary>
        /// 庫對庫，Lift到庫對庫層，lift符合狀態，回報SHC=>Lifter到達目的地
        /// </summary>
        public const string LocToLocCallSHCtoL2Lfloor = "55";

        /// <summary>
        /// 二重格發生，更新二重格命令
        /// </summary>
        public const string DoubleStorageStart = "Q1";

        /// <summary>
        /// 二重格命令發送給SHC
        /// </summary>
        public const string DoubleStorageSHCcall = "Q2";

        /// <summary>
        /// 二重格命令收到回傳SHC收到
        /// </summary>
        public const string DoubleStorageSHCreceive = "Q3";

    }
}
