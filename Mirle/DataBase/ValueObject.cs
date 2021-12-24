using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.DataBase
{
    public abstract class ValueObject
    {
        protected internal abstract ValueObject ConvaertDataRow(DataRow row);
    }
}
