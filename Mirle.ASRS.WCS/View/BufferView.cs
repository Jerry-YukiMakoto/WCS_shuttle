using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunicationPLC.Siemens;
using Mirle.MPLC.DataType;
using PLCConfigSetting.PLCsetting;

namespace Mirle.ASRS.View
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
            string strBCRReadNotice = "BCR讀取訊號:\nF -> 未讀取\nT -> 開始讀取"; 
            string strAllowWriteCommand = "允許寫入命令:\n輸送機是否允許寫入命令"; 
            string strPresence = "荷有:\n橘色V -> 此位置有物";
            string strWriteCommandComplete = "寫入命令完成:\nF -> 未完成\nT -> 完成"; 
            string strStoreInInfo = "入庫資訊:\nT -> 有入庫資訊";


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
            objToolTip.SetToolTip(BCRReadNotice, strBCRReadNotice);
            objToolTip.SetToolTip(AllowWriteCommand, strAllowWriteCommand);
            objToolTip.SetToolTip(lblPresence, strPresence);
            objToolTip.SetToolTip(WriteCommandComplete, strWriteCommandComplete);
            objToolTip.SetToolTip(StoreInInfo, strStoreInInfo);
           
        }

        public void Refresh_Buffer(clsBufferData Plc1,int index)
        {
            Refresh(lblBufferName, "A"+index , Plc1.oPLC.PLC[index].CV.Presence, Plc1.oPLC.PLC[index].CV.Down);
            Refresh(lblCommandId, Plc1.oPLC.PLC[index].CV.Sno);
            Refresh(lblCmdMode, Plc1.oPLC.PLC[index].CV.Mode.ToString());
            Refresh(lblAuto, Auto(Plc1.oPLC.PLC[index].CV.AutoManual), Plc1.oPLC.PLC[index].CV.AutoManual.ToColor(Color.ForestGreen, Color.Yellow));
            Refresh(BCRReadNotice, Plc1.oPLC.PLC[index].CV.ReadBCR.ToString());
            Refresh(AllowWriteCommand, Plc1.oPLC.PLC[index].CV.AllowWriteCommand.ToString());
            Refresh(lblPresence, Presence(Plc1.oPLC.PLC[index].CV.Presence), Plc1.oPLC.PLC[index].CV.Presence.ToColor(Color.Orange, Color.White));
            Refresh(WriteCommandComplete, Plc1.oPLC.PLC[index].CV.WriteCommandComplete.ToString());
            Refresh(StoreInInfo, Plc1.oPLC.PLC[index].CV.StoreInInfo.ToString());
        }

        public void Refresh_Buffer_PC(clsBufferData Plc1, int index)
        {
            lblAuto.Text = "";
            AllowWriteCommand.Text = "";
            lblPresence.Text = "";
            StoreInInfo.Text = "";
            Refresh(lblBufferName, "A" + index);
            Refresh(lblCommandId, Plc1.oPLC.PC[index].CV.Sno);
            Refresh(lblCmdMode, Plc1.oPLC.PC[index].CV.Mode.ToString());
            Refresh(BCRReadNotice, Plc1.oPLC.PC[index].CV.ReadComplete.ToString());
            Refresh(WriteCommandComplete, Plc1.oPLC.PC[index].CV.WriteCommandComplete.ToString());
            //Refresh(StoreInInfo, Plc1.oPLC.PC[index].CV.NoCommand.ToString());
        }

        public void Refresh_BufferPLCError(clsBufferData Plc1)
        {
            Refresh(lblCommandId, String.Empty);
            Refresh(lblCmdMode, strCmdMode.None);
            Refresh(lblAuto, "A", Color.Red);
            Refresh(BCRReadNotice, Ready.NoReady.ToString());
            Refresh(AllowWriteCommand, "X");
            Refresh(lblPresence, String.Empty, Color.White);
            Refresh(WriteCommandComplete, Switch_Ack.NoAck.ToString());
            Refresh(StoreInInfo, InitialNotice.Initial.ToString());
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

                if (error) //紅色異常
                {
                    label.BackColor = Color.Red;
                    label.ForeColor = Color.Black;
                }
                else if (presence) //荷有橘底黑字
                {
                    label.BackColor = Color.Orange;
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
