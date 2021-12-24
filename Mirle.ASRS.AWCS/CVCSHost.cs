using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Config.Net;

using Mirle.ASRS.AWCS.Config;
using Mirle.ASRS.AWCS.Manager;
using Mirle.ASRS.AWCS.View;
using Mirle.DataBase;

namespace Mirle.ASRS.AWCS
{
    public class CVCSHost : IDisposable
    {
        private readonly WCSManager _wcsManager;
        private readonly CVController _cvController;
        private readonly LoggerManager _loggerManager;
        private readonly DataAccessManger _dataAccessManger;
        private readonly Form mainView;

        public CVCSHost()
        {
            var ini = new ConfigurationBuilder<IWCS>().UseIniFile("Config\\WCS.ini").Build();

            var dbOptions = new DBOptions();
            if (ini.Database.DBType == "OracleClient")
            {
                dbOptions.SetDBType(DBTypes.OracleClient);
            }
            else
            {
                dbOptions.SetDBType(DBTypes.SqlServer);
            }
            dbOptions.SetDBServer(ini.Database.DBServer, ini.Database.DBPort);
            dbOptions.SetAccount(ini.Database.DBName, ini.Database.DBUser, ini.Database.DBPassword);
            dbOptions.SetConnectTimeOut(ini.Database.ConnectTimeOut);
            if (ini.Database.WriteLog)
            {
                dbOptions.EnableWriteLog();
            }
            _dataAccessManger = new DataAccessManger(this, dbOptions);
            _loggerManager = new LoggerManager();
            _cvController = new CVController(ini.CVCS.MPLCIP, ini.CVCS.MPLCPort, ini.CVCS.SignalGroup, ini.Simulator.Enable);
            _wcsManager = new WCSManager(this, ini.CVCS.SignalGroup == 0);

            if (ini.CVCS.SignalGroup == 0)
            {
                mainView = new BQAMainView(this);
            }
            else
            {
                mainView = new MFGMainView(this);
            }

            _wcsManager.Start();
        }

        public CVController GetCVControllerr()
        {
            return _cvController;
        }

        public WCSManager GetWCSManager()
        {
            return _wcsManager;
        }

        public LoggerManager GetLoggerManager()
        {
            return _loggerManager;
        }

        public DataAccessManger GetDataAccessManger()
        {
            return _dataAccessManger;
        }

        public Form GetMainView()
        {
            return mainView;
        }

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
