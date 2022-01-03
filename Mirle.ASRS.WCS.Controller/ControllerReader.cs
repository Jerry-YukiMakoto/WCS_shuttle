using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.DataBase;
using Config.Net;
using System.Windows.Forms;
using Mirle.Def.U0NXMA30;
using Mirle.Def;

namespace Mirle.ASRS.WCS.Controller
{
    public class ControllerReader : IDisposable
    {
        private static WCSManager _wcsManager;
        private static CVController _cvController;
        private static LoggerManager _loggerManager;
        private static DataAccessManger _dataAccessManger;
        private static Form mainView;

        

        public static void FunGetController(clsDbConfig dbConfig, clsPlcConfig CVConfig) {

            _wcsManager = new WCSManager(CVConfig.SignalGroup == 0);
            _cvController = new CVController(CVConfig.MPLCIP, CVConfig.MPLCPort, CVConfig.SignalGroup, CVConfig.InMemorySimulator);
            _dataAccessManger = new DataAccessManger(dbConfig);
            _loggerManager = new LoggerManager();            
            _wcsManager.Start();
        }

        #region Get_Manager
        public static LoggerManager GetLoggerManager()
        {
            return _loggerManager;
        }

        public static CVController GetCVControllerr()
        {
            return _cvController;
        }

        public static WCSManager GetWCSManager()
        {
            return _wcsManager;
        }

        public static DataAccessManger GetDataAccessManger()
        {
            return _dataAccessManger;
        }

        #endregion Get_Manager

        public void AppClosing()
        {
            _wcsManager.Stop();
        }

        #region Dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cvController.Dispose();
                    _loggerManager.Dispose();
                    _wcsManager.Dispose();
                    //mainView.Dispose();
                }

                disposedValue = true;
            }
        }
        ~ControllerReader()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion Dispose

        //public Form GetMainView()
        //{
        //    return mainView;
        //}
    }
}
