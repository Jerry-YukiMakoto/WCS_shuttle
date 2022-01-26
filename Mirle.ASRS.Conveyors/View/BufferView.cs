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

        #region 需填入參數
        public class strCmdMode
        {
            public const string None = "0"; //Initial
            public const string StockIn = "1"; 
            public const string StockOut = "2";
            public const string Cycle = "3";
            public const string L2L = "5";
        }

        public class Ready 
        {
            public const int NoReady = 0;
            public const int InReady = 1;
            public const int OutReady = 2;
        }

        public class Switch_Ack
        {
            public const int NoAck = 0;
            public const int Ack = 1;
        }

        public class InitialNotice
        {
            public const int Initial = 0;
            public const int InitialNoticeFinished = 1;
        }
        

        public string Auto(bool isOn)
        {
            if (isOn)
                return "A"; //auto
            else
                return "M"; //manual
        }

        public string Presence(bool isOn)
        {
            if (isOn)
                return "V"; //荷有
            else
                return String.Empty; //荷無
        }

        #endregion 需填入參數

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
            string strBufferName = "輸送機編號\n背景色: 綠色 -> 正常\n橘色 -> 此位置有物\n紅色 -> 異常";
            string strCommandId = "序號\n未顯示表連線失敗";
            string strCmdMode = "模式:\n1 -> 入庫\n2 -> 出庫\n3 -> 盤點";
            string strAuto = "自動模式:\n綠燈A -> 自動ON\n黃燈M -> 手動\n紅燈M -> 連線失敗";
            string strReady = "Ready訊號:\n0 -> No Ready\n1 -> 入庫Ready\n2 -> 出庫Ready"; //A1,4,5,7,9
            string strPathNotice = "路徑編號:\n由WCS告知CV貨物該往哪走"; //A1~A4
            string strPresence = "荷有:\n橘色V -> 此位置有物";
            string strSwitch_Ack = "站口模式切換:\n0 -> 不允許ON\n1 -> 允許"; //A1
            string strInitialNotice = "初始通知:\n1 -> CV通知WCS輸送機初始已完成";
            //string strError = "異常碼";


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
            Refresh(lblBufferName, buffer.BufferName, buffer.Presence, buffer.Error);
            Refresh(lblCommandId, buffer.CommandId.ToString("D4"));
            Refresh(lblCmdMode, buffer.CmdMode.ToString());
            Refresh(lblAuto, Auto(buffer.Auto), buffer.Auto.ToColor(Color.ForestGreen, Color.Yellow));
            Refresh(lblReady, buffer.Ready.ToString());
            Refresh(lblPathNotice, buffer.PathNotice.ToString());
            Refresh(lblPresence, Presence(buffer.Presence), buffer.Presence.ToColor(Color.Orange, Color.White));
            Refresh(lblSwitch_Ack, buffer.Switch_Ack.ToString()); 
            Refresh(lblInitialNotice, buffer.InitialNotice.ToString());
        }

        public void Refresh_BufferPLCError(Buffer buffer)
        {
            Refresh(lblCommandId, String.Empty);
            Refresh(lblCmdMode, strCmdMode.None);
            Refresh(lblAuto, Auto(buffer.Auto), Color.Red);
            Refresh(lblReady, Ready.NoReady.ToString());
            Refresh(lblPathNotice, "X");
            Refresh(lblPresence, String.Empty, Color.White);
            Refresh(lblSwitch_Ack, Switch_Ack.NoAck.ToString());
            Refresh(lblInitialNotice, InitialNotice.Initial.ToString());
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

        private void Refresh(Label label, string value, Color color)
        {
            if (InvokeRequired)
            {
                var action = new Action<Label, string, Color>(Refresh);
                Invoke(action, label, value, color);
            }
            else
            {
                label.Text = value;
                label.BackColor = color;
            }
        }

        private void Refresh(Label label, string bufferName, bool presence, bool error)
        {
            if (InvokeRequired)
            {
                var action = new Action<Label, string, bool, bool>(Refresh);
                Invoke(action, label, presence, error);
            }
            else
            {
                label.Text = bufferName;

                if (presence) //荷有橘底黑字
                {
                    label.BackColor = Color.Orange;
                    label.ForeColor = Color.Black;
                }
                else if(error) //紅色異常
                {
                    label.BackColor = Color.Red;
                    label.ForeColor = Color.Black;
                }
                else
                {
                    label.BackColor = Color.ForestGreen;
                }
            }
        }

    }
}
