﻿using System;
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

namespace ComlabSystem
{
    public partial class UserUI : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

        public string AdminName
        {
            set { AdminNameLabel.Text = value; }

        }

        public UserUI()
        {
            InitializeComponent();
            hideUserManagePnl();

            // Attach the resize event to adjust label position on load or resize
            this.Resize += UserUI_Resize2;

            // Attach event handlers for ComboBox actions
            FilterProgramCB.Enter += ShowRequiredFields;
            FilterYearLevelCB.Enter += ShowRequiredFields;

            // Initially hide RequiredPicture and RequiredLabel
            RequiredPicture.Visible = false;
            RequiredLabel.Visible = false;

            // Attach event handlers for ComboBox actions
            ProgramCB.Enter += AddShowRequiredFields;
            YearLevelCB.Enter += AddShowRequiredFields;

            // Initially hide RequiredPicture and RequiredLabel
            AddRequiredPicture.Visible = false;
            AddRequiredLabel.Visible = false;


            //Automatic EMAIL input
            LNameTB.TextChanged += TextBoxes_TextChanged; // Subscribe to the TextChanged event
            FNameTB.TextChanged += TextBoxes_TextChanged; // Subscribe to the TextChanged event
            EditLNameTB.TextChanged += EditTextBoxes_TextChanged; // Subscribe to the TextChanged event
            EditFNameTB.TextChanged += EditTextBoxes_TextChanged; // Subscribe to the TextChanged event

            //Combo box adjuster
            DepartmentCB.SelectedIndexChanged += DepartmentCB_SelectedIndexChanged;


            // Assign event handlers to checkboxes
            FilterStudentCKB.CheckedChanged += FilterStudentCKB_CheckedChanged;
            FilterLNameCKB.CheckedChanged += FilterLNameCKB_CheckedChanged;
            FilterFNameCKB.CheckedChanged += FilterFNameCKB_CheckedChanged;
            FilterEmailCKB.CheckedChanged += FilterEmailCKB_CheckedChanged;
            FilterContactCKB.CheckedChanged += FilterContactCKB_CheckedChanged;
            FilterDepartmentCKB.CheckedChanged += FilterDepartmentCKB_CheckedChanged;
            FilterProgramCKB.CheckedChanged += FilterProgramCKB_CheckedChanged;
            FilterYearLevelCKB.CheckedChanged += FilterYearLevelCKB_CheckedChanged;
            FilterStatusCKB.CheckedChanged += FilterStatusCKB_CheckedChanged;
            FilterLastLoginCKB.CheckedChanged += FilterLastLoginCKB_CheckedChanged;
            FilterDateRegisteredCKB.CheckedChanged += FilterDateRegisteredCKB_CheckedChanged;
            FilterLastUnitCKB.CheckedChanged += FilterLastUnitCKB_CheckedChanged;
            FilterPasswordCBK.CheckedChanged += FilterPasswordCBK_CheckedChanged;



            // Initialize PasswordToolTip
            PasswordToolTIp = new ToolTip
            {
                ToolTipTitle = "Password Guidance",
                ToolTipIcon = ToolTipIcon.Info,
                IsBalloon = true
            };

            // Set the tooltip message
            string tooltipMessage = "If you leave the password field empty, a random 8-character password will be generated automatically.";

            // Associate the tooltip with the password fields
            PasswordToolTIp.SetToolTip(PasswordTip, tooltipMessage);
            

            // Initialize EditStudentIDTBTT
            EditStudentIDTBTT = new ToolTip
            {
                ToolTipTitle = "Student ID Guidance",
                ToolTipIcon = ToolTipIcon.Info,
                IsBalloon = true
            };

            // Set the tooltip message
            string tooltipMessage2 = "Enter the Student ID and press Enter to auto-fill the other fields.";

            // Associate the tooltip with the Student ID field
            EditStudentIDTBTT.SetToolTip(EditStudentIDTip, tooltipMessage2);


        }


        private void UserUI_Load(object sender, EventArgs e)
        {
            NoArchiveListLabel.Visible = false;
            //LoadArchivedUserListData(); // Reload the archived user list

            AdjustNoArchiveListLabelPosition();

            UserFilterPnl.Visible = false;
            LoadDepartments();
            LoadEditDepartments();
            LoadFilterDepartments();
            UserListDGV.BringToFront();
            UserListPrintDGVFUnc();

            //Print
            PrintExcel.BringToFront();
            PrintLink.BringToFront();
            guna2Panel2.BringToFront();


            LoadUserListData();
            LoadUserCountFromDatabase();

        }



        private void hideallpanel()
        {
            UserFilterPnl.Visible = false;
            UserFilterToggleBtm.Checked = false;
            AddUserBtm.Checked = false;
            EditUserBtm.Checked = false;
            hideUserManagePnl();
        }


        private void UserUI_Resize2(object sender, EventArgs e)
        {
            AdjustNoArchiveListLabelPosition();
        }

        private void AdjustNoArchiveListLabelPosition()
        {
            if (this.ParentForm != null && this.ParentForm.WindowState == FormWindowState.Maximized)
            {
                // Full-screen position
                NoArchiveListLabel.Location = new Point(506, 300);
            }
            else
            {
                // Non-full-screen position
                NoArchiveListLabel.Location = new Point(360, 250);
            }
        }
    



    //User Counts label
    private void LoadUserCountFromDatabase()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string countQuery = "SELECT COUNT(*) FROM UserList"; // Exclude archived users if necessary

