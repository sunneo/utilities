using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class TextBoxEx:TextBox
    {
        Locked<Color> mMouseEnterBackColor = Color.FromArgb(255, 255, 255, 128);
        Locked<Color> mNormalBackColor = Color.FromArgb(255, 255, 255, 255);
        Locked<Color> mFocusBackColor = Color.FromArgb(255, 255, 255, 128);
        volatile bool Focused = false;
        [Browsable(true)]
        public Color MouseEnterBackColor
        {
            get
            {
                return mMouseEnterBackColor;
            }
            set
            {
                mMouseEnterBackColor = value;
            }
        }
        
        [Browsable(true)]
        public Color NormalColor
        {
            get
            {
                return mNormalBackColor;
            }
            set
            {
                mNormalBackColor = value;
            }
        }

        [Browsable(true)]
        public Color FocusBackColor
        {
            get
            {
                return mFocusBackColor;
            }
            set
            {
                mFocusBackColor = value;
            }
        }
        public TextBoxEx()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            Focused = true;
            this.BackColor = FocusBackColor;
            base.OnGotFocus(e);
        }
        protected override void OnLostFocus(EventArgs e)
        {
            Focused = false;
            this.BackColor = NormalColor;
            base.OnLostFocus(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            if (!Focused)
            {
                this.BackColor = NormalColor;
            }
            else
            {
                this.BackColor = FocusBackColor;
            }
            base.OnMouseLeave(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            if (!Focused)
            {
                this.BackColor = MouseEnterBackColor;
            }
            base.OnMouseEnter(e);
        }
    
    }
}
