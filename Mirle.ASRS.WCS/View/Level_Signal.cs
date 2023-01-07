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
    public partial class Level_Signal : UserControl
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

        public Level_Signal()
        {
            InitializeComponent();

            funInitToolTip();

        }

        /// <summary>
        /// 游標提示
        /// </summary>
        private void funInitToolTip()
        {
            string strLevel = "level樓層";
            string strMoveFloor = "移動至第幾層/PC訊號:callToLevel";
            string strSafetyCheck = "樓層Safety Check/PC訊號:carmoveComplete";
            string LevelSensor = "樓層位置確認";


            ToolTip objToolTip = new ToolTip();
            objToolTip.AutoPopDelay = 5000;
            objToolTip.InitialDelay = 100;
            objToolTip.ReshowDelay = 100;
            objToolTip.UseAnimation = false;
            objToolTip.UseFading = false;
            objToolTip.ShowAlways = true;

            objToolTip.SetToolTip(lbLevel, strLevel);
            objToolTip.SetToolTip(lblMovetoLevel, strMoveFloor);
            objToolTip.SetToolTip(lbLevelSafetyCheck, strSafetyCheck);
            objToolTip.SetToolTip(lbLevelSenSor, LevelSensor);

        }

        public void RefreshLevel_Lifter(clsBufferData Plc1,int index)
        {
            string safetycheck="";
            string levelsensor="";
            string Movetolevel="";


            if(index==1)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor1_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor1LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor1.ToString();
            }
            else if (index == 2)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor2_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor2LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor2.ToString();

            }
            else if(index == 3)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor3_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor3LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor3.ToString();
            }
            else if(index == 4)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor4_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor4LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor4.ToString();

            }
            else if(index==5)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor5_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor5LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor5.ToString();
            }
            else if (index == 6)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor6_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor6LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor6.ToString();
            }
            else if (index == 7)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor7_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor7LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor7.ToString();
            }
            else if (index == 8)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor8_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor8LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor8.ToString();
            }
            else if (index == 9)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor9_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor9LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor9.ToString();
            }
            else if (index == 10)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor10_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.Floor10LocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor10.ToString();
            }
            else if (index == 11)
            {
                safetycheck = Plc1.oPLC.PLC[0].Lifter.Floor11_SafetyCheck.ToString();
                levelsensor = Plc1.oPLC.PLC[0].Lifter.UnloadingLocationCheck.ToString();
                Movetolevel = Plc1.oPLC.PLC[0].Lifter.MoveToFloor11.ToString();
            }

            Refresh(lbLevel, "Level"+index);
            Refresh(lbLevelSafetyCheck, safetycheck,Convert.ToBoolean(safetycheck).ToColor(Color.Red,Color.White));
            Refresh(lbLevelSenSor, levelsensor, Convert.ToBoolean(levelsensor).ToColor(Color.Red, Color.White));
            Refresh(lblMovetoLevel, Movetolevel, Convert.ToBoolean(Movetolevel).ToColor(Color.Red, Color.White));
        }

        public void RefreshLevel_Lifter_PC(clsBufferData Plc1, int index)
        {
            string carmovecomplete = "";
            string levelsensor = "";
            string Movetolevel = "";


            if (index == 1)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor1_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor1.ToString();
            }
            else if (index == 2)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor2_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor2.ToString();

            }
            else if (index == 3)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor3_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor3.ToString();
            }
            else if (index == 4)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor4_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor4.ToString();
            }
            else if (index == 5)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor5_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor5.ToString();
            }
            else if (index == 6)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor6_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor6.ToString();
            }
            else if (index == 7)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor7_CarMoveComplete.ToString();;
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor7.ToString();
            }
            else if (index == 8)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor8_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor8.ToString();
            }
            else if (index == 9)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor9_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor9.ToString();
            }
            else if (index == 10)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor9_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor9.ToString();
            }
            else if (index == 11)
            {
                carmovecomplete = Plc1.oPLC.PC[0].Lifter.Floor11_CarMoveComplete.ToString();
                Movetolevel = Plc1.oPLC.PC[0].Lifter.CallToFloor11.ToString();
            }

            lbLevelSenSor.Text = "";
            Refresh(lbLevel, "Level" + index);
            Refresh(lbLevelSafetyCheck, carmovecomplete, Convert.ToBoolean(carmovecomplete).ToColor(Color.Red, Color.White));
            Refresh(lblMovetoLevel, Movetolevel, Convert.ToBoolean(Movetolevel).ToColor(Color.Red, Color.White));
        }


        public void Refresh_LifterlevelViewPLCError(clsBufferData Plc1,int index)
        {
            Refresh(lbLevel, "level"+index);
            Refresh(lbLevelSafetyCheck, "X");
            Refresh(lbLevelSenSor, "X");
            Refresh(lblMovetoLevel, "X");
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