                using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                {
                    try
                    {
                        connection.Open();
                        int userCount = (int)countCommand.ExecuteScalar(); // Execute the count query
                        UpdateUserCountLabel(userCount); // Update the label with the user count
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error retrieving user count: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
            // Method to update the UserListCountsL label based on the count
            private void UpdateUserCountLabel(int count)
        {
            UserListCountsL.Text = count.ToString("D2"); // Display as two digits if count is less than 10

            // Adjust font size based on count
            if (count < 100)
            {
                UserListCountsL.Location = new Point(3, -11);
                guna2CirclePictureBox1.Location = new Point(86, 18);
                label1.Location = new Point(116, 28);
            }
            else if (count >= 100 && count < 1000)
            {
                UserListCountsL.Location = new Point(3, -10);
                guna2CirclePictureBox1.Location = new Point(120, 18);
                label1.Location = new Point(150, 28);
            }
            else if (count >= 1000)
            {
                UserListCountsL.Location = new Point(3, -10);
                guna2CirclePictureBox1.Location = new Point(185, 28);
                label1.Location = new Point(116, 30);
            }
        }



     





        //UserDataGridView LIST
        private void LoadUserListData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Query to retrieve all user details excluding those with ArchiveStatus = 'Archived'
                string query = @"
SELECT 
    u.StudentID AS [Student ID],
    u.UPassword AS [Password], 
    u.LastName AS [Last Name],
    u.FirstName AS [First Name],
    u.Status,
    u.UnitUsed AS [Unit Used],
    u.DateLastLogout AS [Date Last Logout],
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered]
FROM UserList u
JOIN Department d ON u.DepartmentID = d.DepartmentID
JOIN Programs p ON u.ProgramID = p.ProgramID
JOIN YearLevels y ON u.YearLevelID = y.YearLevelID
WHERE u.ArchiveStatus = 'Active'"; // Exclude archived users

                // Query to count the total number of non-archived users
                string countQuery = "SELECT COUNT(*) FROM UserList WHERE ArchiveStatus <> 'Archived'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();

                        // Load user data into the DataTable
                        adapter.Fill(dataTable);
                        UserListDGV.DataSource = dataTable;

                        // Get user count excluding archived users
                        using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                        {
                            int userCount = (int)countCommand.ExecuteScalar(); // Execute the count query
                            UpdateUserCountLabel(userCount); // Update the label with the user count
                        }

                        // Set all columns to use AllCells mode and wrap text
                        foreach (DataGridViewColumn column in UserListDGV.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Check if Edit button column exists, if not, add it
                        if (!UserListDGV.Columns.Contains("EditBC"))
                        {
                            DataGridViewImageColumn editColumn = new DataGridViewImageColumn
                            {
                                Name = "EditBC",
                                HeaderText = "Edit", // Empty header
                                Image = ResizeImage(Properties.Resources.ColoredEditICON2, 30, 30), // Resized image
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                                Width = 25
                            };
                            UserListDGV.Columns.Add(editColumn);
                        }

                        // Check if Archive button column exists, if not, add it
                        if (!UserListDGV.Columns.Contains("ArchiveBC"))
                        {
                            DataGridViewImageColumn archiveColumn = new DataGridViewImageColumn
                            {
                                Name = "ArchiveBC",
                                HeaderText = "Archive", // Empty header
                                Image = ResizeImage(Properties.Resources.ColoredArchiveICON2, 30, 30), // Resized image
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                                Width = 25
                            };
                            UserListDGV.Columns.Add(archiveColumn);
                        }

                        // Change the font color of the "Status" column based on its value
                        foreach (DataGridViewRow row in UserListDGV.Rows)
                        {
                            string status = row.Cells["Status"].Value?.ToString() ?? string.Empty; // Get status value

                            if (status.Equals("Online", StringComparison.OrdinalIgnoreCase))
                            {
                                row.Cells["Status"].Style.ForeColor = Color.FromArgb(45, 198, 109); // RGB (45, 198, 109)
                            }
                            else if (status.Equals("Offline", StringComparison.OrdinalIgnoreCase))
                            {
                                row.Cells["Status"].Style.ForeColor = Color.FromArgb(60, 60, 60); // RGB (60, 60, 60)
                            }
                        }


                        // Refresh layout after setting modes
                        UserListDGV.AutoResizeColumns();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        //ARCHIVE A USER
        private void UserListDGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the row is valid and clicked on a cell, not a header
            if (e.RowIndex >= 0)
            {
                // Handle Edit button click
                if (e.ColumnIndex == UserListDGV.Columns["EditBC"].Index)
                {
                    UserEditPnl.Visible = true; // Make the UserEditPnl visible
                    UserEditPnl.BringToFront(); // Bring it to the front of the UI
                }
            }

            // Check if the clicked column is the ArchiveBC button column
            if (e.RowIndex >= 0 && UserListDGV.Columns.Contains("ArchiveBC") && UserListDGV.Columns[e.ColumnIndex].Name == "ArchiveBC")
            {
                hideallpanel();
                cancelAddingUser();

                // Get the StudentID of the user being archived
                string studentID = UserListDGV.Rows[e.RowIndex].Cells["Student ID"].Value.ToString();

                // Get the FirstName and LastName of the user
                string firstName = UserListDGV.Rows[e.RowIndex].Cells["First Name"].Value.ToString();
                string lastName = UserListDGV.Rows[e.RowIndex].Cells["Last Name"].Value.ToString();
                    
                // Get the AdminName from the label
                string adminName = AdminNameLabel.Text;

                // Call the ArchiveUser function to set ArchiveStatus to 'Inactive' and insert notification
                ArchiveUserToInactive(studentID, firstName, lastName, adminName);
            }
        }
        private void ArchiveUserToInactive(string studentID, string firstName, string lastName, string adminName)
        {
            // Confirmation message box
            DialogResult result = MessageBox.Show($"Are you sure you want to archive this user?", "Confirm Inactivation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Update user ArchiveStatus to Inactive
                    string query = "UPDATE UserList SET ArchiveStatus = 'Inactive' WHERE StudentID = @StudentID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentID", studentID);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Insert a notification into the Notifications table
                            InsertNotification(adminName, studentID, firstName, lastName);

                            MessageBox.Show("User marked as Inactive successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadUserListData(); // Reload the user list
                            LoadUserCountFromDatabase();
                        }
                        else
                        {
                            MessageBox.Show("Failed to mark user as Inactive.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void InsertNotification(string adminName, string studentID, string firstName, string lastName)
        {
            // Get current timestamp
            DateTime timestamp = DateTime.Now;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Get the AdminID from AdminList table based on AdminName
                string getAdminIDQuery = "SELECT AdminID FROM AdminList WHERE UserName = @AdminName";
                int adminID = 0;
                using (SqlCommand cmd = new SqlCommand(getAdminIDQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@AdminName", adminName);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        adminID = Convert.ToInt32(result);
                    }
                }

                // Insert the notification into Notifications table
                string insertQuery = @"INSERT INTO Notifications (Message, Timestamp, AdminID, NotificationType, NotificationKind) 
                               VALUES (@Message, @Timestamp, @AdminID, @NotificationType, @NotificationKind)";

                using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                {
                    string message = $"Admin name {adminName} archived user with Student ID {studentID} ({firstName} {lastName}) at {timestamp.ToString("yyyy-MM-dd HH:mm:ss")}";
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                    cmd.Parameters.AddWithValue("@AdminID", adminID);
                    cmd.Parameters.AddWithValue("@NotificationType", "Information");
                    cmd.Parameters.AddWithValue("@NotificationKind", "ArchiveUser");

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Notification inserted successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Failed to insert notification.");
                    }
                }
            }
        }



        private void RefreshUserListData()
        {
            //Clear existing columns
            UserListDGV.Columns.Clear();

            // Reload the user data
            LoadUserListData();
        }
        //adding tool tip to column buttons


        //IMAGE CODE
        private System.Drawing.Image ResizeImage(System.Drawing.Image img, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(img, 0, 0, width, height);
            }
            return resizedImage;
        }

        private void UserUI_Resize(object sender, EventArgs e)
        {
            int leftpadding = 65; // Adjust this value for desired padding
            int buttompadding = 140;

            // Resize the DataGridView to fill the panel with padding
            UserListsGridPNL.Width = UserPNL.Width - leftpadding; // Right padding
            UserListsGridPNL.Height = UserPNL.Height - buttompadding;
        }






        private void hideUserManagePnl()
        {
            UserAddPanel.Visible = false;
            UserEditPnl.Visible = false;
        }

        private void uncheckedBtm()
        {
            EditUserBtm.Checked = false;
            AddUserBtm.Checked = false;
        }

        private void AddUserBtm_Click(object sender, EventArgs e)
        {
            UserFilterToggleBtm.Checked = false;
            UserFilterPnl.Visible = false;
        }


        private void EditUserBtm_Click(object sender, EventArgs e)
        {
            
            UserFilterToggleBtm.Checked = false;
            UserFilterPnl.Visible = false;
            
        }

        private void UserStatisticPanelShow_Click(object sender, EventArgs e)
        {

            UserStatusTBTM.Enabled = false;

            //Prints
            ArchivePrintLink.BringToFront();
            PrintExcelArchive.BringToFront();

            hideUserManagePnl();
            UserFilterToggleBtm.Checked = false;
            uncheckedBtm();
            UserFilterPnl.Visible = false;
            ArchiveUserListDGV.BringToFront();
            UserListManageBtmsPL.Visible= false;
            LoadUserListData(); // Reload the active user list
            LoadArchivedUserListData(); // Reload the archived user list
            ArchiveUserListPrintDGVFUnc();
            cancelAddingUser();
            ApplyFilters();

            LoadUserCountFromDatabase();
        }

        private void UserListPanelShow_Click(object sender, EventArgs e)
        {
            UserStatusTBTM.Enabled = true;

            //Prints
            PrintLink.BringToFront();
            PrintExcel.BringToFront();

            NoArchiveListLabel.Visible = false;
            UserFilterToggleBtm.Checked = false;
            UserFilterPnl.Visible = false;
            UserListManageBtmsPL.Visible = true;
            UserListDGV.BringToFront();
            UserListPrintDGVFUnc();
            LoadUserListData();
            ApplyFilters();

            LoadUserCountFromDatabase();
        }


        private void UserFilterToggleBtm_Click(object sender, EventArgs e)
        {
            if (UserFilterToggleBtm.Checked)
            {
                // When toggle is on, show the filter panel
                UserFilterPnl.Visible = true;
                hideUserManagePnl();
                uncheckedBtm();
                cancelAddingUser();
            }
            else
            {
                // When toggle is off, hide the filter panel
                UserFilterPnl.Visible = false;
            }
        }





        private void SortButton_CheckedChanged(object sender, EventArgs e)
        {
            // Define sort direction based on toggle button state
            var sortDirection = SortButton.Checked
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;

            // Sort UserListDGV if it has data and the Last Name column exists
            if (UserListDGV.Columns.Contains("Last Name") && UserListDGV.Rows.Count > 0)
            {
                UserListDGV.Sort(UserListDGV.Columns["Last Name"], sortDirection);
            }

            // Sort ArchiveUserListDGV if it has data and the Last Name column exists
            if (ArchiveUserListDGV.Columns.Contains("Last Name") && ArchiveUserListDGV.Rows.Count > 0)
            {
                ArchiveUserListDGV.Sort(ArchiveUserListDGV.Columns["Last Name"], sortDirection);
            }

            // Sort UserListPrintDGV if it has data and the Last Name column exists
            if (UserListPrintDGV.Columns.Contains("Last Name") && UserListPrintDGV.Rows.Count > 0)
            {
                UserListPrintDGV.Sort(UserListPrintDGV.Columns["Last Name"], sortDirection);
            }
        }

    




        




        //Manage User Buttoms

        //ADD User


        //COmbo Boxes
        private void LoadDepartments()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT DepartmentName FROM Department";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        DepartmentCB.Items.Add(reader["DepartmentName"].ToString());
                    }
                }
            }
        }

        private void DepartmentCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear and reload ProgramCB and YearLevelCB based on selected department
            ProgramCB.Items.Clear();
            YearLevelCB.Items.Clear();

            AddRequiredPicture.Visible = false;
            AddRequiredLabel.Visible = false;

            if (DepartmentCB.SelectedItem != null)
            {
                string selectedDepartment = DepartmentCB.SelectedItem.ToString().Trim();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Load Programs based on department selection
                    string programQuery = "SELECT ProgramName FROM Programs " +
                                          "INNER JOIN Department ON Programs.DepartmentID = Department.DepartmentID " +
                                          "WHERE Department.DepartmentName = @DepartmentName";
                    using (SqlCommand command = new SqlCommand(programQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentName", selectedDepartment);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            ProgramCB.Items.Add(reader["ProgramName"].ToString());
                        }
                        reader.Close();
                    }

                    // Load Year Levels based on department selection
                    string yearLevelQuery = "SELECT YearLevelName FROM YearLevels " +
                                            "INNER JOIN Department ON YearLevels.DepartmentID = Department.DepartmentID " +
                                            "WHERE Department.DepartmentName = @DepartmentName";
                    using (SqlCommand command = new SqlCommand(yearLevelQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentName", selectedDepartment);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            YearLevelCB.Items.Add(reader["YearLevelName"].ToString());
                        }
                    }
                }
            }
        }

        // Method to check if StudentID already exists
        private bool IsStudentIDExists(SqlConnection connection, string studentID)
        {
            string query = "SELECT COUNT(*) FROM UserList WHERE StudentID = @StudentID";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentID", studentID);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }


        // Method to generate a random password
        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            char[] generatedPassword = new char[length];

            for (int i = 0; i < length; i++)
            {
                generatedPassword[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(generatedPassword);
        }
        private void UserAddBtm_Click(object sender, EventArgs e)
        {
            // Ensure all fields are filled
            if (string.IsNullOrWhiteSpace(LNameTB.Text) || string.IsNullOrWhiteSpace(FNameTB.Text))
            {
                MessageBox.Show("Please enter both Last Name and First Name.");
                return;
            }

            if (DepartmentCB.SelectedItem == null || ProgramCB.SelectedItem == null || YearLevelCB.SelectedItem == null)
            {
                MessageBox.Show("Please select a department, program, and year level.");
                return;
            }

            string studentID = StudIDTB.Text.Trim();
            string selectedDepartment = DepartmentCB.SelectedItem.ToString().Trim();
            string selectedProgram = ProgramCB.SelectedItem.ToString().Trim();
            string selectedYearLevel = YearLevelCB.SelectedItem.ToString().Trim();
            string password = StudentPasswordTB.Text.Trim();

            // Generate a random password if StudentPasswordTB is empty
            if (string.IsNullOrEmpty(password))
            {
                password = GenerateRandomPassword();
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if StudentID is unique
                if (IsStudentIDExists(connection, studentID))
                {
                    MessageBox.Show("The Student ID is already on the list. Please enter a unique Student ID.");
                    return; // Stop execution if Student ID already exists
                }

                // Get DepartmentID
                int departmentId = GetDepartmentID(connection, selectedDepartment);
                if (departmentId == -1)
                {
                    MessageBox.Show("Selected department does not exist.");
                    return;
                }

                // Get ProgramID
                int programId = GetProgramID(connection, selectedProgram);
                if (programId == -1)
                {
                    MessageBox.Show("Selected program does not exist.");
                    return;
                }

                // Get YearLevelID
                int yearLevelId = GetYearLevelID(connection, selectedYearLevel);
                if (yearLevelId == -1)
                {
                    MessageBox.Show("Selected year level does not exist.");
                    return;
                }

                // Insert into UserList
                string insertQuery = "INSERT INTO UserList (StudentID, LastName, FirstName, DepartmentID, ProgramID, YearLevelID, Email, ContactNo, DateRegistered, UPassword) " +
                                     "VALUES (@StudentID, @LastName, @FirstName, @DepartmentID, @ProgramID, @YearLevelID, @Email, @ContactNo, @DateRegistered, @UPassword)";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@StudentID", studentID);
                    command.Parameters.AddWithValue("@LastName", LNameTB.Text.Trim());
                    command.Parameters.AddWithValue("@FirstName", FNameTB.Text.Trim());
                    command.Parameters.AddWithValue("@DepartmentID", departmentId);
                    command.Parameters.AddWithValue("@ProgramID", programId);
                    command.Parameters.AddWithValue("@YearLevelID", yearLevelId);
                    command.Parameters.AddWithValue("@Email", EmailTB.Text.Trim());
                    command.Parameters.AddWithValue("@ContactNo", ContactTB.Text.Trim());
                    command.Parameters.AddWithValue("@DateRegistered", DateTime.Now);
                    command.Parameters.AddWithValue("@UPassword", password); // Add the password parameter

                    command.ExecuteNonQuery();
                    MessageBox.Show("User added successfully!");

                    // Fetch AdminID from AdminList based on AdminNameLabel.Text
                    string adminUserName = AdminNameLabel.Text; // Assuming AdminNameLabel.Text contains the admin username
                    int adminID = GetAdminID(connection, adminUserName);
                    if (adminID == -1)
                    {
                        MessageBox.Show("Admin not found.");
                        return;
                    }

                    // Insert a new record into Notifications table
                    string notificationQuery = @"
INSERT INTO Notifications (Message, Timestamp, AdminID, NotificationType, NotificationKind) 
VALUES (@Message, @Timestamp, @AdminID, @NotificationType, @NotificationKind)";
                    using (SqlCommand notificationCommand = new SqlCommand(notificationQuery, connection))
                    {
                        string message = $"Admin name {adminUserName} added new user {FNameTB.Text} {LNameTB.Text}, Student ID: {StudIDTB.Text} at {DateTime.Now}";

                        notificationCommand.Parameters.AddWithValue("@Message", message);
                        notificationCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                        notificationCommand.Parameters.AddWithValue("@AdminID", adminID);
                        notificationCommand.Parameters.AddWithValue("@NotificationType", "Information");
                        notificationCommand.Parameters.AddWithValue("@NotificationKind", "AddUser");

                        notificationCommand.ExecuteNonQuery();
                    }

                    // Refresh User list and UI components
                    RefreshUserListData();
                    LoadUserListData();
                    AddUserBtm.Checked = false;
                    hideUserManagePnl();

                    // Clear form fields after adding user
                    StudIDTB.Clear();
                    LNameTB.Clear();
                    FNameTB.Clear();
                    DepartmentCB.SelectedIndex = -1;
                    ProgramCB.Items.Clear();
                    YearLevelCB.Items.Clear();
                    EmailTB.Clear();
                    ContactTB.Clear();
                    StudentPasswordTB.Clear(); // Clear the password textbox
                    LoadUserCountFromDatabase();
                }
            }
        }
        // Method to get AdminID based on the admin's username
        private int GetAdminID(SqlConnection connection, string adminUserName)
        {
            string query = "SELECT AdminID FROM AdminList WHERE UserName = @UserName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserName", adminUserName);

                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }
        private int GetDepartmentID(SqlConnection connection, string departmentName)
        {
            string query = "SELECT DepartmentID FROM Department WHERE DepartmentName = @DepartmentName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@DepartmentName", departmentName);
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        // Method to get ProgramID
        private int GetProgramID(SqlConnection connection, string programName)
        {
            string query = "SELECT ProgramID FROM Programs WHERE ProgramName = @ProgramName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProgramName", programName);
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        // Method to get YearLevelID
        private int GetYearLevelID(SqlConnection connection, string yearLevelName)
        {
            string query = "SELECT YearLevelID FROM YearLevels WHERE YearLevelName = @YearLevelName";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@YearLevelName", yearLevelName);
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        
        //Cancel adding user Functions

        private void cancelAddingUser()
        {
            // Check if all required fields contain text or selected values
            if (!string.IsNullOrWhiteSpace(StudIDTB.Text) ||
                !string.IsNullOrWhiteSpace(StudentPasswordTB.Text) ||
                !string.IsNullOrWhiteSpace(LNameTB.Text) ||
                !string.IsNullOrWhiteSpace(FNameTB.Text) ||
                !string.IsNullOrWhiteSpace(EmailTB.Text) ||
                !string.IsNullOrWhiteSpace(ContactTB.Text) ||
                DepartmentCB.SelectedIndex != -1 ||
                ProgramCB.SelectedIndex != -1 ||
                YearLevelCB.SelectedIndex != -1)
            {
                // Show confirmation dialog
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to cancel adding this user?",
                    "Confirm Cancel",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                // If the user chooses 'Yes', proceed to cancel; otherwise, do nothing
                if (result == DialogResult.Yes)
                {
                    // Clear the textboxes and reset combo boxes
                    StudIDTB.Clear();
                    StudentPasswordTB.Clear();
                    LNameTB.Clear();
                    FNameTB.Clear();
                    EmailTB.Clear();
                    ContactTB.Clear();
                    DepartmentCB.SelectedIndex = -1;
                    ProgramCB.SelectedIndex = -1;
                    YearLevelCB.SelectedIndex = -1;

                    AddUserBtm.Checked = false;
                    UserAddPanel.Visible = false;
                    uncheckedBtm();
                    UserAddPanel.Hide();

                }
                else if (result == DialogResult.No)
                {
                    UserFilterToggleBtm.Checked = false;
                    EditUserBtm.Checked = false;
                    UserListManageBtmsPL.Visible = true;
                    UserListPrintDGV.BringToFront();

                    UserListDGV.BringToFront();
                    UserListPrintDGVFUnc();

                    hideUserManagePnl();
                    EditUserBtm.Checked = false;
                    UserFilterPnl.Visible = false;
                    AddUserBtm.Checked = true;
                    UserAddPanel.Visible = true;
                    UserAddPanel.BringToFront();
                    
                }

            }

        }
        private void AddDraftBtm_Click(object sender, EventArgs e)
        {
            AddUserBtm.Checked = false;
            UserAddPanel.Visible = false;
            uncheckedBtm();
            UserAddPanel.Hide();
            cancelAddingUser();

        }



        //EDIT Button

        //Inputing the EDIT StudentID textbox
        private void EditStudentIDTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string studentID = EditStudentIDTB.Text.Trim();

                if (string.IsNullOrEmpty(studentID))
                    return;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to get user data based on StudentID, including UPassword
                    string query = "SELECT u.LastName, u.FirstName, d.DepartmentName, p.ProgramName, y.YearLevelName, u.Email, u.ContactNo, u.UPassword " +
                                   "FROM UserList u " +
                                   "JOIN Department d ON u.DepartmentID = d.DepartmentID " +
                                   "JOIN Programs p ON u.ProgramID = p.ProgramID " +
                                   "JOIN YearLevels y ON u.YearLevelID = y.YearLevelID " +
                                   "WHERE u.StudentID = @StudentID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentID", studentID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate the textboxes and comboboxes with the retrieved data
                                EditLNameTB.Text = reader["LastName"].ToString();
                                EditFNameTB.Text = reader["FirstName"].ToString();
                                EditDepartmentCB.Text = reader["DepartmentName"].ToString(); // Set by DepartmentName
                                EditProgramCB.Text = reader["ProgramName"].ToString(); // Set by ProgramName
                                EditYearLevelCB.Text = reader["YearLevelName"].ToString(); // Set by YearLevelName
                                EditEmailTB.Text = reader["Email"].ToString();
                                EditContactTB.Text = reader["ContactNo"].ToString();
                                EditStudentPasswordTB.Text = reader["UPassword"]?.ToString();
                            }
                            else
                            {
                                MessageBox.Show("Student ID not found.");
                            }
                        }
                    }
                }

                // Prevents the beep sound on Enter key press in the TextBox
                e.SuppressKeyPress = true;
            }
        }
        //CLicking the row button EDIT
        private void UserListDGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = UserListDGV.Rows[e.RowIndex];
                string studentID = row.Cells["Student ID"].Value?.ToString();

                hideallpanel();
                cancelAddingUser();

                // Check if the Edit button was clicked
                if (UserListDGV.Columns[e.ColumnIndex].Name == "EditBC")
                {
                    // Show the UserEditPnl and bring it to front
                    UserEditPnl.Visible = true;
                    UserEditPnl.BringToFront();
                    EditUserBtm.Checked = true;

                    // Populate the edit fields with the data from the selected row
                    EditStudentIDTB.Text = row.Cells["Student ID"].Value?.ToString();
                    EditLNameTB.Text = row.Cells["Last Name"].Value?.ToString();
                    EditFNameTB.Text = row.Cells["First Name"].Value?.ToString();
                    EditDepartmentCB.Text = row.Cells["Department"].Value?.ToString();
                    EditProgramCB.Text = row.Cells["Program"].Value?.ToString();
                    EditYearLevelCB.Text = row.Cells["Year/Grade Level"].Value?.ToString();
                    EditEmailTB.Text = row.Cells["Email"].Value?.ToString();
                    EditContactTB.Text = row.Cells["Contact"].Value?.ToString();
                    EditStudentPasswordTB.Text = row.Cells["Password"].Value?.ToString();
                }

            }
        }












        // UNARCHIVE CODES

        private void UnarchiveUser(string archiveStudentID, string firstName, string lastName)
        {
            // Confirmation message box
            DialogResult result = MessageBox.Show("Are you sure you want to unarchive this user?", "Confirm Unarchive", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Update user ArchiveStatus back to Active
                    string query = "UPDATE UserList SET ArchiveStatus = 'Active' WHERE StudentID = @StudentID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentID", archiveStudentID);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Fetch the AdminID based on the admin's username (from AdminNameLabel)
                            int adminID = GetAdminID(connection, AdminNameLabel.Text);

                            // Construct the message for the notification
                            string message = $"Admin name {AdminNameLabel.Text} unarchived a user. Student ID: {archiveStudentID}, Name: {firstName} {lastName} at {DateTime.Now}.";

                            // Insert into the Notifications table
                            string notificationQuery = "INSERT INTO Notifications (Message, Timestamp, AdminID, NotificationType, NotificationKind) " +
                                                       "VALUES (@Message, @Timestamp, @AdminID, @NotificationType, @NotificationKind)";

                            using (SqlCommand notificationCommand = new SqlCommand(notificationQuery, connection))
                            {
                                notificationCommand.Parameters.AddWithValue("@Message", message);
                                notificationCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                                notificationCommand.Parameters.AddWithValue("@AdminID", adminID);
                                notificationCommand.Parameters.AddWithValue("@NotificationType", "Information");
                                notificationCommand.Parameters.AddWithValue("@NotificationKind", "UnarchiveUser");

                                notificationCommand.ExecuteNonQuery();
                            }

                            MessageBox.Show("User unarchived successfully and notification recorded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadUserListData(); // Reload the active user list
                            LoadArchivedUserListData(); // Reload the archived user list
                            LoadUserCountFromDatabase();
                        }
                        else
                        {
                            MessageBox.Show("Failed to unarchive user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
        private void ArchiveUserListDGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && ArchiveUserListDGV.Columns[e.ColumnIndex].Name == "UnArchiveUserList")
            {
                hideallpanel();
                cancelAddingUser();
                string archiveStudentID = ArchiveUserListDGV.Rows[e.RowIndex].Cells["Student ID"].Value.ToString();
                string firstName = ArchiveUserListDGV.Rows[e.RowIndex].Cells["First Name"].Value.ToString();
                string lastName = ArchiveUserListDGV.Rows[e.RowIndex].Cells["Last Name"].Value.ToString();

                UnarchiveUser(archiveStudentID, firstName, lastName);
            }
        }
       


        private void LoadArchivedUserListData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
SELECT 
    u.StudentID AS [Student ID],
    u.UPassword AS [Password], 
    u.LastName AS [Last Name],
    u.FirstName AS [First Name],
    u.Status,
    u.UnitUsed AS [Unit Used],
    u.DateLastLogout AS [Date Last Logout],
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered]
FROM UserList u
JOIN Department d ON u.DepartmentID = d.DepartmentID
JOIN Programs p ON u.ProgramID = p.ProgramID
JOIN YearLevels y ON u.YearLevelID = y.YearLevelID
WHERE u.ArchiveStatus = 'Inactive'"; // Filter to show only archived users

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                        {
                            ArchiveUserListDGV.DataSource = dataTable;

                            // Set all columns to use AllCells mode and wrap text
                            foreach (DataGridViewColumn column in ArchiveUserListDGV.Columns)
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                            }

                            // Add Unarchive button column if not present
                            if (ArchiveUserListDGV.Columns["UnArchiveUserList"] == null)
                            {
                                DataGridViewImageColumn unarchiveColumn = new DataGridViewImageColumn
                                {
                                    Name = "UnArchiveUserList",
                                    HeaderText = "Unarchive",
                                    Image = ResizeImage(Properties.Resources.ColoredUnArchiveICON2, 30, 30),
                                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells // Change to AllCells
                                };
                                ArchiveUserListDGV.Columns.Add(unarchiveColumn);
                            }

                            // Ensure the button column is last
                            ArchiveUserListDGV.Columns["UnArchiveUserList"].DisplayIndex = ArchiveUserListDGV.Columns.Count - 1;
                        }
                        else
                        {
                            NoArchiveListLabel.BringToFront();
                            NoArchiveListLabel.Visible = true;

                            ArchiveUserListDGV.DataSource = null; // Clear the DataGridView if no data
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading archived user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }














        //fill the other combo boxes
        private void LoadEditDepartments()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT DepartmentName FROM Department";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        EditDepartmentCB.Items.Add(reader["DepartmentName"].ToString());
                    }
                }
            }
        }
        private void EditDepartmentCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear and reload ProgramCB and YearLevelCB based on selected department
            EditProgramCB.Items.Clear();
            EditYearLevelCB.Items.Clear();

            if (EditDepartmentCB.SelectedItem != null)
            {
                string selectedDepartment = EditDepartmentCB.SelectedItem.ToString().Trim();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Load Programs based on department selection
                    string programQuery = "SELECT ProgramName FROM Programs " +
                                          "INNER JOIN Department ON Programs.DepartmentID = Department.DepartmentID " +
                                          "WHERE Department.DepartmentName = @DepartmentName";
                    using (SqlCommand command = new SqlCommand(programQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentName", selectedDepartment);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            EditProgramCB.Items.Add(reader["ProgramName"].ToString());
                        }
                        reader.Close();
                    }

                    // Load Year Levels based on department selection
                    string yearLevelQuery = "SELECT YearLevelName FROM YearLevels " +
                                            "INNER JOIN Department ON YearLevels.DepartmentID = Department.DepartmentID " +
                                            "WHERE Department.DepartmentName = @DepartmentName";
                    using (SqlCommand command = new SqlCommand(yearLevelQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentName", selectedDepartment);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            EditYearLevelCB.Items.Add(reader["YearLevelName"].ToString());
                        }
                    }
                }
            }
        }
        private void EditUserSaveBtm_Click(object sender, EventArgs e)
        {
            // Check if required fields are filled
            if (string.IsNullOrWhiteSpace(EditLNameTB.Text) ||
                string.IsNullOrWhiteSpace(EditFNameTB.Text) ||
                EditDepartmentCB.SelectedIndex == -1 ||
                EditProgramCB.SelectedIndex == -1 ||
                EditYearLevelCB.SelectedIndex == -1) // Added YearLevelCB check
            {
                MessageBox.Show("Please fill in all required fields: Last Name, First Name, Department, Program, and Year Level.", "Required Fields Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Cancel the execution if required fields are missing
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Get the IDs based on the selected names in the combo boxes
                int departmentID = GetDepartmentID(EditDepartmentCB.SelectedItem.ToString());
                int programID = GetProgramID(EditProgramCB.SelectedItem.ToString());
                int yearLevelID = GetYearLevelID(EditYearLevelCB.SelectedItem.ToString());

                // Generate a random password if EditStudentPasswordTB is empty
                string password = string.IsNullOrWhiteSpace(EditStudentPasswordTB.Text)
                    ? GenerateRandomPassword()
                    : EditStudentPasswordTB.Text;

                // SQL Query to update user information including UPassword
                string query = "UPDATE UserList SET LastName = @LastName, FirstName = @FirstName, DepartmentID = @DepartmentID, " +
                               "ProgramID = @ProgramID, YearLevelID = @YearLevelID, Email = @Email, ContactNo = @ContactNo, " +
                               "UPassword = @UPassword WHERE StudentID = @StudentID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to the command
                    command.Parameters.AddWithValue("@StudentID", EditStudentIDTB.Text);
                    command.Parameters.AddWithValue("@LastName", EditLNameTB.Text);
                    command.Parameters.AddWithValue("@FirstName", EditFNameTB.Text);
                    command.Parameters.AddWithValue("@DepartmentID", departmentID);
                    command.Parameters.AddWithValue("@ProgramID", programID);
                    command.Parameters.AddWithValue("@YearLevelID", yearLevelID);
                    command.Parameters.AddWithValue("@Email", EditEmailTB.Text);
                    command.Parameters.AddWithValue("@ContactNo", EditContactTB.Text);
                    command.Parameters.AddWithValue("@UPassword", password);

                    // Execute the update query
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Fetch the AdminID based on the admin's username (from AdminNameLabel)
                        int adminID = GetAdminID(connection, AdminNameLabel.Text);

                        // Prepare the message for the notification
                        string message = $"Admin name {AdminNameLabel.Text} edited a user. Student ID: {EditStudentIDTB.Text} at {DateTime.Now}.";

                        // Insert into the Notifications table
                        string notificationQuery = "INSERT INTO Notifications (Message, Timestamp, AdminID, NotificationType, NotificationKind) " +
                                                   "VALUES (@Message, @Timestamp, @AdminID, @NotificationType, @NotificationKind)";

                        using (SqlCommand notificationCommand = new SqlCommand(notificationQuery, connection))
                        {
                            notificationCommand.Parameters.AddWithValue("@Message", message);
                            notificationCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                            notificationCommand.Parameters.AddWithValue("@AdminID", adminID);
                            notificationCommand.Parameters.AddWithValue("@NotificationType", "Information");
                            notificationCommand.Parameters.AddWithValue("@NotificationKind", "EditUser");

                            notificationCommand.ExecuteNonQuery();
                        }

                        MessageBox.Show("User information updated successfully and notification recorded.", "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        RefreshUserListData();
                        EditUserBtm.Checked = false;
                        hideUserManagePnl();

                        // Clear input fields
                        EditStudentIDTB.Clear();
                        EditLNameTB.Clear();
                        EditFNameTB.Clear();
                        EditDepartmentCB.SelectedIndex = -1;
                        EditProgramCB.Items.Clear();
                        EditYearLevelCB.Items.Clear();
                        EditEmailTB.Clear();
                        EditContactTB.Clear();
                        EditStudentPasswordTB.Clear();
                        LoadUserListData();

                        LoadUserCountFromDatabase();
                    }
                    else
                    {
                        MessageBox.Show("Student ID not found. Update failed.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }



        private int GetDepartmentID(string departmentName)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT DepartmentID FROM Department WHERE DepartmentName = @DepartmentName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DepartmentName", departmentName);
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1; // Return -1 if not found
                }
            }
        }

        // Method to get ProgramID based on the ProgramName
        private int GetProgramID(string programName)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ProgramID FROM Programs WHERE ProgramName = @ProgramName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProgramName", programName);
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1; // Return -1 if not found
                }
            }
        }

        // Method to get YearLevelID based on the YearLevelName
        private int GetYearLevelID(string yearLevelName)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT YearLevelID FROM YearLevels WHERE YearLevelName = @YearLevelName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@YearLevelName", yearLevelName);
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1; // Return -1 if not found
                }
            }
        }

        private void UserEditCancelBtm_Click(object sender, EventArgs e)
        {
            EditUserBtm.Checked = false;
            uncheckedBtm();
            hideUserManagePnl();
            EditStudentIDTB.Clear();
            EditLNameTB.Clear();
            EditFNameTB.Clear();
            EditDepartmentCB.SelectedIndex = -1;
            EditProgramCB.Items.Clear();
            EditYearLevelCB.Items.Clear();
            EditEmailTB.Clear();
            EditContactTB.Clear();
            EditStudentPasswordTB.Clear();

        }

            


        //AUTO Email input

        //ADD button auto Email Input
        private void TextBoxes_TextChanged(object sender, EventArgs e)
        {
            // Get the last name, trim spaces, and convert to lowercase, remove spaces
            string lname = LNameTB.Text.Trim().ToLower().Replace(" ", "");

            // Get the first name, trim spaces, and remove all spaces, then convert to lowercase
            string fname = FNameTB.Text.Trim().Replace(" ", "").ToLower();

            // Build the email if both fields are not empty
            if (!string.IsNullOrEmpty(lname) && !string.IsNullOrEmpty(fname))
            {
                EmailTB.Text = $"{fname}.{lname}@newbrighton.edu.ph"; // Combine and format the email
            }
            else
            {
                EmailTB.Clear(); // Clear the email field if one of the textboxes is empty
            }
        }
        //EDIT button auto Email Input
        private void EditTextBoxes_TextChanged(object sender, EventArgs e)
        {
            // Get the last name, trim spaces, and convert to lowercase, remove spaces
            string lname = EditLNameTB.Text.Trim().ToLower().Replace(" ", "");

            // Get the first name, trim spaces, and remove all spaces, then convert to lowercase
            string fname = EditFNameTB.Text.Trim().Replace(" ", "").ToLower();

            // Build the email if both fields are not empty
            if (!string.IsNullOrEmpty(lname) && !string.IsNullOrEmpty(fname))
            {
                EditEmailTB.Text = $"{fname}.{lname}@newbrighton.edu.ph"; // Combine and format the email
            }
            else
            {
                EditEmailTB.Clear(); // Clear the email field if one of the textboxes is empty
            }
        }

        /// For FIlter Buttton
        private void LoadFilterDepartments()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT DepartmentName FROM Department";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        FilterDepartmentCB.Items.Add(reader["DepartmentName"].ToString());
                    }
                }
            }
        }


        private void ShowRequiredFields(object sender, EventArgs e)
        {
            // Check if FilterDepartmentCB has no selection (e.g., placeholder or -1)
            if (FilterDepartmentCB.SelectedIndex < 0)
            {
                RequiredPicture.Visible = true;
                RequiredLabel.Visible = true;
            }
        }

        private void AddShowRequiredFields(object sender, EventArgs e)
        {
            // Check if FilterDepartmentCB has no selection (e.g., placeholder or -1)
            if (DepartmentCB.SelectedIndex < 0)
            {
                AddRequiredPicture.Visible = true;
                AddRequiredLabel.Visible = true;
            }
        }




        private void FilterDepartmentCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear and reload ProgramCB and YearLevelCB based on selected department
            FilterProgramCB.Items.Clear();
            FilterYearLevelCB.Items.Clear();

            ApplyFilters();

            RequiredPicture.Visible = false;
            RequiredLabel.Visible = false;

            // Hide RequiredPicture and RequiredLabel when FilterDepartmentCB has a valid selection

            if (FilterDepartmentCB.SelectedItem != null)
            {
                string selectedDepartment = FilterDepartmentCB.SelectedItem.ToString().Trim();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Load Programs based on department selection
                    string programQuery = "SELECT ProgramName FROM Programs " +
                                          "INNER JOIN Department ON Programs.DepartmentID = Department.DepartmentID " +
                                          "WHERE Department.DepartmentName = @DepartmentName";
                    using (SqlCommand command = new SqlCommand(programQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentName", selectedDepartment);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            FilterProgramCB.Items.Add(reader["ProgramName"].ToString());
                        }
                        reader.Close();
                    }

                    // Load Year Levels based on department selection
                    string yearLevelQuery = "SELECT YearLevelName FROM YearLevels " +
                                            "INNER JOIN Department ON YearLevels.DepartmentID = Department.DepartmentID " +
                                            "WHERE Department.DepartmentName = @DepartmentName";
                    using (SqlCommand command = new SqlCommand(yearLevelQuery, connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentName", selectedDepartment);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            FilterYearLevelCB.Items.Add(reader["YearLevelName"].ToString());
                        }
                    }
                }
            }


        }



        //FILTER BUTTONS

        //Clear the filter
        private void ClearFilterCBB_Click(object sender, EventArgs e)
        {
            FilterDepartmentCB.SelectedIndex = -1;
            FilterProgramCB.Items.Clear();
            FilterYearLevelCB.Items.Clear();
            ApplyFilters();

            FilterDefault();
        }

        private void FilterDefault()
        {
            FilterStudentCKB.Checked = true;
            FilterLNameCKB.Checked = true;
            FilterFNameCKB.Checked = true;
            FilterEmailCKB.Checked = true;
            FilterContactCKB.Checked = true;
            FilterDepartmentCKB.Checked = true;
            FilterProgramCKB.Checked = true;
            FilterYearLevelCKB.Checked = true;
            FilterStatusCKB.Checked = true;
            FilterLastLoginCKB.Checked = true;
            FilterDateRegisteredCKB.Checked = true;
            FilterLastUnitCKB.Checked = true;
            FilterPasswordCBK.Checked = true;
        }

        private void FilterDepartmentCBClear_Click(object sender, EventArgs e)
        {
            FilterDepartmentCB.SelectedIndex = -1;
            ApplyFilters();
        }

        private void FilterProgramCBClear_Click(object sender, EventArgs e)
        {
            FilterProgramCB.SelectedIndex = -1;
            ApplyFilters();
        }

        private void FilterYearLevelCBClear_Click(object sender, EventArgs e)
        {
            FilterYearLevelCB.SelectedIndex = -1;
            ApplyFilters();
        }

        //Combo BOX function filter
        private void ApplyFilters()
        {
            StringBuilder filter = new StringBuilder();

            // Filter by Department
            if (FilterDepartmentCB.SelectedIndex != -1)
            {
                string department = FilterDepartmentCB.Text;
                filter.Append($"[Department] = '{department}'");
            }

            // Filter by Program
            if (FilterProgramCB.SelectedIndex != -1)
            {
                if (filter.Length > 0) filter.Append(" AND ");
                string program = FilterProgramCB.Text;
                filter.Append($"[Program] = '{program}'");
            }

            // Filter by Year Level
            if (FilterYearLevelCB.SelectedIndex != -1)
            {
                if (filter.Length > 0) filter.Append(" AND ");
                string yearLevel = FilterYearLevelCB.Text;
                filter.Append($"[Year/Grade Level] = '{yearLevel}'");
            }

            // Check if UserListDGV has a valid DataTable and apply filter if possible
            if (UserListDGV.DataSource is DataTable userTable)
            {
                userTable.DefaultView.RowFilter = filter.ToString();
            }

            // Check if ArchiveUserListDGV has a valid DataTable and apply filter if possible
            if (ArchiveUserListDGV.DataSource is DataTable archiveTable)
            {
                archiveTable.DefaultView.RowFilter = filter.ToString();
            }

            if (UserListPrintDGV.DataSource is DataTable printTable)
            {
                printTable.DefaultView.RowFilter = filter.ToString();
            }
        }


        private void FilterProgramCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FilterDepartmentCB.SelectedIndex == -1)
            {
                RequiredPicture.Visible = true;
                RequiredLabel.Visible = true;
            }
            ApplyFilters();
        }

        private void FilterYearLevelCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FilterDepartmentCB.SelectedIndex == -1)
            {
                RequiredPicture.Visible = true;
                RequiredLabel.Visible = true;
            }
            ApplyFilters();
        }




        //Checkboxes Filter
        // Event handler for checkboxes to control column visibility
        private void FilterStudentCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterLNameCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterFNameCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterEmailCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterContactCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterDepartmentCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterProgramCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterYearLevelCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterStatusCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterLastLoginCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterDateRegisteredCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterLastUnitCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterPasswordCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();




        // Method to update column visibility based on checkbox states
        private void UpdateColumnVisibility()
        {
            // Check if UserListDGV has columns before updating visibility
            if (UserListDGV.Columns.Count > 0)
            {
                UserListDGV.Columns["Student ID"].Visible = FilterStudentCKB.Checked;
                UserListDGV.Columns["Last Name"].Visible = FilterLNameCKB.Checked;
                UserListDGV.Columns["First Name"].Visible = FilterFNameCKB.Checked;
                UserListDGV.Columns["Email"].Visible = FilterEmailCKB.Checked;
                UserListDGV.Columns["Contact"].Visible = FilterContactCKB.Checked;
                UserListDGV.Columns["Department"].Visible = FilterDepartmentCKB.Checked;
                UserListDGV.Columns["Program"].Visible = FilterProgramCKB.Checked;
                UserListDGV.Columns["Year/Grade Level"].Visible = FilterYearLevelCKB.Checked;
                UserListDGV.Columns["Status"].Visible = FilterStatusCKB.Checked;
                UserListDGV.Columns["Date Last Logout"].Visible = FilterLastLoginCKB.Checked;
                UserListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                UserListDGV.Columns["Unit Used"].Visible = FilterLastUnitCKB.Checked;
                UserListDGV.Columns["Password"].Visible = FilterPasswordCBK.Checked;
            }

            // Check if ArchiveUserListDGV has columns before updating visibility
            if (ArchiveUserListDGV.Columns.Count > 0)
            {
                ArchiveUserListDGV.Columns["Student ID"].Visible = FilterStudentCKB.Checked;
                ArchiveUserListDGV.Columns["Last Name"].Visible = FilterLNameCKB.Checked;
                ArchiveUserListDGV.Columns["First Name"].Visible = FilterFNameCKB.Checked;
                ArchiveUserListDGV.Columns["Email"].Visible = FilterEmailCKB.Checked;
                ArchiveUserListDGV.Columns["Contact"].Visible = FilterContactCKB.Checked;
                ArchiveUserListDGV.Columns["Department"].Visible = FilterDepartmentCKB.Checked;
                ArchiveUserListDGV.Columns["Program"].Visible = FilterProgramCKB.Checked;
                ArchiveUserListDGV.Columns["Year/Grade Level"].Visible = FilterYearLevelCKB.Checked;
                ArchiveUserListDGV.Columns["Status"].Visible = FilterStatusCKB.Checked;
                ArchiveUserListDGV.Columns["Date Last Logout"].Visible = FilterLastLoginCKB.Checked;
                ArchiveUserListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                ArchiveUserListDGV.Columns["Unit Used"].Visible = FilterLastUnitCKB.Checked;
                ArchiveUserListDGV.Columns["Password"].Visible = FilterPasswordCBK.Checked;
            }

            // Check if UserListPrintDGV has columns before updating visibility
            if (UserListPrintDGV.Columns.Count > 0)
            {
                UserListPrintDGV.Columns["Student ID"].Visible = FilterStudentCKB.Checked;
                UserListPrintDGV.Columns["Last Name"].Visible = FilterLNameCKB.Checked;
                UserListPrintDGV.Columns["First Name"].Visible = FilterFNameCKB.Checked;
                UserListPrintDGV.Columns["Email"].Visible = FilterEmailCKB.Checked;
                UserListPrintDGV.Columns["Contact"].Visible = FilterContactCKB.Checked;
                UserListPrintDGV.Columns["Department"].Visible = FilterDepartmentCKB.Checked;
                UserListPrintDGV.Columns["Program"].Visible = FilterProgramCKB.Checked;
                UserListPrintDGV.Columns["Year/Grade Level"].Visible = FilterYearLevelCKB.Checked;
                UserListPrintDGV.Columns["Status"].Visible = FilterStatusCKB.Checked;
                UserListPrintDGV.Columns["Date Last Logout"].Visible = FilterLastLoginCKB.Checked;
                UserListPrintDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                UserListPrintDGV.Columns["Unit Used"].Visible = FilterLastUnitCKB.Checked;
                UserListPrintDGV.Columns["Password"].Visible = FilterPasswordCBK.Checked;
            }
        }



        // Search bar event handler
        private void UserSearchBar_TextChanged(object sender, EventArgs e)
        {
            string searchText = UserSearchBar.Text;
            ApplySearchFilter(UserListDGV, searchText);
            ApplySearchFilter(ArchiveUserListDGV, searchText);
            ApplySearchFilter(UserListPrintDGV, searchText);

            UserFilterToggleBtm.Checked = false;
            UserFilterPnl.Visible = false;
        }

        // Generalized method to apply search filter to any DataGridView
        private void ApplySearchFilter(DataGridView gridView, string searchText)
        {
            if (gridView.DataSource is DataTable dataTable)
            {
                // Build filter expression for visible columns with valid DataPropertyName
                var filterExpression = new List<string>();

                foreach (DataGridViewColumn column in gridView.Columns)
                {
                    // Only consider visible columns and columns with a valid DataPropertyName for filtering
                    if (column.Visible && !string.IsNullOrEmpty(column.DataPropertyName))
                    {
                        string columnName = column.DataPropertyName; // Get the bound column name

                        // Check the data type of the column
                        Type columnType = dataTable.Columns[columnName].DataType;

                        // Only use LIKE for string columns
                        if (columnType == typeof(string))
                        {
                            filterExpression.Add($"[{columnName}] LIKE '%{searchText}%'");
                        }
                        else if (columnType == typeof(DateTime))
                        {
                            // Optionally, you can implement a different filter for DateTime columns,
                            // but for now we will skip it in this example
                            continue; // Skip DateTime columns
                        }
                        // You can add other types as necessary, but for now we will only filter strings
                    }
                }

                // Apply the filter expression to the DataTable
                string finalFilter = string.Join(" OR ", filterExpression);

                // Apply filter only if there are columns to filter
                if (filterExpression.Count > 0)
                {
                    dataTable.DefaultView.RowFilter = finalFilter;
                }
                else
                {
                    dataTable.DefaultView.RowFilter = string.Empty; // Clear filter if no columns
                }
            }
        }


        private void PasswordTip_Click(object sender, EventArgs e)
        {

        }

        private void SortButton_Click(object sender, EventArgs e)
        {


        }

        private void PrintToogleBtm_Click(object sender, EventArgs e)
        {
            // Set font for DataGridView headers before printing
            UserListPrintDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);  // Set bold font for column headers
            UserListPrintDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center-align headers

            // Create DGVPrinter instance
            Font extraBoldFont = new Font("Montserrat ExtraBold", 36);  // Title font
            Font regularFont = new Font("Segoe UI", 10, FontStyle.Regular);  // Regular font for subtitle and footer

            DGVPrinter printer = new DGVPrinter();

            // Title Settings
            printer.Title = "USER LIST";
            printer.TitleSpacing = 10;
            printer.TitleFont = extraBoldFont;
            printer.TitleColor = Color.FromArgb(255,255,255);
            printer.TitleBackground = new SolidBrush(Color.FromArgb(128, 0, 0));

            // Subtitle Settings
            printer.SubTitle = string.Format("User List Overview\nShowing all active users in the system\n({0})", DateTime.Now.Date.ToLongDateString());
            printer.SubTitleFont = regularFont;
            printer.SubTitleAlignment = StringAlignment.Center;
            printer.SubTitleColor = Color.DimGray;
            printer.SubTitleSpacing = 7;

            // Page Number Settings
            printer.PageNumbers = true;
            printer.PageNumberInHeader = false;
            printer.PageNumberAlignment = StringAlignment.Far;
            printer.PageNumberFont = new Font("Segoe UI", 7, FontStyle.Bold);
            printer.PageNumberColor = Color.DimGray;
            printer.ShowTotalPageNumber = true;
            printer.PageNumberOnSeparateLine = true;

            // Column Proportions
            printer.PorportionalColumns = true;

            // Footer Settings
            printer.Footer = "NBSPI COMPUTER LABORATORY MONITORING SYSTEM";
            printer.FooterFont = regularFont;
            printer.FooterSpacing = 16;
            printer.FooterAlignment = StringAlignment.Center;
            printer.FooterBackground = new SolidBrush(Color.Gray);
            printer.FooterColor = Color.White;

            // Print Margins
            printer.PrintMargins = new System.Drawing.Printing.Margins(50, 50, 50, 50);

            // Print Preview of DataGridView
            printer.PrintPreviewDataGridView(UserListPrintDGV);
        }

        private void AddUserBtm_CheckedChanged(object sender, EventArgs e)
        {
            if (AddUserBtm.Checked == true)
            {

                hideUserManagePnl();
                UserFilterPnl.Visible = false;
                UserAddPanel.Visible = true;
                UserAddPanel.BringToFront();
                EditUserBtm.Checked = false;
            }
            else { UserAddPanel.Visible = false; }
        }

        private void EditUserBtm_CheckedChanged(object sender, EventArgs e)
        {
            if (EditUserBtm.Checked == true)
            {
                hideUserManagePnl();
                UserEditPnl.Visible = true;
                UserFilterPnl.Visible = false;
                UserEditPnl.BringToFront();
                AddUserBtm.Checked = false;

                // Check if all required fields contain text or selected values
                if (!string.IsNullOrWhiteSpace(StudIDTB.Text) ||
                    !string.IsNullOrWhiteSpace(StudentPasswordTB.Text) ||
                    !string.IsNullOrWhiteSpace(LNameTB.Text) ||
                    !string.IsNullOrWhiteSpace(FNameTB.Text) ||
                    !string.IsNullOrWhiteSpace(EmailTB.Text) ||
                    !string.IsNullOrWhiteSpace(ContactTB.Text) ||
                    DepartmentCB.SelectedIndex != -1 ||
                    ProgramCB.SelectedIndex != -1 ||
                    YearLevelCB.SelectedIndex != -1)
                {
                    // Show confirmation dialog
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to cancel adding this user?",
                        "Confirm Cancel",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    // If the user chooses 'Yes', proceed to cancel; otherwise, do nothing
                    if (result == DialogResult.Yes)
                    {
                        // Clear the textboxes and reset combo boxes
                        StudIDTB.Clear();
                        StudentPasswordTB.Clear();
                        LNameTB.Clear();
                        FNameTB.Clear();
                        EmailTB.Clear();
                        ContactTB.Clear();
                        DepartmentCB.SelectedIndex = -1;
                        ProgramCB.SelectedIndex = -1;
                        YearLevelCB.SelectedIndex = -1;

                        AddUserBtm.Checked = false;
                        UserAddPanel.Visible = false;
                        uncheckedBtm();
                        UserAddPanel.Hide();

                        hideUserManagePnl();
                        UserEditPnl.Visible = true;
                        UserFilterPnl.Visible = false;
                        UserEditPnl.BringToFront();
                        AddUserBtm.Checked = false;
                        EditUserBtm.Checked = true;

                    }
                    else if (result == DialogResult.No)
                    {
                        UserFilterToggleBtm.Checked = false;
                        EditUserBtm.Checked = false;
                        UserListManageBtmsPL.Visible = true;
                        UserListPrintDGV.BringToFront();

                        UserListDGV.BringToFront();
                        UserListPrintDGVFUnc();

                        hideUserManagePnl();
                        EditUserBtm.Checked = false;
                        UserFilterPnl.Visible = false;
                        AddUserBtm.Checked = true;
                        UserAddPanel.Visible = true;
                        UserAddPanel.BringToFront();

                    }

                }
            }
            else { UserEditPnl.Visible = false; }
        }


        //to be arrange
        //Add button


        private void UserListPrintDGVFUnc()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Query to retrieve all user details excluding those with ArchiveStatus = 'Archived'
                string query = @"
