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

namespace ComlabSystem
{
    public partial class UnitUI : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
        public UnitUI()
        {
            InitializeComponent();

            // Attach event handlers for ComboBox actions
            FilterProgramCB.Enter += ShowRequiredFields;
            FilterYearLevelCB.Enter += ShowRequiredFields;

            // Initially hide RequiredPicture and RequiredLabel
            RequiredPicture.Visible = false;
            RequiredLabel.Visible = false;



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
            LoadFilterDepartments();
            UnitListDGV.BringToFront();
            LoadUnitCountFromDatabase();
            UserListPrintDGVFUnc();

            PrintToogleBtm.BringToFront();

            LoadUnitListData();

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



        private void LoadUnitListData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Query to retrieve all unit details and join with the UserList table for user names
                string query = "SELECT ComputerName AS [Unit], Status, CurrentUser AS [Current User], " +
                               "LastUser AS [Last User], DateLastUsed AS [Date Last Used], TotalHoursUsed AS [Total Time Used], " +
                               "Storage, AvailableStorage AS [Available Storage], Ram, IPAddress AS [IP Address], Processor " +
                               "FROM UnitList";

                // Query to count the total number of units (without archived status)
                string countQuery = "SELECT COUNT(*) FROM UnitList"; // No need to filter by ArchiveStatus here

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();

                        // Load unit data into the DataTable
                        adapter.Fill(dataTable);
                        UnitListDGV.DataSource = dataTable;

                        // Get unit count
                        using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                        {
                            int unitCount = (int)countCommand.ExecuteScalar(); // Execute the count query
                            UpdateUserCountLabel(unitCount); // Update unit count label if needed
                        }

                        // Set all columns to use AllCells mode and wrap text, except for CurrentUser and LastUser
                        foreach (DataGridViewColumn column in UnitListDGV.Columns)
                        {
                            // Check if the column is CurrentUser or LastUser, and set their AutoSizeMode to Fill
                            if (column.Name == "Current User" || column.Name == "Last User")
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            }
                            else
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            }

                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Check if Archive button column exists, if not, add it
                        if (!UnitListDGV.Columns.Contains("ArchiveClmBtm"))
                        {
                            DataGridViewImageColumn archiveColumn = new DataGridViewImageColumn
                            {
                                Name = "ArchiveClmBtm",
                                HeaderText = "", // Empty header
                                Image = ResizeImage(Properties.Resources.archive, 20, 20), // Resized image
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                                Width = 25
                            };
                            UnitListDGV.Columns.Add(archiveColumn);
                        }

                        // Optional: Change the font color of the "Status" column based on its value
                        foreach (DataGridViewRow row in UnitListDGV.Rows)
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
                        UnitListDGV.AutoResizeColumns();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading unit data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }















        //User Counts label
        private void LoadUnitCountFromDatabase()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string countQuery = "SELECT COUNT(*) FROM UnitList"; // Exclude archived users if necessary

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
            UpdateUnitCountLabel.Text = count.ToString("D2"); // Display as two digits if count is less than 10

            // Adjust font size based on count
            if (count < 100)
            {
                UpdateUnitCountLabel.Location = new Point(3, -11);
                guna2CirclePictureBox1.Location = new Point(86, 18);
                label1.Location = new Point(116, 28);
            }
            else if (count >= 100 && count < 1000)
            {
                UpdateUnitCountLabel.Location = new Point(3, -10);
                guna2CirclePictureBox1.Location = new Point(120, 18);
                label1.Location = new Point(150, 28);
            }
            else if (count >= 1000)
            {
                UpdateUnitCountLabel.Location = new Point(3, -10);
                guna2CirclePictureBox1.Location = new Point(185, 28);
                label1.Location = new Point(116, 30);
            }
        }



























        //old code
        private void hideallpanel()
        {
            UserFilterPnl.Visible = false;
            UserFilterToggleBtm.Checked = false;
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
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Status,
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered],
    u.LastLogin AS [Last Login],
    u.TotalHoursUsed AS [Total Hours Used],
    u.LastUnitUsed AS [Unit Used]
