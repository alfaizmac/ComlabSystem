using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComlabSystem
{
    public partial class Admin : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
        public string AdminName
        {
            set { AdminUserName.Text = value; }

        }
        public Admin()
        {
            InitializeComponent();

            UnitName.Text = Environment.MachineName;

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




        private void InsertAdminSignOutLog(string adminUserName, string unitName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Query to retrieve UnitID from UnitList and AdminID from AdminList
                    string query = @"
DECLARE @UnitID INT, @AdminID INT;

-- Retrieve UnitID based on UnitName
SELECT @UnitID = UnitID
FROM UnitList
WHERE ComputerName = @UnitName;

-- Retrieve AdminID based on AdminUserName
SELECT @AdminID = AdminID
FROM AdminList
WHERE UserName = @AdminUserName;

-- Insert the sign-out log
INSERT INTO Logs (Action, Timestamp, UnitID, ActionType, AdminID)
VALUES (@ActionDescription, @Timestamp, @UnitID, @ActionType, @AdminID);";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parameters for UnitName, AdminUserName, and other log details
                        command.Parameters.AddWithValue("@UnitName", unitName);
                        command.Parameters.AddWithValue("@AdminUserName", adminUserName);

                        // Construct a professional description for the "Action" column
                        string actionDescription = $"{adminUserName} signed out on {unitName} at {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
                        command.Parameters.AddWithValue("@ActionDescription", actionDescription);

                        // Add the timestamp and action type
                        command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                        command.Parameters.AddWithValue("@ActionType", "SignOut");

                        connection.Open();
                        command.ExecuteNonQuery(); // Execute the insertion
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error logging admin sign-out: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SignOutBtm_Click(object sender, EventArgs e)
        {
            InsertAdminSignOutLog(AdminUserName.Text, UnitName.Text);

            // Show the countdown form and hide the current form
            Form1 countdownForm = new Form1();
            countdownForm.Show();
            this.Hide();
        }

        private void LogBtm_Click(object sender, EventArgs e)
        {
            string AdminName = AdminUserName.Text;
            ZLogsReport myControl = new ZLogsReport { AdminName = AdminName };

            // Clear any existing controls in the panel (optional, if you want to replace the contents)
            MainPNL.Controls.Clear();

            // Set the Dock style of the UserControl to Fill, making it expand to fit the panel
            myControl.Dock = DockStyle.Fill;

            // Add the UserControl to the panel
            MainPNL.Controls.Add(myControl);
        }

    }
}
