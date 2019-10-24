using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class DoubleBufferedTabPage:TabPage
    {
        public DoubleBufferedTabPage()
        {
            DoubleBuffered = true;
        }
    }
    public class DoubleBufferedTabControl : TabControl
    {
        public DoubleBufferedTabControl()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}
