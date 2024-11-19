namespace ComlabSystem
{
    partial class ZDashboard
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AdminNameLabel = new System.Windows.Forms.Label();
            this.MainPNL = new Guna.UI2.WinForms.Guna2Panel();
            this.PasswordToolTIp = new System.Windows.Forms.ToolTip(this.components);
            this.EditStudentIDTBTT = new System.Windows.Forms.ToolTip(this.components);
            this.SortToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.UnitMessageDialog = new Guna.UI2.WinForms.Guna2MessageDialog();
            this.MainPNL.SuspendLayout();
            this.SuspendLayout();
            // 
            // AdminNameLabel
            // 
            this.AdminNameLabel.AutoSize = true;
            this.AdminNameLabel.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AdminNameLabel.Location = new System.Drawing.Point(245, 149);
            this.AdminNameLabel.Name = "AdminNameLabel";
            this.AdminNameLabel.Size = new System.Drawing.Size(203, 30);
            this.AdminNameLabel.TabIndex = 20;
            this.AdminNameLabel.Text = "HELLLLOOOOW PO";
            // 
            // MainPNL
            // 
            this.MainPNL.BackColor = System.Drawing.Color.White;
            this.MainPNL.Controls.Add(this.AdminNameLabel);
            this.MainPNL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPNL.Location = new System.Drawing.Point(0, 0);
            this.MainPNL.Name = "MainPNL";
            this.MainPNL.Size = new System.Drawing.Size(1679, 1044);
            this.MainPNL.TabIndex = 14;
            // 
            // SortToolTip
            // 
            this.SortToolTip.AutomaticDelay = 200;
            this.SortToolTip.AutoPopDelay = 5000;
            this.SortToolTip.InitialDelay = 200;
            this.SortToolTip.ReshowDelay = 40;
            this.SortToolTip.ShowAlways = true;
            this.SortToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.SortToolTip.ToolTipTitle = "Sort Column";
            // 
            // UnitMessageDialog
            // 
            this.UnitMessageDialog.Buttons = Guna.UI2.WinForms.MessageDialogButtons.OK;
            this.UnitMessageDialog.Caption = null;
            this.UnitMessageDialog.Icon = Guna.UI2.WinForms.MessageDialogIcon.None;
            this.UnitMessageDialog.Parent = null;
            this.UnitMessageDialog.Style = Guna.UI2.WinForms.MessageDialogStyle.Default;
            this.UnitMessageDialog.Text = null;
            // 
            // ZDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.Controls.Add(this.MainPNL);
            this.MaximumSize = new System.Drawing.Size(1679, 1044);
            this.MinimumSize = new System.Drawing.Size(1359, 864);
            this.Name = "ZDashboard";
            this.Size = new System.Drawing.Size(1679, 1044);
            this.Load += new System.EventHandler(this.UserUI_Load);
            this.MainPNL.ResumeLayout(false);
            this.MainPNL.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Guna.UI2.WinForms.Guna2Panel MainPNL;
        private System.Windows.Forms.ToolTip PasswordToolTIp;
        private System.Windows.Forms.ToolTip EditStudentIDTBTT;
        private System.Windows.Forms.ToolTip SortToolTip;
        private Guna.UI2.WinForms.Guna2MessageDialog UnitMessageDialog;
        private System.Windows.Forms.Label AdminNameLabel;
    }
}
