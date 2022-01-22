using Mirle.Gird;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.DB.Object.GridData
{
    public class Pallet : IGrid
    {
        public void SubShowCmdtoGrid(ref DataGridView oGrid)
        {
            oGrid.SuspendLayout();
            oGrid.Rows.Clear();
            //母板相關訊息填入

        }
    }
}
