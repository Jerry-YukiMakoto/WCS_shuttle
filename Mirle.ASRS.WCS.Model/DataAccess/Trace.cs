namespace Mirle.ASRS.WCS.Model.DataAccess
{
    public struct Trace
    {

        /// <summary>
        /// 讀取BCR寫入命令到Buffer
        /// </summary>
        public const string StoreInWriteCmdToCV1 = "A";
        /// <summary>
        /// 讀取BCR寫入命令到Buffer
        /// </summary>
        public const string StoreInWriteCmdToCV2 = "B";

        /// <summary>
        /// WCS_commandreportSHCCMD
        /// </summary>
        public const string StoreInWCScommandReportSHC = "A1";

        /// <summary>
        /// 讀取BCR寫入命令到Buffer
        /// </summary>
        public const string StoreInWriteCmdToCV = "21";
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
        /// 拿到撿料命令，Call_SHC移動車子
        /// </summary>
        public const string PickUpStart = "31";
        /// <summary>
        /// 拿到撿料命令，SHC回報
        /// </summary>
        public const string PickUpStartSHCreport = "31A";
        /// <summary>
        /// SHC回報完成，寫入PLC
        /// </summary>
        public const string PickUpStartSHCreportcmplete_WritePLC = "31B";
        /// <summary>
        /// 收到SHC換Lifter到車輛層通知，寫入PLC觸發(SHC_CALL)
        /// </summary>
        public const string PickUpSHCcallWCSChangeLifter = "32";
        /// <summary>
        /// Lift到車輛層，lift符合狀態，回報SHC車子可以進入LIFT
        /// </summary>
        public const string PickUpLiftOK_CallSHCCarGoinLifter = "33";
        /// <summary>
        /// 收到SHC換Lifter到出發層通知，寫入PLC觸發(SHC_CALL)
        /// </summary>
        public const string PickUpSHCcallWCSChangeLifterToStartLevel = "34";
        /// <summary>
        /// Lift到出發層，lift符合狀態，回報SHC車子可以進入LIFT
        /// </summary>
        public const string PickUpLifterToStartLevel = "35";
        /// <summary>
        /// 收到SHC換Lifter到裝卸層通知，寫入PLC觸發，也要寫入命令到lifter中(SHC_CALL)
        /// </summary>
        public const string PickUpSHCCallWcsChangeLifterToStorinLevel = "36";
        /// <summary>
        /// Lift到裝卸層，WCS回報SHC樓層
        /// </summary>
        public const string PickUpLifterToStorinLevel = "37";
        /// <summary>
        /// 出庫貨物完成車子回收，收到SHC換層需求(SHC_CALL)
        /// </summary>
        public const string PickUpCarReturn = "38";
        /// <summary>
        /// 出庫Lifter到車子放置層，回報SHC到達車子放置層(Ending)
        /// </summary>
        public const string PickUpCarToReturnCarLevel = "39";

    }
}
