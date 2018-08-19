namespace Utilities.UI
{
    partial class PairColorPicker
    {
        /// <summary> 
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 元件設計工具產生的程式碼

        /// <summary> 
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panelBGClr = new System.Windows.Forms.Panel();
            this.panelFrClr = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panelBGClr
            // 
            this.panelBGClr.BackColor = System.Drawing.Color.White;
            this.panelBGClr.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelBGClr.Location = new System.Drawing.Point(30, 31);
            this.panelBGClr.Name = "panelBGClr";
            this.panelBGClr.Size = new System.Drawing.Size(55, 52);
            this.panelBGClr.TabIndex = 0;
            this.panelBGClr.Click += new System.EventHandler(this.panelBGClr_Click);
            // 
            // panelFrClr
            // 
            this.panelFrClr.BackColor = System.Drawing.Color.Black;
            this.panelFrClr.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panelFrClr.Location = new System.Drawing.Point(3, 3);
            this.panelFrClr.Name = "panelFrClr";
            this.panelFrClr.Size = new System.Drawing.Size(65, 60);
            this.panelFrClr.TabIndex = 1;
            this.panelFrClr.Click += new System.EventHandler(this.panelFrClr_Click);
            // 
            // PairColorPicker
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.panelFrClr);
            this.Controls.Add(this.panelBGClr);
            this.Name = "PairColorPicker";
            this.Size = new System.Drawing.Size(88, 83);
            this.SizeChanged += new System.EventHandler(this.PairColorPicker_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBGClr;
        private System.Windows.Forms.Panel panelFrClr;
    }
}
