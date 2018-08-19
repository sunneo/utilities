using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities.UI
{
    public partial class InputBox : Form
    {
        public String MessageTitle
        {
            get
            {
                return labelEx1.Text;
            }
            set
            {
                labelEx1.Text = value;
            }
        }
        public String TextContent;
        internal InputBox()
        {
            InitializeComponent();
        }

        public static String Show(String title="",String content="")
        {
            InputBox inputBox = new InputBox();
            if (String.IsNullOrEmpty(title))
            {
                inputBox.MessageTitle = "輸入文字";
            }
            else
            {
                inputBox.MessageTitle = title;
            }
            if (!String.IsNullOrEmpty(content))
            {
                inputBox.TextContent = content;
                inputBox.textBoxEx1.Text = content;
            }
            inputBox.StartPosition = FormStartPosition.CenterScreen;
            if (inputBox.ShowDialog() == DialogResult.OK)
            {
                return inputBox.TextContent;
            }
            return "";
        }

        private void panel1_ClientSizeChanged(object sender, EventArgs e)
        {
            labelEx1.MaximumSize = new Size((sender as Control).ClientSize.Width - labelEx1.Left, 10000);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.TextContent = textBoxEx1.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
