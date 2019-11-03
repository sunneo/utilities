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
    public partial class SaveConfigurationTemplateForm : Form
    {
        public event EventHandler OKClicked;

        public Panel MainPanel
        {
            get
            {
                return panel2;
            }
        }
        public bool TipTextVisible
        {
            get
            {
                return panel3.Visible;
            }
            set
            {
                panel3.Visible = value;
            }
        }
        public String TipText
        {
            get
            {
                return label1.Text;
            }
            set
            {
                label1.Text = value;
            }
        }
        public SaveConfigurationTemplateForm()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (OKClicked != null)
            {
                OKClicked(this, e);
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
