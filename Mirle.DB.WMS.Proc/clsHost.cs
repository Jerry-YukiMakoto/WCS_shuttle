using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirle.Def;

namespace Mirle.DB.WMS.Proc
{
    public class clsHost
    {
        
        private clsDbConfig _config = new clsDbConfig();
        private static object _Lock = new object();
        private static bool _IsConn = false;
        public static bool IsConn
        {
            get { return _IsConn; }
            set
            {
                lock (_Lock)
                {
                    _IsConn = value;
                }
            }
        }

        public clsHost(clsDbConfig config)
        {
            _config = config;
        }

        
    }
}
