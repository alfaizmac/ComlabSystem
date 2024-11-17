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
    public partial class Admin : Form
    {

        public string AdminName
        {
            set { AdminUserName.Text = value; }

        }
        public Admin()
        {
            InitializeComponent();

        }



        private void DashBoardBtm_Click(object sender, EventArgs e)
        {

        }

        private void UserBtm_Click(object sender, EventArgs e)
        {
            string AdminName = AdminUserName.Text;
            // Create an instance of your UserControl
            UserUI myControl = new UserUI { AdminName = AdminName };

            // Clear any existing controls in the panel (optional, if you want to replace the contents)
            MainPNL.Controls.Clear();

            // Set the Dock style of the UserControl to Fill, making it expand to fit the panel
            myControl.Dock = DockStyle.Fill;

            // Add the UserControl to the panel
            MainPNL.Controls.Add(myControl);
        }

        private void ComBtm_Click(object sender, EventArgs e)
        {
            string AdminName = AdminUserName.Text;
            ZUnitListUI myControl = new ZUnitListUI { AdminName = AdminName };

            // Clear any existing controls in the panel (optional, if you want to replace the contents)
            MainPNL.Controls.Clear();

            // Set the Dock style of the UserControl to Fill, making it expand to fit the panel
            myControl.Dock = DockStyle.Fill;

            // Add the UserControl to the panel
            MainPNL.Controls.Add(myControl);
        }





        private void SignOutBtm_Click(object sender, EventArgs e)
        {
            Form1 countdownForm = new Form1();
            countdownForm.Show();
            this.Hide();
        }

        private void LogBtm_Click(object sender, EventArgs e)
        {
            ZLogsReport myControl = new ZLogsReport();

            // Clear any existing controls in the panel (optional, if you want to replace the contents)
            MainPNL.Controls.Clear();

            // Set the Dock style of the UserControl to Fill, making it expand to fit the panel
            myControl.Dock = DockStyle.Fill;

            // Add the UserControl to the panel
            MainPNL.Controls.Add(myControl);
        }

    }
}
