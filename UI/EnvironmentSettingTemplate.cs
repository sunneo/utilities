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
    public partial class EnvironmentSettingTemplate : Form
    {
        public event EventHandler OKClicked;
        public event EventHandler CancelClicked;
        public event EventHandler<TreeNode> NodeSelected;
        public TreeView Tree
        {
            get
            {
                return tree;
            }
        }
        public EnvironmentSettingTemplate()
        {
            InitializeComponent();
        }
        public void SetBody(UserControl ctrl)
        {
            body.Controls.Clear();
            ctrl.Dock = DockStyle.Fill;
            body.Controls.Add(ctrl);
        }
        public void SelectNode(TreeNode e)
        {
            tree.SelectedNode = e;
        }
        public int AddNode(TreeNode node)
        {
            return this.tree.Nodes.Add(node);
        }
        public int AddNode(TreeNode parent, TreeNode node)
        {
            return parent.Nodes.Add(node);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (OKClicked != null)
            {
                OKClicked(this, e);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (CancelClicked != null)
            {
                CancelClicked(this, e);
            }
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (NodeSelected != null)
            {
                NodeSelected(this, e.Node);
            }
        }
    }
}
