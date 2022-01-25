using Mirle.MPLC;
using Mirle.MPLC.DataBlocks;
using Mirle.MPLC.DataBlocks.DeviceRange;
using Mirle.MPLC.SharedMemory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.ASRS.WCS.Controller
{
    public partial class Form1 : Form
    {
        private IMPLCProvider _test;

        public Form1()
        {
            InitializeComponent();
        }
        private void SimMainView_Load(object sender, EventArgs e)
        {
            var smWriter = new SMReadWriter();
            var blockInfos = GetBlockInfos();
            foreach (var block in blockInfos)
            {
                smWriter.AddDataBlock(new SMDataBlockInt32(block.DeviceRange, $@"Global\{block.SharedMemoryName}"));
            }
            testsimulator(smWriter);
        }

        private IEnumerable<BlockInfo> GetBlockInfos()
        {
            yield return new BlockInfo(new DDeviceRange("D101", "D210"), "Read", 0);
            yield return new BlockInfo(new DDeviceRange("D3101", "D3210"), "Write", 1);
        }

        private void testsimulator(IMPLCProvider test)
        {
            _test = test;
        }

        private void buttonwrite(object sender, EventArgs e)
        {
            int text1 = int.Parse(textBox1.Text);

            int text2 = 0;
            if (textBox2.Text != "")
            {
                text2 = int.Parse(textBox2.Text);
            }

            if (CMD.Checked)
            {
                _test.WriteWord("D" + (101+text1*10), text2);
            }
            if(mode.Checked)
            {
                _test.WriteWord("D" + (101 + text1 * 10+1), text2);
            }
            if(Auto.Checked)
            {
                _test.SetBitOn("D" + (101 + text1 * 10 + 2.5));
            }
            if (presence.Checked)
            {
                _test.SetBitOn("D" + (101 + text1 * 10 + 2.7));
            }
            if (initialnotice.Checked)
            {
                _test.WriteWord("D" + (101 + text1 * 10 + 9),text2);
            }
            if (ready.Checked)
            {
                _test.WriteWord("D" + (101 + text1 * 10 + 3), text2);
            }
            if (path.Checked)
            {
                _test.WriteWord("D" + (101 + text1 * 10 + 4), text2);
            }
            if (switchmode.Checked)
            {
                _test.WriteWord("D" + (101 + text1 * 10 + 8), text2);
            }
            if (Inmode.Checked)
            {
                _test.SetBitOn("D" + (101 + text1 * 10 + 2.1));
            }
            if (Outmode.Checked)
            {
                _test.SetBitOn("D" + (101 + text1 * 10 + 2.2));
            }
            if (error.Checked)
            {
                _test.SetBitOn("D" + (101 + text1 * 10 + 2.4));
            }
            if (emptynumber.Checked)
            {
                _test.WriteWord("D" + (101 + text1 * 10 + 7), text2);
            }
        }
    }
}
