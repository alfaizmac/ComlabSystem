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
        }


        //Automatically add the Specifications on UnitlIst
        private void Form1_Load(object sender, EventArgs e)
        {
            InsertOrUpdateUnitInfo();
        }
        public void InsertOrUpdateUnitInfo()
        {
            // Get computer information
            string computerName = Environment.MachineName;
            string ram = GetTotalRAM();
            string processor = GetProcessorName();
            string storage = GetTotalStorage();
            string availableStorage = GetAvailableStorage();
            string ipAddress = GetLocalIPAddress();
            string status = "Online"; // Default status

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
                      AvailableStorage = @AvailableStorage, IPAddress = @IPAddress 
                  WHERE ComputerName = @ComputerName",
                        connection);

                    updateCmd.Parameters.AddWithValue("@Ram", ram);
                    updateCmd.Parameters.AddWithValue("@Processor", processor);
                    updateCmd.Parameters.AddWithValue("@Storage", storage);
                    updateCmd.Parameters.AddWithValue("@AvailableStorage", availableStorage);
                    updateCmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    updateCmd.Parameters.AddWithValue("@ComputerName", computerName);

                    updateCmd.ExecuteNonQuery();
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
                        // Check if the password is correct
                        var userRow = userTable.Rows[0];
                        string storedPassword = userRow["UPassword"].ToString();
                        string archiveStatus = userRow["ArchiveStatus"].ToString();

                        if (storedPassword == password)
                        {
                            // Check if account is archived
                            if (archiveStatus == "Archived")
                            {
                                // Show message if the account is archived
                                WrongAttemptMsgBox.Text = "This account has been removed.";
                                WrongAttemptMsgBox.Show();
                                return;
                            }

                            // Successful login - reset attempts and timer
                            retryAttempts = 5;
                            delayTimeInSeconds = 30; // Reset to 30 seconds for next time
                            retryTimer.Stop();
                            RetryAttemptTimeLabel.Text = "";

                            // Update UserList table to set Status to "Online" and LastUnitUsed
                            UpdateUserStatusAndUnit(studentID);

                            // Insert successful login notification into Notifications table
                            string lName = userRow["LastName"].ToString();
                            string fName = userRow["FirstName"].ToString();
                            InsertLoginAction(studentID, lName, fName);

                            // Show user form and hide login form
                            Form userForm = new user();
                            userForm.Show();
                            this.Hide();
                        }
                        else
                        {
                            // Incorrect password
                            retryAttempts--;
                            WrongAttemptMsgBox.Text = "Wrong Password!";
                            WrongAttemptMsgBox.Show();
                            UserPassTextBox.Focus();
                            HandleFailedLogin();
                        }
                    }
                    else
                    {
                        // Incorrect student ID
                        retryAttempts--;
                        WrongAttemptMsgBox.Text = "Wrong Student ID!";
                        WrongAttemptMsgBox.Show();
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
            RetryAttemptMsgBox.Text = $"Too many failed attempts. Please wait {delayTimeInSeconds/ 60} minutes before trying again.";
            RetryAttemptMsgBox.Show();
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
                    // Insert into Logs table instead of Notifications table
                    SqlCommand cmd = new SqlCommand("INSERT INTO Logs (StudentID, Action, Timestamp) VALUES (@StudentID, @Action, @Timestamp)", connection);
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    cmd.Parameters.AddWithValue("@Action", $"{firstName} {lastName} has logged in on {computerName}.");
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to log login action: " + ex.Message);
                }
            }
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

    }

  
}
