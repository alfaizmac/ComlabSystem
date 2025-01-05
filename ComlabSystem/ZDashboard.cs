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
using System.Windows.Forms.DataVisualization.Charting;
using Guna.UI2.WinForms;

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
            AllNotificationFunction();

            WeeklyReportFirst();
            WeeklyReportSecond();

            UserTimeUsageChart();
            UnitTimeUsageChart();

        }

        private void UserUI_Load(object sender, EventArgs e)
        {
            MainPNL.BringToFront();
            AllNotificationFunction();
            CurrentOnline();

        }

        //Notification code

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
                    NotificationDGV.DataSource = dataTable;

                    // Hide the NotificationID and IsRead columns
                    NotificationDGV.Columns["NotificationID"].Visible = false;
                    NotificationDGV.Columns["IsRead"].Visible = false;
                    NotificationDGV.Columns["Timestamp"].Visible = false;

                    // Set DataGridView AutoSizeMode for other columns to Fill
                    foreach (DataGridViewColumn column in NotificationDGV.Columns)
                    {
                        if (column.Name != "NotificationID" && column.Name != "IsRead")
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }
                    }

                    // Change row color based on IsRead column (still checking but column is hidden)
                    foreach (DataGridViewRow row in NotificationDGV.Rows)
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





        private void AdminNameLabel_Click(object sender, EventArgs e)
        {

        }

        private void userchart_Click(object sender, EventArgs e)
        {

        }

        private void NotificationDGV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Get the IsRead value of the clicked row
                var isRead = Convert.ToBoolean(NotificationDGV.Rows[e.RowIndex].Cells["IsRead"].Value);

                if (isRead == false) // Only update if it is not already marked as read
                {
                    // Get the NotificationID value of the clicked row (now it exists because it's part of the query)
                    int notificationId = Convert.ToInt32(NotificationDGV.Rows[e.RowIndex].Cells["NotificationID"].Value);

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
                            NotificationDGV.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245); // Change row color to read
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error updating notification status: " + ex.Message);
                        }
                    }
                }
            }
        }

        



        //new

        private void WeeklyReportFirst()
        {
            // SQL query to get the latest record from WeeklyReports
            string query = @"SELECT TOP 1 ImproperShutdownCount, UsageFrequency
                     FROM WeeklyReports
                     ORDER BY ReportID DESC"; // assuming ReportID is an auto-increment column

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Display the latest data in the labels
                                TotalImproperShutdownLabel.Text = reader["ImproperShutdownCount"].ToString();
                                TotalUsageFrequencyLabel.Text = reader["UsageFrequency"].ToString();
                            }
                            else
                            {
                                // If no data is found, set default values for the labels
                                TotalImproperShutdownLabel.Text = "0";
                                TotalUsageFrequencyLabel.Text = "0";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading the Weekly Report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void WeeklyReportSecond()
        {
            // SQL query to get the number of records from UserList
            string userCountQuery = @"SELECT COUNT(*) FROM UserList";

            // SQL query to get the number of records from UnitList
            string unitCountQuery = @"SELECT COUNT(*) FROM UnitList";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Get the count for UserList (students)
                    using (SqlCommand userCommand = new SqlCommand(userCountQuery, connection))
                    {
                        int totalStudents = (int)userCommand.ExecuteScalar();
                        TotalStudentLabel.Text = totalStudents.ToString();
                    }

                    // Get the count for UnitList (computers)
                    using (SqlCommand unitCommand = new SqlCommand(unitCountQuery, connection))
                    {
                        int totalComputers = (int)unitCommand.ExecuteScalar();
                        TotalComputerLabel.Text = totalComputers.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading the Weekly Report data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void UserTimeUsageChart()
        {
            // SQL query to get the StudentID and TotalHoursUsed (top 5 highest TotalHoursUsed)
            string query = @"
        SELECT TOP 5 StudentID, TotalHoursUsed 
        FROM UserList
        ORDER BY 
            CASE 
                WHEN CHARINDEX(':', TotalHoursUsed) > 0 THEN 
                    DATEPART(HOUR, CAST(TotalHoursUsed AS TIME)) 
                ELSE 0 
            END DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Create a SqlCommand to execute the query
                    SqlCommand command = new SqlCommand(query, connection);

                    // Execute the query and retrieve the data
                    SqlDataReader reader = command.ExecuteReader();

                    // Clear any previous chart data
                    userchart.Series.Clear();
                    userchart.Legends.Clear();

                    // Add a new series to the chart
                    Series series = new Series
                    {
                        Name = "Hours",
                        IsValueShownAsLabel = true,
                        ChartType = SeriesChartType.Bar,
                        BorderWidth = 2, // Set border width for the bars
                        BorderColor = Color.Black // Set border color for the bars
                    };

                    // Set the width of the bars (adjust this value to make them bigger or smaller)
                    series["BarWidth"] = "1.0"; // Increase the bar width (1.0 is the default)

                    // Add the series to the chart
                    userchart.Series.Add(series);

                    // Create a legend for the chart (optional)
                    userchart.Legends.Add("Legend");

                    // Loop through the data and add it to the chart
                    while (reader.Read())
                    {
                        string studentID = reader["StudentID"].ToString();
                        string totalHoursUsedStr = reader["TotalHoursUsed"].ToString();

                        // Convert the TotalHoursUsed time string (HH:mm:ss) to an integer representing total hours
                        int totalHoursUsed = ConvertTimeToTotalHours(totalHoursUsedStr);

                        // Add the data point to the chart
                        series.Points.AddXY(studentID, totalHoursUsed);
                    }

                    // Set chart area properties (optional)
                    userchart.ChartAreas[0].AxisX.Title = ""; // Remove X-axis title
                    userchart.ChartAreas[0].AxisY.Title = ""; // Remove Y-axis title

                    // Remove axis labels if you prefer
                    userchart.ChartAreas[0].AxisX.LabelStyle.IsStaggered = false; // Remove staggered labels
                    userchart.ChartAreas[0].AxisY.LabelStyle.Angle = 0; // Reset label angle
                    userchart.ChartAreas[0].AxisX.LabelStyle.IsEndLabelVisible = true; // Ensure labels are visible

                    // Adjust axis scale to fit the data better (optional)
                    userchart.ChartAreas[0].AxisX.Interval = 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading the chart data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Helper function to convert HH:mm:ss format to total hours as integer
        private int ConvertTimeToTotalHours(string timeStr)
        {
            try
            {
                // Parse the time string in HH:mm:ss format
                TimeSpan time = TimeSpan.Parse(timeStr);

                // Return the total hours (integer part only)
                return (int)time.TotalHours;
            }
            catch (FormatException)
            {
                // Return 0 if the time string is not in valid format
                return 0;
            }
        }

        private void UnitTimeUsageChart()
        {
            // SQL query to get the ComputerName and TotalHoursUsed (top 5 highest TotalHoursUsed)
            string query = @"
        SELECT TOP 5 ComputerName, TotalHoursUsed 
        FROM UnitList
        ORDER BY 
            CASE 
                WHEN CHARINDEX(':', TotalHoursUsed) > 0 THEN 
                    DATEPART(HOUR, CAST(TotalHoursUsed AS TIME)) 
                ELSE 0 
            END DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Create a SqlCommand to execute the query
                    SqlCommand command = new SqlCommand(query, connection);

                    // Execute the query and retrieve the data
                    SqlDataReader reader = command.ExecuteReader();

                    // Clear any previous chart data
                    UnitChart.Series.Clear();
                    UnitChart.Legends.Clear();

                    // Add a new series to the chart
                    Series series = new Series
                    {
                        Name = "Hours",
                        IsValueShownAsLabel = true,
                        ChartType = SeriesChartType.Bar,
                        BorderWidth = 2, // Set border width for the bars
                        BorderColor = Color.Black // Set border color for the bars
                    };

                    // Set the width of the bars (adjust this value to make them bigger or smaller)
                    series["BarWidth"] = "1.0"; // Increase the bar width (1.0 is the default)

                    // Add the series to the chart
                    UnitChart.Series.Add(series);

                    // Create a legend for the chart (optional)
                    UnitChart.Legends.Add("Legend");

                    // Loop through the data and add it to the chart
                    while (reader.Read())
                    {
                        string computerName = reader["ComputerName"].ToString();
                        string totalHoursUsedStr = reader["TotalHoursUsed"].ToString();

                        // Convert the TotalHoursUsed time string (HH:mm:ss) to an integer representing total hours
                        int totalHoursUsed = ConvertTimeToTotalHourst(totalHoursUsedStr);

                        // Add the data point to the chart
                        series.Points.AddXY(computerName, totalHoursUsed);
                    }

                    // Set chart area properties (optional)
                    UnitChart.ChartAreas[0].AxisX.Title = ""; // Remove X-axis title
                    UnitChart.ChartAreas[0].AxisY.Title = ""; // Remove Y-axis title

                    // Remove axis labels if you prefer
                    UnitChart.ChartAreas[0].AxisX.LabelStyle.IsStaggered = false; // Remove staggered labels
                    UnitChart.ChartAreas[0].AxisY.LabelStyle.Angle = 0; // Reset label angle
                    UnitChart.ChartAreas[0].AxisX.LabelStyle.IsEndLabelVisible = true; // Ensure labels are visible

                    // Adjust axis scale to fit the data better (optional)
                    UnitChart.ChartAreas[0].AxisX.Interval = 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading the chart data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Helper function to convert HH:mm:ss format to total hours as integer
        private int ConvertTimeToTotalHourst(string timeStr)
        {
            try
            {
                // Parse the time string in HH:mm:ss format
                TimeSpan time = TimeSpan.Parse(timeStr);

                // Return the total hours (integer part only)
                return (int)time.TotalHours;
            }
            catch (FormatException)
            {
                // Return 0 if the time string is not in valid format
                return 0;
            }
        }








        private void UpdateImproperShutdownLabelLocation()
        {
            // SQL query to get the total count of UsageFrequency from WeeklyReports table
            string query = @"SELECT SUM(ImproperShutdownCount) FROM WeeklyReports";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Create the SQL command to execute the query
                    SqlCommand command = new SqlCommand(query, connection);

                    // Execute the query and get the total count
                    object result = command.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int count))
                    {
                        TotalImproperShutdownLabel.Text = count.ToString("D2"); // Display count as two digits if it's less than 10

                        // Adjust location based on count value
                        if (count < 10)
                        {
                            TotalImproperShutdownLabel.Location = new Point(71, 9);
                            // Adjust other controls based on count, if necessary
                        }
                        else if (count < 100)
                        {
                            TotalImproperShutdownLabel.Location = new Point(64, 9);
                            // Adjust other controls based on count, if necessary
                        }
                        else if (count < 1000)
                        {
                            TotalImproperShutdownLabel.Location = new Point(45, 9);
                            // Adjust other controls based on count, if necessary
                        }

                        else if (count >= 10000)
                        {
                            TotalImproperShutdownLabel.Location = new Point(36, 9);
                            // Adjust other controls based on count, if necessary
                        }
                    }
                    else
                    {
                        // Handle case where the result is null or not an integer
                        MessageBox.Show("Unable to retrieve the count of UsageFrequency.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void UpdateUsageFrequencyLabelLocation()
        {
            // SQL query to get the total count of UsageFrequency from WeeklyReports table
            string query = @"SELECT SUM(UsageFrequency) FROM WeeklyReports";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Create the SQL command to execute the query
                    SqlCommand command = new SqlCommand(query, connection);

                    // Execute the query and get the total count
                    object result = command.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int count))
                    {
                        TotalUsageFrequencyLabel.Text = count.ToString("D2"); // Display count as two digits if it's less than 10

                        // Adjust location based on count value
                        if (count < 10)
                        {
                            TotalUsageFrequencyLabel.Location = new Point(71, 9);
                            // Adjust other controls based on count, if necessary
                        }
                        else if (count < 100)
                        {
                            TotalUsageFrequencyLabel.Location = new Point(64, 9);
                            // Adjust other controls based on count, if necessary
                        }
                        else if (count < 1000)
                        {
                            TotalUsageFrequencyLabel.Location = new Point(45, 9);
                            // Adjust other controls based on count, if necessary
                        }

                        else if (count >= 10000)
                        {
                            TotalUsageFrequencyLabel.Location = new Point(36, 9);
                            // Adjust other controls based on count, if necessary
                        }
                    }
                    else
                    {
                        // Handle case where the result is null or not an integer
                        MessageBox.Show("Unable to retrieve the count of UsageFrequency.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


    }
}  