SELECT 
    u.StudentID AS [Student ID],
    u.UPassword AS [Password], 
    u.LastName AS [Last Name],
    u.FirstName AS [First Name],
    u.Status,
    u.UnitUsed AS [Unit Used],
    u.DateLastLogout AS [Date Last Logout],
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered]
FROM UserList u
JOIN Department d ON u.DepartmentID = d.DepartmentID
JOIN Programs p ON u.ProgramID = p.ProgramID
JOIN YearLevels y ON u.YearLevelID = y.YearLevelID
WHERE u.ArchiveStatus <> 'Archived'"; // Exclude archived users

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();

                        // Load user data into the DataTable
                        adapter.Fill(dataTable);
                        UserListPrintDGV.DataSource = dataTable;

                        // Set all columns to use AllCells mode and wrap text
                        foreach (DataGridViewColumn column in UserListPrintDGV.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Loop through the rows to change font color based on the Status
                        foreach (DataGridViewRow row in UserListPrintDGV.Rows)
                        {
                            string status = row.Cells["Status"].Value.ToString(); // Assuming "Status" column is named "Status"
                            if (status == "Online")
                            {
                                // Change the font color for "Online" status to RGB(45, 198, 109)
                                row.Cells["Status"].Style.ForeColor = Color.FromArgb(45, 198, 109);
                            }
                            else if (status == "Offline")
                            {
                                // Change the font color for "Offline" status to RGB(60, 60, 60)
                                row.Cells["Status"].Style.ForeColor = Color.FromArgb(60, 60, 60);
                            }
                        }

                        // Refresh layout after setting modes
                        UserListPrintDGV.AutoResizeColumns();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }



        private void ArchiveUserListPrintDGVFUnc()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
