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
    public partial class ZLogsReport : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

        public string AdminName
        {
            set { AdminNameLabel.Text = value; }

        }


        public ZLogsReport()
        {
            InitializeComponent();



        }






        private void UserUI_Load(object sender, EventArgs e)
        {

            UserActivityReport();

            FilterUserActivityPnl.Visible = false;


            //Print
            PrintExcelReport.BringToFront();
            guna2Panel2.BringToFront();


        }



        private void UserUI_Resize(object sender, EventArgs e)
        {
            int leftpadding = 65; // Adjust this value for desired padding
            int buttompadding = 140;

            // Resize the DataGridView to fill the panel with padding
            ZListsGridPNL.Width = ZReportPnl.Width - leftpadding; // Right padding
            ZListsGridPNL.Height = ZReportPnl.Height - buttompadding;
        }




        private void PrintExcel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ReportGDV.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.ApplicationClass MExcel = new Microsoft.Office.Interop.Excel.ApplicationClass();
                MExcel.Application.Workbooks.Add(Type.Missing);
                for (int i = 1; i < ReportGDV.Columns.Count + 1; i++)
                {
                    MExcel.Cells[1, i] = ReportGDV.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < ReportGDV.Rows.Count; i++)
                {
                    for (int j = 0; j < ReportGDV.Columns.Count; j++)
                    {
                        MExcel.Cells[i + 2, j + 1] = ReportGDV.Rows[i].Cells[j].Value.ToString();
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
            if (ReportGDV.Rows.Count > 0)
            {
                // Set font for DataGridView headers before printing
                ReportGDV.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);  // Set bold font for column headers
                ReportGDV.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // Center-align headers

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
                printer.PrintPreviewDataGridView(ReportGDV);
            }
            else
            {
                MessageBox.Show("No records found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            }


















        //Head Buttons
        private void ReportUserActBtm_Click(object sender, EventArgs e)
        {
            UserActivityReport();
        }

        private void UserActivityReport()
        {
            // Create the SQL query
            string query = @"SELECT 
                        StudentID, 
                        CONCAT(FirstName, ' ', LastName) AS Name, 
                        TotalHoursUsed AS 'Overall Time Utilized', 
                        AverageSessionDuration AS 'Average Session Duration', 
                        SessionCount AS 'Total Sessions', 
                        UserImproperShutdownCount AS 'Improper Power-Offs or Multi-Unit Access', 
                        AutoShutdownCount AS 'System-Initiated Shutdown Count'
                     FROM UserList";

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
                    ReportGDV.DataSource = dataTable;

                    // Set DataGridView AutoSizeMode to Fill for all columns
                    foreach (DataGridViewColumn column in ReportGDV.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., database connection issues)
                    MessageBox.Show("Error retrieving data: " + ex.Message);
                }
            }
        }

        private void UnitUsageBtm_Click(object sender, EventArgs e)
        {
            UnitUsageReport();
        }
        private void UnitUsageReport()
        {
            // Create the SQL query
            string query = @"SELECT 
                        ComputerName AS 'Unit', 
                        DateLastUsed AS 'Last Used Date', 
                        UsageFrequency AS 'Usage Frequency', 
                        ImproperShutdownCount AS 'Improper Shutdown Count', 
                        AvailableStorage AS 'Available Storage'
                     FROM UnitList";

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
                    ReportGDV.DataSource = dataTable;

                    // Set DataGridView AutoSizeMode to Fill for all columns
                    foreach (DataGridViewColumn column in ReportGDV.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., database connection issues)
                    MessageBox.Show("Error retrieving data: " + ex.Message);
                }
            }
        }

        private void AdminActionBtm_Click(object sender, EventArgs e)
        {
            AdminActioReport();

        }
        private void AdminActioReport()
        {
            // Create the SQL query
            string query = @"SELECT 
                        AdminName AS 'Admin', 
                        Message AS 'Action', 
                        Timestamp AS 'Timestamp'
                     FROM Notifications";

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
                    ReportGDV.DataSource = dataTable;

                    // Set DataGridView AutoSizeMode to Fill for all columns
                    foreach (DataGridViewColumn column in ReportGDV.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., database connection issues)
                    MessageBox.Show("Error retrieving data: " + ex.Message);
                }
            }
        }

        private void LogsReportBtm_Click(object sender, EventArgs e)
        {
            // Create the SQL query
            string query = @"SELECT 
                        Action AS 'Action', 
                        TimeDuration AS 'Time Duration', 
                        Timestamp AS 'Timestamp'
                     FROM Logs
                     WHERE UserType = 'Student'";

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
                    ReportGDV.DataSource = dataTable;

                    // Set DataGridView AutoSizeMode to Fill for all columns
                    foreach (DataGridViewColumn column in ReportGDV.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., database connection issues)
                    MessageBox.Show("Error retrieving data: " + ex.Message);
                }
            }
        }

        private void FilteruserActivityBtm_Click(object sender, EventArgs e)
        {
            if (FilteruserActivityBtm.Checked)
            {
                // When toggle is on, show the filter panel
                FilterUserActivityPnl.Visible = true;
            }
            else
            {
                // When toggle is off, hide the filter panel
                FilterUserActivityPnl.Visible = false;
            }
        }


    }
}  