FROM UserList u
JOIN Department d ON u.DepartmentID = d.DepartmentID
JOIN Programs p ON u.ProgramID = p.ProgramID
JOIN YearLevels y ON u.YearLevelID = y.YearLevelID
WHERE u.ArchiveStatus <> 'Archived'"; // Exclude archived users

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
                        UnitListDGV.DataSource = dataTable;

                        // Get user count excluding archived users
                        using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                        {
                            int userCount = (int)countCommand.ExecuteScalar(); // Execute the count query
                            UpdateUserCountLabel(userCount); // Update the label with the user count
                        }

                        // Set all columns to use AllCells mode and wrap text
                        foreach (DataGridViewColumn column in UnitListDGV.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Check if Edit button column exists, if not, add it
                        if (!UnitListDGV.Columns.Contains("EditBC"))
                        {
                            DataGridViewImageColumn editColumn = new DataGridViewImageColumn
                            {
                                Name = "EditBC",
                                HeaderText = "", // Empty header
                                Image = ResizeImage(Properties.Resources.pencil, 20, 20), // Resized image
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                                Width = 25
                            };
                            UnitListDGV.Columns.Add(editColumn);
                        }

                        // Check if Archive button column exists, if not, add it
                        if (!UnitListDGV.Columns.Contains("ArchiveBC"))
                        {
                            DataGridViewImageColumn archiveColumn = new DataGridViewImageColumn
                            {
                                Name = "ArchiveBC",
                                HeaderText = "", // Empty header
                                Image = ResizeImage(Properties.Resources.archive, 20, 20), // Resized image
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                                Width = 25
                            };
                            UnitListDGV.Columns.Add(archiveColumn);
                        }

                        // Change the font color of the "Status" column based on its value
                        foreach (DataGridViewRow row in UnitListDGV.Rows)
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
                        UnitListDGV.AutoResizeColumns();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        //adding tool tip to column buttons
        private void UserListDGV_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {

            if (e.ColumnIndex == UnitListDGV.Columns["ArchiveBC"].Index && e.RowIndex >= 0)
            {
                e.ToolTipText = "Click to archive this user.";
            }
            else
            {
                e.ToolTipText = string.Empty; // Clear tooltip for other cells
            }
        }



        private void UserUI_Resize(object sender, EventArgs e)
        {
            int leftpadding = 65; // Adjust this value for desired padding
            int buttompadding = 140;

            // Resize the DataGridView to fill the panel with padding
            UserListsGridPNL.Width = UserPNL.Width - leftpadding; // Right padding
            UserListsGridPNL.Height = UserPNL.Height - buttompadding;
        }







        private void UserStatisticPanelShow_Click(object sender, EventArgs e)
        {

            UserStatusTBTM.Enabled = false;
            ArchivePrintToogleBtm.BringToFront();
            UserFilterToggleBtm.Checked = false;
            UserFilterPnl.Visible = false;
            ArchiveUnitListDGV.BringToFront();
            unLoadUserListData(); // Reload the active user list
            LoadArchivedUserListData(); // Reload the archived user list
            ArchiveUserListPrintDGVFUnc();
            ApplyFilters();

        }

        private void UserListPanelShow_Click(object sender, EventArgs e)
        {
            UserStatusTBTM.Enabled = true;
            PrintToogleBtm.BringToFront();
            UserFilterToggleBtm.Checked = false;
            UserFilterPnl.Visible = false;
            UnitListDGV.BringToFront();
            UserListPrintDGVFUnc();
            ApplyFilters();
        }

        private void UserListDGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

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
            // Define sort direction based on toggle button state
            var sortDirection = SortButton.Checked
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;

            // Sort UserListDGV if it has data and the Last Name column exists
            if (UnitListDGV.Columns.Contains("Last Name") && UnitListDGV.Rows.Count > 0)
            {
                UnitListDGV.Sort(UnitListDGV.Columns["Last Name"], sortDirection);
            }

            // Sort ArchiveUserListDGV if it has data and the Last Name column exists
            if (ArchiveUnitListDGV.Columns.Contains("Last Name") && ArchiveUnitListDGV.Rows.Count > 0)
            {
                ArchiveUnitListDGV.Sort(ArchiveUnitListDGV.Columns["Last Name"], sortDirection);
            }

            // Sort UserListPrintDGV if it has data and the Last Name column exists
            if (UnitListPrintDGV.Columns.Contains("Last Name") && UnitListPrintDGV.Rows.Count > 0)
            {
                UnitListPrintDGV.Sort(UnitListPrintDGV.Columns["Last Name"], sortDirection);
            }
        }

    




    //PRINT FUNCTIONS
    private void PrintToogleBtm_CheckedChanged(object sender, EventArgs e)
        {

        }





        // ARCHIVE CODES


        private void unLoadUserListData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
