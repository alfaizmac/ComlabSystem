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
    public partial class ZUnitListUI : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;


        public ZUnitListUI()
        {
            InitializeComponent();

            // Attach the resize event to adjust label position on load or resize
            this.Resize += UserUI_Resize2;

            // Assign event handlers to checkboxes
            FilterUnitNameCKB.CheckedChanged += FilterStudentCKB_CheckedChanged;
            FilterIPCKB.CheckedChanged += FilterLNameCKB_CheckedChanged;
            FilterRamCKB.CheckedChanged += FilterFNameCKB_CheckedChanged;
            FilterLastUserlCKB.CheckedChanged += FilterEmailCKB_CheckedChanged;
            FilterCurrentUserCKB.CheckedChanged += FilterContactCKB_CheckedChanged;
            FilterTotalStorageCKB.CheckedChanged += FilterDepartmentCKB_CheckedChanged;
            FilterAvailableStorageCKB.CheckedChanged += FilterProgramCKB_CheckedChanged;
            FilterProcessorCKB.CheckedChanged += FilterYearLevelCKB_CheckedChanged;
            FilterStatusCBK.CheckedChanged += FilterStatusCBK_CheckedChanged;
            FilterLoginStartCBK.CheckedChanged += FilterLoginStartCBK_CheckedChanged;
            FilterDateRegisteredCKB.CheckedChanged += FilterDateRegisteredCKB_CheckedChanged;
            FilterLastUsedDateCBK.CheckedChanged += FilterPasswordCBK_CheckedChanged;


        }


        private void UserUI_Load(object sender, EventArgs e)
        {
            NoArchiveListLabel.Visible = false;
            //LoadArchivedUserListData(); // Reload the archived user list

            AdjustNoArchiveListLabelPosition();

            UserFilterPnl.Visible = false;

            UnitListDGV.BringToFront();

            LoadUnitCountFromDatabase();

            //Print
            PrintExcel.BringToFront();
            PrintLink.BringToFront();
            guna2Panel2.BringToFront();


            LoadUnitListData();
        }







        //LOAD THE DATAGRIDVIEWS
        //UnitList Datagridview
        private void LoadUnitListData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Query to retrieve the required columns where ArchiveStatus is "Active"
                string query = @"
SELECT 
    ComputerName AS [Unit],
    Status,
    CurrentUser AS [Current User],
    DateNewLogin AS [Login Start Time],
    LastUserName AS [Last User],
    DateLastUsed AS [Last Used Date],
    Storage AS [Total Storage],
    AvailableStorage AS [Available Storage],
    Ram,
    IPAddress AS [IP Address],
    Processor,
    DateRegistered AS [Date Registered]
