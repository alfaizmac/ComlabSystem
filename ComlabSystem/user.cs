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

            CurrentPassTB.UseSystemPasswordChar = true;
            NewPassTB.UseSystemPasswordChar = true;
            ConfirmNewPassTB.UseSystemPasswordChar = true;
            LockScreenPassTB.UseSystemPasswordChar = true;

            SignOutBtm.Visible = false;
            ShutDownBtm.Visible = false;



            string MenuBtmtip = "Side Menu";
            MenuToolTip.SetToolTip(MenuButton, MenuBtmtip);

            string ReportBtmtip = "Report/Feedback";
            MenuToolTip.SetToolTip(ReportButton, ReportBtmtip);

            string ChangePasstip = "Change Password";
            MenuToolTip.SetToolTip(ChangePasswordButton, ChangePasstip);

            string LockScreenTip = "Lock Screen";
            MenuToolTip.SetToolTip(PauseBtm, LockScreenTip);

            string SignoutTIp = "Lock Screen";
            MenuToolTip.SetToolTip(SignOutButtom, SignoutTIp);
        }

        private void user_Load(object sender, EventArgs e)
        {
            UpdateFullName();

            StudPassLoginL.Visible = false;

            // Get the working area of the screen where the form is displayed
            Rectangle workingArea = Screen.GetWorkingArea(this);

            // Set the form's location to the bottom-right corner of the screen
            this.Location = new Point(workingArea.Right - this.Width, workingArea.Bottom - this.Height);


            sharedTimer = new Timer();
            sharedTimer.Interval = 1000; // Set interval to 1 second (1000 ms)
            sharedTimer.Tick += Timer_Tick;
            sharedTimer.Start();

            LockScreenStudPassLoginL.Visible =false;


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
            int logId = InsertLoginAction(studentID, fName);

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

            // Get the user input from the TextBox
            string feedback = SendReportFeedbackTB.Text;
            string fullName = FLNameLabel.Text; // Full name of the user
            string computerName = UnitNameLabel.Text; // Computer name (UnitNameLabel.Text)
            DateTime currentDateTime = DateTime.Now; // Get the current timestamp

            // Determine the message type based on the selected radio button
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

            // Format the issue description
            string issueDescription = $"{fullName} has sent a {messageType}: \"{feedback}\" while using the computer named {computerName}.";

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
                        "INSERT INTO Help_Desk (UserID, MessageType, IssueDescription, Timestamp) " +
                        "VALUES (@UserID, @MessageType, @IssueDescription, @Timestamp)", connection);

                    insertFeedbackCmd.Parameters.AddWithValue("@UserID", userID);  // Use the userID
                    insertFeedbackCmd.Parameters.AddWithValue("@MessageType", messageType);  // 'Report' or 'Feedback'
                    insertFeedbackCmd.Parameters.AddWithValue("@IssueDescription", issueDescription);  // The formatted feedback
                    insertFeedbackCmd.Parameters.AddWithValue("@Timestamp", currentDateTime);  // The current timestamp

                    // Execute the command
                    insertFeedbackCmd.ExecuteNonQuery();

                    // Insert into Notifications table
                    string notificationMessage = $"{fullName} has sent {messageType}.";
                    SqlCommand insertNotificationCmd = new SqlCommand(
                        "INSERT INTO Notifications (UserID, Message, Timestamp, NotificationType, NotificationKind) " +
                        "VALUES (@UserID, @Message, @Timestamp, @NotificationType, @NotificationKind)", connection);

                    insertNotificationCmd.Parameters.AddWithValue("@UserID", userID);  // Use the userID
                    insertNotificationCmd.Parameters.AddWithValue("@Message", notificationMessage);  // The notification message
                    insertNotificationCmd.Parameters.AddWithValue("@Timestamp", currentDateTime);  // The current timestamp
                    insertNotificationCmd.Parameters.AddWithValue("@NotificationType", "Info");  // Notification type
                    insertNotificationCmd.Parameters.AddWithValue("@NotificationKind", $"{messageType}");  // Report/Feedback

                    // Execute the command
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
                    // Handle any errors during the insert operation
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

                    // Update UserList table: Set Status to 'Offline', DateLastLogout to DateTime.Now, and UnitUsed to the computer name
                    SqlCommand updateUserListCmd = new SqlCommand(
                        "UPDATE UserList SET Status = @Status, UnitUsed = @UnitUsed, DateLastLogout = @DateLastLogout " +
                        "WHERE StudentID = @StudentID", connection);
                    updateUserListCmd.Parameters.AddWithValue("@Status", "Offline");
                    updateUserListCmd.Parameters.AddWithValue("@UnitUsed", $"Last Unit Used {computerName}");
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
                        "UPDATE UnitList SET DateLastUsed = @DateLastUsed, LastUserID = @LastUserID, CurrentUser = @CurrentUser, Status = @Status WHERE ComputerName = @ComputerName", connection);
                    updateUnitListCmd.Parameters.AddWithValue("@DateLastUsed", currentDateTime);
                    updateUnitListCmd.Parameters.AddWithValue("@LastUserID", userID);
                    updateUnitListCmd.Parameters.AddWithValue("@Status", "Offline");
                    updateUnitListCmd.Parameters.AddWithValue("@CurrentUser", "No Current User");
                    updateUnitListCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    updateUnitListCmd.ExecuteNonQuery();

                    // Insert into the Logs table with TimeDuration
                    string logAction = $"{FLNameLabel.Text} sign out on Computer Name {computerName} at {currentDateTime}";
                    SqlCommand insertLogCmd = new SqlCommand(
                        "INSERT INTO Logs (Action, UserID, UnitID, TimeDuration, ActionType) VALUES (@Action, @UserID, @UnitID, @TimeDuration, @ActionType)", connection);
                    insertLogCmd.Parameters.AddWithValue("@Action", logAction);
                    insertLogCmd.Parameters.AddWithValue("@UserID", userID);
                    insertLogCmd.Parameters.AddWithValue("@UnitID", unitID);  // Use the retrieved UnitID here
                    insertLogCmd.Parameters.AddWithValue("@TimeDuration", sessionTime);  // Insert the session time as TimeDuration
                    insertLogCmd.Parameters.AddWithValue("@ActionType", "Signout");
                    insertLogCmd.ExecuteNonQuery();
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
                SideMenuDialogs.Icon = MessageDialogIcon.Error;
                SideMenuDialogs.Caption = "Password Mismatch";
                SideMenuDialogs.Text = "The new passwords do not match. Please try again.";
                SideMenuDialogs.Show();
                return;
            }

            string studentID = StudIDLabel.Text;
            string currentPasswordInput = CurrentPassTB.Text;
            string newPassword = NewPassTB.Text;
            string fullName = FLNameLabel.Text; // Full name of the student (e.g., "John Doe")

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

                        SideMenuDialogs.Icon = MessageDialogIcon.Information;
                        SideMenuDialogs.Caption = "Success";
                        SideMenuDialogs.Text = "Password changed successfully.";
                        SideMenuDialogs.Show();

                        int rowsAffected = updatePasswordCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {

                            ChangePasswordPNL.Hide();
                            CurrentPassTB.Text = "";
                            NewPassTB.Text = "";
                            ConfirmNewPassTB.Text = "";

                            // Step 4: Insert a notification record into the Notifications table
                            SqlCommand insertNotifCmd = new SqlCommand(
                                "INSERT INTO Notifications (UserID, NotificationType, NotificationKind, Message, Timestamp) " +
                                "VALUES (@UserID, @NotificationType, @NotificationKind, @Message, @Timestamp)", connection);

                            insertNotifCmd.Parameters.AddWithValue("@UserID", userID);
                            insertNotifCmd.Parameters.AddWithValue("@NotificationType", "Info");
                            insertNotifCmd.Parameters.AddWithValue("@NotificationKind", "ChangePassword");
                            insertNotifCmd.Parameters.AddWithValue("@Message", fullName + " changed to a new password: " + newPassword);
                            insertNotifCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                            insertNotifCmd.ExecuteNonQuery();

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
            if (shutDownTimeLeft == 860)
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
                ShutDownBtm.Visible = true;

                // Code to shut down the computer
                ShutDownComputer();
            }
        }

        private void ShutDownComputer()
        {
            // You can use the Process.Start method to run a shutdown command
            System.Diagnostics.Process.Start("shutdown", "/s /f /t 0");
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





        // Insert login action into the Logs table
        private int InsertLoginAction(string studentID, string lastName, string firstName)
        {
            string computerName = UnitNameLabel.Text;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT UserID FROM UserList WHERE StudentID = @StudentID", connection);
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    int userID = (int)cmd.ExecuteScalar();

                    SqlCommand unitCmd = new SqlCommand("SELECT UnitID FROM UnitList WHERE ComputerName = @ComputerName", connection);
                    unitCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    int unitID = (int)unitCmd.ExecuteScalar();

                    SqlCommand logCmd = new SqlCommand(
                        "INSERT INTO Logs (UnitID, UserID, Action, Timestamp, TimeDuration, ActionType) OUTPUT INSERTED.LogID VALUES (@UnitID, @UserID, @Action, @Timestamp, @TimeDuration, @ActionType)", connection);
                    logCmd.Parameters.AddWithValue("@UnitID", unitID);
                    logCmd.Parameters.AddWithValue("@UserID", userID);
                    logCmd.Parameters.AddWithValue("@Action", $"{firstName} {lastName} has logged in on {computerName} at " + DateTime.Now + ".");
                    logCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    logCmd.Parameters.AddWithValue("@TimeDuration", TimeSpan.Zero);  // Set initial duration to 0
                    logCmd.Parameters.AddWithValue("@ActionType", "Login");

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













        // Add a Timer to track duration
        private Timer sessionTimer;
        private DateTime loginTime;

        // Insert login action into the Logs table
        private int InsertLoginAction(string studentID, string fName)
        {
            string computerName = UnitNameLabel.Text;  // Get the computer name label text

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Get the UserID for the StudentID
                    SqlCommand cmd = new SqlCommand("SELECT UserID FROM UserList WHERE StudentID = @StudentID", connection);
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    int userID = (int)cmd.ExecuteScalar();

                    // Get the UnitID for the Computer Name
                    SqlCommand unitCmd = new SqlCommand("SELECT UnitID FROM UnitList WHERE ComputerName = @ComputerName", connection);
                    unitCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    int unitID = (int)unitCmd.ExecuteScalar();

                    // Insert the login action into the Logs table
                    SqlCommand logCmd = new SqlCommand(
                        "INSERT INTO Logs (UnitID, UserID, Action, Timestamp, TimeDuration, ActionType) OUTPUT INSERTED.LogID VALUES (@UnitID, @UserID, @Action, @Timestamp, @TimeDuration, @ActionType)", connection);
                    logCmd.Parameters.AddWithValue("@UnitID", unitID);
                    logCmd.Parameters.AddWithValue("@UserID", userID);
                    logCmd.Parameters.AddWithValue("@Action", $"{fName} has logged in on {computerName} at " + DateTime.Now + ".");
                    logCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    logCmd.Parameters.AddWithValue("@TimeDuration", TimeSpan.Zero);  // Set initial duration to 0
                    logCmd.Parameters.AddWithValue("@ActionType", "Login");

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


    }
}