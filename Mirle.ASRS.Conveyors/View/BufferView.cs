using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Mirle.MPLC.DataType;

namespace Mirle.ASRS.Conveyors.View
{
    public partial class BufferView : UserControl
    {
        public int BufferIndex
        {
            get { return Convert.ToInt32(lblBufferIndex.Text); }
            set { lblBufferIndex.Text = Convert.ToString(value); }
        }

        public BufferView()
        {
            InitializeComponent();

            funInitToolTip();

        }

        /// <summary>
        /// 游標提示
        /// </summary>
        private void funInitToolTip()
        {
            string strBufferIndex = "位置編號";
            string strBufferName = "輸送機編號\n背景色: 綠色 -> 正常\n紅色 -> 異常";
            string strCommandId = "序號";
            string strCmdMode = "模式:\n1 -> 入庫\n2 -> 出庫\n3 -> 盤點";
            string strAuto = "自動模式:\n綠燈 -> 自動ON\n黃燈 -> 手動";
            string strReady = "Ready訊號:\n0 -> No Ready\n1 -> 入庫Ready\n2 -> 出庫Ready"; //A1,4,5,7,9
            string strPathNotice = "路徑編號:\n由WCS告知CV貨物該往哪走"; //A1~A4
            string strPresence = "荷有:\nV -> 此位置有物";
            string strSwitch_Ack = "站口模式切換:\n0 -> 不允許ON\n1 -> 允許"; //A1
            string strInitialNotice = "初始通知:\n1 -> CV通知WCS輸送機初始已完成";
            string strError = "異常碼";


            ToolTip objToolTip = new ToolTip();
            objToolTip.AutoPopDelay = 5000;
            objToolTip.InitialDelay = 100;
            objToolTip.ReshowDelay = 100;
            objToolTip.UseAnimation = false;
            objToolTip.UseFading = false;
            objToolTip.ShowAlways = true;

            objToolTip.SetToolTip(lblBufferIndex, strBufferIndex);
            objToolTip.SetToolTip(lblBufferName, strBufferName);
            objToolTip.SetToolTip(lblCommandId, strCommandId);
            objToolTip.SetToolTip(lblCmdMode, strCmdMode);
            objToolTip.SetToolTip(lblAuto, strAuto);
            objToolTip.SetToolTip(lblReady, strReady);
            objToolTip.SetToolTip(lblPathNotice, strPathNotice);
            objToolTip.SetToolTip(lblPresence, strPresence);
            objToolTip.SetToolTip(lblSwitch_Ack, strSwitch_Ack);
            objToolTip.SetToolTip(lblInitialNotice, strInitialNotice);
           
        }

        public void Refresh_Buffer(Buffer buffer)
        {
            Refresh(lblBufferName, buffer.BufferName, buffer.Error);
            Refresh(lblCommandId, buffer.CommandId.ToString("D4"));
            Refresh(lblCmdMode, buffer.CmdMode.ToString());
            Refresh(lblAuto, buffer.Auto.ToColor(Color.LightGreen, Color.Yellow));
            Refresh(lblReady, buffer.Ready.ToString());
            Refresh(lblPathNotice, buffer.PathNotice.ToString());
            Refresh(lblPresence, buffer.Presence.ToColor());
            Refresh(lblSwitch_Ack, buffer.Switch_Ack.ToString()); 
            Refresh(lblInitialNotice, buffer.InitialNotice.ToString());
        }

        private void Refresh(Label label, string value)
        {
            if (InvokeRequired)
            {
                var action = new Action<Label, string>(Refresh);
                Invoke(action, label, value);
            }
            else
            {
                label.Text = value;
            }
        }

        private void Refresh(Label label, Color color)
        {
            if (InvokeRequired)
            {
                var action = new Action<Label, Color>(Refresh);
                Invoke(action, label, color);
            }
            else
            {
                label.BackColor = color;
            }
        }

        private void Refresh(Label label, string bufferName, bool error)
        {
            if (InvokeRequired)
            {
                var action = new Action<Label, string, bool>(Refresh);
                Invoke(action, label, error);
            }
            else
            {
                label.Text = bufferName;

                label.BackColor = error ? Color.Red : Color.LimeGreen;
                
            }
        }
    }
}
