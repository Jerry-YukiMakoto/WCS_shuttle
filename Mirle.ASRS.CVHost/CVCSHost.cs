using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Mirle.ASRS.AWCS.Manager;
//using Mirle.ASRS.AWCS.Config;
using Config.Net;
using Mirle.DataBase;

namespace Mirle.ASRS.CVCSHost
{
    public class CVCSHost : IDisposable
    {
        //private readonly WCSManager _wcsManager;
        //private readonly CVController _cvController;
        //private readonly LoggerManager _loggerManager;
        //private readonly DataAccessManger _dataAccessManger;
        private readonly Form mainView;

        public CVCSHost()
        {
            

        }
        #region Dispose
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //_cvController.Dispose();
                    //_loggerManager.Dispose();
                    //_wcsManager.Dispose();
                    mainView.Dispose();
                }

                disposedValue = true;
            }
        }

        ~CVCSHost()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion Dispose

    }
}
