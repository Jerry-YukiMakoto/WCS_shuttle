using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Grid
{
    public interface IGrid
    {
        void SubShowCmdtoGrid(ref DataGridView oGrid);
    }
}
