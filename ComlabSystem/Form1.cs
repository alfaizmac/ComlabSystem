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

            ForgotPassStudPnl.Visible = false;
            ForgotPassStudPnl.Hide();


            loginCheckTimer.Start();
            // Configure form properties to make it unclosable
            this.TopMost = true; // Keep the form on top


            //Hide Labels that wrong the login form
            StudUserLoginL.Visible = false;
            StudPassLoginL.Visible = false;
            AdminUserLoginL.Visible = false;
            AdminPassLoginL.Visible = false;

            UserIDTextBox.Focus();
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
                        @"INSERT INTO UnitList (ComputerName, Ram, Processor, Storage, AvailableStorage, IPAddress, DateRegistered)
                  VALUES (@ComputerName, @Ram, @Processor, @Storage, @AvailableStorage, @IPAddress, @DateRegistered)",
                        connection);

                    insertCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    insertCmd.Parameters.AddWithValue("@Ram", ram);
                    insertCmd.Parameters.AddWithValue("@Processor", processor);
                    insertCmd.Parameters.AddWithValue("@Storage", storage);
                    insertCmd.Parameters.AddWithValue("@AvailableStorage", availableStorage);
                    insertCmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    insertCmd.Parameters.AddWithValue("@DateRegistered", DateTime.Now);

                    insertCmd.ExecuteNonQuery();

                    // Insert notification into Notifications table
                    SqlCommand notificationCmd = new SqlCommand(
                        @"INSERT INTO Notifications (Message, Timestamp, NotificationType, NotificationKind, UnitName)
                  VALUES (@Message, @Timestamp, @NotificationType, @NotificationKind, @UnitName)",
                        connection);

                    string notificationMessage = $"New unit '{computerName}' has been successfully added to the computer unit list on {DateTime.Now:MMMM dd, yyyy h:mm tt}.";
                    notificationCmd.Parameters.AddWithValue("@Message", notificationMessage);
                    notificationCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    notificationCmd.Parameters.AddWithValue("@NotificationType", "Information");
                    notificationCmd.Parameters.AddWithValue("@NotificationKind", "NewUnit");
                    notificationCmd.Parameters.AddWithValue("@UnitName", computerName);

                    notificationCmd.ExecuteNonQuery();
                }
                else
                {
                    // Update existing record if found
                    SqlCommand updateCmd = new SqlCommand(
                        @"UPDATE UnitList 
                  SET Ram = @Ram, Processor = @Processor, Storage = @Storage, 
                      AvailableStorage = @AvailableStorage, IPAddress = @IPAddress, 
                      ArchiveStatus = @ArchiveStatus, Status = @Status
                  WHERE ComputerName = @ComputerName",
                        connection);

                    updateCmd.Parameters.AddWithValue("@Ram", ram);
                    updateCmd.Parameters.AddWithValue("@Processor", processor);
                    updateCmd.Parameters.AddWithValue("@Storage", storage);
                    updateCmd.Parameters.AddWithValue("@AvailableStorage", availableStorage);
                    updateCmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    updateCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    updateCmd.Parameters.AddWithValue("@ArchiveStatus", "Active");
                    updateCmd.Parameters.AddWithValue("@Status", "Online");

                    updateCmd.ExecuteNonQuery();
                }

                // Increment ImproperShutdownCount if Status is Online
                SqlCommand statusCheckCmd = new SqlCommand(
                    "SELECT Status FROM UnitList WHERE ComputerName = @ComputerName", connection);
                statusCheckCmd.Parameters.AddWithValue("@ComputerName", UnitNameLabel.Text);

                string status = statusCheckCmd.ExecuteScalar()?.ToString();

                if (status == "Online")
                {
                    SqlCommand incrementCmd = new SqlCommand(
                        @"UPDATE UnitList 
                  SET ImproperShutdownCount = ImproperShutdownCount + 1
                  WHERE ComputerName = @ComputerName", connection);
                    incrementCmd.Parameters.AddWithValue("@ComputerName", UnitNameLabel.Text);

                    incrementCmd.ExecuteNonQuery();
                }

                // Check if available storage is below threshold
                double availableStorageValue = Convert.ToDouble(availableStorage.Replace(" GB", ""));
                if (availableStorageValue < lowStorageThreshold)
                {
                    // Insert notification if storage is low
                    SqlCommand notificationCmd = new SqlCommand(
                        @"INSERT INTO Notifications (Message, Timestamp, NotificationType, NotificationKind, UnitName) 
                  VALUES (@Message, @Timestamp, @NotificationType, @NotificationKind, @UnitName)",
                        connection);

                    string notificationMessage = $"Warning: Storage on {computerName} is running low. " +
                                                 $"Available storage is {availableStorageValue} GB. Please take action to free up space.";
                    notificationCmd.Parameters.AddWithValue("@Message", notificationMessage);
                    notificationCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    notificationCmd.Parameters.AddWithValue("@NotificationType", "Warning");
                    notificationCmd.Parameters.AddWithValue("@NotificationKind", "LowStorage");
                    notificationCmd.Parameters.AddWithValue("@UnitName", computerName);

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
            UserIDTextBox.Focus();

        }

        private void AdminShowBtm_Click(object sender, EventArgs e)
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
                AdminNameTB.Focus();
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
            countdownTime = 300;

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
                                AccountRemovedMsgBox.Text = "The account you are trying to access has been hold. Please contact support.";
                                AccountRemovedMsgBox.Show();
                                return;
                            }



                            retryAttempts = 5;
                            delayTimeInSeconds = 30;
                            retryTimer.Stop();
                            RetryAttemptTimeLabel.Text = "";

                            loginTimeoutTimer.Stop();

                            IncrementUserImproperShutdownFrequency();

                            // Update user status and unit usage
                            UpdateUserStatusAndUnit(studentID);

                            // Increment session count and unit usage frequency
                            IncrementUserSessionCount();
                            IncrementUnitUsageFrequency();


                            // Get user information for logging
                            string userID = userRow["UserID"].ToString();
                            string lName = userRow["LastName"].ToString();
                            string fName = userRow["FirstName"].ToString();


                            // Update UnitList with current user
                            UpdateUnitListUserID(lName, fName);
                            


                            // Create and show the user form, passing the LogID for duration tracking
                            user userForm = new user
                                {
                                    StudentID = studentID,
                                    LockScreenStudentID = studentID,

                                };
                                ResetLoginTimeoutTimer();
                                loginTimeoutTimer.Stop();

                                userIsLoggedIn = true;
                                loginCheckTimer.Stop();
                                this.TopMost = false;

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
        private void IncrementUserSessionCount()
        {
            string studentID = UserIDTextBox.Text.Trim();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // First, fetch the UserID using the StudentID
                    SqlCommand getUserIdCmd = new SqlCommand("SELECT UserID FROM UserList WHERE StudentID = @StudentID", connection);
                    getUserIdCmd.Parameters.AddWithValue("@StudentID", studentID);
                    var result = getUserIdCmd.ExecuteScalar();

                    if (result != null)
                    {
                        string userID = result.ToString();

                        // Now increment the SessionCount using the UserID
                        SqlCommand updateCmd = new SqlCommand("UPDATE UserList SET SessionCount = SessionCount + 1 WHERE UserID = @UserID", connection);
                        updateCmd.Parameters.AddWithValue("@UserID", userID);
                        updateCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to increment session count: " + ex.Message);
                }
            }
        }
        private void IncrementUserImproperShutdownFrequency()
        {
            string studentID = UserIDTextBox.Text.Trim(); // Get the User ID from the textbox

            if (string.IsNullOrEmpty(studentID))
            {
                MessageBox.Show("User ID is empty. Cannot update improper shutdown frequency.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Query to check the status of the user based on the StudentID
                    SqlCommand checkUserCmd = new SqlCommand(
                        "SELECT Status, UserID, FirstName, LastName FROM UserList WHERE StudentID = @StudentID",
                        connection);
                    checkUserCmd.Parameters.AddWithValue("@StudentID", studentID);

                    string userStatus = string.Empty;
                    string userID = string.Empty;
                    string firstName = string.Empty;
                    string lastName = string.Empty;

                    // Use a SqlDataReader to fetch the user details
                    using (SqlDataReader reader = checkUserCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userStatus = reader["Status"].ToString().Trim();
                            userID = reader["UserID"].ToString();
                            firstName = reader["FirstName"].ToString();
                            lastName = reader["LastName"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("No user found with the provided User ID.");
                            return;
                        }
                    } // The SqlDataReader is now closed.

                    // Proceed only if the user status is "Online"
                    if (userStatus == "Online")
                    {
                        // Increment the UserImproperShutdownCount
                        SqlCommand updateUserCmd = new SqlCommand(
                            "UPDATE UserList SET UserImproperShutdownCount = UserImproperShutdownCount + 1 WHERE StudentID = @StudentID",
                            connection);
                        updateUserCmd.Parameters.AddWithValue("@StudentID", studentID);

                        int rowsAffected = updateUserCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Insert a notification into the Notifications table
                            SqlCommand insertNotificationCmd = new SqlCommand(
                                @"INSERT INTO Notifications 
                        (Message, Timestamp, UserID, NotificationType, NotificationKind, StudName, UserType, UnitName) 
                        VALUES (@Message, @Timestamp, @UserID, @NotificationType, @NotificationKind, @StudName, @UserType, @UnitName)",
                                connection);

                            string studentName = $"{firstName} {lastName}";
                            string notificationMessage = $"{studentName} ({studentID}) did not properly shut down the computer '{UnitNameLabel.Text}' on {DateTime.Now:MMMM dd, yyyy h:mm tt}. Please investigate.";

                            insertNotificationCmd.Parameters.AddWithValue("@Message", notificationMessage);
                            insertNotificationCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                            insertNotificationCmd.Parameters.AddWithValue("@UserID", userID);
                            insertNotificationCmd.Parameters.AddWithValue("@NotificationType", "Warning");
                            insertNotificationCmd.Parameters.AddWithValue("@NotificationKind", "UserNotProperShutdow");
                            insertNotificationCmd.Parameters.AddWithValue("@StudName", studentName);
                            insertNotificationCmd.Parameters.AddWithValue("@UserType", "Student");
                            insertNotificationCmd.Parameters.AddWithValue("@UnitName", UnitNameLabel.Text);

                            insertNotificationCmd.ExecuteNonQuery();

                            // Show a warning message to the user
                            AccountRemovedMsgBox.Caption = "Warning";
                            AccountRemovedMsgBox.Icon = MessageDialogIcon.Warning;
                            AccountRemovedMsgBox.Text = "Improper shutdown and using multiple PCs can result in your account being held.";
                            AccountRemovedMsgBox.Show();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update user improper shutdown frequency: " + ex.Message);
                }
            }
        }


        private void IncrementUnitUsageFrequency()
        {
            string computerName = UnitNameLabel.Text.Trim();
            if (string.IsNullOrEmpty(computerName))
            {
                MessageBox.Show("Computer name is empty. Cannot update usage frequency.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Query to check if the computer exists in the UnitList table
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM UnitList WHERE ComputerName = @ComputerName", connection);
                    checkCmd.Parameters.AddWithValue("@ComputerName", computerName);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0) // Proceed only if the computer exists in the table
                    {
                        // Update the UsageFrequency
                        SqlCommand updateCmd = new SqlCommand("UPDATE UnitList SET UsageFrequency = UsageFrequency + 1 WHERE ComputerName = @ComputerName", connection);
                        updateCmd.Parameters.AddWithValue("@ComputerName", computerName);
                        updateCmd.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update unit usage frequency: " + ex.Message);
                }
            }
        }
       
        private void HandleFailedLogin()
        {
            if (retryAttempts == 0)
            {
                StartRetryTimer();
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


        private void UpdateUserStatusAndUnit(string studentID)
        {
            string computerName = UnitNameLabel.Text; // Get the current computer unit name

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {

                    connection.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE UserList SET Status = @Status, UnitUsed = @UnitUsed, DateLastLogout = @DateLastLogout, LastUnitUsed = @LastUnitUsed WHERE StudentID = @StudentID", connection);
                    cmd.Parameters.AddWithValue("@Status", "Online");
                    cmd.Parameters.AddWithValue("@UnitUsed", $"Current Using ({computerName})");
                    cmd.Parameters.AddWithValue("@DateLastLogout", DBNull.Value);  // NULL initially since it's a new login
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    cmd.Parameters.AddWithValue("@LastUnitUsed", computerName);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update user status: " + ex.Message);
                }
            }
        }

        // New method to update UserID in the UnitList table based on the logged-in user's ID
        private void UpdateUnitListUserID(string lastName, string firstName)
        {
            string computerName = UnitNameLabel.Text;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE UnitList SET CurrentUser = @CurrentUser, DateNewLogin = @DateNewLogin WHERE ComputerName = @ComputerName", connection);
                    cmd.Parameters.AddWithValue("@CurrentUser", $"{firstName} {lastName}");
                    cmd.Parameters.AddWithValue("@ComputerName", computerName);
                    cmd.Parameters.AddWithValue("@DateNewLogin", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update unit with user ID: " + ex.Message);
                }
            }
        }


























        // Admin Login
        private int adminRetryAttempts = 3;
        private int adminDelayTimeInSeconds = 30; // Initial delay of 30 seconds for admin
        private Timer adminRetryTimer;


        private void AdminLoginBtm_Click(object sender, EventArgs e)
        {
            countdownTime = 300;

            if (AdminNameTB.Text == "alfaizmac" && AdminPassTB.Text == "1834561834561" || AdminNameTB.Text == "EmergencyHeadAdmin2024" && AdminPassTB.Text == "Aa7Dxao2aSMa76SaX9")
            {

                // Successful login
                adminRetryAttempts = 3;
                adminDelayTimeInSeconds = 30; // Reset to 30 seconds for next time
                adminRetryTimer.Stop();
                AdminRetryAttemptTimeLabel.Text = "";


                // Reset login timeout timer
                ResetLoginTimeoutTimer();
                loginTimeoutTimer.Stop();

                userIsLoggedIn = true;
                loginCheckTimer.Stop(); // Stop the timer after successful login
                this.TopMost = false; // Allow other applications to come to the front

                Admin adminForm = new Admin(); // Replace with the actual HeadAdmin form
                adminForm.Show();
                this.Hide();
            }
            else if (adminRetryAttempts > 0)
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
                        // Check ArchiveStatus first
                        var adminRow = adminTable.Rows[0];
                        string archiveStatus = adminRow["ArchiveStatus"].ToString();

                        if (archiveStatus == "Inactive")
                        {
                            From1MsgBox.Icon = MessageDialogIcon.Information;
                            From1MsgBox.Caption = "Account Status: Hold";
                            From1MsgBox.Text = "Your account has been hold. Please contact support for further assistance.";
                            return; // Stop further login attempts if the account is inactive
                        }

                        // Check password if account is active
                        string storedPassword = adminRow["Password"].ToString();
                        if (storedPassword == adminPassword)
                        {
                            // Successful login
                            adminRetryAttempts = 3;
                            adminDelayTimeInSeconds = 30; // Reset to 30 seconds for next time
                            adminRetryTimer.Stop();
                            AdminRetryAttemptTimeLabel.Text = "";

                            // Insert login success notification
                            AdminLogs();


                            // Check AdminRole
                            string adminRole = adminRow["AdminRole"].ToString();

                            Form adminForm;

                            // Show the appropriate form based on the AdminRole
                            if (adminRole == "Head Admin")
                            {
                                adminForm = new Admin { AdminName = adminName };
                            }
                            else
                            {
                                adminForm = new Admin { AdminName = adminName }; // Replace with the actual Admin form
                            }

                            // Reset login timeout timer
                            ResetLoginTimeoutTimer();
                            loginTimeoutTimer.Stop();

                            userIsLoggedIn = true;
                            loginCheckTimer.Stop(); // Stop the timer after successful login
                            this.TopMost = false; // Allow other applications to come to the front

                            // Show the admin form and hide the login form
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
            // Set delay to 3 minutes after first 30-second delay
            adminDelayTimeInSeconds = 300; // 3 minutes in seconds
        }

        private void ShowAdminRetryMessage()
        {
            FailedAttempCountdownMsgBox.Caption = "Too many attempts.";
            FailedAttempCountdownMsgBox.Text = $"Too many unsuccessful login attempts. Please wait {adminDelayTimeInSeconds / 60} minutes before trying again.";
            FailedAttempCountdownMsgBox.Show();
        }

        private void AdminLogs()
        {
            // Get values from the form
            string adminName = AdminNameTB.Text; // Admin name (username)
            string unitName = UnitNameLabel.Text; // Computer unit name
            DateTime timestamp = DateTime.Now; // Current timestamp

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get AdminID from AdminList based on AdminNameTB.Text
                    string adminIdQuery = "SELECT AdminID FROM AdminList WHERE UserName = @AdminName";
                    SqlCommand adminIdCommand = new SqlCommand(adminIdQuery, connection);
                    adminIdCommand.Parameters.AddWithValue("@AdminName", adminName);
                    var adminIdResult = adminIdCommand.ExecuteScalar();

                    if (adminIdResult == null)
                    {
                        MessageBox.Show("Admin ID not found for the provided admin name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int adminId = Convert.ToInt32(adminIdResult);

                    // Get UnitID from UnitList based on UnitNameLabel.Text
                    string unitIdQuery = "SELECT UnitID FROM UnitList WHERE ComputerName = @UnitName";
                    SqlCommand unitIdCommand = new SqlCommand(unitIdQuery, connection);
                    unitIdCommand.Parameters.AddWithValue("@UnitName", unitName);
                    var unitIdResult = unitIdCommand.ExecuteScalar();

                    if (unitIdResult == null)
                    {
                        MessageBox.Show("Unit ID not found for the provided unit name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int unitId = Convert.ToInt32(unitIdResult);

                    // Prepare log entry details
                    string userType = "Admin";
                    string actionType = "Login";
                    string actionMessage = $"Admin name {adminName} has successfully logged into {unitName} on {timestamp:MMMM dd, yyyy hh:mm:ss tt}.";

                    // Insert into Logs table
                    string insertLogQuery = @"
                INSERT INTO Logs (AdminID, UserType, ActionType, UnitID, Timestamp, Action, AdminName)
                VALUES (@AdminID, @UserType, @ActionType, @UnitID, @Timestamp, @Action, @AdminName)";

                    SqlCommand insertLogCommand = new SqlCommand(insertLogQuery, connection);
                    insertLogCommand.Parameters.AddWithValue("@AdminID", adminId);
                    insertLogCommand.Parameters.AddWithValue("@UserType", userType);
                    insertLogCommand.Parameters.AddWithValue("@ActionType", actionType);
                    insertLogCommand.Parameters.AddWithValue("@UnitID", unitId);
                    insertLogCommand.Parameters.AddWithValue("@Timestamp", timestamp);
                    insertLogCommand.Parameters.AddWithValue("@Action", actionMessage);
                    insertLogCommand.Parameters.AddWithValue("@AdminName", adminName);
                    insertLogCommand.ExecuteNonQuery();

                    // Update Admin Status to "Online"
                    string updateStatusQuery = "UPDATE AdminList SET Status = 'Online' WHERE AdminID = @AdminID";
                    SqlCommand updateStatusCommand = new SqlCommand(updateStatusQuery, connection);
                    updateStatusCommand.Parameters.AddWithValue("@AdminID", adminId);
                    updateStatusCommand.ExecuteNonQuery();

                    // Insert into Notifications table
                    string insertNotificationQuery = @"
                INSERT INTO Notifications (Message, Timestamp, AdminID, NotificationType, NotificationKind, AdminName, UserType)
                VALUES (@Message, @Timestamp, @AdminID, @NotificationType, @NotificationKind, @AdminName, @UserType)";

                    string notificationMessage = $"Admin name {adminName} has successfully logged into {unitName} on {timestamp:MMMM dd, yyyy hh:mm:ss tt}.";

                    SqlCommand insertNotificationCommand = new SqlCommand(insertNotificationQuery, connection);
                    insertNotificationCommand.Parameters.AddWithValue("@Message", notificationMessage);
                    insertNotificationCommand.Parameters.AddWithValue("@Timestamp", timestamp);
                    insertNotificationCommand.Parameters.AddWithValue("@AdminID", adminId);
                    insertNotificationCommand.Parameters.AddWithValue("@NotificationType", "Information");
                    insertNotificationCommand.Parameters.AddWithValue("@NotificationKind", "AdminLogin");
                    insertNotificationCommand.Parameters.AddWithValue("@AdminName", adminName);
                    insertNotificationCommand.Parameters.AddWithValue("@UserType", userType);
                    insertNotificationCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the log entry or updating the admin status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private int countdownTime = 300; // 3 minutes in seconds


        private void LoginTimeoutTimer_Tick(object sender, EventArgs e)
        {
            countdownTime--;

            // Show countdown form at 2 minutes (120 seconds)
            if (countdownTime == 120)
            {
                CountdownForm countdownForm = new CountdownForm
                {
                    TopMost = true // Set CountdownForm as topmost
                };

                countdownForm.Show(); // Show the countdown form

                // Bring CountdownForm to the front explicitly in case other forms are topmost
                countdownForm.BringToFront();
            }

            // Check if time is up (3 minutes)
            if (countdownTime <= 0)
            {
                loginTimeoutTimer.Stop();
                InitiateShutdown();
            }
        }

        // Initiate the shutdown process
        private void InitiateShutdown()
        {
            try
            {

                // Increment AutoShutdownCount in the UnitList table where ComputerName matches UnitNameLabel.Text
                string unitName = UnitNameLabel.Text; // Get the computer unit name from the label
                string updateCountQuery = "UPDATE UnitList SET AutoShutdownCount = AutoShutdownCount + 1 WHERE ComputerName = @UnitName";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand updateCountCommand = new SqlCommand(updateCountQuery, connection);
                    updateCountCommand.Parameters.AddWithValue("@UnitName", unitName);
                    int rowsAffected = updateCountCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("No matching unit found to update the AutoShutdownCount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                System.Diagnostics.Process.Start("shutdown", "/s /f /t 0");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initiate shutdown: " + ex.Message);
            }
        }


        private void ResetLoginTimeoutTimer()
        {
            countdownTime = 300; // Reset to 3 minutes
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
                ClosingTheAppMsgBox.Caption = "Sign In Required";
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

        private void ShutdownBtm_Click(object sender, EventArgs e)
        {
            // Assuming SignOutMSGDialog is already a defined Guna2MessageDialog
            From1MsgBox.Buttons = MessageDialogButtons.YesNo;  // YesNo buttons
            From1MsgBox.Icon = MessageDialogIcon.Question;     // Question icon
            From1MsgBox.Caption = "Shutdown";
            From1MsgBox.Text = "Are you sure you want to shutdown?";
            From1MsgBox.Style = MessageDialogStyle.Dark;

            // Show the dialog and get the result
            DialogResult result = From1MsgBox.Show();

            if (result == DialogResult.Yes)
            {

                // You can use the Process.Start method to run a shutdown command
                System.Diagnostics.Process.Start("shutdown", "/s /f /t 0"); ;
            }
        }

        private void ForgotCancel_Click(object sender, EventArgs e)
        {
            ForgotPassStudPnl.Visible = false;
            ForgotPassStudPnl.Hide();
        }

        private void ForgotPassStudLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            EmailnotOnListLabel.Visible = false;
            ForgotUserRB.Checked = true;
            ForgotPassStudPnl.Visible = true;
            ForgotPassStudPnl.BringToFront();
            ForgotPassTB.Focus();
        }


        private void ForgotAdminLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            EmailnotOnListLabel.Visible = false;
            ForgotAdminRB.Checked = true;
            ForgotPassStudPnl.Visible = true;
            ForgotPassStudPnl.BringToFront();
            ForgotPassTB.Focus();
        }

        private void ForgotConfirm_Click(object sender, EventArgs e)
        {
            string email = ForgotPassTB.Text.Trim();
            string userType = ForgotAdminRB.Checked ? "Admin" : "Student";
            string password = "";
            string message = "";
            int? userID = null;
            int? adminID = null;
            string fullName = "";
            string adminName = "";
            string studentID = "";
            string notificationKind = ForgotAdminRB.Checked ? "ForgotPassword" : "ForgotPassword";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    if (ForgotUserRB.Checked)
                    {
                        // Query UserList table for the email
                        SqlCommand userCmd = new SqlCommand(
                            "SELECT UserID, FirstName, LastName, StudentID, UPassword FROM UserList WHERE Email = @Email",
                            connection);
                        userCmd.Parameters.AddWithValue("@Email", email);

                        SqlDataReader userReader = userCmd.ExecuteReader();

                        if (userReader.Read())
                        {
                            userID = (int)userReader["UserID"];
                            password = userReader["UPassword"].ToString();
                            fullName = userReader["FirstName"].ToString() + " " + userReader["LastName"].ToString();
                            studentID = userReader["StudentID"].ToString();
                        }
                        else
                        {
                            EmailnotOnListLabel.Visible = true;
                            userReader.Close();
                            return;
                        }
                        userReader.Close();
                    }
                    else if (ForgotAdminRB.Checked)
                    {
                        // Query AdminList table for the email
                        SqlCommand adminCmd = new SqlCommand(
                            "SELECT AdminID, UserName, Password FROM AdminList WHERE Email = @Email",
                            connection);
                        adminCmd.Parameters.AddWithValue("@Email", email);

                        SqlDataReader adminReader = adminCmd.ExecuteReader();

                        if (adminReader.Read())
                        {
                            adminID = (int)adminReader["AdminID"];
                            password = adminReader["Password"].ToString();
                            adminName = adminReader["UserName"].ToString();
                        }
                        else
                        {
                            EmailnotOnListLabel.Visible = true;
                            adminReader.Close();
                            return;
                        }
                        adminReader.Close();
                    }

                    // Generate notification message
                    message = ForgotAdminRB.Checked
                        ? $"The admin {adminName} forgot their password and requested it. The current password is {password}."
                        : $"The student {fullName} ({studentID}) forgot their password and requested it. The current password is {password}.";

                    // Insert into Notifications table
                    SqlCommand insertNotificationCmd = new SqlCommand(
                        @"INSERT INTO Notifications 
                (Message, Timestamp, UserID, AdminID, NotificationType, NotificationKind, StudName, AdminName, UserType, UnitName, StudentID, Email) 
                VALUES 
                (@Message, @Timestamp, @UserID, @AdminID, @NotificationType, @NotificationKind, @StudName, @AdminName, @UserType, @UnitName, @StudentID, @Email)",
                        connection);

                    insertNotificationCmd.Parameters.AddWithValue("@Message", message);
                    insertNotificationCmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    insertNotificationCmd.Parameters.AddWithValue("@UserID", userID.HasValue ? (object)userID.Value : DBNull.Value);
                    insertNotificationCmd.Parameters.AddWithValue("@AdminID", adminID.HasValue ? (object)adminID.Value : DBNull.Value);
                    insertNotificationCmd.Parameters.AddWithValue("@NotificationType", "Information");
                    insertNotificationCmd.Parameters.AddWithValue("@NotificationKind", notificationKind);
                    insertNotificationCmd.Parameters.AddWithValue("@StudName", string.IsNullOrEmpty(fullName) ? DBNull.Value : (object)fullName);
                    insertNotificationCmd.Parameters.AddWithValue("@AdminName", string.IsNullOrEmpty(adminName) ? DBNull.Value : (object)adminName);
                    insertNotificationCmd.Parameters.AddWithValue("@UserType", userType);
                    insertNotificationCmd.Parameters.AddWithValue("@UnitName", UnitNameLabel.Text);
                    insertNotificationCmd.Parameters.AddWithValue("@StudentID", string.IsNullOrEmpty(studentID) ? DBNull.Value : (object)studentID);
                    insertNotificationCmd.Parameters.AddWithValue("@Email", email);

                    insertNotificationCmd.ExecuteNonQuery();

                    // Success message
                    From1MsgBox.Icon = MessageDialogIcon.Information;
                    From1MsgBox.Caption = "Request Sent";
                    From1MsgBox.Text = "Your request has been successfully recorded.";
                    From1MsgBox.Show();
                    ForgotPassStudPnl.Hide();


                    // Clear input
                    ForgotPassTB.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ForgotPassTB_TextChanged(object sender, EventArgs e)
        {
            EmailnotOnListLabel.Visible = false;
        }
    }
}