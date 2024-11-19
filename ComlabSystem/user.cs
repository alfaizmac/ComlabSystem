﻿using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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

        public string LockScreenStudentID
        {
            set { LockScreenStudIDLabel.Text = value; }
        }

        bool SideBarExpand;
        public user()
        {
            InitializeComponent();


            HideOtherPanel();

            UnitNameLabel.Text = Environment.MachineName;
            ComputerNameL.Text = Environment.MachineName;

            CurrentPassTB.UseSystemPasswordChar = true;
            NewPassTB.UseSystemPasswordChar = true;
            ConfirmNewPassTB.UseSystemPasswordChar = true;
            LockScreenPassTB.UseSystemPasswordChar = true;

            SignOutBtm.Visible = false;
            ShutDownBtm.Visible = false;
            PassNotmatch.Visible = false;



            string MenuBtmtip = "Side Menu";
            MenuToolTip.SetToolTip(MenuButton, MenuBtmtip);

            string ReportBtmtip = "Report/Feedback";
            MenuToolTip.SetToolTip(ReportButton, ReportBtmtip);

            string ChangePasstip = "Change Password";
            MenuToolTip.SetToolTip(ChangePasswordButton, ChangePasstip);

            string LockScreenTip = "Lock Screen";
            MenuToolTip.SetToolTip(PauseBtm, LockScreenTip);

            string SignoutTIp = "Sign Out";
            LockTooltip.SetToolTip(SignOutButtom, SignoutTIp);

            string Shutdwontip = "Shutdown";
            LockTooltip.SetToolTip(ShutDownBtmNonlock, Shutdwontip);

            string SignoutTip = "SignOut";
            LockTooltip.SetToolTip(SignOutBtm, SignoutTip);

            string LockShutdown = "Shutdown";
            LockTooltip.SetToolTip(ShutDownBtm, LockShutdown);
        }

        private void user_Load(object sender, EventArgs e)
        {
            UpdateFullName();

            StudPassLoginL.Visible = false;

            // Get the working area of the screen where the form is displayed
            //Rectangle workingArea = Screen.GetWorkingArea(this);

            // Set the form's location to the bottom-right corner of the screen
            //this.Location = new Point(workingArea.Right - this.Width, workingArea.Bottom - this.Height);


            sharedTimer = new Timer();
            sharedTimer.Interval = 1000; // Set interval to 1 second (1000 ms)
            sharedTimer.Tick += Timer_Tick;
            sharedTimer.Start();

            LockScreenStudPassLoginL.Visible =false;
            PassNotmatch.Visible = true;


            this.WindowState = FormWindowState.Normal;
            this.Size = new Size(600, 350);

            TopMost = false;

            // Hide UserPausePnL and show UserPnL
            UserPausePnL.Visible = false;
            UserPnL.Visible = true;




            // Assuming the student details (ID, LastName, FirstName) are available from labels or session variables:
            string studentID = StudIDLabel.Text;  // Or use session variable if applicable
            string fName = FLNameLabel.Text;   // Assuming you have FirstNameLabel for the student's first name

            // Insert login action into Logs and get the LogID
            int logId = InsertLoginAction(studentID);

            // Start the session timer to track duration
            loginTime = DateTime.Now;
            StartSessionTimer(logId);
        }




        // Static variables to track the time
        public static int seconds = 0;
        public static int minutes = 0;
        public static int hours = 0;

        // Timer variable to track time
        private static Timer sharedTimer;
        // Initialize Timer if not already initialized
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
            SendReportFeedbackTB.Focus();
        }

        private void ChangePasswordButton_Click(object sender, EventArgs e)
        {
            ChangePasswordPNL.BringToFront();
            ChangePasswordPNL.Visible = true;
            ReportPnl.Visible = false;
            CurrentPassTB.Focus();
            PassNotmatch.Visible = false;


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
            // Get the user details
            string studentID = StudIDLabel.Text;
            string feedback = SendReportFeedbackTB.Text;
            string fullName = FLNameLabel.Text;
            string computerName = UnitNameLabel.Text;
            DateTime currentDateTime = DateTime.Now;

            string messageType = string.Empty;
            if (ReportRadioButton.Checked)
            {
                messageType = "Report";
            }
            else if (FeedbackRadioBtm.Checked)
            {
                messageType = "Feedback";
            }

            if (string.IsNullOrEmpty(messageType))
            {
                MessageBox.Show("Please select either 'Report' or 'Feedback' before submitting.");
                return;
            }

            string issueDescription = $"{fullName} has sent a {messageType}: \"{feedback}\" while using the computer named {computerName}.";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Retrieve the UserID and Email based on StudentID
                    SqlCommand getUserDetailsCmd = new SqlCommand(
                        "SELECT UserID, Email FROM UserList WHERE StudentID = @StudentID", connection);
                    getUserDetailsCmd.Parameters.AddWithValue("@StudentID", studentID);

                    int userID = 0;
                    string email = string.Empty;

                    using (SqlDataReader reader = getUserDetailsCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userID = Convert.ToInt32(reader["UserID"]);
                            email = reader["Email"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("No user found with the provided Student ID.");
                            return;
                        }
                    } // Close the reader here

                    // Insert into Notifications table
                    SqlCommand insertNotificationCmd = new SqlCommand(
                        "INSERT INTO Notifications (UserID, Message, Timestamp, NotificationType, NotificationKind, UserType, UnitName, StudentID, Email) " +
                        "VALUES (@UserID, @Message, @Timestamp, @NotificationType, @NotificationKind, @UserType, @UnitName, @StudentID, @Email)", connection);

                    insertNotificationCmd.Parameters.AddWithValue("@UserID", userID);
                    insertNotificationCmd.Parameters.AddWithValue("@Message", issueDescription);
                    insertNotificationCmd.Parameters.AddWithValue("@Timestamp", currentDateTime);
                    insertNotificationCmd.Parameters.AddWithValue("@NotificationType", "Information");
                    insertNotificationCmd.Parameters.AddWithValue("@NotificationKind", messageType);
                    insertNotificationCmd.Parameters.AddWithValue("@UserType", "Student");
                    insertNotificationCmd.Parameters.AddWithValue("@UnitName", computerName);
                    insertNotificationCmd.Parameters.AddWithValue("@StudentID", studentID);
                    insertNotificationCmd.Parameters.AddWithValue("@Email", email);

                    insertNotificationCmd.ExecuteNonQuery();

                    // Hide the report panel and clear the feedback text box
                    ReportPnl.Hide();
                    SendReportFeedbackTB.Text = "";

                    SideMenuDialogs.Icon = MessageDialogIcon.Information;
                    SideMenuDialogs.Caption = "Success";
                    SideMenuDialogs.Text = "Your " + messageType + " has been sent successfully.";
                    SideMenuDialogs.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to send report/feedback: " + ex.Message);
                }
            }
        }







        //Signout Button
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

            if (result == DialogResult.Yes)
            {
                sessionTimer.Stop();
                UpdateSignOutStatus();

                allowClose = true;

                seconds = 0;
                minutes = 0;
                hours = 0;
                sharedTimer.Stop();

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

                    // Update UserList table: Set Status to 'Offline', DateLastLogout to DateTime.Now, and UnitUsed to the computer name
                    SqlCommand updateUserListCmd = new SqlCommand(
                        "UPDATE UserList SET Status = @Status, UnitUsed = @UnitUsed, DateLastLogout = @DateLastLogout " +
                        "WHERE StudentID = @StudentID", connection);
                    updateUserListCmd.Parameters.AddWithValue("@Status", "Offline");
                    updateUserListCmd.Parameters.AddWithValue("@UnitUsed", $"Last Unit Used '{computerName}'");
                    updateUserListCmd.Parameters.AddWithValue("@StudentID", studentID);
                    updateUserListCmd.Parameters.AddWithValue("@DateLastLogout", currentDateTime);
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
                        "UPDATE UnitList SET DateLastUsed = @DateLastUsed, LastUserID = @LastUserID, CurrentUser = @CurrentUser, LastUserName = @LastUserName, DateNewLogin = @DateNewLogin WHERE ComputerName = @ComputerName", connection);
                    updateUnitListCmd.Parameters.AddWithValue("@DateLastUsed", currentDateTime);
                    updateUnitListCmd.Parameters.AddWithValue("@LastUserID", userID);
                    updateUnitListCmd.Parameters.AddWithValue("@CurrentUser", "No Current User");
                    updateUnitListCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    updateUnitListCmd.Parameters.AddWithValue("@LastUserName", FLNameLabel.Text);
                    updateUnitListCmd.Parameters.AddWithValue("@DateNewLogin", "No Current User");
                    updateUnitListCmd.ExecuteNonQuery();

                    // Insert into the Logs table with TimeDuration
                    string logAction = $"{FLNameLabel.Text} sign out on Computer Name {computerName} at {currentDateTime}";
                    SqlCommand insertLogCmd = new SqlCommand(
                        "INSERT INTO Logs (Action, UserID, UnitID, TimeDuration, ActionType, UserType, UnitName) VALUES (@Action, @UserID, @UnitID, @TimeDuration, @ActionType, @UserType, @UnitName)", connection);
                    insertLogCmd.Parameters.AddWithValue("@Action", logAction);
                    insertLogCmd.Parameters.AddWithValue("@UserID", userID);
                    insertLogCmd.Parameters.AddWithValue("@UnitID", unitID);  // Use the retrieved UnitID here
                    insertLogCmd.Parameters.AddWithValue("@TimeDuration", sessionTime);  // Insert the session time as TimeDuration
                    insertLogCmd.Parameters.AddWithValue("@ActionType", "Sign out");
                    insertLogCmd.Parameters.AddWithValue("@UserType", "Student");
                    insertLogCmd.Parameters.AddWithValue("@UnitName", computerName);

                    insertLogCmd.ExecuteNonQuery();

                    // Calculate and update AverageSessionDuration in UserList
                    TimeSpan averageDuration = CalculateAverageSessionDuration(connection, userID);
                    SqlCommand updateAverageCmd = new SqlCommand(
                        "UPDATE UserList SET AverageSessionDuration = @AverageSessionDuration WHERE UserID = @UserID", connection);
                    updateAverageCmd.Parameters.AddWithValue("@AverageSessionDuration", averageDuration.ToString(@"hh\:mm\:ss"));
                    updateAverageCmd.Parameters.AddWithValue("@UserID", userID);
                    updateAverageCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update sign-out status: " + ex.Message);
                }
            }
        }

        private void ShutdownUpdateSignOutStatus()
        {
            // Get the StudentID from the StudIDLabel
            string studentID = StudIDLabel.Text;
            DateTime currentDateTime = DateTime.Now;
            string computerName = UnitNameLabel.Text; // Assuming UnitNameLabel has the computer name

            // Get the TimerLabel's text in HH:MM:SS format
            string timerText = TimerLabel.Text;
            TimeSpan sessionTime = ParseTimeToTimeSpan(timerText);  // Convert TimerLabel text to TimeSpan

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

                    // Update UserList table: Set Status to 'Offline', DateLastLogout to DateTime.Now, and UnitUsed to the computer name
                    SqlCommand updateUserListCmd = new SqlCommand(
                        "UPDATE UserList SET Status = @Status, UnitUsed = @UnitUsed, DateLastLogout = @DateLastLogout " +
                        "WHERE StudentID = @StudentID", connection);
                    updateUserListCmd.Parameters.AddWithValue("@Status", "Offline");
                    updateUserListCmd.Parameters.AddWithValue("@UnitUsed", $"Last Unit Used '{computerName}'");
                    updateUserListCmd.Parameters.AddWithValue("@StudentID", studentID);
                    updateUserListCmd.Parameters.AddWithValue("@DateLastLogout", currentDateTime);
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
                        "UPDATE UnitList SET DateLastUsed = @DateLastUsed, LastUserID = @LastUserID, CurrentUser = @CurrentUser, Status = @Status, LastUserName = @LastUserName, DateNewLogin = @DateNewLogin WHERE ComputerName = @ComputerName", connection);
                    updateUnitListCmd.Parameters.AddWithValue("@DateLastUsed", currentDateTime);
                    updateUnitListCmd.Parameters.AddWithValue("@Status", "Offline");
                    updateUnitListCmd.Parameters.AddWithValue("@LastUserID", userID);
                    updateUnitListCmd.Parameters.AddWithValue("@CurrentUser", "No Current User");
                    updateUnitListCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    updateUnitListCmd.Parameters.AddWithValue("@LastUserName", FLNameLabel.Text);
                    updateUnitListCmd.Parameters.AddWithValue("@DateNewLogin", "No Current User");
                    updateUnitListCmd.ExecuteNonQuery();

                    // Insert into the Logs table with TimeDuration
                    string logAction = $"{FLNameLabel.Text} sign out on Computer Name {computerName} at {currentDateTime}";
                    SqlCommand insertLogCmd = new SqlCommand(
                        "INSERT INTO Logs (Action, UserID, UnitID, TimeDuration, ActionType, UserType, UnitName) VALUES (@Action, @UserID, @UnitID, @TimeDuration, @ActionType, @UserType, @UnitName)", connection);
                    insertLogCmd.Parameters.AddWithValue("@Action", logAction);
                    insertLogCmd.Parameters.AddWithValue("@UserID", userID);
                    insertLogCmd.Parameters.AddWithValue("@UnitID", unitID);  // Use the retrieved UnitID here
                    insertLogCmd.Parameters.AddWithValue("@TimeDuration", sessionTime);  // Insert the session time as TimeDuration
                    insertLogCmd.Parameters.AddWithValue("@ActionType", "Sign out");
                    insertLogCmd.Parameters.AddWithValue("@UserType", "Student");
                    insertLogCmd.Parameters.AddWithValue("@UnitName", computerName);

                    insertLogCmd.ExecuteNonQuery();

                    // Calculate and update AverageSessionDuration in UserList
                    TimeSpan averageDuration = CalculateAverageSessionDuration(connection, userID);
                    SqlCommand updateAverageCmd = new SqlCommand(
                        "UPDATE UserList SET AverageSessionDuration = @AverageSessionDuration WHERE UserID = @UserID", connection);
                    updateAverageCmd.Parameters.AddWithValue("@AverageSessionDuration", averageDuration.ToString(@"hh\:mm\:ss"));
                    updateAverageCmd.Parameters.AddWithValue("@UserID", userID);
                    updateAverageCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update sign-out status: " + ex.Message);
                }
            }
        }

        // Method to calculate the average session duration for a user
        private TimeSpan CalculateAverageSessionDuration(SqlConnection connection, int userID)
        {
            string query = "SELECT TimeDuration FROM Logs WHERE UserID = @UserID";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@UserID", userID);
            SqlDataReader reader = cmd.ExecuteReader();

            List<TimeSpan> durations = new List<TimeSpan>();
            while (reader.Read())
            {
                durations.Add(TimeSpan.Parse(reader["TimeDuration"].ToString()));
            }
            reader.Close();

            if (durations.Count == 0)
            {
                return TimeSpan.Zero;
            }

            // Calculate average duration
            double totalSeconds = durations.Sum(duration => duration.TotalSeconds);
            return TimeSpan.FromSeconds(totalSeconds / durations.Count);
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
                            UserNameL.Text = $"{firstName} {lastName}";
                        }
                        else
                        {
                            // If no data is found, handle accordingly (e.g., clear the label or display a default message)
                            FLNameLabel.Text = "Name not found";
                            UserNameL.Text = "Name not found";
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
                PassNotmatch.Visible = true;
                return;
            }

            string studentID = StudIDLabel.Text.Trim();
            string currentPasswordInput = CurrentPassTB.Text.Trim();
            string newPassword = NewPassTB.Text.Trim();
            string fullName = FLNameLabel.Text.Trim(); // Full name of the student (e.g., "John Doe")

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Step 2: Retrieve the UserID, current password, and email
                    SqlCommand getUserCmd = new SqlCommand(
                        "SELECT UserID, UPassword, Email FROM UserList WHERE StudentID = @StudentID",
                        connection);
                    getUserCmd.Parameters.AddWithValue("@StudentID", studentID);

                    SqlDataReader reader = getUserCmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int userID = (int)reader["UserID"];
                        string currentPasswordInDB = reader["UPassword"].ToString();
                        string email = reader["Email"].ToString(); // Retrieve the email from the database

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
                            "UPDATE UserList SET UPassword = @NewPassword WHERE UserID = @UserID",
                            connection);
                        updatePasswordCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                        updatePasswordCmd.Parameters.AddWithValue("@UserID", userID);

                        int rowsAffected = updatePasswordCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // Step 4: Insert a notification into the Notifications table
                            SqlCommand insertNotificationCmd = new SqlCommand(
                                @"INSERT INTO Notifications 
                        (Message, Timestamp, UserID, NotificationType, NotificationKind, StudName, UserType, StudentID, Email, UnitName) 
                        VALUES 
                        (@Message, @Timestamp, @UserID, @NotificationType, @NotificationKind, @StudName, @UserType, @StudentID, @Email, @UnitName)",
                                connection);

                            string notificationMessage = $"{fullName} ({studentID}) has changed their password from {currentPasswordInput} to {newPassword} on '{UnitNameLabel.Text}' at {DateTime.Now:MMMM dd, yyyy h:mm tt}.";

                            insertNotificationCmd.Parameters.AddWithValue("@Message", notificationMessage);
                            insertNotificationCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                            insertNotificationCmd.Parameters.AddWithValue("@UserID", userID);
                            insertNotificationCmd.Parameters.AddWithValue("@NotificationType", "Information");
                            insertNotificationCmd.Parameters.AddWithValue("@NotificationKind", "UserChangePassword");
                            insertNotificationCmd.Parameters.AddWithValue("@StudName", fullName);
                            insertNotificationCmd.Parameters.AddWithValue("@UserType", "Student");
                            insertNotificationCmd.Parameters.AddWithValue("@StudentID", studentID);
                            insertNotificationCmd.Parameters.AddWithValue("@Email", email);
                            insertNotificationCmd.Parameters.AddWithValue("@UnitName", UnitNameLabel.Text);

                            insertNotificationCmd.ExecuteNonQuery();

                            // Step 5: Show success message and clear input fields
                            SideMenuDialogs.Icon = MessageDialogIcon.Information;
                            SideMenuDialogs.Caption = "Success";
                            SideMenuDialogs.Text = "Password changed successfully.";
                            SideMenuDialogs.Show();

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








        //Lock Screen COde

        private void PauseBtm_Click(object sender, EventArgs e)
        {
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
                UserPnL.Visible = false;
                UserPausePnL.Visible = true;
                TopMost = true;
                this.WindowState = FormWindowState.Maximized;
                InitializeTimers();
                LockScreenPassTB.Focus();

            }
            else { return; }

        }





        private void UserLoginBtm_Click(object sender, EventArgs e)
        {

            string studentID = LockScreenStudIDLabel.Text;
            string userPassword = LockScreenPassTB.Text;

            // Connection to your database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // SQL command to verify StudentID and UPassword
                    SqlCommand verifyCredentialsCmd = new SqlCommand(
                        "SELECT COUNT(1) FROM UserList WHERE StudentID = @StudentID AND UPassword = @UPassword",
                        connection);

                    // Set the parameters
                    verifyCredentialsCmd.Parameters.AddWithValue("@StudentID", studentID);
                    verifyCredentialsCmd.Parameters.AddWithValue("@UPassword", userPassword);

                    // Execute the query
                    int result = (int)verifyCredentialsCmd.ExecuteScalar();

                    if (result == 1)
                    {

                        this.WindowState = FormWindowState.Normal;
                        this.Size = new Size(600, 350);

                        CountdownForm countdownForm = new CountdownForm
                        {
                            TopMost = false // Set CountdownForm as topmost
                        };

                        countdownForm.Hide();

                        TopMost = false;

                        // Hide UserPausePnL and show UserPnL
                        UserPausePnL.Visible = false;
                        UserPnL.Visible = true;

                        LockScreenPassTB.Text = "";

                        signOutTimeLeft = 30;  // Reset to 10 minutes
                        shutDownTimeLeft = 15 * 60;  // Reset to 15 minutes
                        SignOutBtm.Visible = false;
                        ShutDownBtm.Visible = false;
                        signOutTimer.Stop();
                        shutDownTimer.Stop();



                    }
                    else
                    {
                        LockScreenStudPassLoginL.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during login: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }



        }

        private void UserLockScreenShowPassBtm_Click(object sender, EventArgs e)
        {
            // Toggle the visibility of the password
            if (LockScreenPassTB.UseSystemPasswordChar)
            {
                // Show the password (set to false)
                LockScreenPassTB.UseSystemPasswordChar = false;

            }
            else
            {
                // Hide the password (set to true)
                LockScreenPassTB.UseSystemPasswordChar = true;
            }
        }

        private void LockScreenPassTB_TextChanged(object sender, EventArgs e)
        {
            LockScreenStudPassLoginL.Visible = false;
        }








        //TImer auto shutdown


        private Timer signOutTimer;
        private Timer shutDownTimer;
        private int signOutTimeLeft = 30;  // 10 minutes in seconds
        private int shutDownTimeLeft = 15 * 60;  // 15 minutes in seconds
        private void InitializeTimers()
        {
            // Initialize SignOut Timer
            signOutTimer = new Timer();
            signOutTimer.Interval = 1000;  // 1 second interval
            signOutTimer.Tick += SignOutTimer_Tick;

            // Initialize Shutdown Timer
            shutDownTimer = new Timer();
            shutDownTimer.Interval = 1000;  // 1 second interval
            shutDownTimer.Tick += ShutDownTimer_Tick;

            // Start both timers when the form loads
            signOutTimer.Start();
            shutDownTimer.Start();
        }

        private void SignOutTimer_Tick(object sender, EventArgs e)
        {
            // Decrease the time left for sign out
            signOutTimeLeft--;

            // Convert the time left to minutes and seconds
            int minutes = signOutTimeLeft / 60;
            int seconds = signOutTimeLeft % 60;

            // Update the label
            AllowSignOutL.Text = $"{minutes:D2}:{seconds:D2}";

            // When time reaches zero, show the buttons
            if (signOutTimeLeft <= 0)   
            {
                signOutTimer.Stop();
                ShowSignoutButtonAvai.Caption = "Sign Out Now Available";
                ShowSignoutButtonAvai.Text =
                    "The **Sign Out** button is now accessible. " +
                    "To sign out, simply click the **Sign Out** button located at the top-right corner of your screen. " +
                    "This ensures the security of your session and helps manage computer usage effectively.";
                ShowSignoutButtonAvai.Show();
                SignOutBtm.Visible = true;
                ShutDownBtm.Visible = true;
            }
        }

        private void ShutDownTimer_Tick(object sender, EventArgs e)
        {
            // Decrease the time left for shutdown
            shutDownTimeLeft--;

            // Convert the time left to minutes and seconds
            int minutes = shutDownTimeLeft / 60;
            int seconds = shutDownTimeLeft % 60;

            // Update the label
            AllowShutDownL.Text = $"{minutes:D2}:{seconds:D2}";

            // When time reaches 14 minutes (1 minute before shutdown), show the CountdownForm
            if (shutDownTimeLeft == 840)
            {
                CountdownForm countdownForm = new CountdownForm
                {
                    TopMost = true // Set CountdownForm as topmost
                };

                countdownForm.Show(); // Show the countdown form

                // Bring CountdownForm to the front explicitly in case other forms are topmost
                countdownForm.BringToFront();

            }

            // When time reaches zero, shut down the computer
            if (shutDownTimeLeft <= 0)
            {
                shutDownTimer.Stop();

                // Code to shut down the computer
                ShutDownComputer();
            }
        }

        private void ShutDownComputer()
        {
            try
            {

                // Get the ComputerName and StudentID
                string computerName = ComputerNameL.Text; // Get the computer name from the label
                string studentId = LockScreenStudIDLabel.Text; // Get the student ID from the label

                // SQL queries to update the AutoShutdownCount for both UnitList and UserList
                string updateUnitListQuery = "UPDATE UnitList SET AutoShutdownCount = AutoShutdownCount + 1 WHERE ComputerName = @ComputerName";
                string updateUserListQuery = "UPDATE UserList SET AutoShutdownCount = AutoShutdownCount + 1 WHERE StudentID = @StudentID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Update AutoShutdownCount for the UnitList table
                    SqlCommand updateUnitListCommand = new SqlCommand(updateUnitListQuery, connection);
                    updateUnitListCommand.Parameters.AddWithValue("@ComputerName", computerName);
                    int unitRowsAffected = updateUnitListCommand.ExecuteNonQuery();

                    if (unitRowsAffected == 0)
                    {
                        MessageBox.Show("No matching computer found to update the AutoShutdownCount in UnitList.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    // Update AutoShutdownCount for the UserList table
                    SqlCommand updateUserListCommand = new SqlCommand(updateUserListQuery, connection);
                    updateUserListCommand.Parameters.AddWithValue("@StudentID", studentId);
                    int userRowsAffected = updateUserListCommand.ExecuteNonQuery();

                    if (userRowsAffected == 0)
                    {
                        MessageBox.Show("No matching student found to update the AutoShutdownCount in UserList.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                // Command to shut down the computer
                System.Diagnostics.Process.Start("shutdown", "/s /f /t 0");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initiate shutdown: " + ex.Message);
            }
        }


        private void SignOutBtm_Click(object sender, EventArgs e)
        {
            ShowSignOutDialog();
        }







        // Variable to control whether the form can be closed
        private bool allowClose = false;
        private void user_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!allowClose)
            {
                // Cancel the close action
                e.Cancel = true;
                MessageBox.Show("This form cannot be closed at this time.", "Close Blocked", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        // Method to allow closing the form programmatically

        private void LockScreenPassTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Check if the Enter key was pressed
            {
                e.SuppressKeyPress = true; // Optional: Prevents the 'ding' sound
                UserLoginBtm.PerformClick(); // Simulate button click
            }
        }




        // Add a Timer to track duration
        private Timer sessionTimer;
        private DateTime loginTime;

        // Insert login action into the Logs table
        private int InsertLoginAction(string studentID)
        {
            string computerName = UnitNameLabel.Text;  // Get the computer name label text
            string Fname = FLNameLabel.Text;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Get the UserID for the StudentID
                    SqlCommand cmd = new SqlCommand("SELECT UserID FROM UserList WHERE StudentID = @StudentID", connection);
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    int userID = (int)cmd.ExecuteScalar();

                    SqlCommand unitCmd = new SqlCommand("SELECT UnitID FROM UnitList WHERE ComputerName = @ComputerName", connection);
                    unitCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    int unitID = (int)unitCmd.ExecuteScalar();

                    // Insert the login action into the Logs table
                    SqlCommand logCmd = new SqlCommand(
                        "INSERT INTO Logs (UnitID, UserID, Action, Timestamp, TimeDuration, ActionType, UserType, StudID, UnitName) OUTPUT INSERTED.LogID VALUES (@UnitID, @UserID, @Action, @Timestamp, @TimeDuration, @ActionType, @UserType, @StudID, @UnitName)", connection);
                    logCmd.Parameters.AddWithValue("@UnitID", unitID);
                    logCmd.Parameters.AddWithValue("@UserID", userID);
                    logCmd.Parameters.AddWithValue("@Action", $"{Fname} has logged in on {computerName} at " + DateTime.Now + ".");
                    logCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    logCmd.Parameters.AddWithValue("@TimeDuration", TimeSpan.Zero);  // Set initial duration to 0
                    logCmd.Parameters.AddWithValue("@ActionType", "Login");
                    logCmd.Parameters.AddWithValue("@UserType", "Student");
                    logCmd.Parameters.AddWithValue("@StudID", studentID);  // Set initial duration to 0
                    logCmd.Parameters.AddWithValue("@UnitName", computerName);

                    // Get the inserted LogID for future updates
                    int logId = (int)logCmd.ExecuteScalar();
                    return logId;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to log login action: " + ex.Message);
                    return -1;
                }
            }
        }

        // Start the timer to track session duration
        private void StartSessionTimer(int logId)
        {
            sessionTimer = new Timer();
            sessionTimer.Interval = 1000; // 1 second interval
            sessionTimer.Tick += (sender, e) => UpdateSessionDuration(logId);
            sessionTimer.Start();
        }

        // Update session duration in the database every second
        private void UpdateSessionDuration(int logId)
        {
            TimeSpan sessionDuration = DateTime.Now - loginTime;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Logs SET TimeDuration = @TimeDuration WHERE LogID = @LogID", connection);
                    cmd.Parameters.AddWithValue("@TimeDuration", sessionDuration);
                    cmd.Parameters.AddWithValue("@LogID", logId);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update session duration: " + ex.Message);
                }
            }
        }



        //Shutdown btm
        private void ShutDownBtmNonlock_Click(object sender, EventArgs e)
        {
            // Assuming SignOutMSGDialog is already a defined Guna2MessageDialog
            SignOutMSGDialog.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            SignOutMSGDialog.Icon = MessageDialogIcon.Question;     // Question icon
            SignOutMSGDialog.Caption = "Shutdown";
            SignOutMSGDialog.Text = "Are you sure you want to shutdown?";
            SignOutMSGDialog.Style = MessageDialogStyle.Light;

            // Show the dialog and get the result
            DialogResult result = SignOutMSGDialog.Show();

            if (result == DialogResult.Yes)
            {
                sessionTimer.Stop();
                ShutdownUpdateSignOutStatus();

                allowClose = true;

                seconds = 0;
                minutes = 0;
                hours = 0;
                sharedTimer.Stop();

                // You can use the Process.Start method to run a shutdown command
                System.Diagnostics.Process.Start("shutdown", "/s /f /t 0"); ;
            }
        }

        private void ShutDownBtm_Click(object sender, EventArgs e)
        {
            // Assuming SignOutMSGDialog is already a defined Guna2MessageDialog
            SignOutMSGDialog.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            SignOutMSGDialog.Icon = MessageDialogIcon.Question;     // Question icon
            SignOutMSGDialog.Caption = "Shutdown";
            SignOutMSGDialog.Text = "Are you sure you want to shutdown?";
            SignOutMSGDialog.Style = MessageDialogStyle.Light;

            // Show the dialog and get the result
            DialogResult result = SignOutMSGDialog.Show();

            if (result == DialogResult.Yes)
            {
                sessionTimer.Stop();
                ShutdownUpdateSignOutStatus();

                allowClose = true;

                seconds = 0;
                minutes = 0;
                hours = 0;
                sharedTimer.Stop();

                // You can use the Process.Start method to run a shutdown command
                System.Diagnostics.Process.Start("shutdown", "/s /f /t 0"); ;
            }
        }

        private void d_Click(object sender, EventArgs e)
        {
            ReportRadioButton.Checked = true;
        }

        private void f_Click(object sender, EventArgs e)
        {
            FeedbackRadioBtm.Checked = true;
        }

        private void ConfirmNewPassTB_TextChanged(object sender, EventArgs e)
        {
            PassNotmatch.Visible = false;
        }

        private void LockShutdownBtm_Click(object sender, EventArgs e)
        {
            // Assuming SignOutMSGDialog is already a defined Guna2MessageDialog
            SignOutMSGDialog.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            SignOutMSGDialog.Icon = MessageDialogIcon.Question;     // Question icon
            SignOutMSGDialog.Caption = "Shutdown";
            SignOutMSGDialog.Text = "Are you sure you want to shutdown?";
            SignOutMSGDialog.Style = MessageDialogStyle.Light;

            // Show the dialog and get the result
            DialogResult result = SignOutMSGDialog.Show();

            if (result == DialogResult.Yes)
            {
                sessionTimer.Stop();
                UpdateSignOutStatus();

                allowClose = true;

                seconds = 0;
                minutes = 0;
                hours = 0;
                sharedTimer.Stop();

                // You can use the Process.Start method to run a shutdown command
                System.Diagnostics.Process.Start("shutdown", "/s /f /t 0"); ;
            }
        }
    }
}