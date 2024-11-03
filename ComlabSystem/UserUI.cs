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

namespace ComlabSystem
{
    public partial class UserUI : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
        public UserUI()
        {
            InitializeComponent();
            hideUserManagePnl();

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
            FilterTotalHourCKB.CheckedChanged += FilterTotalHourCKB_CheckedChanged;
            FilterLastUnitCKB.CheckedChanged += FilterLastUnitCKB_CheckedChanged;
            FilterPasswordCBK.CheckedChanged += FilterPasswordCBK_CheckedChanged;

        }


        private void UserUI_Load(object sender, EventArgs e)
        {

            UserFilterPnl.Visible = false;
            LoadDepartments();
            LoadUserListData();
            LoadEditDepartments();
            LoadFilterDepartments();

        }

        //UserDataGridView LIST
        private void LoadUserListData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
SELECT 
    u.StudentID AS [Student ID],
    u.LastName AS [Last Name],
    u.FirstName AS [First Name],
    u.UPassword AS [Password],  -- Use the correct column name here
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Status,
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered],
    u.LastLogin AS [Last Login],
    u.TotalHoursUsed AS [Total Hours Used],
    u.LastUnitUsed AS [Last Unit Used]
FROM UserList u
JOIN Department d ON u.DepartmentID = d.DepartmentID
JOIN Programs p ON u.ProgramID = p.ProgramID
JOIN YearLevels y ON u.YearLevelID = y.YearLevelID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();
                        adapter.Fill(dataTable);
                        UserListDGV.DataSource = dataTable;

                        // Set all columns to use AllCells mode and wrap text
                        foreach (DataGridViewColumn column in UserListDGV.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Adding Edit button column
                        DataGridViewImageColumn editColumn = new DataGridViewImageColumn
                        {
                            Name = "EditBC",
                            HeaderText = "", // Empty header
                            Image = ResizeImage(Properties.Resources.pencil, 20, 20), // Resized image
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                            Width = 25
                        };
                        UserListDGV.Columns.Add(editColumn);

                        // Adding Archive button column
                        DataGridViewImageColumn archiveColumn = new DataGridViewImageColumn
                        {
                            Name = "ArchiveBC",
                            HeaderText = "", // Empty header
                            Image = ResizeImage(Properties.Resources.archive, 20, 20), // Resized image
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                            Width = 25
                        };
                        UserListDGV.Columns.Add(archiveColumn);

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

        private void RefreshUserListData()
        {
            // Clear existing columns
            UserListDGV.Columns.Clear();

            // Reload the user data
            LoadUserListData();
        }
        //adding tool tip to column buttons
        private void UserListDGV_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            // Check if the hovered cell is in the EditBC or ArchiveBC column
            if (e.ColumnIndex == UserListDGV.Columns["EditBC"].Index && e.RowIndex >= 0)
            {
                e.ToolTipText = "Click to edit this user.";
            }
            else if (e.ColumnIndex == UserListDGV.Columns["ArchiveBC"].Index && e.RowIndex >= 0)
            {
                e.ToolTipText = "Click to archive this user.";
            }
            else
            {
                e.ToolTipText = string.Empty; // Clear tooltip for other cells
            }
        }


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

        private void ViewUserListBtm_Click(object sender, EventArgs e)
        {
            hideUserManagePnl();
        }

        private void AddUserBtm_Click(object sender, EventArgs e)
        {
            hideUserManagePnl();

            UserAddPanel.Visible = true;
            UserAddPanel.BringToFront();
        }


        private void EditUserBtm_Click(object sender, EventArgs e)
        {
            hideUserManagePnl();
            UserEditPnl.Visible=true;
            UserEditPnl.BringToFront();
        }

        private void UserStatisticPanelShow_Click(object sender, EventArgs e)
        {
            StatisticPNL.BringToFront();
        }

        private void UserListPanelShow_Click(object sender, EventArgs e)
        {
            UserPNL.BringToFront();
        }

        private void UserListDGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the row is valid and clicked on a cell, not a header
            if (e.RowIndex >= 0)
            {
                // Handle Archive button click
                if (e.ColumnIndex == UserListDGV.Columns["ArchiveBC"].Index)
                {
                    DialogResult archiveResult = MessageBox.Show("Do you want to Archive this row?", "Archive Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (archiveResult == DialogResult.Yes)
                    {
                        // Archive (hide) the row from UserListDGV
                        UserListDGV.Rows[e.RowIndex].Visible = false;

                        // Optionally transfer the row to UserArchiveDGV
                    }
                }

                // Handle Edit button click
                if (e.ColumnIndex == UserListDGV.Columns["EditBC"].Index)
                {
                    UserEditPnl.Visible = true; // Make the UserEditPnl visible
                    UserEditPnl.BringToFront(); // Bring it to the front of the UI
                }
            }
        }


        private void UserFilterToggleBtm_Click(object sender, EventArgs e)
        {
            if (UserFilterToggleBtm.Checked)
            {
                // When toggle is on, show the filter panel
                UserFilterPnl.Visible = true;
            }
            else
            {
                // When toggle is off, hide the filter panel
                UserFilterPnl.Visible = false;
            }
        }


      


        private void SortButton_CheckedChanged(object sender, EventArgs e)
        {
            // Check the state of the ToggleButton
            if (SortButton.Checked)
            {
                // Sort LastName column A-Z
                UserListDGV.Sort(UserListDGV.Columns["Last Name"], System.ComponentModel.ListSortDirection.Ascending);
            }
            else
            {
                // Sort LastName column Z-A
                UserListDGV.Sort(UserListDGV.Columns["Last Name"], System.ComponentModel.ListSortDirection.Descending);
            }
        
    }




        //PRINT FUNCTIONS
        private void PrintToogleBtm_CheckedChanged(object sender, EventArgs e)
        {

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
                    command.Parameters.AddWithValue("@LastName", LNameTB.Text);
                    command.Parameters.AddWithValue("@FirstName", FNameTB.Text);
                    command.Parameters.AddWithValue("@DepartmentID", departmentId);
                    command.Parameters.AddWithValue("@ProgramID", programId);
                    command.Parameters.AddWithValue("@YearLevelID", yearLevelId);
                    command.Parameters.AddWithValue("@Email", EmailTB.Text);
                    command.Parameters.AddWithValue("@ContactNo", ContactTB.Text);
                    command.Parameters.AddWithValue("@DateRegistered", DateTime.Now);
                    command.Parameters.AddWithValue("@UPassword", password); // Add the password parameter

                    command.ExecuteNonQuery();
                    MessageBox.Show("User added successfully!");

                    RefreshUserListData();
                    ViewUserListBtm.Checked = true;
                    hideUserManagePnl();
                    StudIDTB.Clear();
                    LNameTB.Clear();
                    FNameTB.Clear();
                    DepartmentCB.SelectedIndex = -1;
                    ProgramCB.Items.Clear();
                    YearLevelCB.Items.Clear();
                    EmailTB.Clear();
                    ContactTB.Clear();
                    StudentPasswordTB.Clear(); // Clear the password textbox
                }
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
        //draft button
        private void AddDraftBtm_Click(object sender, EventArgs e)
        {
            ViewUserListBtm.Checked = true;
            UserAddPanel.Visible = false;
            UserAddPanel.Hide();
        }

        //EDIT Button

        //SHow TIP on StudentID Textbox
        private void EditStudentIDTB_Enter(object sender, EventArgs e)
        {
            EditStudentIDTBTT.Show("Enter the Student ID and press Enter to fill other fields.", EditStudentIDTB);
        }

        private void EditStudentIDTB_MouseHover(object sender, EventArgs e)
        {
            EditStudentIDTBTT.Show("Enter the Student ID and press Enter to fill other fields.", EditStudentIDTB);
        }

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

                    // Query to get user data based on StudentID
                    string query = "SELECT u.LastName, u.FirstName, d.DepartmentName, p.ProgramName, y.YearLevelName, u.Email, u.ContactNo " +
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
            if (e.RowIndex >= 0 && UserListDGV.Columns[e.ColumnIndex].Name == "EditBC")
            {
                // Show the UserEditPnl and bring it to front
                UserEditPnl.Visible = true;
                UserEditPnl.BringToFront();
                EditUserBtm.Checked = true;

                // Get the data from the selected row
                DataGridViewRow row = UserListDGV.Rows[e.RowIndex];

                // Populate the edit fields with the data from the selected row
                EditStudentIDTB.Text = row.Cells["Student ID"].Value?.ToString();
                EditLNameTB.Text = row.Cells["Last Name"].Value?.ToString();
                EditFNameTB.Text = row.Cells["First Name"].Value?.ToString();
                EditDepartmentCB.Text = row.Cells["Department"].Value?.ToString(); // Assuming ComboBox text is set by DepartmentName
                EditProgramCB.Text = row.Cells["Program"].Value?.ToString();       // Assuming ComboBox text is set by ProgramName
                EditYearLevelCB.Text = row.Cells["Year/Grade Level"].Value?.ToString(); // Assuming ComboBox text is set by YearLevelName
                EditEmailTB.Text = row.Cells["Email"].Value?.ToString();
                EditContactTB.Text = row.Cells["Contact"].Value?.ToString();
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
                MessageBox.Show("Please fill in all required fields: Last Name, First Name, Department, Program, and Year Level.", "Required Fields Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Cancel the execution if required fields are missing
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Get the IDs based on the selected names in the combo boxes
                int departmentID = GetDepartmentID(EditDepartmentCB.SelectedItem.ToString());
                int programID = GetProgramID(EditProgramCB.SelectedItem.ToString());
                int yearLevelID = GetYearLevelID(EditYearLevelCB.SelectedItem.ToString());

                // SQL Query to update user information
                string query = "UPDATE UserList SET LastName = @LastName, FirstName = @FirstName, DepartmentID = @DepartmentID, " +
                               "ProgramID = @ProgramID, YearLevelID = @YearLevelID, Email = @Email, ContactNo = @ContactNo " +
                               "WHERE StudentID = @StudentID";

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

                    // Execute the update query
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("User information updated successfully.", "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        RefreshUserListData();
                        ViewUserListBtm.Checked = true;
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
            ViewUserListBtm.Checked = true;
            hideUserManagePnl();
            EditStudentIDTB.Clear();
            EditLNameTB.Clear();
            EditFNameTB.Clear();
            EditDepartmentCB.SelectedIndex = -1;
            EditProgramCB.Items.Clear();
            EditYearLevelCB.Items.Clear();
            EditEmailTB.Clear();
            EditContactTB.Clear();
            StudentPasswordTB.Clear();
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

        private void FilterDepartmentCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear and reload ProgramCB and YearLevelCB based on selected department
            FilterProgramCB.Items.Clear();
            FilterYearLevelCB.Items.Clear();

            ApplyFilters();

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
            FilterTotalHourCKB.Checked = true;
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

    // Apply the filter to the DataGridView
    (UserListDGV.DataSource as DataTable).DefaultView.RowFilter = filter.ToString();
        }

        private void FilterProgramCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void FilterYearLevelCB_SelectedIndexChanged(object sender, EventArgs e)
        {
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
        private void FilterTotalHourCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterLastUnitCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterPasswordCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();


        // Method to update column visibility based on checkbox states
        private void UpdateColumnVisibility()
        {
            // Set column visibility based on checkbox states
            UserListDGV.Columns["Student ID"].Visible = FilterStudentCKB.Checked;
            UserListDGV.Columns["Last Name"].Visible = FilterLNameCKB.Checked;
            UserListDGV.Columns["First Name"].Visible = FilterFNameCKB.Checked;
            UserListDGV.Columns["Email"].Visible = FilterEmailCKB.Checked;
            UserListDGV.Columns["Contact"].Visible = FilterContactCKB.Checked;
            UserListDGV.Columns["Department"].Visible = FilterDepartmentCKB.Checked;
            UserListDGV.Columns["Program"].Visible = FilterProgramCKB.Checked;
            UserListDGV.Columns["Year/Grade Level"].Visible = FilterYearLevelCKB.Checked;
            UserListDGV.Columns["Status"].Visible = FilterStatusCKB.Checked;
            UserListDGV.Columns["Last Login"].Visible = FilterLastLoginCKB.Checked;
            UserListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
            UserListDGV.Columns["Total Hours Used"].Visible = FilterTotalHourCKB.Checked;
            UserListDGV.Columns["Last Unit Used"].Visible = FilterLastUnitCKB.Checked;
            UserListDGV.Columns["Password"].Visible = FilterPasswordCBK.Checked;
        }



        //Search bar
        private void UserSearchBar_TextChanged(object sender, EventArgs e)
        {
            ApplySearchFilter(UserSearchBar.Text);
        }
        private void ApplySearchFilter(string searchText)
        {
            if (UserListDGV.DataSource is DataTable dataTable)
            {
                // Build filter expression for visible columns with valid DataPropertyName
                var filterExpression = new List<string>();

                foreach (DataGridViewColumn column in UserListDGV.Columns)
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

    }  
}
