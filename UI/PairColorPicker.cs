using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities.UI
{
    public partial class PairColorPicker : UserControl
    {
        Color mFrColor = Color.Black;
        Color mBgColor = Color.White;
        [Browsable(true)]
        public event EventHandler<Color> FRColorChanged;

        [Browsable(true)]
        public event EventHandler<Color> BGColorChanged;
        /// <summary>
        /// 前景色
        /// </summary>
        public Color FrColor
        {
            get
            {
                return mFrColor;
            }
            set
            {
                Color orig = mFrColor;
                mFrColor = value;
                panelFrClr.BackColor = value;
                if (orig != value)
                {
                    if (FRColorChanged != null)
                    {
                        FRColorChanged(this, value);
                    }
                }
            }
        }
        /// <summary>
        /// 背景色
        /// </summary>
        public Color BgColor
        {
            get
            {
                return mBgColor;
            }
            set
            {
                Color orig = mBgColor;
                mBgColor = value;
                panelBGClr.BackColor = value;
                if (orig != value)
                {
                    if (BGColorChanged != null)
                    {
                        BGColorChanged(this, value);
                    }
                }
            }
        }
        public PairColorPicker()
        {
            InitializeComponent();
        }

        private void panelBGClr_Click(object sender, EventArgs e)
        {
            ColorDialog pick = new ColorDialog();
            pick.Color = BgColor;
            if (pick.ShowDialog() == DialogResult.OK)
            {
                BgColor = pick.Color;
            }
        }

        private void panelFrClr_Click(object sender, EventArgs e)
        {
            ColorDialog pick = new ColorDialog();
            pick.Color = FrColor;
            if (pick.ShowDialog() == DialogResult.OK)
            {
                FrColor = pick.Color;
            }
        }

        private void PairColorPicker_SizeChanged(object sender, EventArgs e)
        {
            panelFrClr.Size = new Size(this.Width * 3 / 4, this.Height * 3 / 4);
            panelBGClr.Size = new Size(this.Width * 3 / 4, this.Height * 3 / 4);
            panelBGClr.Location = new Point(this.Width / 4, this.Height / 4);
        }
    }
}
