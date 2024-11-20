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

        public string AdminName
        {
            set { AdminNameLabel.Text = value; }

        }



        public ZUnitListUI()
        {
            InitializeComponent();
;

            // Attach the resize event to adjust label position on load or resize
            this.Resize += UserUI_Resize2;

            // Assign event handlers to checkboxes
            FilterUnitNameCKB.CheckedChanged += FilterUnitNameCKB_CheckedChanged;
            FilterStatusCBK.CheckedChanged += FilterStatusCBK_CheckedChanged;
            FilterCurrentUserCKB.CheckedChanged += FilterCurrentUserCKB_CheckedChanged;
            FilterLoginStartCBK.CheckedChanged += FilterLoginStartCBK_CheckedChanged;
            FilterLastUserlCKB.CheckedChanged += FilterLastUserlCKB_CheckedChanged;
            FilterLastUsedDateCBK.CheckedChanged += FilterLastUsedDateCBK_CheckedChanged;
            FilterTotalStorageCKB.CheckedChanged += FilterTotalStorageCKB_CheckedChanged;
            FilterAvailableStorageCKB.CheckedChanged += FilterAvailableStorageCKB_CheckedChanged;
            FilterRamCKB.CheckedChanged += FilterRamCKB_CheckedChanged;
            FilterIPCKB.CheckedChanged += FilterIPCKB_CheckedChanged;
            FilterProcessorCKB.CheckedChanged += FilterProcessorCKB_CheckedChanged;
            FilterDateRegisteredCKB.CheckedChanged += FilterDateRegisteredCKB_CheckedChanged;


        }


        private void UserUI_Load(object sender, EventArgs e)
        {


            InitializeComboBox();

            


            AdjustNoArchiveListLabelPosition();

            UnitFilterPnl.Visible = false;

            UnitListDGV.BringToFront();


            //Print
            PrintExcel.BringToFront();
            PrintLink.BringToFront();
            guna2Panel2.BringToFront();


            LoadUnitListData();
            PrintLoadUnitListData();

            //should always last
            NoArchiveListLabel.Visible = false;
            LoadUnitCountFromDatabase();

            ComLabCB.BringToFront();
        }







        //LOAD THE DATAGRIDVIEWS
        //UnitList Datagridview
        private void LoadUnitListData()
        {
            string selectedLab = ComLabCB.SelectedItem?.ToString() ?? "All Units"; // Default to "All Units" if null

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Base query with placeholders for filtering
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

                // Add filter for ComputerName starting with ComLab1 or ComLab2
                if (selectedLab != "All Units")
                {
                    query += " AND ComputerName LIKE @LabPrefix";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Set parameter for lab prefix if filtering
                    if (selectedLab != "All Units")
                    {
                        command.Parameters.AddWithValue("@LabPrefix", selectedLab + "%"); // Add wildcard for prefix match
                    }

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

                        // Set the "Current User" column to AutoSizeMode.Fill if it exists
                        if (UnitListDGV.Columns.Contains("Login Start Time"))
                        {
                            UnitListDGV.Columns["Login Start Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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

        private void PrintLoadUnitListData()
        {
            string selectedLab = ComLabCB.SelectedItem?.ToString() ?? "All Units"; // Default to "All Units" if null

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Base query with placeholders for filtering
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

                // Add filter for ComputerName starting with ComLab1 or ComLab2
                if (selectedLab != "All Units")
                {
                    query += " AND ComputerName LIKE @LabPrefix";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Set parameter for lab prefix if filtering
                    if (selectedLab != "All Units")
                    {
                        command.Parameters.AddWithValue("@LabPrefix", selectedLab + "%"); // Add wildcard for prefix match
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();

                        // Load unit data into the DataTable
                        adapter.Fill(dataTable);
                        UnitListPrintDGV.DataSource = dataTable;

                        // Set all columns to use AllCells mode and wrap text
                        foreach (DataGridViewColumn column in UnitListPrintDGV.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Set the "Current User" column to AutoSizeMode.Fill if it exists
                        if (UnitListPrintDGV.Columns.Contains("Login Start Time"))
                        {
                            UnitListPrintDGV.Columns["Login Start Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }

                        // Refresh layout after setting modes
                        UnitListPrintDGV.AutoResizeColumns();
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
                HideFilterPanel();

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
                                        PrintLoadUnitListData();

                                        // Insert a notification about the archive action
                                        string adminName = AdminNameLabel.Text; // Replace with the actual label for the admin's name
                                        InsertNotificationForUnitArchive(adminName, computerName);
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
        private void InsertNotificationForUnitArchive(string adminName, string computerName)
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
                    string message = $"Admin name {adminName} archived unit '{computerName}' at {timestamp:yyyy-MM-dd HH:mm:ss}";
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                    cmd.Parameters.AddWithValue("@AdminID", adminID);
                    cmd.Parameters.AddWithValue("@NotificationType", "Information");
                    cmd.Parameters.AddWithValue("@NotificationKind", "ArchiveUnit");

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
        private void ArchiveLoadUnitListData()
        {
            string selectedLab = ComLabCB.SelectedItem?.ToString() ?? "All Units"; // Default to "All Units" if null

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Base query to filter by ArchiveStatus = 'Inactive'
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
WHERE ArchiveStatus = 'Inactive'";

                // Add filter for ComputerName starting with ComLab1 or ComLab2
                if (selectedLab != "All Units")
                {
                    query += " AND ComputerName LIKE @LabPrefix";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Set parameter for lab prefix if filtering
                    if (selectedLab != "All Units")
                    {
                        command.Parameters.AddWithValue("@LabPrefix", selectedLab + "%"); // Add wildcard for prefix match
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();

                        // Load unit data into the DataTable
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                        {
                            ArchiveUnitListDGV.DataSource = dataTable;

                            // Adjust columns for readability
                            foreach (DataGridViewColumn column in ArchiveUnitListDGV.Columns)
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                            }

                            // Set specific column to fill mode if exists
                            if (ArchiveUnitListDGV.Columns.Contains("Login Start Time"))
                            {
                                ArchiveUnitListDGV.Columns["Login Start Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            }

                            // Add "Unarchive" button column if it doesn't exist
                            if (!ArchiveUnitListDGV.Columns.Contains("UnArchiveBtmC"))
                            {
                                DataGridViewImageColumn archiveColumn = new DataGridViewImageColumn
                                {
                                    Name = "UnArchiveBtmC",
                                    HeaderText = "Unarchive",
                                    Image = ResizeImage(Properties.Resources.ColoredUnArchiveICON2, 30, 30), // Resized image
                                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                                    Width = 25
                                };
                                ArchiveUnitListDGV.Columns.Add(archiveColumn);
                            }

                            // Adjust layout after setting modes
                            ArchiveUnitListDGV.AutoResizeColumns();
                            NoArchiveListLabel.Visible = false; // Hide the "no data" label if data exists
                        }
                        else
                        {
                            NoArchiveListLabel.BringToFront();
                            NoArchiveListLabel.Visible = true;

                            ArchiveUnitListDGV.DataSource = null; // Clear DataGridView if no data
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading archived unit data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void ComboBoxArchiveLoadUnitListData()
        {
            string selectedLab = ComLabCB.SelectedItem?.ToString() ?? "All Units"; // Default to "All Units" if null

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Base query to filter by ArchiveStatus = 'Inactive'
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
WHERE ArchiveStatus = 'Inactive'";

                // Add filter for ComputerName starting with ComLab1 or ComLab2
                if (selectedLab != "All Units")
                {
                    query += " AND ComputerName LIKE @LabPrefix";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Set parameter for lab prefix if filtering
                    if (selectedLab != "All Units")
                    {
                        command.Parameters.AddWithValue("@LabPrefix", selectedLab + "%"); // Add wildcard for prefix match
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();

                        // Load unit data into the DataTable
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                        {
                            ArchiveUnitListDGV.DataSource = dataTable;

                            // Adjust columns for readability
                            foreach (DataGridViewColumn column in ArchiveUnitListDGV.Columns)
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                            }

                            // Set specific column to fill mode if exists
                            if (ArchiveUnitListDGV.Columns.Contains("Login Start Time"))
                            {
                                ArchiveUnitListDGV.Columns["Login Start Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            }

                            // Add "Unarchive" button column if it doesn't exist
                            if (!ArchiveUnitListDGV.Columns.Contains("UnArchiveBtmC"))
                            {
                                DataGridViewImageColumn archiveColumn = new DataGridViewImageColumn
                                {
                                    Name = "UnArchiveBtmC",
                                    HeaderText = "Unarchive",
                                    Image = ResizeImage(Properties.Resources.ColoredUnArchiveICON2, 30, 30), // Resized image
                                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                                    Width = 25
                                };
                                ArchiveUnitListDGV.Columns.Add(archiveColumn);
                            }

                            // Move the "Unarchive" column to the far right position
                            ArchiveUnitListDGV.Columns["UnArchiveBtmC"].DisplayIndex = ArchiveUnitListDGV.Columns.Count - 1;

                            // Adjust layout after setting modes
                            ArchiveUnitListDGV.AutoResizeColumns();
                        }
                        else
                        {
                            ArchiveUnitListDGV.DataSource = null; // Clear DataGridView if no data
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading archived unit data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void PrintArchiveLoadUnitListData()
        {
            string selectedLab = ComLabCB.SelectedItem?.ToString() ?? "All Units"; // Default to "All Units" if null

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Base query to filter by ArchiveStatus = 'Inactive'
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
WHERE ArchiveStatus = 'Inactive'";

                // Add filter for ComputerName starting with ComLab1 or ComLab2
                if (selectedLab != "All Units")
                {
                    query += " AND ComputerName LIKE @LabPrefix";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Set parameter for lab prefix if filtering
                    if (selectedLab != "All Units")
                    {
                        command.Parameters.AddWithValue("@LabPrefix", selectedLab + "%"); // Add wildcard for prefix match
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();

                        // Load unit data into the DataTable
                        adapter.Fill(dataTable);

                        if (dataTable.Rows.Count > 0)
                        {
                            UnitListPrintDGV.DataSource = dataTable;

                            // Adjust columns for readability
                            foreach (DataGridViewColumn column in UnitListPrintDGV.Columns)
                            {
                                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                                column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                                column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                            }

                            // Set specific column to fill mode if exists
                            if (UnitListPrintDGV.Columns.Contains("Login Start Time"))
                            {
                                UnitListPrintDGV.Columns["Login Start Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                            }

                            // Adjust layout after setting modes
                            UnitListPrintDGV.AutoResizeColumns();
                        }
                        else
                        {

                            UnitListPrintDGV.DataSource = null; // Clear DataGridView if no data
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading archived unit data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ArchiveUnitListDGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the Unarchive button column is clicked
            if (e.RowIndex >= 0 && ArchiveUnitListDGV.Columns[e.ColumnIndex].Name == "UnArchiveBtmC")
            {
                string computerName = ArchiveUnitListDGV.Rows[e.RowIndex].Cells["Unit"].Value?.ToString();

                HideFilterPanel();

                if (!string.IsNullOrEmpty(computerName))
                {
                    DialogResult result = MessageBox.Show("Are you sure you want to unarchive this unit?", "Confirm Unarchive", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            string updateQuery = "UPDATE UnitList SET ArchiveStatus = 'Active' WHERE ComputerName = @ComputerName";

                            using (SqlCommand command = new SqlCommand(updateQuery, connection))
                            {
                                command.Parameters.AddWithValue("@ComputerName", computerName);

                                try
                                {
                                    connection.Open();
                                    int rowsAffected = command.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        MessageBox.Show("Unit successfully unarchived.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        LoadUnitListData(); // Reload the data to reflect changes
                                        ArchiveLoadUnitListData();
                                        PrintArchiveLoadUnitListData();
                                        PrintLoadUnitListData();

                                        // Insert a notification about the unarchive action
                                        string adminName = AdminNameLabel.Text; // Replace with the actual label for the admin's name
                                        InsertNotificationForUnitUnarchive(adminName, computerName);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to unarchive the unit. Unit not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Error unarchiving the unit: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void InsertNotificationForUnitUnarchive(string adminName, string computerName)
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
                    string message = $"Admin name {adminName} unarchived unit '{computerName}' at {timestamp:yyyy-MM-dd HH:mm:ss}";
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                    cmd.Parameters.AddWithValue("@AdminID", adminID);
                    cmd.Parameters.AddWithValue("@NotificationType", "Information");
                    cmd.Parameters.AddWithValue("@NotificationKind", "UnarchiveUnit");

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







        //FilterButton// Checkbox event handlers
private void FilterUnitNameCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterStatusCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterCurrentUserCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterLoginStartCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterLastUserlCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterLastUsedDateCBK_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterTotalStorageCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterAvailableStorageCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterRamCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterIPCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterProcessorCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();
private void FilterDateRegisteredCKB_CheckedChanged(object sender, EventArgs e) => UpdateColumnVisibility();


        // Method to update column visibility based on checkbox states
        private void UpdateColumnVisibility()
        {
            // Check if UnitListDGV has columns before updating visibility
            if (UnitListDGV.Columns.Count > 0)
            {
                UnitListDGV.Columns["Unit"].Visible = FilterUnitNameCKB.Checked;
                UnitListDGV.Columns["Status"].Visible = FilterStatusCBK.Checked;
                UnitListDGV.Columns["Current User"].Visible = FilterCurrentUserCKB.Checked;
                UnitListDGV.Columns["Login Start Time"].Visible = FilterLoginStartCBK.Checked;
                UnitListDGV.Columns["Last User"].Visible = FilterLastUserlCKB.Checked;
                UnitListDGV.Columns["Last Used Date"].Visible = FilterLastUsedDateCBK.Checked;
                UnitListDGV.Columns["Total Storage"].Visible = FilterTotalStorageCKB.Checked;
                UnitListDGV.Columns["Available Storage"].Visible = FilterAvailableStorageCKB.Checked;
                UnitListDGV.Columns["RAM"].Visible = FilterRamCKB.Checked;
                UnitListDGV.Columns["IP Address"].Visible = FilterIPCKB.Checked;
                UnitListDGV.Columns["Processor"].Visible = FilterProcessorCKB.Checked;
                UnitListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
            }

            // Check if ArchiveUnitListDGV has columns before updating visibility
            if (ArchiveUnitListDGV.Columns.Count > 0)
            {

                ArchiveUnitListDGV.Columns["Unit"].Visible = FilterUnitNameCKB.Checked;
                ArchiveUnitListDGV.Columns["Status"].Visible = FilterStatusCBK.Checked;
                ArchiveUnitListDGV.Columns["Current User"].Visible = FilterCurrentUserCKB.Checked;
                ArchiveUnitListDGV.Columns["Login Start Time"].Visible = FilterLoginStartCBK.Checked;
                ArchiveUnitListDGV.Columns["Last User"].Visible = FilterLastUserlCKB.Checked;
                ArchiveUnitListDGV.Columns["Last Used Date"].Visible = FilterLastUsedDateCBK.Checked;
                ArchiveUnitListDGV.Columns["Total Storage"].Visible = FilterTotalStorageCKB.Checked;
                ArchiveUnitListDGV.Columns["Available Storage"].Visible = FilterAvailableStorageCKB.Checked;
                ArchiveUnitListDGV.Columns["RAM"].Visible = FilterRamCKB.Checked;
                ArchiveUnitListDGV.Columns["IP Address"].Visible = FilterIPCKB.Checked;
                ArchiveUnitListDGV.Columns["Processor"].Visible = FilterProcessorCKB.Checked;
                ArchiveUnitListDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
            }

            // Check if UnitListPrintDGV has columns before updating visibility
            if (UnitListPrintDGV.Columns.Count > 0)
            {

                UnitListPrintDGV.Columns["Unit"].Visible = FilterUnitNameCKB.Checked;
                UnitListPrintDGV.Columns["Status"].Visible = FilterStatusCBK.Checked;
                UnitListPrintDGV.Columns["Current User"].Visible = FilterCurrentUserCKB.Checked;
                UnitListPrintDGV.Columns["Login Start Time"].Visible = FilterLoginStartCBK.Checked;
                UnitListPrintDGV.Columns["Last User"].Visible = FilterLastUserlCKB.Checked;
                UnitListPrintDGV.Columns["Last Used Date"].Visible = FilterLastUsedDateCBK.Checked;
                UnitListPrintDGV.Columns["Total Storage"].Visible = FilterTotalStorageCKB.Checked;
                UnitListPrintDGV.Columns["Available Storage"].Visible = FilterAvailableStorageCKB.Checked;
                UnitListPrintDGV.Columns["RAM"].Visible = FilterRamCKB.Checked;
                UnitListPrintDGV.Columns["IP Address"].Visible = FilterIPCKB.Checked;
                UnitListPrintDGV.Columns["Processor"].Visible = FilterProcessorCKB.Checked;
                UnitListPrintDGV.Columns["Date Registered"].Visible = FilterDateRegisteredCKB.Checked;
            }
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





        //Offline toggle Button
        private void UserStatusTBTM_CheckedChanged(object sender, EventArgs e)
        {
            if (UnitStatusTBTM.Checked)
            {
                // When checked, sort "Online" statuses at the top
                SortDataGridViews("Online");
                UnitStatusTBTM.Text = "Online";
            }
            else
            {
                // When unchecked, sort "Offline" statuses at the top
                SortDataGridViews("Offline");
                UnitStatusTBTM.Text = "Offline";
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






        private void InitializeComboBox()
        {
            // Add items to the ComboBox
            ComLabCB.Items.Add("All Units");
            ComLabCB.Items.Add("ComLab1");
            ComLabCB.Items.Add("ComLab2");

            // Set the default selected item to "All Units"
            ComLabCB.SelectedItem = "All Units";

            // Add items to the ComboBox
            ArchiveComLabCB.Items.Add("All Units");
            ArchiveComLabCB.Items.Add("ComLab1");
            ArchiveComLabCB.Items.Add("ComLab2");

            // Set the default selected item to "All Units"
            ArchiveComLabCB.SelectedItem = "All Units";
        }
        private void ComLabCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Prevent recursion by temporarily disabling the event handling
            ComLabCB.SelectedIndexChanged -= ComLabCB_SelectedIndexChanged;

            string selectedLab = ComLabCB.SelectedItem.ToString();

            if (selectedLab == "All Units")
            {
                // Call the functions to load all data
                PrintLoadUnitListData();
                PrintArchiveLoadUnitListData();
                LoadUnitListData();
            }
            else if (selectedLab == "ComLab1" || selectedLab == "ComLab2")
            {
                // Filter data based on the selected lab name
                LoadFilteredUnitListData(selectedLab, "Active", UnitListDGV);
                LoadFilteredUnitListData(selectedLab, "Inactive", ArchiveUnitListDGV);
                PrintLoadUnitListData();
            }

            // Sync the selection between both ComboBoxes
            ArchiveComLabCB.SelectedItem = ComLabCB.SelectedItem;

            ComLabCB.SelectedIndexChanged += ComLabCB_SelectedIndexChanged;
        }

        private void ArchiveComLabCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Prevent recursion by temporarily disabling the event handling
            ComLabCB.SelectedIndexChanged -= ComLabCB_SelectedIndexChanged;

            string ArchiveselectedLab = ArchiveComLabCB.SelectedItem.ToString();


            if (ArchiveselectedLab == "All Units")
            {
                // Call the functions to load all data
                PrintLoadUnitListData();
                PrintArchiveLoadUnitListData();
                ComboBoxArchiveLoadUnitListData();
            }
            else if (ArchiveselectedLab == "ComLab1" || ArchiveselectedLab == "ComLab2")
            {
                // Filter data based on the selected lab name
                LoadFilteredUnitListData(ArchiveselectedLab, "Active", UnitListDGV);
                LoadFilteredUnitListData(ArchiveselectedLab, "Inactive", ArchiveUnitListDGV);

                PrintArchiveLoadUnitListData();
            }

            // Sync the selection between both ComboBoxes
            ComLabCB.SelectedItem = ArchiveComLabCB.SelectedItem;

            ComLabCB.SelectedIndexChanged += ComLabCB_SelectedIndexChanged;
        }
        private void LoadFilteredUnitListData(string labPrefix, string archiveStatus, DataGridView targetDGV)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Use SQL LIKE operator to filter data starting with ComLab1 or ComLab2
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
        WHERE ComputerName LIKE @LabPrefix AND ArchiveStatus = @ArchiveStatus";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LabPrefix", labPrefix + "%");  // Add wildcard for filtering based on prefix
                    command.Parameters.AddWithValue("@ArchiveStatus", archiveStatus);

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();

                    try
                    {
                        connection.Open();
                        adapter.Fill(dataTable);
                        targetDGV.DataSource = dataTable;

                        // Set column styles
                        foreach (DataGridViewColumn column in targetDGV.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                            column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                            column.HeaderCell.Style.WrapMode = DataGridViewTriState.True;
                        }

                        // Set the "Current User" column to AutoSizeMode.Fill if present
                        if (targetDGV.Columns.Contains("Login Start Time"))
                        {
                            targetDGV.Columns["Login Start Time"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }

                        // Refresh layout
                        targetDGV.AutoResizeColumns();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading filtered data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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
            HideFilterPanel();
            UnitStatusTBTM.Enabled = false;

            //Prints
            ArchivePrintLink.BringToFront();
            PrintExcelArchive.BringToFront();

            UnitFilterToggleBtm.Checked = false;
            UnitFilterPnl.Visible = false;
            ArchiveUnitListDGV.BringToFront();


            ComLabCB.SelectedItem = "All Units";
            ArchiveComLabCB.SelectedItem = "All Units";
            string ArchiveselectedLab = ArchiveComLabCB.SelectedItem.ToString();


            if (ArchiveselectedLab == "All Units")
            {
                // Call the functions to load all data
                PrintLoadUnitListData();
                PrintArchiveLoadUnitListData();
                LoadUnitListData();
                ArchiveLoadUnitListData();
            }
            else if (ArchiveselectedLab == "ComLab1" || ArchiveselectedLab == "ComLab2")
            {
                // Filter data based on the selected lab name
                LoadFilteredUnitListData(ArchiveselectedLab, "Active", UnitListDGV);
                LoadFilteredUnitListData(ArchiveselectedLab, "Inactive", ArchiveUnitListDGV);

                PrintArchiveLoadUnitListData();
            }

            ArchiveComLabCB.BringToFront();

        }

        private void UserListPanelShow_Click(object sender, EventArgs e)
        {
            UnitStatusTBTM.Enabled = true;
            HideFilterPanel();

            //Prints
            PrintLink.BringToFront();
            PrintExcel.BringToFront();

            NoArchiveListLabel.Visible = false;
            UnitFilterToggleBtm.Checked = false;
            UnitFilterPnl.Visible = false;
            UnitListDGV.BringToFront();

            ComLabCB.SelectedItem = "All Units";
            ArchiveComLabCB.SelectedItem = "All Units";

            string selectedLab = ComLabCB.SelectedItem.ToString();


            if (selectedLab == "All Units")
            {
                // Call the functions to load all data
                PrintLoadUnitListData();
                PrintArchiveLoadUnitListData();
                LoadUnitListData();
                ArchiveLoadUnitListData();
            }
            else if (selectedLab == "ComLab1" || selectedLab == "ComLab2")
            {
                // Filter data based on the selected lab name
                LoadFilteredUnitListData(selectedLab, "Active", UnitListDGV);
                LoadFilteredUnitListData(selectedLab, "Inactive", ArchiveUnitListDGV);

                PrintLoadUnitListData();
            }


            ComLabCB.BringToFront();
            NoArchiveListLabel.Visible = false;
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








        // Search bar event handler
        private void UserSearchBar_TextChanged(object sender, EventArgs e)
        {
            string searchText = UserSearchBar.Text;
            ApplySearchFilter(UnitListDGV, searchText);
            ApplySearchFilter(ArchiveUnitListDGV, searchText);
            ApplySearchFilter(UnitListPrintDGV, searchText);

            UnitFilterToggleBtm.Checked = false;
            UnitFilterPnl.Visible = false;

            ComLabCB.SelectedItem = "All Units";
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
        private void ArchivePrintLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (UnitListPrintDGV.Rows.Count > 0)
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
            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            }

        private void PrintLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (UnitListPrintDGV.Rows.Count > 0)
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

            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
    }


      
        
        

        private void UserSearchBar_Click(object sender, EventArgs e)
        {
            HideFilterPanel();
        }

    }  
}
