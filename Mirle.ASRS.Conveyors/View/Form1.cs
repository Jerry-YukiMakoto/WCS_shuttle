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
            string text = textBox1.Text;
            int text1 = int.Parse(textBox2.Text);
            
            
            if (checkBox1.Checked)
            {
                _test.SetBitOn("D"+text);
            }
            else
            {
                _test.WriteWord("D" + text, text1);
            }
        }


    }
}