SELECT 
    u.StudentID AS [Student ID],
    u.UPassword AS [Password], 
    u.LastName AS [Last Name],
    u.FirstName AS [First Name],
    u.Status,
    u.UnitUsed AS [Unit Used],
    u.DateLastLogout AS [Date Last Logout],
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered]
FROM UserList u
JOIN Department d ON u.DepartmentID = d.DepartmentID
JOIN Programs p ON u.ProgramID = p.ProgramID
JOIN YearLevels y ON u.YearLevelID = y.YearLevelID
WHERE u.ArchiveStatus = 'Archived'";  // Filter to show only active users

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                        {
                            UserListPrintDGV.DataSource = dataTable;

                            // Set all columns to use AllCells mode and wrap text
                            foreach (DataGridViewColumn column in UserListPrintDGV.Columns)
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                            }

                           
                        }
                        else
                        {
                            UserListPrintDGV.DataSource = null; // Clear the DataGridView if no data
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SortButton_MouseHover(object sender, EventArgs e)
        {
            SortToolTip.Show("Click to sort the Last Name column A-Z or Z-A.", SortButton);
        }



        //SHowing the Online and Offline
        private void UserStatusTBTM_CheckedChanged(object sender, EventArgs e)
        {
            if (UserStatusTBTM.Checked)
            {
                // When checked, sort "Online" statuses at the top
                SortDataGridViews("Online");
                UserStatusTBTM.Text = "Online";
            }
            else
            {
                // When unchecked, sort "Offline" statuses at the top
                SortDataGridViews("Offline");
                UserStatusTBTM.Text = "Offline";
            }
        }

        private void SortDataGridViews(string status)
        {
            // Apply sorting for UserListDGV
            ApplySortingToDataGridView(UserListDGV, status);

            // Apply sorting for ArchiveUserListDGV
            ApplySortingToDataGridView(UserListPrintDGV, status);
        }

        private void ApplySortingToDataGridView(DataGridView dgv, string status)
        {
            if (dgv.Columns.Contains("Status"))
            {
                // Assuming the "Status" column is named "Status" in all DataGridViews
                if (status == "Online")
                {
                    // Sort "Online" first
                    dgv.Sort(dgv.Columns["Status"], ListSortDirection.Descending); // Online first
                }
                else
                {
                    // Sort "Offline" first
                    dgv.Sort(dgv.Columns["Status"], ListSortDirection.Ascending); // Offline first
                }

            }
            else
            {
                // Handle the case where the "Status" column does not exist
                MessageBox.Show("The 'Status' column does not exist in the DataGridView.");
            }
        }

        private void UserListDGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Check if the column is the "Status" column (usually by index or name)
            if (UserListDGV.Columns[e.ColumnIndex].Name == "Status")
            {
                // Check if the value is "Online" or "Offline"
                if (e.Value != null)
                {
                    string status = e.Value.ToString();

                    if (status.Equals("Online", StringComparison.OrdinalIgnoreCase))
                    {
                        // Set the font color to RGB(45, 198, 109) for "Online"
                        e.CellStyle.ForeColor = Color.FromArgb(45, 198, 109);
                    }
                    else if (status.Equals("Offline", StringComparison.OrdinalIgnoreCase))
                    {
                        // Set the font color to RGB(60, 60, 60) for "Offline"
                        e.CellStyle.ForeColor = Color.FromArgb(60, 60, 60);
                    }
                }
            }
        }


        private void PrintLink_Click(object sender, EventArgs e)
        {
            if (UserListPrintDGV.Rows.Count > 0) { 
                // Set font for DataGridView headers before printing
                UserListPrintDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);  // Set bold font for column headers
            UserListPrintDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center-align headers

            // Create DGVPrinter instance
            Font extraBoldFont = new Font("Montserrat ExtraBold", 36);  // Title font
            Font regularFont = new Font("Segoe UI", 10, FontStyle.Regular);  // Regular font for subtitle and footer

            DGVPrinter printer = new DGVPrinter();

            // Title Settings
            printer.Title = "USER LIST";
            printer.TitleSpacing = 10;
            printer.TitleFont = extraBoldFont;
            printer.TitleColor = Color.FromArgb(255, 255, 255);
            printer.TitleBackground = new SolidBrush(Color.FromArgb(128, 0, 0));

            // Subtitle Settings
            printer.SubTitle = string.Format("User List Overview\nShowing all active users in the system\n({0})", DateTime.Now.Date.ToLongDateString());
            printer.SubTitleFont = regularFont;
            printer.SubTitleAlignment = StringAlignment.Center;
            printer.SubTitleColor = Color.DimGray;
            printer.SubTitleSpacing = 7;

            // Page Number Settings
            printer.PageNumbers = true;
            printer.PageNumberInHeader = false;
            printer.PageNumberAlignment = StringAlignment.Far;
            printer.PageNumberFont = new Font("Segoe UI", 7, FontStyle.Bold);
            printer.PageNumberColor = Color.DimGray;
            printer.ShowTotalPageNumber = true;
            printer.PageNumberOnSeparateLine = true;

            // Column Proportions
            printer.PorportionalColumns = true;

            // Footer Settings
            printer.Footer = "NBSPI COMPUTER LABORATORY MONITORING SYSTEM";
            printer.FooterFont = regularFont;
            printer.FooterSpacing = 16;
            printer.FooterAlignment = StringAlignment.Center;
            printer.FooterBackground = new SolidBrush(Color.Gray);
            printer.FooterColor = Color.White;

            // Print Margins
            printer.PrintMargins = new System.Drawing.Printing.Margins(50, 50, 50, 50);

            // Print Preview of DataGridView
            printer.PrintPreviewDataGridView(UserListPrintDGV);
        }
            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        private void ArchivePrintLink_Click(object sender, EventArgs e)
        {
            if (UserListPrintDGV.Rows.Count > 0)
            {
                // Set font for DataGridView headers before printing
                UserListPrintDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);  // Set bold font for column headers
            UserListPrintDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center-align headers

            // Create DGVPrinter instance
            Font extraBoldFont = new Font("Montserrat ExtraBold", 36);  // Title font
            Font regularFont = new Font("Segoe UI", 10, FontStyle.Regular);  // Regular font for subtitle and footer

            DGVPrinter printer = new DGVPrinter();

            // Title Settings
            printer.Title = "ARCHIVED USER LIST";
            printer.TitleSpacing = 10;
            printer.TitleFont = extraBoldFont;
            printer.TitleColor = Color.FromArgb(255, 255, 255);
            printer.TitleBackground = new SolidBrush(Color.FromArgb(128, 0, 0));

            // Subtitle Settings
            printer.SubTitle = string.Format("Archived User List Overview\nShowing all archived users in the system\n({0})", DateTime.Now.Date.ToLongDateString());
            printer.SubTitleFont = regularFont;
            printer.SubTitleAlignment = StringAlignment.Center;
            printer.SubTitleColor = Color.DimGray;
            printer.SubTitleSpacing = 7;

            // Page Number Settings
            printer.PageNumbers = true;
            printer.PageNumberInHeader = false;
            printer.PageNumberAlignment = StringAlignment.Far;
            printer.PageNumberFont = new Font("Segoe UI", 7, FontStyle.Bold);
            printer.PageNumberColor = Color.DimGray;
            printer.ShowTotalPageNumber = true;
            printer.PageNumberOnSeparateLine = true;

            // Column Proportions
            printer.PorportionalColumns = true;

            // Footer Settings
            printer.Footer = "NBSPI COMPUTER LABORATORY MONITORING SYSTEM";
            printer.FooterFont = regularFont;
            printer.FooterSpacing = 16;
            printer.FooterAlignment = StringAlignment.Center;
            printer.FooterBackground = new SolidBrush(Color.Gray);
            printer.FooterColor = Color.White;

            // Print Margins
            printer.PrintMargins = new System.Drawing.Printing.Margins(50, 50, 50, 50);

            // Print Preview of DataGridView
            printer.PrintPreviewDataGridView(UserListPrintDGV);
            }
            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintExcelArchive_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (UserListPrintDGV.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.ApplicationClass MExcel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                MExcel.Application.Workbooks.Add(Type.Missing);
                for (int i = 1; i < UserListPrintDGV.Columns.Count + 1; i++)
                {
                    MExcel.Cells[1, i] = UserListPrintDGV.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < UserListPrintDGV.Rows.Count; i++)
                {
                    for (int j = 0; j < UserListPrintDGV.Columns.Count; j++)
                    {
                        MExcel.Cells[i + 2, j + 1] = UserListPrintDGV.Rows[i].Cells[j].Value.ToString();
                    }
                }
                MExcel.Columns.AutoFit();
                MExcel.Rows.AutoFit();
                MExcel.Columns.Font.Size = 12;
                MExcel.Visible = true;
            }
            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintExcel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (UserListPrintDGV.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.ApplicationClass MExcel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                MExcel.Application.Workbooks.Add(Type.Missing);
                for (int i = 1; i < UserListPrintDGV.Columns.Count + 1; i++)
                {
                    MExcel.Cells[1, i] = UserListPrintDGV.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < UserListPrintDGV.Rows.Count; i++)
                {
                    for (int j = 0; j < UserListPrintDGV.Columns.Count; j++)
                    {
                        MExcel.Cells[i + 2, j + 1] = UserListPrintDGV.Rows[i].Cells[j].Value.ToString();
                    }
                }
                MExcel.Columns.AutoFit();
                MExcel.Rows.AutoFit();
                MExcel.Columns.Font.Size = 12;
                MExcel.Visible = true;
            }
            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }  
}
