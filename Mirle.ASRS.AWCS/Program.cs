using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Mirle.ASRS.AWCS.View;

namespace Mirle.ASRS.AWCS
{
    static class Program
    {
        [MTAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainPanel());
        }
    }
}