SELECT 
    u.StudentID AS [Student ID],
    u.UPassword AS [Password],
    u.LastName AS [Last Name],
    u.FirstName AS [First Name],
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Status AS [Status],
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered],
    u.LastLogin AS [Last Login],
    u.TotalHoursUsed AS [Total Hours Used],
    u.LastUnitUsed AS [Unit Used]
FROM UserList u
JOIN Department d ON u.DepartmentID = d.DepartmentID
JOIN Programs p ON u.ProgramID = p.ProgramID
JOIN YearLevels y ON u.YearLevelID = y.YearLevelID
WHERE u.ArchiveStatus = 'Active'";  // Filter to show only active users

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
                            UnitListDGV.DataSource = dataTable;

                            // Set all columns to use AllCells mode and wrap text
                            foreach (DataGridViewColumn column in UnitListDGV.Columns)
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                            }

                            // Change the font color of the "Status" column cells based on the status
                            foreach (DataGridViewRow row in UnitListDGV.Rows)
                            {
                                string status = row.Cells["Status"].Value.ToString(); // Get status value

                                if (status == "Online")
                                {
                                    // Set font color to RGB (45, 198, 109) for Online
                                    row.Cells["Status"].Style.ForeColor = Color.FromArgb(45, 198, 109);
                                }
                                else if (status == "Offline")
                                {
                                    // Set font color to RGB (60, 60, 60) for Offline
                                    row.Cells["Status"].Style.ForeColor = Color.FromArgb(60, 60, 60);
                                }
                            }

                            // Add Edit button column if not present
                            if (UnitListDGV.Columns["EditBC"] == null)
                            {
                                DataGridViewImageColumn editColumn = new DataGridViewImageColumn
                                {
                                    Name = "EditBC",
                                    HeaderText = "",
                                    Image = ResizeImage(Properties.Resources.pencil, 20, 20),
                                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                                };
                                UnitListDGV.Columns.Add(editColumn);
                            }

                            // Add Archive button column if not present
                            if (UnitListDGV.Columns["ArchiveBC"] == null)
                            {
                                DataGridViewImageColumn archiveColumn = new DataGridViewImageColumn
                                {
                                    Name = "ArchiveBC",
                                    HeaderText = "",
                                    Image = ResizeImage(Properties.Resources.archive, 20, 20),
                                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                                };
                                UnitListDGV.Columns.Add(archiveColumn);
                            }
                        }
                        else
                        {
                            UnitListDGV.DataSource = null; // Clear the DataGridView if no data
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
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
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Status AS [Status],
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered],
    u.LastLogin AS [Last Login],
    u.TotalHoursUsed AS [Total Hours Used],
    u.LastUnitUsed AS [Unit Used]
