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
    public partial class ZDashboard : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

        public string AdminName
        {
            set { AdminNameLabel.Text = value; }

        }


        public ZDashboard()
        {
            InitializeComponent();


        }

        private void UserUI_Load(object sender, EventArgs e)
        {
            MainPNL.BringToFront();
            AllNotificationFunction();
            CurrentOnline();
            MostUseDUser();

        }

        //Notification code

        private void AllNotificationFunction()
        {
            // Create the SQL query to retrieve Notification data including NotificationID
            string query = @"SELECT 
                        NotificationID, 
                        Message, 
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
                    NotificationDGV.DataSource = dataTable;

                    // Hide the NotificationID and IsRead columns
                    NotificationDGV.Columns["NotificationID"].Visible = false;
                    NotificationDGV.Columns["IsRead"].Visible = false;
                    NotificationDGV.Columns["Timestamp"].Visible = false;

                    // Set the header text of the Message column to an empty string
                    NotificationDGV.Columns["Message"].HeaderText = "";

                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., database connection issues)
                    MessageBox.Show("Error retrieving data: " + ex.Message);
                }
            }
        }

        private void CurrentOnline()
        {
            // Create a connection using the connection string
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Query to get online units and their users
                string query = @"
            SELECT 
                ComputerName AS [Unit],
                CurrentUser AS [User],
                Status
            FROM UnitList
            WHERE Status = 'Online'";

                // Create the SQL command
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();

                try
                {
                    // Open the connection
                    connection.Open();

                    // Fill the DataTable with the results from the query
                    adapter.Fill(dataTable);

                    // Bind the DataTable to the DataGridView
                    CurrentOnlineDGV.DataSource = dataTable;

                    // Optionally, you can set specific column widths and other settings
                    foreach (DataGridViewColumn column in CurrentOnlineDGV.Columns)
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    MessageBox.Show("Error loading online units: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MostUseDUser()
        {
            // Query to retrieve data from UserList and rank by TotalHoursUsed in descending order
            string query = @"
        SELECT 
            FirstName + ' ' + LastName AS UserName, 
            TotalHoursUsed 
        FROM UserList
        ORDER BY TotalHoursUsed DESC";

            // Set up the connection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    connection.Open();

                    // Create a command to execute the query
                    SqlCommand command = new SqlCommand(query, connection);

                    // Execute the query and read the data
                    SqlDataReader reader = command.ExecuteReader();

                    // Clear existing data in the chart
                    userchart.Series.Clear();

                    // Add a new series to the chart
                    var series = userchart.Series.Add("Total Hours Used");
                    series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

                    // Populate the series with data from the query
                    while (reader.Read())
                    {
                        string userName = reader["UserName"].ToString();
                        double totalHours = Convert.ToDouble(reader["TotalHoursUsed"]);

                        // Add data points to the series
                        series.Points.AddXY(userName, totalHours);
                    }

                    // Close the reader
                    reader.Close();
                }
                catch (Exception ex)
                {
                    // Handle any exceptions (e.g., database connection issues)
                    MessageBox.Show("Error retrieving chart data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void AdminNameLabel_Click(object sender, EventArgs e)
        {

        }
    }
}  