FROM UnitList
WHERE ArchiveStatus = 'Active'";

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

                        // Set all columns to use AllCells mode and wrap text
                        foreach (DataGridViewColumn column in UnitListDGV.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Check if Archive button column exists, if not, add it
                        if (!UnitListDGV.Columns.Contains("ArchiveBtmC"))
                        {
                            DataGridViewImageColumn archiveColumn = new DataGridViewImageColumn
                            {
                                Name = "ArchiveBtmC",
                                HeaderText = "Archive",
                                Image = ResizeImage(Properties.Resources.ColoredArchiveICON2, 30, 30), // Resized image
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                                Width = 25
                            };
                            UnitListDGV.Columns.Add(archiveColumn);
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

        private void UnitListDGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the Archive button column is clicked
            if (e.RowIndex >= 0 && UnitListDGV.Columns[e.ColumnIndex].Name == "ArchiveBtmC")
            {
                string computerName = UnitListDGV.Rows[e.RowIndex].Cells["Unit"].Value?.ToString();

                if (!string.IsNullOrEmpty(computerName))
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to archive this unit?", "Confirm Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            string updateQuery = "UPDATE UnitList SET ArchiveStatus = 'Inactive' WHERE ComputerName = @ComputerName";

                            using (SqlCommand command = new SqlCommand(updateQuery, connection))
                            {
                                command.Parameters.AddWithValue("@ComputerName", computerName);

                                try
                                {
                                    connection.Open();
                                    int rowsAffected = command.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        MessageBox.Show("Unit successfully archived.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        LoadUnitListData(); // Reload the data to reflect changes
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to archive the unit. Unit not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Error archiving the unit: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Unit information is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }


























        private void hideallpanel()
        {
            UserFilterPnl.Visible = false;
            UserFilterToggleBtm.Checked = false;
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
    



    //Unit Counts label
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




        private void UserStatisticPanelShow_Click(object sender, EventArgs e)
        {

            UserStatusTBTM.Enabled = false;

            //Prints
            ArchivePrintLink.BringToFront();
            PrintExcelArchive.BringToFront();

            UserFilterToggleBtm.Checked = false;
            UserFilterPnl.Visible = false;
            ArchiveUnitListDGV.BringToFront();


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
            UnitListDGV.BringToFront();
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

    

        //Archive button
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

                        }
                        else
                        {
                            MessageBox.Show("Failed to unarchive user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
  
        











        //FILTER BUTTONS

        //Clear the filter
        private void ClearFilterCBB_Click(object sender, EventArgs e)
        {

            FilterUnitNameCKB.Checked = true;
            FilterIPCKB.Checked = true;
            FilterRamCKB.Checked = true;
            FilterLastUserlCKB.Checked = true;
            FilterCurrentUserCKB.Checked = true;
            FilterTotalStorageCKB.Checked = true;
            FilterAvailableStorageCKB.Checked = true;
            FilterProcessorCKB.Checked = true;
            FilterStatusCBK.Checked = true;
            FilterLoginStartCBK.Checked = true;
            FilterDateRegisteredCKB.Checked = true;
            FilterLastUsedDateCBK.Checked = true;
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
        private void FilterStatusCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterLoginStartCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterDateRegisteredCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
        private void FilterPasswordCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();




        // Method to update column visibility based on checkbox states
        private void UpdateColumnVisibility()
        {
            // Check if UserListDGV has columns before updating visibility
            if (UnitListDGV.Columns.Count > 0)
            {
                UnitListDGV.Columns["Student ID"].Visible = FilterUnitNameCKB.Checked;
                UnitListDGV.Columns["Last Name"].Visible = FilterIPCKB.Checked;
                UnitListDGV.Columns["First Name"].Visible = FilterRamCKB.Checked;
                UnitListDGV.Columns["Email"].Visible = FilterLastUserlCKB.Checked;
                UnitListDGV.Columns["Contact"].Visible = FilterCurrentUserCKB.Checked;
                UnitListDGV.Columns["Department"].Visible = FilterTotalStorageCKB.Checked;
                UnitListDGV.Columns["Program"].Visible = FilterAvailableStorageCKB.Checked;
                UnitListDGV.Columns["Year/Grade Level"].Visible = FilterProcessorCKB.Checked;
                UnitListDGV.Columns["Status"].Visible = FilterStatusCBK.Checked;
                UnitListDGV.Columns["Last Login"].Visible = FilterLoginStartCBK.Checked;
                UnitListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                UnitListDGV.Columns["Password"].Visible = FilterLastUsedDateCBK.Checked;
            }

            // Check if ArchiveUserListDGV has columns before updating visibility
            if (ArchiveUnitListDGV.Columns.Count > 0)
            {
                ArchiveUnitListDGV.Columns["Student ID"].Visible = FilterUnitNameCKB.Checked;
                ArchiveUnitListDGV.Columns["Last Name"].Visible = FilterIPCKB.Checked;
                ArchiveUnitListDGV.Columns["First Name"].Visible = FilterRamCKB.Checked;
                ArchiveUnitListDGV.Columns["Email"].Visible = FilterLastUserlCKB.Checked;
                ArchiveUnitListDGV.Columns["Contact"].Visible = FilterCurrentUserCKB.Checked;
                ArchiveUnitListDGV.Columns["Department"].Visible = FilterTotalStorageCKB.Checked;
                ArchiveUnitListDGV.Columns["Program"].Visible = FilterAvailableStorageCKB.Checked;
                ArchiveUnitListDGV.Columns["Year/Grade Level"].Visible = FilterProcessorCKB.Checked;
                ArchiveUnitListDGV.Columns["Status"].Visible = FilterStatusCBK.Checked;
                ArchiveUnitListDGV.Columns["Last Login"].Visible = FilterLoginStartCBK.Checked;
                ArchiveUnitListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                ArchiveUnitListDGV.Columns["Password"].Visible = FilterLastUsedDateCBK.Checked;
            }

            // Check if UserListPrintDGV has columns before updating visibility
            if (UnitListPrintDGV.Columns.Count > 0)
            {
                UnitListPrintDGV.Columns["Student ID"].Visible = FilterUnitNameCKB.Checked;
                UnitListPrintDGV.Columns["Last Name"].Visible = FilterIPCKB.Checked;
                UnitListPrintDGV.Columns["First Name"].Visible = FilterRamCKB.Checked;
                UnitListPrintDGV.Columns["Email"].Visible = FilterLastUserlCKB.Checked;
                UnitListPrintDGV.Columns["Contact"].Visible = FilterCurrentUserCKB.Checked;
                UnitListPrintDGV.Columns["Department"].Visible = FilterTotalStorageCKB.Checked;
                UnitListPrintDGV.Columns["Program"].Visible = FilterAvailableStorageCKB.Checked;
                UnitListPrintDGV.Columns["Year/Grade Level"].Visible = FilterProcessorCKB.Checked;
                UnitListPrintDGV.Columns["Status"].Visible = FilterStatusCBK.Checked;
                UnitListPrintDGV.Columns["Last Login"].Visible = FilterLoginStartCBK.Checked;
                UnitListPrintDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
                UnitListPrintDGV.Columns["Password"].Visible = FilterLastUsedDateCBK.Checked;
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


        private void PrintLink_Click(object sender, EventArgs e)
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
            printer.PrintPreviewDataGridView(UnitListPrintDGV);
        }

        private void ArchivePrintLink_Click(object sender, EventArgs e)
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

        private void PrintExcelArchive_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (UnitListPrintDGV.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.ApplicationClass MExcel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                MExcel.Application.Workbooks.Add(Type.Missing);
                for (int i = 1; i < UnitListPrintDGV.Columns.Count + 1; i++)
                {
                    MExcel.Cells[1, i] = UnitListPrintDGV.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < UnitListPrintDGV.Rows.Count; i++)
                {
                    for (int j = 0; j < UnitListPrintDGV.Columns.Count; j++)
                    {
                        MExcel.Cells[i + 2, j + 1] = UnitListPrintDGV.Rows[i].Cells[j].Value.ToString();
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
            if (UnitListPrintDGV.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.ApplicationClass MExcel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                MExcel.Application.Workbooks.Add(Type.Missing);
                for (int i = 1; i < UnitListPrintDGV.Columns.Count + 1; i++)
                {
                    MExcel.Cells[1, i] = UnitListPrintDGV.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < UnitListPrintDGV.Rows.Count; i++)
                {
                    for (int j = 0; j < UnitListPrintDGV.Columns.Count; j++)
                    {
                        MExcel.Cells[i + 2, j + 1] = UnitListPrintDGV.Rows[i].Cells[j].Value.ToString();
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
