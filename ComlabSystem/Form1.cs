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
using System.Configuration;
using System.Data.SqlClient;


namespace ComlabSystem
{
    public partial class Form1 : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
        public Form1()
        {
            InitializeComponent();
            UserPassTextBox.UseSystemPasswordChar = true;
            AdminPassTB.UseSystemPasswordChar = true;
        }

        private void UserShowBtm_Click(object sender, EventArgs e)
        {
            UserFormPNL.BringToFront();
            AdminShowBtm.BringToFront();

        }

        private void AdminShowBtm_Click(object sender, EventArgs e)
        {
            AdminShowForm();
        }

        private void AdminShowForm()
        {
            // Assuming SignOutMSGDialog is already a defined Guna2MessageDialog
            AdminFormDialog.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            AdminFormDialog.Icon = MessageDialogIcon.Warning;     // Question icon
            AdminFormDialog.Caption = "Administrator";
            AdminFormDialog.Text = "Only Administrator is allowed here do you want to procced?";

            AdminFormDialog.Style = MessageDialogStyle.Light;

            // Show the dialog and get the result
            DialogResult result = AdminFormDialog.Show();

            // Check if user clicked Yes
            if (result == DialogResult.Yes)
            {
                AdminFormPNL.BringToFront();
                UserShowBtm.BringToFront();
            }
            // No need to handle No as the MessageDialog will automatically disappear
        }

        private void ShowUserPassBtm_Click(object sender, EventArgs e)
        {
            // Toggle the visibility of the password
            if (UserPassTextBox.UseSystemPasswordChar)
            {
                // Show the password (set to false)
                UserPassTextBox.UseSystemPasswordChar = false;

            }
            else
            {
                // Hide the password (set to true)
                UserPassTextBox.UseSystemPasswordChar = true;
            }
        }

        private void ShowAdminPassBtm_Click(object sender, EventArgs e)
        {
            if (AdminPassTB.UseSystemPasswordChar)
            {
                // Show the password (set to false)
                AdminPassTB.UseSystemPasswordChar = false;

            }
            else
            {
                // Hide the password (set to true)
                AdminPassTB.UseSystemPasswordChar = true;
            }
        }


        private void UserLoginBtm_Click(object sender, EventArgs e)
        {
            OpenAuserUIAtBottomRight();


       }
        private void OpenAuserUIAtBottomRight()
        {
            // Create an instance of AuserUI
            user auserUIForm = new user();

            // Get the screen's working area (excluding taskbars and docked windows)
            var screenWorkingArea = Screen.PrimaryScreen.WorkingArea;

            // Set the form's StartPosition to Manual so we can set its location manually
            auserUIForm.StartPosition = FormStartPosition.Manual;

            // Position the form at the bottom-right corner of the screen
            auserUIForm.Location = new System.Drawing.Point(
                screenWorkingArea.Right - auserUIForm.Width,  // Align with the right edge of the screen
                screenWorkingArea.Bottom - auserUIForm.Height // Align with the bottom edge of the screen
            );

            // Show the form
            auserUIForm.Show();
            this.Hide();
        }

        private void AdminLoginBtm_Click(object sender, EventArgs e)
        {
            string username = AdminNameTB.Text;
            string password = AdminPassTB.Text;

            // Check specific hardcoded users first
            if (username == "alfaizmac" && password == "1834561834561")
            {
                MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Admin dashboard = new Admin();
                dashboard.Show();
                this.Hide();
            }
            else if (username == "NBSPIsuperadmin" && password == "headadmin@128*")
            {
                MessageBox.Show("You successfully logged in as a head admin!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Admin dashboard = new Admin();
                dashboard.Show();
                this.Hide();
            }
            else
            {
                // Validate against the database
                if (IsUsernameValid(username))
                {
                    // Username exists, so check if the password is correct
                    if (ValidateAdminLogin(username, password))
                    {
                        MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Admin dashboard = new Admin();
                        dashboard.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Incorrect password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AdminPassTB.Focus();  // Focus on password textbox
                    }
                }
                else
                {
                    MessageBox.Show("Username does not exist.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AdminNameTB.Focus();  // Focus on username textbox
                }
            }
        }
        private bool IsUsernameValid(string username)
        {
            string query = "SELECT COUNT(*) FROM AdminList WHERE UserName = @username";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;  // Return true if username exists
                }
            }
        }

        // Method to validate the username and password in the database
        private bool ValidateAdminLogin(string username, string password)
        {
            string query = "SELECT COUNT(*) FROM AdminList WHERE UserName = @username AND Password = @password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;  // Return true if credentials are valid
                }
            }
        }

        private void AdminPassTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Check if the Enter key was pressed
            {
                e.SuppressKeyPress = true; // Optional: Prevents the 'ding' sound
                AdminLoginBtm.PerformClick(); // Simulate button click
            }
        }

        private void AdminNameTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Check if the Enter key was pressed
            {
                e.SuppressKeyPress = true; // Optional: Prevents the 'ding' sound
                AdminLoginBtm.PerformClick(); // Simulate button click
            }
        }
    }

  
}
