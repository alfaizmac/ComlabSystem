using Guna.UI2.WinForms;
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
                    // Query to retrieve UnitID from UnitList and AdminID from AdminList, and perform inserts
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

                -- Insert the sign-out log into Logs table
                INSERT INTO Logs (Action, Timestamp, UnitID, ActionType, AdminID, AdminName)
                VALUES (@ActionDescription, @Timestamp, @UnitID, @ActionType, @AdminID, @AdminName);

                -- Insert the notification into Notifications table
                INSERT INTO Notifications (Message, Timestamp, AdminID, NotificationType, NotificationKind, AdminName, UserType)
                VALUES (@NotificationMessage, @Timestamp, @AdminID, @NotificationType, @NotificationKind, @AdminName, @UserType);";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parameters for UnitName and AdminUserName
                        command.Parameters.AddWithValue("@UnitName", unitName);
                        command.Parameters.AddWithValue("@AdminUserName", adminUserName);

                        // Construct professional descriptions
                        string actionDescription = $"{adminUserName} signed out on {unitName} at {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
                        command.Parameters.AddWithValue("@ActionDescription", actionDescription);

                        // Construct the notification message
                        string notificationMessage = $"{adminUserName} sign out on {unitName} at {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
                        command.Parameters.AddWithValue("@NotificationMessage", notificationMessage);

                        // Add timestamp, action type, and other details
                        command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                        command.Parameters.AddWithValue("@ActionType", "SignOut");
                        command.Parameters.AddWithValue("@NotificationType", "Information");
                        command.Parameters.AddWithValue("@NotificationKind", "AdminSignOut");
                        command.Parameters.AddWithValue("@AdminName", adminUserName);
                        command.Parameters.AddWithValue("@UserType", "Admin");

                        // Open the connection and execute the queries
                        connection.Open();
                        command.ExecuteNonQuery();
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

            // Assuming SignOutMSGDialog is already a defined Guna2MessageDialog
            AdminDialog.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            AdminDialog.Icon = MessageDialogIcon.Question;     // Question icon
            AdminDialog.Caption = "Sign Out";
            AdminDialog.Text = "Are you sure you want to sign out?";
            AdminDialog.Style = MessageDialogStyle.Light;

            // Show the dialog and get the result
            DialogResult result = AdminDialog.Show();

            if (result == DialogResult.Yes)
            {
                InsertAdminSignOutLog(AdminUserName.Text, UnitName.Text);
                // Show the countdown form and hide the current form
                Form1 countdownForm = new Form1();
                countdownForm.Show();
                this.Hide();
            }
            else
            {
                return;
            }


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

        private void SettingsBtm_Click(object sender, EventArgs e)
        {

        }
        private void InsertAdminShutdownLog(string adminUserName, string unitName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Query to retrieve UnitID from UnitList and AdminID from AdminList, and perform inserts
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

                -- Insert the sign-out log into Logs table
                INSERT INTO Logs (Action, Timestamp, UnitID, ActionType, AdminID, AdminName)
                VALUES (@ActionDescription, @Timestamp, @UnitID, @ActionType, @AdminID, @AdminName);

                -- Insert the notification into Notifications table
                INSERT INTO Notifications (Message, Timestamp, AdminID, NotificationType, NotificationKind, AdminName, UserType)
                VALUES (@NotificationMessage, @Timestamp, @AdminID, @NotificationType, @NotificationKind, @AdminName, @UserType);";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parameters for UnitName and AdminUserName
                        command.Parameters.AddWithValue("@UnitName", unitName);
                        command.Parameters.AddWithValue("@AdminUserName", adminUserName);

                        // Construct professional descriptions
                        string actionDescription = $"{adminUserName} shutdown on {unitName} at {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
                        command.Parameters.AddWithValue("@ActionDescription", actionDescription);

                        // Construct the notification message
                        string notificationMessage = $"{adminUserName} shutdown on {unitName} at {DateTime.Now:MMMM dd, yyyy hh:mm tt}";
                        command.Parameters.AddWithValue("@NotificationMessage", notificationMessage);

                        // Add timestamp, action type, and other details
                        command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                        command.Parameters.AddWithValue("@ActionType", "SignOut");
                        command.Parameters.AddWithValue("@NotificationType", "Information");
                        command.Parameters.AddWithValue("@NotificationKind", "AdminShutdown");
                        command.Parameters.AddWithValue("@AdminName", adminUserName);
                        command.Parameters.AddWithValue("@UserType", "Admin");

                        // Open the connection and execute the queries
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error logging admin sign-out: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void ShutdownBtm_Click(object sender, EventArgs e)
        {
            // Assuming SignOutMSGDialog is already a defined Guna2MessageDialog
            AdminDialog.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            AdminDialog.Icon = MessageDialogIcon.Question;     // Question icon
            AdminDialog.Caption = "Sign Out";
            AdminDialog.Text = "Are you sure you want to sign out?";
            AdminDialog.Style = MessageDialogStyle.Light;

            // Show the dialog and get the result
            DialogResult result = AdminDialog.Show();

            if (result == DialogResult.Yes)
            {
                InsertAdminShutdownLog(AdminUserName.Text, UnitName.Text);

                // You can use the Process.Start method to run a shutdown command
                System.Diagnostics.Process.Start("shutdown", "/s /f /t 0"); ;
            }
            else
            {
                return;
            }
        }

        private void NotificationsBtm_Click(object sender, EventArgs e)
        {
            ZNotifications myControl = new ZNotifications();

            // Clear any existing controls in the panel (optional, if you want to replace the contents)
            MainPNL.Controls.Clear();

            // Set the Dock style of the UserControl to Fill, making it expand to fit the panel
            myControl.Dock = DockStyle.Fill;

            // Add the UserControl to the panel
            MainPNL.Controls.Add(myControl);
        }
    }
}
