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
using Guna.UI2.WinForms;

namespace ComlabSystem
{
    public partial class ZNotifications : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

        public string AdminName
        {
            set { AdminNameLabel.Text = value; }

        }



        public ZNotifications()
        {
            InitializeComponent();
;

            // Attach the resize event to adjust label position on load or resize
            this.Resize += UserUI_Resize2;



        }


        private void UserUI_Load(object sender, EventArgs e)
        {

            AdjustNoArchiveListLabelPosition();

            UnitFilterPnl.Visible = false;

            AllNotificationDGV.BringToFront();
            AllNotificationFunction();
            SearchBar.Text = " ";

            PrintExcelALL.BringToFront();
            PrintLinkALL.BringToFront();


            //Print
            PrintExcel.BringToFront();
            PrintLink.BringToFront();
            guna2Panel2.BringToFront();



        }











        private void UserFilterToggleBtm_Click(object sender, EventArgs e)
        {
            if (UnitFilterToggleBtm.Checked)
            {
                // When toggle is on, show the filter panel
                UnitFilterPnl.Visible = true;
            }
            else
            {
                // When toggle is off, hide the filter panel
                UnitFilterPnl.Visible = false;
            }
        }
        private void HideFilterPanel()
        {
            UnitFilterPnl.Visible = false;
            UnitFilterToggleBtm.Checked = false;
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
            HideFilterPanel();

            //Prints
            PrintLinkALL.BringToFront();
            PrintExcelALL.BringToFront();

            UnitFilterToggleBtm.Checked = false;
            UnitFilterPnl.Visible = false;


        }


        //Clear the filter
        private void ClearFilterCBB_Click(object sender, EventArgs e)
        {

        }


