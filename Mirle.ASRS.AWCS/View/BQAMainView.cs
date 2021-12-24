using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Mirle.ASRS.AWCS.Manager;
using Mirle.ASRS.Conveyors;

namespace Mirle.ASRS.AWCS.View
{
    public partial class BQAMainView : Form
    {
        private readonly Conveyor _conveyor;
        private readonly LoggerManager _loggerManager;
        private DateTime _logTraceFocusedLeaveTime = DateTime.Now;
        private bool _isLogTraceFocusedFlag = false;

        private BQAMainView()
        {
            InitializeComponent();
        }

        public BQAMainView(CVCSHost host) : this()
        {
            _conveyor = host.GetCVControllerr().GetConveryor();
            _loggerManager = host.GetLoggerManager();
        }

        private void Refresh_Tick(object sender, EventArgs e)
        {
            timRefresh.Stop();
            try
            {
                lblDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                if (pnlGeneralEnvironment.Visible)
                {
                    for (int index = 0; index < pnlGeneralEnvironment.Controls.Count; index++)
                    {
                        if (pnlGeneralEnvironment.Controls[index] is BufferView bufferView)
                        {
                            if (_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                            {
                                bufferView.Refresh_BQA(buffer);
                            }
                        }
                    }
                }
                else if (pnl3ALinkLowerFloor.Visible)
                {
                    for (int index = 0; index < pnl3ALinkLowerFloor.Controls.Count; index++)
                    {
                        if (pnl3ALinkLowerFloor.Controls[index] is BufferView bufferView)
                        {
                            if (_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                            {
                                bufferView.Refresh_BQA(buffer);
                            }
                        }
                    }
                }
                else if (pnl3ALinkUpperFloor.Visible)
                {
                    for (int index = 0; index < pnl3ALinkUpperFloor.Controls.Count; index++)
                    {
                        if (pnl3ALinkUpperFloor.Controls[index] is BufferView bufferView)
                        {
                            if (_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                            {
                                bufferView.Refresh_BQA(buffer);
                            }
                        }
                    }
                }

                var log = _loggerManager.GetCurrentTrace();
                if (log.Any())
                {
                    lstLogTrace.Items.AddRange(log.ToArray());

                    if (_isLogTraceFocusedFlag == false && _logTraceFocusedLeaveTime.AddSeconds(30) < DateTime.Now)
                    {
                        lstLogTrace.SelectedIndex = lstLogTrace.Items.Count - 1;
                    }

                    while (lstLogTrace.Items.Count > 300)
                    {
                        lstLogTrace.Items.RemoveAt(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
            finally
            {
                timRefresh.Start();
            }
        }

        private void BQAMainView_Load(object sender, EventArgs e)
        {
            timRefresh.Start();

            pnl3ALinkLowerFloor.Visible = false;
            pnl3ALinkUpperFloor.Visible = false;
            pnlGeneralEnvironment.Visible = true;
        }

        private void GeneralHP_Click(object sender, EventArgs e)
        {
            pnl3ALinkLowerFloor.Visible = false;
            pnl3ALinkUpperFloor.Visible = false;
            pnlGeneralEnvironment.Visible = true;
        }

        private void LowerFloor_Click(object sender, EventArgs e)
        {
            pnlGeneralEnvironment.Visible = false;
            pnl3ALinkUpperFloor.Visible = false;
            pnl3ALinkLowerFloor.Visible = true;
        }

        private void UpperFloor_Click(object sender, EventArgs e)
        {
            pnlGeneralEnvironment.Visible = false;
            pnl3ALinkLowerFloor.Visible = false;
            pnl3ALinkUpperFloor.Visible = true;
        }

        private void LogTrace_Click(object sender, EventArgs e)
        {
            splitContainer3.SplitterDistance = splitContainer3.Width - 300;
            splitContainer3.Panel2Collapsed = !splitContainer3.Panel2Collapsed;
        }

        private void LogTrace_MouseEnter(object sender, EventArgs e)
        {
            _isLogTraceFocusedFlag = true;
        }

        private void LogTrace_MouseLeave(object sender, EventArgs e)
        {
            _isLogTraceFocusedFlag = false;
            _logTraceFocusedLeaveTime = DateTime.Now;
        }
    }
}
