using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using DGVPrinterHelper;
using Guna.UI2.WinForms;

namespace ComlabSystem
{
    public partial class Settingss : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

        public string AdminName
        {
            set { AdminNameLabel.Text = value; }

        }



        public Settingss()
        {
            InitializeComponent();

            LoadAutoShutdown();

            CurrentPassTextBox.UseSystemPasswordChar = true;
            NewPassTextBox.UseSystemPasswordChar = true;
            ConfirmPassTextBox.UseSystemPasswordChar = true;

        }


        private void UserUI_Load(object sender, EventArgs e)
        {



        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Get the inputs from textboxes
            string currentPasswordInput = CurrentPassTextBox.Text.Trim();
            string newPassword = NewPassTextBox.Text.Trim();
            string confirmPassword = ConfirmPassTextBox.Text.Trim();
            string userId = AdminNameLabel.Text.Trim(); // Admin's UserID from the label

            // Validation flags
            bool isCurrentPasswordValid = false;

            // Connection string to the database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Step 1: Check if the current password matches the one in the database
                    string checkPasswordQuery = "SELECT Password FROM AdminList WHERE UserID = @UserID";
                    using (SqlCommand checkPasswordCommand = new SqlCommand(checkPasswordQuery, connection))
                    {
                        checkPasswordCommand.Parameters.AddWithValue("@UserID", userId);

                        string storedPassword = checkPasswordCommand.ExecuteScalar()?.ToString();

                        if (storedPassword != null && storedPassword == currentPasswordInput)
                        {
                            isCurrentPasswordValid = true;
                        }
                    }

                    // Show error if the current password is invalid
                    CurrentPassInvalidL.Visible = !isCurrentPasswordValid;

                    // Step 2: Check if the new password and confirm password match
                    bool doPasswordsMatch = newPassword == confirmPassword;
                    ConfirmPassInvalidL.Visible = !doPasswordsMatch;

                    // Step 3: If all conditions are valid, update the password in the database
                    if (isCurrentPasswordValid && doPasswordsMatch)
                    {
                        string updatePasswordQuery = "UPDATE AdminList SET Password = @NewPassword WHERE UserID = @UserID";
                        using (SqlCommand updatePasswordCommand = new SqlCommand(updatePasswordQuery, connection))
                        {
                            updatePasswordCommand.Parameters.AddWithValue("@NewPassword", newPassword);
                            updatePasswordCommand.Parameters.AddWithValue("@UserID", userId);

                            int rowsAffected = updatePasswordCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Password changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Clear the textboxes after a successful update
                                CurrentPassTextBox.Clear();
                                NewPassTextBox.Clear();
                                ConfirmPassTextBox.Clear();

                                // Hide validation labels
                                CurrentPassInvalidL.Visible = false;
                                ConfirmPassInvalidL.Visible = false;
                            }
                            else
                            {
                                MessageBox.Show("Failed to update password. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CurrentPassTextBox_TextChanged(object sender, EventArgs e)
        {
            CurrentPassInvalidL.Visible = false;
        }

        private void ConfirmPassInvalidL_TextChanged(object sender, EventArgs e)
        {
            ConfirmPassInvalidL.Visible = false;
        }

        private void AutoShutdownToggleBtm_CheckedChanged(object sender, EventArgs e)
        {
            string activationValue = AutoShutdownToggleBtm.Checked ? "1" : "0";
            string query = @"UPDATE Setting SET Activation = @Activation WHERE SettingFunction = 'AutoShutdown'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Activation", activationValue);
                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Check the state of the toggle button
            if (AutoShutdownToggleBtm.Checked)
            {
                // If the toggle is checked, update the label and database
                AutoShutdownL.Text = "Enable";  // Set label text to Enable

            }
            else
            {
                // If the toggle is unchecked, update the label and database
                AutoShutdownL.Text = "Disable";  // Set label text to Disable
            }
        }

        private void LoadAutoShutdown()
        {
            // SQL query to fetch Activation value for AutoShutdown SettingFunction
            string query = @"SELECT Activation FROM Setting WHERE SettingFunction = 'AutoShutdown'";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Execute the query and fetch the Activation value
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            // Convert the result to a boolean
                            bool activationValue = Convert.ToBoolean(result);

                            // Update the label text and toggle button state based on the Activation value
                            if (activationValue)
                            {
                                AutoShutdownL.Text = "Enable";  // Label to "Enable"
                                AutoShutdownToggleBtm.Checked = true;  // Toggle checked
                            }
                            else
                            {
                                AutoShutdownL.Text = "Disable";  // Label to "Disable"
                                AutoShutdownToggleBtm.Checked = false;  // Toggle unchecked
                            }
                        }
                        else
                        {
                            MessageBox.Show("AutoShutdown setting not found in the database.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading the AutoShutdown status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            // Toggle the visibility of the password
            if (CurrentPassTextBox.UseSystemPasswordChar)
            {
                // Show the password (set to false)
                CurrentPassTextBox.UseSystemPasswordChar = false;

            }
            else
            {
                // Hide the password (set to true)
                CurrentPassTextBox.UseSystemPasswordChar = true;
            }
        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            // Toggle the visibility of the password
            if (NewPassTextBox.UseSystemPasswordChar)
            {
                // Show the password (set to false)
                NewPassTextBox.UseSystemPasswordChar = false;

            }
            else
            {
                // Hide the password (set to true)
                NewPassTextBox.UseSystemPasswordChar = true;
            }
        }
    }
}
