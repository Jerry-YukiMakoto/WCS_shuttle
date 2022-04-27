using Mirle.ASRS.Conveyors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WCS
{
    public partial class frmBuffer_Initial : Form
    {

        private Timer timRefresh = new Timer();
        private readonly Conveyor _conveyor;

        public int StnIdx { get; private set; }

        public frmBuffer_Initial(Conveyor conveyor)
        {
            InitializeComponent();
            _conveyor = conveyor;
            timRefresh.Stop();
            timRefresh.Tick += new EventHandler(timRefresh_Tick);
            timRefresh.Interval = 100;
            timRefresh.Start();
        }

        private void timRefresh_Tick(object sender, EventArgs e)
        {
            StnIdx = 0;

            if (!string.IsNullOrWhiteSpace(cboBuffer_Name.Text.Trim()))
            {
                StnIdx = cboBuffer_Name.SelectedIndex+1;

                txtCmdSno_Plc.Text=_conveyor.GetBuffer(StnIdx).CommandId.ToString();
                txtCmdSno_Pc.Text = _conveyor.GetBuffer(StnIdx).PC_CommandId.ToString();
                txtCmdMode_Plc.Text = _conveyor.GetBuffer(StnIdx).CmdMode.ToString();
                txtCmdMode_Pc.Text = _conveyor.GetBuffer(StnIdx).PC_CmdMode.ToString();
                txtSwitchMode_PLC.Text = _conveyor.GetBuffer(StnIdx).Switch_Ack.ToString();
                txtSwitchMode_PC.Text = _conveyor.GetBuffer(StnIdx).PC_Switch_Ack.ToString();
                txtInitial_Plc.Text = _conveyor.GetBuffer(StnIdx).InitialNotice.ToString();
                txtInitial_Pc.Text = _conveyor.GetBuffer(StnIdx).PC_InitialNotice.ToString();

            }
            else
            {
                txtCmdSno_Plc.Text = "";
                txtCmdSno_Pc.Text = "";
                txtCmdMode_Plc.Text = "0";
                txtCmdMode_Pc.Text = "0";
                txtInitial_Pc.Text = "0";
                txtInitial_Plc.Text = "0";
                txtPath_Pc.Text = "0";
                txtPath_Plc.Text = "0";
                txtRead_Ack.Text = "0";
                txtReceive.Text = "0";
                txtSwitchMode_PLC.Text = "0";
                txtSwitchMode_PC.Text = "0";
            }



        }

        private void frmBuffer_Initial_Load(object sender, EventArgs e)
        {
                    cboBuffer_Name.Items.Clear();
                    cboBuffer_Name.Items.Add("A1");
                    cboBuffer_Name.Items.Add("A2");
                    cboBuffer_Name.Items.Add("A3");
                    cboBuffer_Name.Items.Add("A4");
                    cboBuffer_Name.Items.Add("A5");
                    cboBuffer_Name.Items.Add("A6");
                    cboBuffer_Name.Items.Add("A7");
                    cboBuffer_Name.Items.Add("A8");
                    cboBuffer_Name.Items.Add("A9");
                    cboBuffer_Name.Items.Add("A10");
            
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnInitial_Pc_Click(object sender, EventArgs e)
        {
           //已做好自己清值功能，暫不做清PC初始
        }

        private void btnInitial_Plc_Click(object sender, EventArgs e)
        {
            _conveyor.GetBuffer(StnIdx).InitialNoticeTrigger();
        }
    }
}