FROM UserList u
JOIN Department d ON u.DepartmentID = d.DepartmentID
JOIN Programs p ON u.ProgramID = p.ProgramID
JOIN YearLevels y ON u.YearLevelID = y.YearLevelID
WHERE u.ArchiveStatus = 'Archived'"; // Filter to show only archived users

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
                            ArchiveUnitListDGV.DataSource = dataTable;

                            // Set all columns to use AllCells mode and wrap text
                            foreach (DataGridViewColumn column in ArchiveUnitListDGV.Columns)
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                            }

                            // Add Unarchive button column if not present
                            if (ArchiveUnitListDGV.Columns["UnArchiveUserList"] == null)
                            {
                                DataGridViewImageColumn unarchiveColumn = new DataGridViewImageColumn
                                {
                                    Name = "UnArchiveUserList",
                                    HeaderText = "",
                                    Image = ResizeImage(Properties.Resources.unarchive, 20, 20),
                                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells // Change to AllCells
                                };
                                ArchiveUnitListDGV.Columns.Add(unarchiveColumn);
                            }

                            // Ensure the button column is last
                            ArchiveUnitListDGV.Columns["UnArchiveUserList"].DisplayIndex = ArchiveUnitListDGV.Columns.Count - 1;
                        }
                        else
                        {

                            ArchiveUnitListDGV.DataSource = null; // Clear the DataGridView if no data
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading archived user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void ArchiveUserListDGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && ArchiveUnitListDGV.Columns[e.ColumnIndex].Name == "UnArchiveUserList")
            {
                hideallpanel();
                string archiveStudentID = ArchiveUnitListDGV.Rows[e.RowIndex].Cells["Student ID"].Value.ToString();


                
                    UnarchiveUser(archiveStudentID);
                
            }
        }
        private void UnarchiveUser(string archiveStudentID)
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
                            MessageBox.Show("User unarchived successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            unLoadUserListData(); // Reload the active user list
                            LoadArchivedUserListData(); // Reload the archived user list
                        }
                        else
                        {
                            MessageBox.Show("Failed to unarchive user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
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

            // Check if UserListDGV has a valid DataTable and apply filter if possible
            if (UnitListDGV.DataSource is DataTable userTable)
            {
                userTable.DefaultView.RowFilter = filter.ToString();
            }

            // Check if ArchiveUserListDGV has a valid DataTable and apply filter if possible
            if (ArchiveUnitListDGV.DataSource is DataTable archiveTable)
            {
                archiveTable.DefaultView.RowFilter = filter.ToString();
            }

            if (UnitListPrintDGV.DataSource is DataTable printTable)
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
        private void FilterTotalHourCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterLastUnitCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterPasswordCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();




        // Method to update column visibility based on checkbox states
        private void UpdateColumnVisibility()
        {
            // Check if UserListDGV has columns before updating visibility
            if (UnitListDGV.Columns.Count > 0)
            {
                UnitListDGV.Columns["Student ID"].Visible = FilterStudentCKB.Checked;
                UnitListDGV.Columns["Last Name"].Visible = FilterLNameCKB.Checked;
                UnitListDGV.Columns["First Name"].Visible = FilterFNameCKB.Checked;
                UnitListDGV.Columns["Email"].Visible = FilterEmailCKB.Checked;
                UnitListDGV.Columns["Contact"].Visible = FilterContactCKB.Checked;
                UnitListDGV.Columns["Department"].Visible = FilterDepartmentCKB.Checked;
                UnitListDGV.Columns["Program"].Visible = FilterProgramCKB.Checked;
                UnitListDGV.Columns["Year/Grade Level"].Visible = FilterYearLevelCKB.Checked;
                UnitListDGV.Columns["Status"].Visible = FilterStatusCKB.Checked;
                UnitListDGV.Columns["Last Login"].Visible = FilterLastLoginCKB.Checked;
                UnitListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                UnitListDGV.Columns["Total Hours Used"].Visible = FilterTotalHourCKB.Checked;
                UnitListDGV.Columns["Last Unit Used"].Visible = FilterLastUnitCKB.Checked;
                UnitListDGV.Columns["Password"].Visible = FilterPasswordCBK.Checked;
            }

            // Check if ArchiveUserListDGV has columns before updating visibility
            if (ArchiveUnitListDGV.Columns.Count > 0)
            {
                ArchiveUnitListDGV.Columns["Student ID"].Visible = FilterStudentCKB.Checked;
                ArchiveUnitListDGV.Columns["Last Name"].Visible = FilterLNameCKB.Checked;
                ArchiveUnitListDGV.Columns["First Name"].Visible = FilterFNameCKB.Checked;
                ArchiveUnitListDGV.Columns["Email"].Visible = FilterEmailCKB.Checked;
                ArchiveUnitListDGV.Columns["Contact"].Visible = FilterContactCKB.Checked;
                ArchiveUnitListDGV.Columns["Department"].Visible = FilterDepartmentCKB.Checked;
                ArchiveUnitListDGV.Columns["Program"].Visible = FilterProgramCKB.Checked;
                ArchiveUnitListDGV.Columns["Year/Grade Level"].Visible = FilterYearLevelCKB.Checked;
                ArchiveUnitListDGV.Columns["Status"].Visible = FilterStatusCKB.Checked;
                ArchiveUnitListDGV.Columns["Last Login"].Visible = FilterLastLoginCKB.Checked;
                ArchiveUnitListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                ArchiveUnitListDGV.Columns["Total Hours Used"].Visible = FilterTotalHourCKB.Checked;
                ArchiveUnitListDGV.Columns["Last Unit Used"].Visible = FilterLastUnitCKB.Checked;
                ArchiveUnitListDGV.Columns["Password"].Visible = FilterPasswordCBK.Checked;
            }

            // Check if UserListPrintDGV has columns before updating visibility
            if (UnitListPrintDGV.Columns.Count > 0)
            {
                UnitListPrintDGV.Columns["Student ID"].Visible = FilterStudentCKB.Checked;
                UnitListPrintDGV.Columns["Last Name"].Visible = FilterLNameCKB.Checked;
                UnitListPrintDGV.Columns["First Name"].Visible = FilterFNameCKB.Checked;
                UnitListPrintDGV.Columns["Email"].Visible = FilterEmailCKB.Checked;
                UnitListPrintDGV.Columns["Contact"].Visible = FilterContactCKB.Checked;
                UnitListPrintDGV.Columns["Department"].Visible = FilterDepartmentCKB.Checked;
                UnitListPrintDGV.Columns["Program"].Visible = FilterProgramCKB.Checked;
                UnitListPrintDGV.Columns["Year/Grade Level"].Visible = FilterYearLevelCKB.Checked;
                UnitListPrintDGV.Columns["Status"].Visible = FilterStatusCKB.Checked;
                UnitListPrintDGV.Columns["Last Login"].Visible = FilterLastLoginCKB.Checked;
                UnitListPrintDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                UnitListPrintDGV.Columns["Total Hours Used"].Visible = FilterTotalHourCKB.Checked;
                UnitListPrintDGV.Columns["Last Unit Used"].Visible = FilterLastUnitCKB.Checked;
                UnitListPrintDGV.Columns["Password"].Visible = FilterPasswordCBK.Checked;
            }
        }



        // Search bar event handler
        private void UserSearchBar_TextChanged(object sender, EventArgs e)
        {
            string searchText = UserSearchBar.Text;
            ApplySearchFilter(UnitListDGV, searchText);
            ApplySearchFilter(ArchiveUnitListDGV, searchText);
            ApplySearchFilter(UnitListPrintDGV, searchText);

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
            UnitListPrintDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);  // Set bold font for column headers
            UnitListPrintDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center-align headers

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
            printer.PrintPreviewDataGridView(UnitListPrintDGV);
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
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Status,
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered],
    u.LastLogin AS [Last Login],
    u.TotalHoursUsed AS [Total Hours Used],
    u.LastUnitUsed AS [Unit Used]
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
                        UnitListPrintDGV.DataSource = dataTable;

                        // Set all columns to use AllCells mode and wrap text
                        foreach (DataGridViewColumn column in UnitListPrintDGV.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Loop through the rows to change font color based on the Status
                        foreach (DataGridViewRow row in UnitListPrintDGV.Rows)
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
                        UnitListPrintDGV.AutoResizeColumns();
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
    d.DepartmentName AS [Department],
    p.ProgramName AS [Program],
    y.YearLevelName AS [Year/Grade Level],
    u.Status AS [Status],
    u.Email,
    u.ContactNo AS [Contact],
    u.DateRegistered AS [Date Registered],
    u.LastLogin AS [Last Login],
    u.TotalHoursUsed AS [Total Hours Used],
    u.LastUnitUsed AS [Unit Used]
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
                            UnitListPrintDGV.DataSource = dataTable;

                            // Set all columns to use AllCells mode and wrap text
                            foreach (DataGridViewColumn column in UnitListPrintDGV.Columns)
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                            }

                           
                        }
                        else
                        {
                            UnitListPrintDGV.DataSource = null; // Clear the DataGridView if no data
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

        private void ArchivePrintToogleBtm_Click(object sender, EventArgs e)
        {
            // Set font for DataGridView headers before printing
            UnitListPrintDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);  // Set bold font for column headers
            UnitListPrintDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center-align headers

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
            printer.PrintPreviewDataGridView(UnitListPrintDGV);
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
            ApplySortingToDataGridView(UnitListDGV, status);

            // Apply sorting for ArchiveUserListDGV
            ApplySortingToDataGridView(UnitListPrintDGV, status);
        }

        private void ApplySortingToDataGridView(DataGridView dgv, string status)
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

        private void UserListDGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Check if the column is the "Status" column (usually by index or name)
            if (UnitListDGV.Columns[e.ColumnIndex].Name == "Status")
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


    }  
}
