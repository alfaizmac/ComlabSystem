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


    public partial class user : Form
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
        public string StudentID
        {
            set { StudIDLabel.Text = value; }
        }


        bool SideBarExpand;
        public user()
        {
            InitializeComponent();

            HideOtherPanel();

            UnitNameLabel.Text = Environment.MachineName;



            CurrentPassTB.UseSystemPasswordChar = true;
            NewPassTB.UseSystemPasswordChar = true;
            ConfirmNewPassTB.UseSystemPasswordChar = true;

        }

        private void user_Load(object sender, EventArgs e)
        {
            UpdateFullName();

            StudPassLoginL.Visible = false;

            // Get the working area of the screen where the form is displayed
            Rectangle workingArea = Screen.GetWorkingArea(this);

            // Set the form's location to the bottom-right corner of the screen
            this.Location = new Point(workingArea.Right - this.Width, workingArea.Bottom - this.Height);

            InitializeTimer();
        }




        // Static variables to track the time
        public static int seconds = 0;
        public static int minutes = 0;
        public static int hours = 0;

        // Timer variable to track time
        private static Timer sharedTimer;
        // Initialize Timer if not already initialized
        private void InitializeTimer()
        {
            // Create a new Timer instance if it doesn't exist
            if (sharedTimer == null)
            {
                sharedTimer = new Timer();
                sharedTimer.Interval = 1000; // Set interval to 1 second (1000 ms)
                sharedTimer.Tick += Timer_Tick;
                sharedTimer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Increment seconds
            seconds++;

            // Check if seconds exceed 59 and reset to 0, increment minutes
            if (seconds >= 60)
            {
                seconds = 0;
                minutes++;
            }

            // Check if minutes exceed 59 and reset to 0, increment hours
            if (minutes >= 60)
            {
                minutes = 0;
                hours++;
            }

            // Update the timer labels in both forms
            UpdateTimerLabels();

        }

        private void UpdateTimerLabels()
        {
            // Update TimerLabel in UserForm
            TimerLabel.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);


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






        //sending feedback and report
        private void SendReportFeedbackBtm_Click(object sender, EventArgs e)
        {
            // Get the user ID (from previous logic, assuming it's retrieved and stored in userID)
            string studentID = StudIDLabel.Text;  // Assuming userID is the StudentID for now

            // Get the user feedback from the TextBox
            string feedback = SendReportFeedbackTB.Text;
            string fullName = FLNameLabel.Text; // Full name of the user
            string computerName = UnitNameLabel.Text; // Computer name (UnitNameLabel.Text)
            DateTime currentDateTime = DateTime.Now; // Get the current timestamp

            // Format the issue description
            string issueDescription = $"{fullName} has reported: \"{feedback}\" while using the computer named {computerName}.";

            // Now, insert this into the Help_Desk table
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Retrieve the UserID based on StudentID
                    SqlCommand getUserIDCmd = new SqlCommand("SELECT UserID FROM UserList WHERE StudentID = @StudentID", connection);
                    getUserIDCmd.Parameters.AddWithValue("@StudentID", studentID);
                    int userID = (int)getUserIDCmd.ExecuteScalar();

                    // Insert the report or feedback into the Help_Desk table
                    SqlCommand insertFeedbackCmd = new SqlCommand(
                        "INSERT INTO Help_Desk (UserID, IssueDescription, Timestamp) " +
                        "VALUES (@UserID, @IssueDescription, @Timestamp)", connection);

                    insertFeedbackCmd.Parameters.AddWithValue("@UserID", userID);  // Use the userID
                    insertFeedbackCmd.Parameters.AddWithValue("@IssueDescription", issueDescription);  // The formatted feedback
                    insertFeedbackCmd.Parameters.AddWithValue("@Timestamp", currentDateTime);  // The current timestamp

                    // Execute the command
                    insertFeedbackCmd.ExecuteNonQuery();

                    ReportPnl.Hide();
                    SendReportFeedbackTB.Text = "";
                    // Optionally, you can give a feedback message to the user confirming the submission
                    MessageBox.Show("Your report/feedback has been sent successfully.");
                }
                catch (Exception ex)
                {
                    // Handle any errors during the insert operation
                    MessageBox.Show("Failed to send report/feedback: " + ex.Message);
                }
            }
        }






        //Signout Button
        private void SignOutButtom_Click(object sender, EventArgs e)
        {
            ShowSignOutDialog();
            UpdateLockScreenStatus();
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

            if (result == DialogResult.Yes)
            {
                // Call the method to update the database tables before signing out
                UpdateSignOutStatus();

                // Code to sign out and show Form1
                Form1 form1 = new Form1();
                form1.Show();
                this.Hide();
            }
        }

        private void UpdateSignOutStatus()
        {
            // Get the StudentID from the StudIDLabel
            string studentID = StudIDLabel.Text;
            DateTime currentDateTime = DateTime.Now;
            string computerName = UnitNameLabel.Text; // Assuming UnitNameLabel has the computer name

            // Get the TimerLabel's text in HH:MM:SS format
            string timerText = TimerLabel.Text;
            TimeSpan sessionTime = ParseTimeToTimeSpan(timerText);  // Convert TimerLabel text to TimeSpan

            // Calculate the total seconds used
            int sessionTotalSeconds = (int)sessionTime.TotalSeconds;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Retrieve the current TotalHoursUsed value from UserList and UnitList
                    string currentTotalUserTime = GetCurrentTotalHoursUsed(connection, "UserList", studentID);
                    string currentTotalUnitTime = GetCurrentTotalHoursUsed(connection, "UnitList", computerName);

                    // Add the session time to the current values
                    TimeSpan currentUserTime = ParseTimeToTimeSpan(currentTotalUserTime);
                    TimeSpan currentUnitTime = ParseTimeToTimeSpan(currentTotalUnitTime);

                    TimeSpan newUserTotalTime = currentUserTime.Add(sessionTime);
                    TimeSpan newUnitTotalTime = currentUnitTime.Add(sessionTime);

                    // Update UserList table: Add TotalHoursUsed time
                    UpdateTotalHoursUsed(connection, "UserList", studentID, newUserTotalTime);

                    // Update UnitList table: Add TotalHoursUsed time
                    UpdateTotalHoursUsed(connection, "UnitList", computerName, newUnitTotalTime);

                    // Update UserList table: Set Status to 'Offline', LastLogin to DateTime.Now, and LastUnitUsed to the computer name
                    SqlCommand updateUserListCmd = new SqlCommand(
                        "UPDATE UserList SET Status = @Status, LastLogin = @LastLogin, LastUnitUsed = @LastUnitUsed " +
                        "WHERE StudentID = @StudentID", connection);
                    updateUserListCmd.Parameters.AddWithValue("@Status", "Offline");
                    updateUserListCmd.Parameters.AddWithValue("@LastLogin", currentDateTime);
                    updateUserListCmd.Parameters.AddWithValue("@LastUnitUsed", $"Last Unit Used {computerName}");
                    updateUserListCmd.Parameters.AddWithValue("@StudentID", studentID);
                    updateUserListCmd.ExecuteNonQuery();

                    // Retrieve the UserID based on StudentID
                    SqlCommand getUserIDCmd = new SqlCommand("SELECT UserID FROM UserList WHERE StudentID = @StudentID", connection);
                    getUserIDCmd.Parameters.AddWithValue("@StudentID", studentID);
                    int userID = (int)getUserIDCmd.ExecuteScalar();

                    // Retrieve the UnitID based on the computer name (UnitNameLabel.Text)
                    SqlCommand getUnitIDCmd = new SqlCommand("SELECT UnitID FROM UnitList WHERE ComputerName = @ComputerName", connection);
                    getUnitIDCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    int unitID = (int)getUnitIDCmd.ExecuteScalar();

                    // Update UnitList table: Set DateLastUsed to DateTime.Now and LastUserID to the retrieved UserID
                    SqlCommand updateUnitListCmd = new SqlCommand(
                        "UPDATE UnitList SET DateLastUsed = @DateLastUsed, LastUserID = @LastUserID WHERE ComputerName = @ComputerName", connection);
                    updateUnitListCmd.Parameters.AddWithValue("@DateLastUsed", currentDateTime);
                    updateUnitListCmd.Parameters.AddWithValue("@LastUserID", userID);
                    updateUnitListCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    updateUnitListCmd.ExecuteNonQuery();

                    // Insert into the Logs table
                    string logAction = $"{FLNameLabel.Text} sign out on Computer Name {computerName} at {currentDateTime}";
                    SqlCommand insertLogCmd = new SqlCommand(
                        "INSERT INTO Logs (Action, UserID, UnitID) VALUES (@Action, @UserID, @UnitID)", connection);
                    insertLogCmd.Parameters.AddWithValue("@Action", logAction);
                    insertLogCmd.Parameters.AddWithValue("@UserID", userID);
                    insertLogCmd.Parameters.AddWithValue("@UnitID", unitID);  // Use the retrieved UnitID here
                    insertLogCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update sign-out status: " + ex.Message);
                }
            }
        }

        private void UpdateLockScreenStatus()
        {
            // Get the StudentID from the StudIDLabel
            string studentID = StudIDLabel.Text;
            DateTime currentDateTime = DateTime.Now;
            string computerName = UnitNameLabel.Text; // Assuming UnitNameLabel has the computer name

            // Get the TimerLabel's text in HH:MM:SS format
            string timerText = TimerLabel.Text;
            TimeSpan sessionTime = ParseTimeToTimeSpan(timerText);  // Convert TimerLabel text to TimeSpan

            // Calculate the total seconds used
            int sessionTotalSeconds = (int)sessionTime.TotalSeconds;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Retrieve the current TotalHoursUsed value from UserList and UnitList
                    string currentTotalUserTime = GetCurrentTotalHoursUsed(connection, "UserList", studentID);
                    string currentTotalUnitTime = GetCurrentTotalHoursUsed(connection, "UnitList", computerName);

                    // Add the session time to the current values
                    TimeSpan currentUserTime = ParseTimeToTimeSpan(currentTotalUserTime);
                    TimeSpan currentUnitTime = ParseTimeToTimeSpan(currentTotalUnitTime);

                    TimeSpan newUserTotalTime = currentUserTime.Add(sessionTime);
                    TimeSpan newUnitTotalTime = currentUnitTime.Add(sessionTime);

                    // Update UserList table: Add TotalHoursUsed time
                    UpdateTotalHoursUsed(connection, "UserList", studentID, newUserTotalTime);

                    // Update UnitList table: Add TotalHoursUsed time
                    UpdateTotalHoursUsed(connection, "UnitList", computerName, newUnitTotalTime);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update sign-out status: " + ex.Message);
                }
            }
        }


        // Method to convert a time string in HH:MM:SS format to TimeSpan
        private TimeSpan ParseTimeToTimeSpan(string timeString)
        {
            string[] timeParts = timeString.Split(':');
            int hours = int.Parse(timeParts[0]);
            int minutes = int.Parse(timeParts[1]);
            int seconds = int.Parse(timeParts[2]);
            return new TimeSpan(hours, minutes, seconds);
        }

        // Method to retrieve the current TotalHoursUsed value from a table
        private string GetCurrentTotalHoursUsed(SqlConnection connection, string tableName, string identifier)
        {
            string columnName = "TotalHoursUsed";
            string query = $"SELECT {columnName} FROM {tableName} WHERE {(tableName == "UserList" ? "StudentID" : "ComputerName")} = @Identifier";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Identifier", identifier);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? "00:00:00" : result.ToString();
        }

        // Method to update TotalHoursUsed in the UserList or UnitList table
        private void UpdateTotalHoursUsed(SqlConnection connection, string tableName, string identifier, TimeSpan totalTime)
        {
            string columnName = "TotalHoursUsed";
            string formattedTime = totalTime.ToString(@"hh\:mm\:ss");
            string query = $"UPDATE {tableName} SET {columnName} = @TotalHoursUsed WHERE {(tableName == "UserList" ? "StudentID" : "ComputerName")} = @Identifier";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@TotalHoursUsed", formattedTime);
            cmd.Parameters.AddWithValue("@Identifier", identifier);
            cmd.ExecuteNonQuery();
        }













        private void UpdateFullName()
        {
            // Get the StudentID from the StudIDLabel
            string studentID = StudIDLabel.Text;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Query to get FirstName and LastName based on StudentID
                    SqlCommand getFullNameCmd = new SqlCommand(
                        "SELECT FirstName, LastName FROM UserList WHERE StudentID = @StudentID", connection);
                    getFullNameCmd.Parameters.AddWithValue("@StudentID", studentID);

                    // Execute the query and retrieve the result
                    using (SqlDataReader reader = getFullNameCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string firstName = reader["FirstName"].ToString();
                            string lastName = reader["LastName"].ToString();

                            // Set the text of the FLNameLabel to display the full name
                            FLNameLabel.Text = $"{firstName} {lastName}";
                        }
                        else
                        {
                            // If no data is found, handle accordingly (e.g., clear the label or display a default message)
                            FLNameLabel.Text = "Name not found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to retrieve full name: " + ex.Message);
                }
            }
        }




 











        private void ShowUserPassBtm_Click(object sender, EventArgs e)
        {
            // Toggle the visibility of the password
            if (NewPassTB.UseSystemPasswordChar)
            {
                // Show the password (set to false)
                NewPassTB.UseSystemPasswordChar = false;

            }
            else
            {
                // Hide the password (set to true)
                NewPassTB.UseSystemPasswordChar = true;
            }
        }

        private void ShowUserCurrentPassBtm_Click(object sender, EventArgs e)
        {
            // Toggle the visibility of the password
            if (CurrentPassTB.UseSystemPasswordChar)
            {
                // Show the password (set to false)
                CurrentPassTB.UseSystemPasswordChar = false;

            }
            else
            {
                // Hide the password (set to true)
                CurrentPassTB.UseSystemPasswordChar = true;
            }
        }

        private void CurrentPassTB_TextChanged(object sender, EventArgs e)
        {
            StudPassLoginL.Visible = false;
        }








        //Change Password
        private void ChangePassSaveBtm_Click(object sender, EventArgs e)
        {
            // Step 1: Verify if New Password and Confirm New Password match
            if (NewPassTB.Text != ConfirmNewPassTB.Text)
            {
                MessageBox.Show("The new passwords do not match. Please try again.", "Password Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string studentID = StudIDLabel.Text;
            string currentPasswordInput = CurrentPassTB.Text;
            string newPassword = NewPassTB.Text;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Step 2: Retrieve the UserID and verify the current password
                    SqlCommand getUserCmd = new SqlCommand(
                        "SELECT UserID, UPassword FROM UserList WHERE StudentID = @StudentID", connection);
                    getUserCmd.Parameters.AddWithValue("@StudentID", studentID);

                    SqlDataReader reader = getUserCmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int userID = (int)reader["UserID"];
                        string currentPasswordInDB = reader["UPassword"].ToString();

                        // Check if the current password entered matches the one in the database
                        if (currentPasswordInDB != currentPasswordInput)
                        {
                            StudPassLoginL.Visible = true; // Show the error label
                            reader.Close();
                            return;
                        }
                        reader.Close();

                        // Step 3: Update the password in the database
                        SqlCommand updatePasswordCmd = new SqlCommand(
                            "UPDATE UserList SET UPassword = @NewPassword WHERE UserID = @UserID", connection);
                        updatePasswordCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                        updatePasswordCmd.Parameters.AddWithValue("@UserID", userID);

                        int rowsAffected = updatePasswordCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Password changed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ChangePasswordPNL.Hide();
                            CurrentPassTB.Text = "";
                            NewPassTB.Text = "";
                            ConfirmNewPassTB.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("Failed to change the password. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("User not found. Please check your Student ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PauseBtm_Click(object sender, EventArgs e)
        {
            string studentID = StudIDLabel.Text.Trim();
            string timer = TimerLabel.Text.Trim();
            // Configure the SignOutMSGDialog
            SignOutMSGDialog.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            SignOutMSGDialog.Icon = MessageDialogIcon.Question;     // Question icon
            SignOutMSGDialog.Caption = "Lock Screen";
            SignOutMSGDialog.Text = "Would you like to lock the screen?";
            SignOutMSGDialog.Style = MessageDialogStyle.Light;

            // Show the dialog and get the result
            DialogResult result = SignOutMSGDialog.Show();

            // If the user clicks "Yes," show the PauseForm
            if (result == DialogResult.Yes)
            {
                // Create instance of user form and pass Student ID to StudIDLabel
                PauseForm pauseForm = new PauseForm
                {
                    StudentID = studentID, // Pass the Student ID to the user form
                    Timer = timer
                };

                pauseForm.Show();

            }
        }

    }
}
