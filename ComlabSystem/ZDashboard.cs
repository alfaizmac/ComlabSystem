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
            AllNotificationFunction();

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
    }
}  
