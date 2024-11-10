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
    public partial class Form1 : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
        public Form1()
        {
            InitializeComponent();
            UserPassTextBox.UseSystemPasswordChar = true;
            AdminPassTB.UseSystemPasswordChar = true;

            UnitNameLabel.Text = Environment.MachineName;
            retryTimer = new Timer();
            retryTimer.Interval = 1000;  // Timer interval set to 1 second
            retryTimer.Tick += RetryTimer_Tick;
            RetryAttemptTimeLabel.Text = "";
            AdminRetryAttemptTimeLabel.Text = "";


            adminRetryTimer = new Timer();
            adminRetryTimer.Interval = 1000; // Timer ticks every second
            adminRetryTimer.Tick += AdminRetryTimer_Tick;


            // Initialize and start the timer
            loginTimeoutTimer = new Timer();
            loginTimeoutTimer.Tick += LoginTimeoutTimer_Tick;
            loginTimeoutTimer.Interval = 1000; // Tick every 1 second
            loginTimeoutTimer.Start();


            // Initialize and configure the timer
            loginCheckTimer = new Timer();
            loginCheckTimer.Interval = 1000; // Check every second
            loginCheckTimer.Tick += LoginCheckTimer_Tick;
        }


        //Automatically add the Specifications on UnitlIst
        private void Form1_Load(object sender, EventArgs e)
        {
            InsertOrUpdateUnitInfo();
            loginTimeoutTimer.Start();


            loginCheckTimer.Start();
            // Configure form properties to make it unclosable
            this.TopMost = true; // Keep the form on top


            //Hide Labels that wrong the login form
            StudUserLoginL.Visible = false;
            StudPassLoginL.Visible = false;
            AdminUserLoginL.Visible = false;
            AdminPassLoginL.Visible = false;
        }
        public void InsertOrUpdateUnitInfo()
        {
            // Define low storage threshold in GB
            const double lowStorageThreshold = 10.0; // 10 GB

            // Get computer information
            string computerName = Environment.MachineName;
            string ram = GetTotalRAM();
            string processor = GetProcessorName();
            string storage = GetTotalStorage();
            string availableStorage = GetAvailableStorage();
            string ipAddress = GetLocalIPAddress();
            string status = "Offline"; // Default status

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if the computer is already in the UnitList
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM UnitList WHERE ComputerName = @ComputerName", connection);
                checkCmd.Parameters.AddWithValue("@ComputerName", computerName);

                int recordCount = (int)checkCmd.ExecuteScalar();

                if (recordCount == 0)
                {
                    // Insert new record if not found
                    SqlCommand insertCmd = new SqlCommand(
                        @"INSERT INTO UnitList (ComputerName, Ram, Processor, Storage, AvailableStorage, IPAddress, Status) 
                  VALUES (@ComputerName, @Ram, @Processor, @Storage, @AvailableStorage, @IPAddress, @Status)",
                        connection);

                    insertCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    insertCmd.Parameters.AddWithValue("@Ram", ram);
                    insertCmd.Parameters.AddWithValue("@Processor", processor);
                    insertCmd.Parameters.AddWithValue("@Storage", storage);
                    insertCmd.Parameters.AddWithValue("@AvailableStorage", availableStorage);
                    insertCmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    insertCmd.Parameters.AddWithValue("@Status", status);

                    insertCmd.ExecuteNonQuery();
                }
                else
                {
                    // Update existing record if found (keeping Status as-is)
                    SqlCommand updateCmd = new SqlCommand(
                        @"UPDATE UnitList 
                  SET Ram = @Ram, Processor = @Processor, Storage = @Storage, 
                      AvailableStorage = @AvailableStorage, IPAddress = @IPAddress, Status = @Status 
                  WHERE ComputerName = @ComputerName",
                        connection);

                    updateCmd.Parameters.AddWithValue("@Ram", ram);
                    updateCmd.Parameters.AddWithValue("@Processor", processor);
                    updateCmd.Parameters.AddWithValue("@Storage", storage);
                    updateCmd.Parameters.AddWithValue("@AvailableStorage", availableStorage);
                    updateCmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    updateCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    updateCmd.Parameters.AddWithValue("@Status", status);

                    updateCmd.ExecuteNonQuery();
                }

                // Check if available storage is below threshold
                double availableStorageValue = Convert.ToDouble(availableStorage.Replace(" GB", ""));
                if (availableStorageValue < lowStorageThreshold)
                {
                    // Insert notification if storage is low
                    SqlCommand notificationCmd = new SqlCommand(
                        @"INSERT INTO Notifications (Message, Timestamp) 
                  VALUES (@Message, @Timestamp)",
                        connection);

                    string notificationMessage = $"Warning: Storage on {computerName} is running low. " +
                                                 $"Available storage is {availableStorageValue} GB. Please take action to free up space.";
                    notificationCmd.Parameters.AddWithValue("@Message", notificationMessage);
                    notificationCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);

                    notificationCmd.ExecuteNonQuery();
                }
            }
        }

        private string GetTotalRAM()
        {
            double totalMemory = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    totalMemory += Convert.ToDouble(obj["Capacity"]);
                }
            }
            return $"{Math.Round(totalMemory / (1024 * 1024 * 1024), 2)} GB"; // Convert bytes to GB
        }

        private string GetProcessorName()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["Name"].ToString();
                }
            }
            return "Unknown Processor";
        }

        private string GetTotalStorage()
        {
            double totalStorage = 0;
            foreach (var drive in System.IO.DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                totalStorage += drive.TotalSize;
            }
            return $"{Math.Round(totalStorage / (1024 * 1024 * 1024), 2)} GB"; // Convert bytes to GB
        }

        private string GetAvailableStorage()
        {
            double availableStorage = 0;
            foreach (var drive in System.IO.DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                availableStorage += drive.AvailableFreeSpace;
            }
            return $"{Math.Round(availableStorage / (1024 * 1024 * 1024), 2)} GB"; // Convert bytes to GB
        }

        private string GetLocalIPAddress()
        {
            string ipAddress = "Not Available";
            foreach (IPAddress address in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = address.ToString();
                    break;
                }
            }
            return ipAddress;
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
            AdminFormDialog.Text = "Only Admin is allowed here, do you want to procced?";

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






        //Student Login
        private int retryAttempts = 5;
        private int delayTimeInSeconds = 30; // Starts with 30 seconds
        private Timer retryTimer;

        private void UserLoginBtm_Click(object sender, EventArgs e)
        {
            if (retryAttempts > 0)
            {
                AuthenticateUser();
            }
            else
            {
                StartRetryTimer();
                ShowRetryMessage();
            }
        }

        private void AuthenticateUser()
        {
            string studentID = UserIDTextBox.Text.Trim();
            string password = UserPassTextBox.Text.Trim();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM UserList WHERE StudentID = @StudentID", connection);
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable userTable = new DataTable();
                    adapter.Fill(userTable);

                    if (userTable.Rows.Count > 0)
                    {
                        var userRow = userTable.Rows[0];
                        string storedPassword = userRow["UPassword"].ToString();
                        string archiveStatus = userRow["ArchiveStatus"].ToString();

                        if (storedPassword == password)
                        {
                            if (archiveStatus == "Archived")
                            {
                                AccountRemovedMsgBox.Caption = "Account Removed";
                                AccountRemovedMsgBox.Text = "The account you are trying to access has been removed. Please contact support.";
                                AccountRemovedMsgBox.Show();
                                return;
                            }

                            retryAttempts = 5;
                            delayTimeInSeconds = 30;
                            retryTimer.Stop();
                            RetryAttemptTimeLabel.Text = "";

                            UpdateUserStatusAndUnit(studentID);

                            string userID = userRow["UserID"].ToString();
                            UpdateUnitListUserID(userID);

                            string lName = userRow["LastName"].ToString();
                            string fName = userRow["FirstName"].ToString();
                            InsertLoginAction(studentID, lName, fName);

                            Form userForm = new user();
                            ResetLoginTimeoutTimer();

                            userIsLoggedIn = true;
                            loginCheckTimer.Stop(); // Stop the timer after successful login
                            this.TopMost = false; // Allow other applications to come to the front

                            userForm.Show();
                            this.Hide();
                        }
                        else
                        {
                            retryAttempts--;
                            StudPassLoginL.Visible = true;
                            UserPassTextBox.Focus();
                            HandleFailedLogin();
                        }
                    }
                    else
                    {
                        retryAttempts--;
                        StudUserLoginL.Visible = true;
                        UserIDTextBox.Focus();
                        HandleFailedLogin();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void HandleFailedLogin()
        {
            if (retryAttempts == 0)
            {
                StartRetryTimer();
                LogUnsuccessfulAttempt(UserIDTextBox.Text.Trim());
                ShowRetryMessage();
            }
            else
            {
                RetryAttemptTimeLabel.Text = $"Retry attempts left: {retryAttempts}";
            }
        }

        private void StartRetryTimer()
        {
            retryTimer.Start();
            RetryAttemptTimeLabel.Text = $"Wait {delayTimeInSeconds} seconds to retry.";
        }

        private void RetryTimer_Tick(object sender, EventArgs e)
        {
            if (delayTimeInSeconds > 0)
            {
                delayTimeInSeconds--;
                RetryAttemptTimeLabel.Text = $"Wait {delayTimeInSeconds} seconds to retry.";
            }
            else
            {
                retryTimer.Stop();
                retryAttempts = 5; // Reset attempts after waiting period
                UpdateRetryDelay(); // Set the next delay time
                RetryAttemptTimeLabel.Text = $"Retry attempts left: {retryAttempts}";
            }
        }

        private void UpdateRetryDelay()
        {
            // Update delay time sequence: 30s -> 3m -> 15m
            if (delayTimeInSeconds == 30)
            {
                delayTimeInSeconds = 180;  // Next wait time is 3 minutes (180 seconds)
            }
            else
            {
                delayTimeInSeconds = 180;  // Keep 15-minute wait for subsequent attempts
            }
        }

        private void ShowRetryMessage()
        {
            FailedAttempCountdownMsgBox.Caption = "Too many attempts.";
            FailedAttempCountdownMsgBox.Text = $"Too many unsuccessful login attempts. Please wait {adminDelayTimeInSeconds / 60} minutes before trying again.";
            FailedAttempCountdownMsgBox.Show();
        }

        private void LogUnsuccessfulAttempt(string studentID)
        {
            string computerName = UnitNameLabel.Text; // Assuming UnitName label has been initialized

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Notifications (Message, Timestamp) VALUES (@Message, @Timestamp)", connection);
                    cmd.Parameters.AddWithValue("@Message", $"Unsuccessful login attempt on computer unit name {computerName} by a user using a Student ID: {studentID}");
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to log unsuccessful attempt: " + ex.Message);
                }
            }
        }

        private void UpdateUserStatusAndUnit(string studentID)
        {
            string computerName = UnitNameLabel.Text; // Get the current computer unit name

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE UserList SET Status = @Status, LastUnitUsed = @LastUnitUsed WHERE StudentID = @StudentID", connection);
                    cmd.Parameters.AddWithValue("@Status", "Online");
                    cmd.Parameters.AddWithValue("@LastUnitUsed", $"Current Using ({computerName})");
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update user status: " + ex.Message);
                }
            }
        }

        // Updated method name and SQL query to log action in the Logs table
        private void InsertLoginAction(string studentID, string lastName, string firstName)
        {
            string computerName = UnitNameLabel.Text; // Get the current computer unit name

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Retrieve UserID based on the StudentID
                    SqlCommand userCmd = new SqlCommand("SELECT UserID FROM UserList WHERE StudentID = @StudentID", connection);
                    userCmd.Parameters.AddWithValue("@StudentID", studentID);
                    int userID = (int)userCmd.ExecuteScalar();

                    // Retrieve UnitID based on the computer name
                    SqlCommand unitCmd = new SqlCommand("SELECT UnitID FROM UnitList WHERE ComputerName = @ComputerName", connection);
                    unitCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    int unitID = (int)unitCmd.ExecuteScalar();

                    // Insert into Logs table
                    SqlCommand logCmd = new SqlCommand(
                        "INSERT INTO Logs (UnitID, UserID, Action, Timestamp) VALUES (@UnitID, @UserID, @Action, @Timestamp)", connection);
                    logCmd.Parameters.AddWithValue("@UnitID", unitID);
                    logCmd.Parameters.AddWithValue("@UserID", userID);
                    logCmd.Parameters.AddWithValue("@Action", $"{firstName} {lastName} has logged in on {computerName} at " + DateTime.Now + ".");
                    logCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    logCmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to log login action: " + ex.Message);
                }
            }
        }

        // New method to update UserID in the UnitList table based on the logged-in user's ID
        private void UpdateUnitListUserID(string userID)
        {
            string computerName = UnitNameLabel.Text; // Current computer unit name

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE UnitList SET UserID = @UserID, Status = @Status WHERE ComputerName = @ComputerName", connection);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@Status", "Online");
                    cmd.Parameters.AddWithValue("@ComputerName", computerName);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update UnitList UserID: " + ex.Message);

                }
            }
        }










        // Admin Login
        private int adminRetryAttempts = 3;
        private int adminDelayTimeInSeconds = 30; // Initial delay of 30 seconds for admin
        private Timer adminRetryTimer;


        private void AdminLoginBtm_Click(object sender, EventArgs e)
        {
            if (adminRetryAttempts > 0)
            {
                AuthenticateAdmin();
            }
            else
            {
                StartAdminRetryTimer();
                ShowAdminRetryMessage();
            }
        }
        private void AuthenticateAdmin()
        {
            string adminName = AdminNameTB.Text.Trim();
            string adminPassword = AdminPassTB.Text.Trim();
            string computerName = UnitNameLabel.Text; // Get computer name

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM AdminList WHERE UserName = @UserName", connection);
                    cmd.Parameters.AddWithValue("@UserName", adminName);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable adminTable = new DataTable();
                    adapter.Fill(adminTable);

                    if (adminTable.Rows.Count > 0)
                    {
                        // Check password
                        var adminRow = adminTable.Rows[0];
                        string storedPassword = adminRow["Password"].ToString();

                        if (storedPassword == adminPassword)
                        {
                            // Successful login
                            adminRetryAttempts = 3;
                            adminDelayTimeInSeconds = 30; // Reset to 30 seconds for next time
                            adminRetryTimer.Stop();
                            AdminRetryAttemptTimeLabel.Text = "";

                            // Insert login success notification
                            InsertAdminLoginNotification(adminName, computerName, true);

                            // Show admin form and hide login form
                            Form adminForm = new Admin();
                            ResetLoginTimeoutTimer();

                            userIsLoggedIn = true;
                            loginCheckTimer.Stop(); // Stop the timer after successful login
                            this.TopMost = false; // Allow other applications to come to the front

                            adminForm.Show();
                            this.Hide();
                        }
                        else
                        {
                            // Incorrect password
                            adminRetryAttempts--;
                            AdminPassLoginL.Visible = true;
                            AdminPassTB.Focus();
                            HandleAdminFailedLogin(adminName, computerName);
                        }
                    }
                    else
                    {
                        // Incorrect username
                        adminRetryAttempts--;
                        AdminUserLoginL.Visible = true;
                        AdminNameTB.Focus();
                        HandleAdminFailedLogin(adminName, computerName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void HandleAdminFailedLogin(string adminName, string computerName)
        {
            if (adminRetryAttempts == 0)
            {
                StartAdminRetryTimer();
                LogAdminUnsuccessfulAttempt(adminName, computerName);
                ShowAdminRetryMessage();
            }
            else
            {
                AdminRetryAttemptTimeLabel.Text = $"Retry attempts left: {adminRetryAttempts}";
            }
        }

        private void StartAdminRetryTimer()
        {
            adminRetryTimer.Start();
            AdminRetryAttemptTimeLabel.Text = $"Wait {adminDelayTimeInSeconds} seconds to retry.";
        }

        private void AdminRetryTimer_Tick(object sender, EventArgs e)
        {
            if (adminDelayTimeInSeconds > 0)
            {
                adminDelayTimeInSeconds--;
                AdminRetryAttemptTimeLabel.Text = $"Wait {adminDelayTimeInSeconds} seconds to retry.";
            }
            else
            {
                adminRetryTimer.Stop();
                adminRetryAttempts = 3; // Reset attempts after waiting period
                UpdateAdminRetryDelay(); // Increase delay time for next attempt
                AdminRetryAttemptTimeLabel.Text = $"Retry attempts left: {adminRetryAttempts}";
            }
        }

        private void UpdateAdminRetryDelay()
        {
            // Set delay to 15 minutes after first 30-second delay
            adminDelayTimeInSeconds = 900; // 15 minutes in seconds
        }

        private void ShowAdminRetryMessage()
        {
            FailedAttempCountdownMsgBox.Caption = "Too many attempts.";
            FailedAttempCountdownMsgBox.Text = $"Too many unsuccessful login attempts. Please wait {adminDelayTimeInSeconds / 60} minutes before trying again.";
            FailedAttempCountdownMsgBox.Show();
        }

        private void LogAdminUnsuccessfulAttempt(string adminName, string computerName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Notifications (Message, Timestamp) VALUES (@Message, @Timestamp)", connection);
                    cmd.Parameters.AddWithValue("@Message", $"Unsuccessful admin login attempt on computer {computerName} by user {adminName}.");
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to log unsuccessful admin attempt: " + ex.Message);
                }
            }
        }

        private void InsertAdminLoginNotification(string adminName, string computerName, bool isSuccess)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string statusMessage = isSuccess ? "successfully logged in" : "unsuccessful login attempt";
                    SqlCommand cmd = new SqlCommand("INSERT INTO Notifications (Message, Timestamp) VALUES (@Message, @Timestamp)", connection);
                    cmd.Parameters.AddWithValue("@Message", $"{adminName} {statusMessage} as admin on {computerName} at {DateTime.Now}.");
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to log admin login notification: " + ex.Message);
                }
            }
        }










        //Keydown Functions
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

        private void UserIDTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Check if the Enter key was pressed
            {
                e.SuppressKeyPress = true; // Optional: Prevents the 'ding' sound
                UserLoginBtm.PerformClick(); // Simulate button click
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















        //SYSTEM FUNCTIONS

        // Timer for the auto-shutdown
        private Timer loginTimeoutTimer;
        private int countdownTime = 180; // 3 minutes in seconds
    

    private void LoginTimeoutTimer_Tick(object sender, EventArgs e)
    {
        countdownTime--;

        // Show countdown form at 2 minutes (120 seconds)
        if (countdownTime == 120)
        {
            ShowCountdownForm();
        }

        // Check if time is up (3 minutes)
        if (countdownTime <= 0)
        {
            loginTimeoutTimer.Stop();
            InitiateShutdown();
        }
    }

    // Show the countdown form to warn the user
    private void ShowCountdownForm()
    {
        CountdownForm countdownForm = new CountdownForm();
        countdownForm.Show();
    }

    // Initiate the shutdown process
    private void InitiateShutdown()
    {
        try
        {
            // Command to shut down the computer
            System.Diagnostics.Process.Start("shutdown", "/s /f /t 0");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to initiate shutdown: " + ex.Message);
        }
    }

        private void ResetLoginTimeoutTimer()
        {
            countdownTime = 180; // Reset to 3 minutes
            loginTimeoutTimer.Stop(); // Stop the timer
        }




        private Timer loginCheckTimer;
        private bool userIsLoggedIn = false; // Set to true after successful login

        // Timer tick event to keep Form1 on top and in focus
        private void LoginCheckTimer_Tick(object sender, EventArgs e)
        {
            if (!this.Focused)
            {
                this.BringToFront();
                this.Activate();
            }
        }
        // Prevent the form from closing if the user is not logged in
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!userIsLoggedIn)
            {
                e.Cancel = true;
                // Display the message box with a more detailed and helpful message
                ClosingTheAppMsgBox.Text = "You need to sign in to access the system. Please enter your username and password to continue.";
                ClosingTheAppMsgBox.Caption =  "Sign In Required";
                ClosingTheAppMsgBox.Show();

            }
            else
            {
                base.OnFormClosing(e);
            }
        }


        //Hide Labels that wrong the login form
        private void UserIDTextBox_TextChanged(object sender, EventArgs e)
        {
            StudUserLoginL.Visible = false;
        }

        private void UserPassTextBox_TextChanged(object sender, EventArgs e)
        {
            StudPassLoginL.Visible = false;
        }

        private void AdminNameTB_TextChanged(object sender, EventArgs e)
        {
            AdminUserLoginL.Visible = false;
        }

        private void AdminPassTB_TextChanged(object sender, EventArgs e)
        {
            AdminPassLoginL.Visible = false;
        }
    }
}
