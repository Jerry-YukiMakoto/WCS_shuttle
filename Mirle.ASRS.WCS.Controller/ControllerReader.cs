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
        private static CVController _cvController;
        private static LoggerManager _loggerManager;
        private static Form mainView;

        public static void FunGetController(clsPlcConfig CVConfig) 
        {
            _loggerManager = new LoggerManager();
            _cvController = new CVController(CVConfig.MPLCIP, CVConfig.MPLCPort, CVConfig.SignalGroup, CVConfig.InMemorySimulator);
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
        #endregion Get_Manager

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
