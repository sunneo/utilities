using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTool
{
    public class DoubleBufferDataGridView : System.Windows.Forms.DataGridView
    {
        public DoubleBufferDataGridView()
        {
            SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer | System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }
    }
}
