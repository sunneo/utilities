using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class LabelEx:Label
    {
        public LabelEx()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer,true);
        }
    }
}
