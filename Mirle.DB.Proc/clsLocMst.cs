using System;
using System.Collections.Generic;
using System.Data;
using Mirle.DataBase;
using Mirle.Def;

namespace Mirle.DB.Proc
{
    public class clsLocMst
    {
        private Fun.clsLocMst LocMst = new Fun.clsLocMst();
        private clsDbConfig _config = new clsDbConfig();
        public clsLocMst(clsDbConfig config)
        {
            _config = config;
        }

       
    }
}
