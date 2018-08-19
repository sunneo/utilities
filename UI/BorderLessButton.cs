using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class BorderLessButton:Button
    {
        Color OrigBackColor;
        public BorderLessButton()
        {

        }
        protected override void OnLostFocus(EventArgs e)
        {
            this.BackColor = OrigBackColor;
            base.OnLostFocus(e);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            OrigBackColor = this.BackColor;
            this.BackColor = Color.FromArgb(0x40, FlatAppearance.MouseOverBackColor.R | 0x30, FlatAppearance.MouseOverBackColor.G | 0x30, FlatAppearance.MouseOverBackColor.B | 0x30);
            base.OnGotFocus(e);
        }
        protected override bool ShowFocusCues
        {
            get
            {
                return false;
            }
        }
        public override void NotifyDefault(bool value)
        {
            if (this.FlatStyle == System.Windows.Forms.FlatStyle.Flat &&
                this.FlatAppearance.BorderSize == 0)
            {
                base.NotifyDefault(false);
            }
            else
            {
                base.NotifyDefault(value);
            }
        }
    }
}
