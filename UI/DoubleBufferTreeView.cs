using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CommonTool
{
    public class DoubleBufferTreeView : System.Windows.Forms.TreeView
    {
        private const int WM_LBUTTONDBLCLK = 0x0203;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                var info = this.HitTest(PointToClient(System.Windows.Forms.Cursor.Position));
                if (info.Location == System.Windows.Forms.TreeViewHitTestLocations.StateImage)
                {
                    m.Result = IntPtr.Zero;
                    return;
                }
            }
            base.WndProc(ref m);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }

        // Pinvoke:
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }
}
