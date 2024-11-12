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
using System.Management;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


namespace ComlabSystem
{
    public partial class PauseForm : Form

    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

        public string StudentID
        {
            set { StudIDLabel.Text = value; }
        }

        public string Timer
        {
            set { TimerLabel.Text = value; }
        }

        private Timer signOutTimer;
        private Timer shutDownTimer;

        private int signOutTimeLeft = 5 * 60;  // 10 minutes in seconds
        private int shutDownTimeLeft = 15 * 60;  // 15 minutes in seconds

        public PauseForm()
        {
            InitializeComponent();
            UserPassTextBox.UseSystemPasswordChar = true;

            UnitNameLabel.Text = Environment.MachineName;

            UserPassTextBox.UseSystemPasswordChar = true;

            InitializeTimers();
        }

        //Automatically add the Specifications on UnitlIst
        private void Form1_Load(object sender, EventArgs e)
        {
            StudPassLoginL.Visible = false;
            UpdateFullName();

            SignOutBtm.Visible = false;
            ShutDownBtm.Visible = false;


            this.TopMost = true; // Keep the form on top
        }










        //TImer auto shutdown
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
            if (shutDownTimeLeft == 60)
            {
                CountdownForm countdownForm = new CountdownForm();
                countdownForm.Show();
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






        //Login Again

        private void UserLoginBtm_Click(object sender, EventArgs e)
        {
            string studentID = StudIDLabel.Text;
            string userPassword = UserPassTextBox.Text;

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


                        userIsLoggedIn = true;
                        this.TopMost = false; // Allow other applications to come to the front
                        signOutTimer.Stop();
                        shutDownTimer.Stop();
                        this.Hide();
                    }
                    else
                    {
                        StudPassLoginL.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred during login: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        





        //SYSTEM FUNCTIONS


        //Hide Labels that wrong the login form

        private void UserPassTextBox_TextChanged(object sender, EventArgs e)
        {
            StudPassLoginL.Visible = false;
        }


        private bool userIsLoggedIn = false; // Set to true after successful login
        private void PauseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!userIsLoggedIn)
            {
                e.Cancel = true;
                // Display the message box with a more detailed and helpful message
                ClosingTheAppMsgBox.Text = "You need to sign in to access the system. Please enter your username and password to continue.";
                ClosingTheAppMsgBox.Caption = "Sign In Required";
                ClosingTheAppMsgBox.Show();

            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        private void UserPassTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Check if the Enter key was pressed
            {
                e.SuppressKeyPress = true; // Optional: Prevents the 'ding' sound
                UserLoginBtm.PerformClick(); // Simulate button click
            }
        }

        private void SignOutBtm_Click(object sender, EventArgs e)
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
    }
}
