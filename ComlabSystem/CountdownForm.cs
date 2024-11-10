using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ComlabSystem
{
    public partial class CountdownForm : Form
    {
        private int countdownTime = 60; // Countdown starting from 60 seconds
        private Timer countdownTimer;
        public CountdownForm()
        {
            InitializeComponent();
            SetupCountdownTimer();
        }

        private void CountdownForm_Load(object sender, EventArgs e)
        {
            CountLabel.Text = countdownTime.ToString();
            countdownTimer.Start();
        }

        private void SetupCountdownTimer()
        {
            // Initialize the timer
            countdownTimer = new Timer();
            countdownTimer.Interval = 1000; // 1 second intervals
            countdownTimer.Tick += CountdownTimer_Tick;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            if (countdownTime > 0)
            {
                countdownTime--;
                CountLabel.Text = countdownTime.ToString();
            }
            else
            {
                countdownTimer.Stop();
                this.Close(); // Close the form when countdown reaches 0
            }
        }

        private void UserLoginBtm_Click(object sender, EventArgs e)
        {
            countdownTimer.Stop(); // Stop the timer when the button is clicked
            this.Close(); // Close the form immediately
        }
    }
}
