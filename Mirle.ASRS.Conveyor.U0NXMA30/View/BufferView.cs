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

namespace Mirle.ASRS.Conveyors.U0NXMA30.View
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

        private void funInitToolTip()
        {
            string strBufferName = "輸送機編號";
            string strCommandId = "序號";
            string strCmdMode = "模式:\n1 -> 入庫\n2 -> 出庫\n3 -> 盤點\n5 -> 庫對庫";
            string strAuto = "自動模式:\n1 -> 自動ON\n2 -> 手動";
            string strReady = "Ready訊號:\n1 -> 入庫Ready\n2 -> 出庫Ready";
            string strPathNotice = "路徑編號:\n由WCS告知CV貨物該往哪走";
            string strError = "異常碼";
            string strInitialNotice = "初始通知:\n1 -> CV通知WCS輸送機初始已完成";
            string strPresence = "荷有:\nV -> 此位置有物";

            ToolTip objToolTip = new ToolTip();
            objToolTip.AutoPopDelay = 5000;
            objToolTip.InitialDelay = 100;
            objToolTip.ReshowDelay = 100;
            objToolTip.UseAnimation = false;
            objToolTip.UseFading = false;
            objToolTip.ShowAlways = true;

            objToolTip.SetToolTip(lblBufferName, strBufferName);
            objToolTip.SetToolTip(lblCommandId, strCommandId);
            objToolTip.SetToolTip(lblCmdMode, strCmdMode);
            objToolTip.SetToolTip(lblAuto, strAuto);
            objToolTip.SetToolTip(lblReady, strReady);
            objToolTip.SetToolTip(lblPathNotice, strPathNotice);
            objToolTip.SetToolTip(lblError, strError);
            objToolTip.SetToolTip(lblPresence, strPresence);
            objToolTip.SetToolTip(lblInitialNotice, strInitialNotice);
           
        }

        public void Refresh_Buffer(Buffer buffer)
        {
            Refresh(lblBufferName, buffer.BufferName, buffer.Auto, buffer.Manual, buffer.Error);
            Refresh(lblCommandId, buffer.CommandId.ToString("D4"));
            Refresh(lblInitialNotice, buffer.InitialNotice.ToString());
            Refresh(lblReady, buffer.Ready.ToString());
            Refresh(lblAuto, buffer.Auto.ToString());
            Refresh(lbl2ndLayer, buffer.PickingDirection.ToString());
            Refresh(lblPresence, buffer.Presence.ToColor());
            Refresh(lblError, buffer.Error.ToColor());
            Refresh(lblSwitch_Ack, buffer.ReadNotice.ToString());
            Refresh(lblCmdMode, buffer.Ready.ToString());
            Refresh(lblPathNotice, buffer.PathNotice.ToString());
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
        private void Refresh(Label label, string bufferName, bool auto, bool manual, bool error)
        {
            if (InvokeRequired)
            {
                var action = new Action<Label, string, bool, bool, bool>(Refresh);
                Invoke(action, label, auto, manual, error);
            }
            else
            {
                label.Text = bufferName;
                if (error)
                {
                    label.BackColor = Color.Red;
                }
                else if (manual)
                {
                    label.BackColor = Color.Yellow;
                }
                else if (auto)
                {
                    label.BackColor = Color.Lime;
                }
                else
                {
                    label.BackColor = Color.Red;
                }
            }
        }
    }
}
