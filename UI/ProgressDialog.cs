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
    public partial class ProgressDialog : Form
    {
        int internalTotal = 0;
        int maxVal = 0;
        public event EventHandler OnDone;
        public event EventHandler OnCancel;
        volatile bool m_Cancelled;
        volatile bool autoClose = true;
        public void SetAutoClose(bool bEnable)
        {
            autoClose = bEnable;
        }
        public ProgressDialog(bool cancellable = true)
        {
            InitializeComponent();
            if (!cancellable)
            {
                buttonCancel.Visible = false;
            }
        }
        public virtual void SubTask(String txt)
        {
            try
            {
                if (!this.Created || this.IsDisposed)
                {
                    return;
                }
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<String>(SubTask), txt);
                    return;
                }
                this.labelSubTask.Text = txt;
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.ToString());
            }
        }
        public virtual void BeginTask(String title, int Total)
        {
            try
            {
                if (!this.Created || this.IsDisposed)
                {
                    return;
                }
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<String, int>(BeginTask), title, Total);
                    return;
                }
                this.labelTask.Text = title;
                if (Total < 0)
                {
                    this.progressBar1.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    this.progressBar1.Style = ProgressBarStyle.Continuous;
                    this.progressBar1.Maximum = Total;
                    this.maxVal = Total;
                }
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.ToString());
            }
        }
        public bool IsCancelled
        {
            get
            {
                return m_Cancelled;
            }
        }
        public virtual void Cancel()
        {
            try
            {
                if (!this.Created || this.IsDisposed)
                {
                    return;
                }
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(Cancel));
                    return;
                }
                m_Cancelled = true;
                if (OnCancel != null)
                {
                    OnCancel(this, EventArgs.Empty);
                }
                if (autoClose)
                {
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.ToString());
            }
        }
        public virtual void Work(int prog)
        {
            try
            {
                if (!this.Created || this.IsDisposed)
                {
                    return;
                }
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<int>(Work), prog);
                    return;
                }
                internalTotal += prog;
                if (internalTotal >= maxVal)
                {
                    this.progressBar1.Value = maxVal;
                    Done(autoClose);
                }
                else
                {
                    this.progressBar1.Value = internalTotal;
                }
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.ToString());
            }
        }
        public virtual void Done()
        {
            Done(true);
        }
        protected virtual void Done(bool triggerClose)
        {
            try
            {
                if (!this.Created || this.IsDisposed)
                {
                    return;
                }
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(Done));
                    return;
                }
                if (OnDone != null)
                {
                    OnDone(this, EventArgs.Empty);
                }
                if (triggerClose)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ee)
            {
                Console.Error.WriteLine(ee.ToString());
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }
    }
}
