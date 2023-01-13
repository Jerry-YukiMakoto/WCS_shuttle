using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using HslCommunicationPLC.Siemens;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunicationPLC;
using HslCommunication.Profinet.Siemens;

namespace TestConnect
{
    public partial class Form1 : Form
    {
        private clsHost plc = null;
        public Form1()
        {
            InitializeComponent();

            string[] plcList = new string[]
            {
                SiemensPLCS.S1200.ToString(),
                SiemensPLCS.S1500.ToString(),
                SiemensPLCS.S200.ToString(),
                SiemensPLCS.S200Smart.ToString(),
                SiemensPLCS.S300.ToString(),
                SiemensPLCS.S400.ToString()
            };

            cboPLC.Items.Clear();
            cboPLC.Items.AddRange(plcList);
            cboPLC.SelectedIndex = 0;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                var b = sender as Button;
                b.Enabled = false;
                try
                {
                    PlcConfig config = new PlcConfig
                    {
                        //ConnectionType = Convert.ToByte(txtConnectionType.Text),
                        IpAddress = txtIP.Text,
                        //LocalTSAP = Convert.ToInt32(txtLocalTSAP.Text),
                        //Port = Convert.ToInt32(txtPort.Text),
                        siemensPLCS = (SiemensPLCS)Enum.Parse(typeof(SiemensPLCS),cboPLC.Text)
                    };

                    plc = new clsHost(config);
                    plc.Connect();
                }
                finally
                {
                    b.Enabled = true;
                }
            }
        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            plc.Disconnect();
        }

        private void btnWriWord_Click(object sender, EventArgs e)
        {
            var b = sender as Button;
            b.Enabled = false;
            try
            {
                if (string.IsNullOrWhiteSpace(txtWordDevice.Text) ||
                    string.IsNullOrWhiteSpace(txtWordValue.Text) ||
                    plc == null || !plc.IsConn)
                    return;
                string[] strRetData = txtWordValue.Text.Split(',');
                short[] intRetData = null;
                bool bolRet = clsTool.ArrayTransfer_S2I(strRetData, ref intRetData);
                if (bolRet)
                {
                    if (plc.WriteBlock(txtWordDevice.Text.Trim(), intRetData))
                    {
                        if (plc.ReadBlock(txtWordDevice.Text.Trim(), ref intRetData))
                        {
                            string value = txtWordDevice.Text.Trim() + ": ";
                            for (int i = 0; i < intRetData.Length; i++)
                            {
                                if (i == 0) value += intRetData[i];
                                else value += $",{intRetData[i]}"; 
                            }

                            SubWriTraceLog_CV(value);
                        }
                        else return;
                    }
                    else return;
                }
                else return;
            }
            finally
            {
                b.Enabled = true;
            }
        }

        private void btnWriBit_Click(object sender, EventArgs e)
        {
            var b = sender as Button;
            b.Enabled = false;
            try
            {
                if (string.IsNullOrWhiteSpace(txtWordBitDevice.Text) ||
                    plc == null || !plc.IsConn)
                    return;
                if (plc.WriteBit(txtWordBitDevice.Text.Trim(), chkBit.Checked))
                {
                    string sBit = chkBit.Checked ? "ON" : "OFF";
                    string value = txtWordBitDevice.Text.Trim() + $": {sBit}";
                    SubWriTraceLog_CV(value);
                }
                else return;
            }
            finally
            {
                b.Enabled = true;
            }
        }

        private void btnReadWord_Click(object sender, EventArgs e)
        {
            var b = sender as Button;
            b.Enabled = false;
            try
            {
                if (string.IsNullOrWhiteSpace(txtWordDevice.Text) ||
                   string.IsNullOrWhiteSpace(txtWordValue.Text) ||
                   plc == null || !plc.IsConn)
                    return;
                short[] intRetData = new short[Convert.ToInt32(txtWordValue.Text)];
                if (plc.ReadBlock(txtWordDevice.Text.Trim(), ref intRetData))
                {
                    string value = txtWordDevice.Text.Trim() + ": ";
                    for (int i = 0; i < intRetData.Length; i++)
                    {
                        if (i == 0) value += intRetData[i];
                        else value += $",{intRetData[i]}";
                    }

                    SubWriTraceLog_CV(value);
                }
                else return;
            }
            finally
            {
                b.Enabled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            try
            {
                if (plc == null || !plc.IsConn) lblConn.BackColor = Color.Red;
                else lblConn.BackColor = Color.GreenYellow;
            }
            finally
            {
                timer1.Enabled = true;
            }
        }

        delegate void degSubWriTraceLog_CV(string sValue);
        /// <summary>
        /// Trace Log for周邊
        /// </summary>
        /// <param name="bFlag"></param>
        /// <param name="sValue">Trace Msg</param>
        private void SubWriTraceLog_CV(string sValue)
        {
            degSubWriTraceLog_CV obj;
            try
            {
                if (InvokeRequired)
                {
                    obj = new degSubWriTraceLog_CV(SubWriTraceLog_CV);
                    Invoke(obj, sValue);
                }
                else
                {
                    if (TraceLog_CV.Items.Count > 15)
                    {
                        TraceLog_CV.Items.Clear();
                    }

                    string sDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string sTraceLog = "[" + sDate + "] " + sValue;
                    TraceLog_CV.Items.Insert(0, sTraceLog);
                }
            }
            finally
            {
                obj = null;
            }
        }
    }
}
