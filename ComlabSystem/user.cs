using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComlabSystem
{
    public partial class user : Form
    {

        bool SideBarExpand;
        public user()
        {
            InitializeComponent();

            HideOtherPanel();
        }

        private void HideOtherPanel()
        {
            ChangePasswordPNL.Visible = false;
            ReportPnl.Visible = false;
        }

        private void SideBarTimer_Tick(object sender, EventArgs e)
        {
            if (SideBarExpand)
            {
                SideBar.Width -= 10;
                if (SideBar.Width == SideBar.MinimumSize.Width)
                {

                    SideBarExpand = false;
                    SideBarTimer.Stop();
                }
            }
            else
            {
                SideBar.Width += 10;
                if (SideBar.Width == SideBar.MaximumSize.Width)
                {
                    SideBarExpand = true;
                    SideBarTimer.Stop();
                }
            }
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            SideBarTimer.Start();
        }

        private void ReportButton_Click(object sender, EventArgs e)
        {
            ReportPnl.BringToFront();
            ReportPnl.Visible = true;
            ChangePasswordPNL.Visible = false;
        }

        private void ChangePasswordButton_Click(object sender, EventArgs e)
        {
            ChangePasswordPNL.BringToFront();
            ChangePasswordPNL.Visible = true;
            ReportPnl.Visible = false;


        }

        private void ReportCloseBtm_Click(object sender, EventArgs e)
        {
            ReportPnl.Visible = false;
        }

        private void ChangePassCloseBtm_Click(object sender, EventArgs e)
        {
            ChangePasswordPNL.Visible = false;
        }

        private void SignOutButtom_Click(object sender, EventArgs e)
        {
            ShowSignOutDialog();
        }

        private void ShowSignOutDialog()
        {
            // Assuming SignOutMSGDialog is already a defined Guna2MessageDialog
            SignOutMSGDialog.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            SignOutMSGDialog.Icon = MessageDialogIcon.Question;     // Question icon
            SignOutMSGDialog.Caption = "Sign Out";
            SignOutMSGDialog.Text = "Are you sure you want to sign out?";

            SignOutMSGDialog.Style = MessageDialogStyle.Light;

            // Show the dialog and get the result
            DialogResult result = SignOutMSGDialog.Show();

            // Check if user clicked Yes
            if (result == DialogResult.Yes)
            {
                // Code to sign out and show Form1
                Form1 form1 = new Form1();
                form1.Show();
                this.Hide();
            }
            // No need to handle No as the MessageDialog will automatically disappear
        }

        private void guna2TextBox9_TextChanged(object sender, EventArgs e)
        {

        }

        
    }
}
