namespace LastPassSSOHelper
{
    partial class SingleSignOnView
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.wvContent = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.wvContent)).BeginInit();
            this.SuspendLayout();
            // 
            // wvContent
            // 
            this.wvContent.CreationProperties = null;
            this.wvContent.DefaultBackgroundColor = System.Drawing.Color.White;
            this.wvContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wvContent.Location = new System.Drawing.Point(0, 0);
            this.wvContent.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.wvContent.Name = "wvContent";
            this.wvContent.Size = new System.Drawing.Size(924, 598);
            this.wvContent.TabIndex = 0;
            this.wvContent.ZoomFactor = 1D;
            // 
            // SingleSignOnView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 598);
            this.Controls.Add(this.wvContent);
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "SingleSignOnView";
            this.Text = "SSO Login";
            this.Shown += new System.EventHandler(this.SingleSignOnView_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.wvContent)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 wvContent;
    }
}