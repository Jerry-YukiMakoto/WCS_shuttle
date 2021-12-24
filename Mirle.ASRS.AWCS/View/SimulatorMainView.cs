using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Mirle.ASRS.Conveyors;

namespace Mirle.ASRS.AWCS.View
{
    public partial class SimulatorMainView : Form
    {
        private readonly Conveyor _conveyor;

        private SimulatorMainView()
        {
            InitializeComponent();
        }
        public SimulatorMainView(CVCSHost host) : this()
        {
            _conveyor = host.GetCVControllerr().GetConveryor();
        }
    }
}
