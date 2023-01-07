namespace Mirle.ASRS.WCS.Model.DataAccess
{
    public struct Remark
    {
        public const string NotInMode = "站口不是入庫模式";
        public const string NotOutMode = "站口不是出庫模式";
        public const string NotPickUpMode = "站口不是撿料模式";
        public const string NotAutoMode = "站口不是自動模式";
        public const string BufferError = "站口buffer異常中";
        public const string CycleOperating = "站口執行盤點中";
        public const string PresenceExist = "站口有荷有訊號，請移除荷有";
        public const string PresenceShuttle = "電梯有shuttle荷有";
        public const string PresenceNotExist = "站口沒有荷有訊號，請確認貨物和訊號";
        public const string CmdLeftOver = "站口有命令號，請確認命令號是否是異常殘留";
        public const string CmdNotOnBuffer = "站口沒有命令號，請確認命令號是否異常消失";
        public const string EmptyWillBefull = "即將滿板，出庫作業暫停";
        public const string Crane_EmptyWillBefull = "產生Crane命令前確認:即將滿板，出庫作業暫停";
        public const string A4EmptyisEmpty = "沒有空棧板，入庫作業暫停";
        public const string NotStoreInReady = "不是StoreInReady狀態";
        public const string NotStoreOutReady = "不是StoreOutReady狀態";
        public const string InsideLocWait = "內儲位有命令，請等待命令做完";
        public const string WMSReportComplete = "WMS出庫回報完成";
        public const string WMSReportFailTask = "WMS入庫開始任務回報失敗";
        public const string WMSReportFailDisplay = "WMS入庫開始看板回報失敗";
        public const string A4NoEmpty = "A4無空棧板，一般貨物入庫暫停";
        public const string WMSOutReportFailDisplay = "WMS出庫開始看板回報失敗";
        public const string WMSOutReportFailTask = "WMS出庫開始任務回報失敗";
        public const string WMSOutReportFailTaskFinish = "WMS出庫結束任務回報失敗";
        public const string WMSInReportFailTaskFinish = "WMS入庫結束任務回報失敗";
        public const string WMSInReportFailDisplayFinish = "WMS入庫結束看板回報失敗";
        public const string BCRReadNotON = "BCR讀取未開啟";
        public const string CV_NOTallowWriteCommand = "CV狀態_不允許寫入命令";
        public const string NotIdle = "CV_不是閒置狀態";
        public const string NoStoreInInfo = "沒有入庫資訊";
        public const string LifterNotAutoMode = "電梯不是自動模式";
        public const string LifterNotIdleMode = "電梯不是idle模式";
        public const string ExistCmdSno = "命令號已存在buffer上";
        public const string Loc_DDexistSTS_S = "內儲位有貨物，先產生庫對庫命令";
    }
}