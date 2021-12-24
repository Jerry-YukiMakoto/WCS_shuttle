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
    public partial class MFGMainView : Form
    {
        private readonly Conveyor _conveyor;
        private readonly LoggerManager _loggerManager;
        private DateTime _logTraceFocusedLeaveTime = DateTime.Now;
        private bool _isLogTraceFocusedFlag = false;

        private MFGMainView()
        {
            InitializeComponent();
        }

        public MFGMainView(CVCSHost host) : this()
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
                if (pnlGeneralHP.Visible)
                {
                    for (int index = 0; index < pnlGeneralHP.Controls.Count; index++)
                    {
                        if (pnlGeneralHP.Controls[index] is BufferView bufferView)
                        {
                            if (_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                            {
                                bufferView.Refresh_MFG(buffer);
                            }
                        }
                    }
                }
                else if (pnlGeneralOP.Visible)
                {
                    for (int index = 0; index < pnlGeneralOP.Controls.Count; index++)
                    {
                        if (pnlGeneralOP.Controls[index] is BufferView bufferView)
                        {
                            if (_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                            {
                                bufferView.Refresh_MFG(buffer);
                            }
                        }
                    }
                }
                else if (pnl3ALink.Visible)
                {
                    for (int index = 0; index < pnl3ALink.Controls.Count; index++)
                    {
                        if (pnl3ALink.Controls[index] is BufferView bufferView)
                        {
                            if (_conveyor.TryGetBuffer(bufferView.BufferIndex, out var buffer))
                            {
                                bufferView.Refresh_MFG(buffer);
                            }
                        }
                    }
                }

                var log = _loggerManager.GetCurrentTrace();
                if (log.Any())
                {
                    lstLogTrace.Items.AddRange(log.ToArray());

                    while (lstLogTrace.Items.Count > 300)
                    {
                        lstLogTrace.Items.RemoveAt(0);
                    }

                    if (_isLogTraceFocusedFlag == false && _logTraceFocusedLeaveTime.AddSeconds(30) < DateTime.Now)
                    {
                        lstLogTrace.SelectedIndex = lstLogTrace.Items.Count - 1;
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

        private void MFGMainView_Load(object sender, EventArgs e)
        {
            timRefresh.Start();

            pnl3ALink.Visible = false;
            pnlGeneralOP.Visible = false;
            pnlGeneralHP.Visible = true;
        }

        private void LogTrace_Click(object sender, EventArgs e)
        {
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

        private void GeneralHP_Click(object sender, EventArgs e)
        {
            pnl3ALink.Visible = false;
            pnlGeneralOP.Visible = false;
            pnlGeneralHP.Visible = true;
        }

        private void GeneralOP_Click(object sender, EventArgs e)
        {
            pnl3ALink.Visible = false;
            pnlGeneralHP.Visible = false;
            pnlGeneralOP.Visible = true;
        }

        private void Link_Click(object sender, EventArgs e)
        {
            pnlGeneralOP.Visible = false;
            pnlGeneralHP.Visible = false;
            pnl3ALink.Visible = true;
        }
    }
}
