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
    public partial class Settingss : UserControl
    {

        private string connectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

        public string AdminName
        {
            set { AdminNameLabel.Text = value; }

        }



        public Settingss()
        {
            InitializeComponent();
;




        }


        private void UserUI_Load(object sender, EventArgs e)
        {

         


        }






    }  
}
