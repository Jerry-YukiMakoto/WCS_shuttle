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

namespace Mirle.ASRS.AWCS.View
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
        }

        public void Refresh_BQA(Conveyors.Buffer buffer)
        {
            Refresh(lblBufferName, buffer.BufferName, buffer.Auto, buffer.Manual, buffer.Error);
            Refresh(lblCommandId, buffer.CommandId.ToString("D4"));
            Refresh(lblInitialNotice, buffer.InitialNotice.ToString());
            Refresh(lblLoadCategory, buffer.LoadCategory.ToString());
            Refresh(lblPathNotice, buffer.PathNotice.ToString());
            Refresh(lblPickingDirection, buffer.PickingDirection.ToString());
            Refresh(lblPosition, buffer.Position.ToColor());
            Refresh(lblPresence, buffer.Presence.ToColor());
            Refresh(lblReadNotice, buffer.ReadNotice.ToString());
            Refresh(lblReady, buffer.Ready.ToString());
            Refresh(lblTrayType, buffer.TrayType.ToString());
        }
        public void Refresh_MFG(Conveyors.Buffer buffer)
        {
            Refresh(lblBufferName, buffer.BufferName, buffer.Auto, buffer.Manual, buffer.Error);
            Refresh(lblCommandId, $"T{buffer.CommandId:D5}");
            Refresh(lblInitialNotice, buffer.InitialNotice.ToString());
            Refresh(lblLoadCategory, buffer.LoadCategory.ToString());
            Refresh(lblPathNotice, buffer.PathNotice.ToString());
            Refresh(lblPickingDirection, buffer.PickingDirection.ToString());
            Refresh(lblPosition, buffer.Position.ToColor());
            Refresh(lblPresence, buffer.Presence.ToColor());
            Refresh(lblReadNotice, buffer.ReadNotice.ToString());
            Refresh(lblReady, buffer.Ready.ToString());
            Refresh(lblTrayType, buffer.TrayType.ToString());
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