        // Search bar event handler
        private void UserSearchBar_TextChanged(object sender, EventArgs e)
        {
            string searchText = SearchBar.Text;
            ApplySearchFilter(AllNotificationDGV, searchText);
            ApplySearchFilter(NotificationDGV, searchText);

            UnitFilterToggleBtm.Checked = false;
            UnitFilterPnl.Visible = false;
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
        private void AllSearchBar_TextChanged(object sender, EventArgs e)
        {
            string searchText = SearchBar.Text;
            AllApplySearchFilter(AllNotificationDGV, searchText);

            UnitFilterToggleBtm.Checked = false;
            UnitFilterPnl.Visible = false;
        }

        // Generalized method to apply search filter to any DataGridView
        private void AllApplySearchFilter(DataGridView gridView, string searchText)
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

        private void PrintExcel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (NotificationDGV.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.ApplicationClass MExcel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                MExcel.Application.Workbooks.Add(Type.Missing);
                for (int i = 1; i < NotificationDGV.Columns.Count + 1; i++)
                {
                    MExcel.Cells[1, i] = NotificationDGV.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < NotificationDGV.Rows.Count; i++)
                {
                    for (int j = 0; j < NotificationDGV.Columns.Count; j++)
                    {
                        MExcel.Cells[i + 2, j + 1] = NotificationDGV.Rows[i].Cells[j].Value.ToString();
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

        private void PrintExcelALL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (AllNotificationDGV.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.ApplicationClass MExcel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                MExcel.Application.Workbooks.Add(Type.Missing);
                for (int i = 1; i < AllNotificationDGV.Columns.Count + 1; i++)
                {
                    MExcel.Cells[1, i] = AllNotificationDGV.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < AllNotificationDGV.Rows.Count; i++)
                {
                    for (int j = 0; j < AllNotificationDGV.Columns.Count; j++)
                    {
                        MExcel.Cells[i + 2, j + 1] = AllNotificationDGV.Rows[i].Cells[j].Value.ToString();
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


        private void PrintLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (NotificationDGV.Rows.Count > 0)
            {
                // Set font for DataGridView headers before printing
                NotificationDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);  // Set bold font for column headers
                NotificationDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center-align headers

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
                printer.PrintPreviewDataGridView(NotificationDGV);
            }

            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
    }

        private void PrintLinkALL_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (AllNotificationDGV.Rows.Count > 0)
            {
                // Set font for DataGridView headers before printing
                AllNotificationDGV.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);  // Set bold font for column headers
                AllNotificationDGV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center-align headers

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
                printer.PrintPreviewDataGridView(AllNotificationDGV);
            }

            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }







        private void UserSearchBar_Click(object sender, EventArgs e)
        {
            HideFilterPanel();
        }






        //Buttons

        private void AllNotificationBtm_Click(object sender, EventArgs e)
        {
            PrintLinkALL.BringToFront();
            PrintExcelALL.BringToFront();
            AllNotificationDGV.BringToFront();
            SearchBar.Text = " ";

            AllNotificationFunction();
        }
        private void AllNotificationFunction()
        {
            // Create the SQL query to retrieve Notification data including NotificationID
            string query = @"SELECT 
                        NotificationID, -- Add this line to include NotificationID
                        Message AS 'Notification', 
                        Timestamp AS 'Timestamp', 
                        IsRead 
                     FROM Notifications 
                     ORDER BY Timestamp DESC";

            // Set up the connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create the command
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);

                // Create a DataTable to hold the data
                DataTable dataTable = new DataTable();

                try
                {
                    // Fill the DataTable with data from the database
                    dataAdapter.Fill(dataTable);

                    // Bind the DataTable to the DataGridView
                    AllNotificationDGV.DataSource = dataTable;

                    // Hide the NotificationID and IsRead columns
                    AllNotificationDGV.Columns["NotificationID"].Visible = false;
                    AllNotificationDGV.Columns["IsRead"].Visible = false;
                    AllNotificationDGV.Columns["Timestamp"].Visible = false;

                    // Set DataGridView AutoSizeMode for other columns to Fill
                    foreach (DataGridViewColumn column in AllNotificationDGV.Columns)
                    {
                        if (column.Name != "NotificationID" && column.Name != "IsRead")
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }
                    }

                    // Change row color based on IsRead column (still checking but column is hidden)
                    foreach (DataGridViewRow row in AllNotificationDGV.Rows)
                    {
                        if (row.Cells["IsRead"].Value != DBNull.Value)
                        {
                            bool isRead = Convert.ToBoolean(row.Cells["IsRead"].Value);
                            if (isRead)
                            {
                                row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245); // Read
                            }
                            else
                            {
                                row.DefaultCellStyle.BackColor = Color.FromArgb(230, 245, 255); // Unread
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., database connection issues)
                    MessageBox.Show("Error retrieving data: " + ex.Message);
                }
            }
        }

        private void AllNotificationDGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Get the IsRead value of the clicked row
                var isRead = Convert.ToBoolean(AllNotificationDGV.Rows[e.RowIndex].Cells["IsRead"].Value);

                if (isRead == false) // Only update if it is not already marked as read
                {
                    // Get the NotificationID value of the clicked row (now it exists because it's part of the query)
                    int notificationId = Convert.ToInt32(AllNotificationDGV.Rows[e.RowIndex].Cells["NotificationID"].Value);

                    // Update the IsRead field to 1
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string updateQuery = @"UPDATE Notifications 
                                       SET IsRead = 1 
                                       WHERE NotificationID = @NotificationID";

                        SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@NotificationID", notificationId);

                        try
                        {
                            connection.Open();
                            updateCommand.ExecuteNonQuery(); // Execute the update query
                            AllNotificationDGV.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245); // Change row color to read
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error updating notification status: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void FeedbackReportBtm_Click(object sender, EventArgs e)
        {
            NotificationDGV.BringToFront();
            PrintExcel.BringToFront();
            PrintLink.BringToFront();
            SearchBar.Text = " ";

            FeedbackReporFunction();
        }
        private void FeedbackReporFunction()
        {
            // SQL query to fetch data where NotificationKind is "Feedback" or "Report"

            string query = @"
        SELECT 
            IssueDescription AS 'Insights', 
            Timestamp AS 'Timestamp'
        FROM Help_Desk
        WHERE MessageType IN ('Feedback', 'Report') ORDER BY Timestamp DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();

                try
                {

                    // Fill the DataTable with data from the query
                    dataAdapter.Fill(dataTable);

                    // Bind the DataTable to the DataGridView
                    NotificationDGV.DataSource = dataTable;

                    NotificationDGV.Columns["Timestamp"].Visible = false;
                    // Set AutoSizeMode for all columns
                    foreach (DataGridViewColumn column in NotificationDGV.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }

                    // Adjust additional styles if needed
                    NotificationDGV.ClearSelection(); // Clear initial selection
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error retrieving feedback and reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ActivityReportBtm_Click(object sender, EventArgs e)
        {
            NotificationDGV.BringToFront();
            PrintExcel.BringToFront();
            PrintLink.BringToFront();
            SearchBar.Text = " ";

            ActivityReportFunction();
        }
        private void ActivityReportFunction()
        {
            // SQL query to fetch data where NotificationKind matches specified activities
            string query = @"
        SELECT 
            Message AS 'Action', 
            Timestamp AS 'Timestamp'
        FROM Notifications
        WHERE NotificationKind IN 
            ('Feedback', 'Report', 'AddUser', 'EditUser', 'ArchiveUser', 
             'UnarchiveUser', 'ArchiveUnit', 'UnarchiveUnit', 'NewUnit', 'LowStorage')
        ORDER BY Timestamp DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();

                try
                {
                    // Fill the DataTable with data from the query
                    dataAdapter.Fill(dataTable);

                    // Bind the DataTable to the DataGridView
                    NotificationDGV.DataSource = dataTable;

                    NotificationDGV.Columns["Timestamp"].Visible = false;
                    // Set AutoSizeMode for all columns
                    foreach (DataGridViewColumn column in NotificationDGV.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }

                    // Adjust additional styles if needed
                    NotificationDGV.ClearSelection(); // Clear initial selection
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error retrieving activity reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                NotificationDGV.BringToFront();
            }
        }

       


    }  
}
