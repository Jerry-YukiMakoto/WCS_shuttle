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

namespace Mirle.ASRS.View
{
    public partial class LifterView : UserControl
    {

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

        public LifterView()
        {
            InitializeComponent();

            funInitToolTip();

        }

        /// <summary>
        /// 游標提示
        /// </summary>
        private void funInitToolTip()
        {
            string strVehicleID = "車子編號";
            string strCommandId = "序號\n未顯示表連線失敗";
            string strTaskNo = "任務序號\n未顯示表連線失敗";
            string strAuto = "自動模式:\n綠燈A -> 自動ON\n黃燈M -> 手動\n紅燈M -> 連線失敗";
            string strAllowWriteCommand = "允許寫入命令:\n輸送機是否允許寫入命令"; 
            string strPresence = "荷有:\n橘色V -> 荷有 (梭車)";
            string strWriteCommandComplete = "寫入命令完成:\n0 -> 未完成\n1 -> 完成";
            string strLifterDown = "Lifter_Down:\n1 -> Down";
            string strLifterRun = "Lifter_Run:\n1 -> Run";
            string strLifterIdle = "Lifter_Idle:\n1 -> Idle";
            string strLiftPosition = "電梯位置";


            ToolTip objToolTip = new ToolTip();
            objToolTip.AutoPopDelay = 5000;
            objToolTip.InitialDelay = 100;
            objToolTip.ReshowDelay = 100;
            objToolTip.UseAnimation = false;
            objToolTip.UseFading = false;
            objToolTip.ShowAlways = true;

            objToolTip.SetToolTip(lbVehicleID, strVehicleID);
            objToolTip.SetToolTip(lblCommandId, strCommandId);
            objToolTip.SetToolTip(lblTaskNo, strTaskNo);
            objToolTip.SetToolTip(Liftermode, strAuto);
            objToolTip.SetToolTip(WriteCommanComplete, strWriteCommandComplete);
            objToolTip.SetToolTip(lbAllowwritecommand, strAllowWriteCommand);
            objToolTip.SetToolTip(lbLiftPresence, strPresence);
            objToolTip.SetToolTip(LiftPosition, strLiftPosition);
            objToolTip.SetToolTip(LifterDown, strLifterDown);
            objToolTip.SetToolTip(LifterRun, strLifterRun);
            objToolTip.SetToolTip(LifterIdle, strLifterIdle);
        }

        public void Refresh_Lifter(clsBufferData Plc1)
        {
            Refresh(lbVehicleID, "" , Plc1.oPLC.PLC[0].Lifter.presence_shuttle, Plc1.oPLC.PLC[0].CV.Down);
            Refresh(lblCommandId, Plc1.oPLC.PLC[0].Lifter.CMDno);
            Refresh(lblTaskNo, Plc1.oPLC.PLC[0].Lifter.Taskno.ToString());
            Refresh(Liftermode, Auto(Plc1.oPLC.PLC[0].Lifter.LiftMode), Plc1.oPLC.PLC[0].Lifter.LiftMode.ToColor(Color.ForestGreen, Color.Yellow));
            Refresh(WriteCommanComplete, Plc1.oPLC.PLC[0].Lifter.WriteCommandComplete.ToString());
            Refresh(lbAllowwritecommand, Plc1.oPLC.PLC[0].Lifter.AllowWriteCommand.ToString());
            Refresh(lbLiftPresence, Presence(Plc1.oPLC.PLC[0].Lifter.presence_shuttle), Plc1.oPLC.PLC[0].Lifter.presence_shuttle.ToColor(Color.Orange, Color.White));
            Refresh(LiftPosition, Plc1.oPLC.PLC[0].Lifter.LiftPosition.ToString());
            Refresh(LifterDown, Plc1.oPLC.PLC[0].Lifter.LiftDown.ToString());
            Refresh(LifterRun, Plc1.oPLC.PLC[0].Lifter.LiftRun.ToString());
            Refresh(LifterIdle, Plc1.oPLC.PLC[0].Lifter.LiftIdle.ToString());
        }

        public void Refresh_LifterViewPLCError(clsBufferData Plc1)
        {
            Refresh(lblCommandId, String.Empty);
            Refresh(lblTaskNo, strCmdMode.None);
            Refresh(Liftermode, "A", Color.Red);
            Refresh(WriteCommanComplete, Ready.NoReady.ToString());
            Refresh(lbAllowwritecommand, "X");
            Refresh(lbLiftPresence, String.Empty, Color.White);
            Refresh(LiftPosition, Switch_Ack.NoAck.ToString());
            Refresh(LifterDown, InitialNotice.Initial.ToString());
            Refresh(LifterRun, InitialNotice.Initial.ToString());
            Refresh(LifterIdle, InitialNotice.Initial.ToString());
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
