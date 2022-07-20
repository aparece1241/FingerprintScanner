using Enrollment;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;

namespace Enrollment
{
    public partial class frmMain : Form
    {
        private DPFP.Template Template;
        private bool isConnected;
        private bool isSuccess;
        private string companyName;
        DataManager dal;


        public frmMain()
        {
            setUser();
            InitializeComponent();
        }

        private void main_Load(object sender, EventArgs e)
        {
            try
            {
                isConnected = RealTimeHandler.isInternetConnnected;
                isSuccess = RealTimeHandler.isInternetConnnected;

                Config company_name = new Config();
                companyName = company_name.company;
                this.Text = companyName;

                txtUserName.Text = "Admin";
                txtPassword.Text = "@dm1n";

                // Create the needed files
                // asynchronous loading of files
                Task.Factory.StartNew(() => {
                    FileHandler.saveData(new EmployeeModel());
                    FileHandler.saveData(new AttendanceModel());
                    FileHandler.IsDataLoaded = true;
                    Console.WriteLine("File Created and loaded!");
                });
            }
            catch (Exception exception)
            {
                MessageBox.Show(String.Format("Error: {0} {1}", exception.Message, companyName), "fingerprint enrollment");
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            isSuccess = true;
            isSuccess = ValidateUser();

            if (!isSuccess)
            {
                displayStatus();
                return;
            }

            this.Hide();
            frmRegistration frm = new frmRegistration();
            frm.Closed += (s, args) => this.Close();
            frm.Show();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            isSuccess = true;
            isSuccess = ValidateUser();

            if (!isSuccess)
            {
                displayStatus();
                return;
            }

            this.Hide();
            frmVerificar frm = new frmVerificar();
            frm.Closed += (s, args) => this.Close();
            frm.Show();
        }

        private void OnTemplate(DPFP.Template template)
        {
            this.Invoke(new Function(delegate ()
            {
                Template = template;

                if (template != null)
                {
                    MessageBox.Show("the fingerprint template is ready for fingerprint verification.", "fingerprint enrollment");
                }
                else
                {
                    MessageBox.Show("the fingerprint template is not valid. repeat fingerprint enrollment.", "fingerprint enrollment");
                }
            }));
        }

        private bool ValidateUser()
        {
            string Uname = txtUserName.Text.Trim();
            string Pword = txtPassword.Text.Trim();
            var response = Login.Authenticate(Uname, Pword);

            return response.Value;
        }

        private void setUser()
        {
            int uname = "Admin".GetHashCode();
            int pword = "@dm1n".GetHashCode();

            // set user adiministrator login

            string objAppConfigValue = GetSetting("Uname");
            SetSetting("Uname", uname.ToString());

            objAppConfigValue = GetSetting("Pword");
            SetSetting("Pword", pword.ToString());

            string objModifiedAppConfigValue = GetSetting("Uname");
        }

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static void SetSetting(string key, string value)
        {
            try
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                // administrator login implementation
                configuration.AppSettings.Settings[key].Value = value;
                configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            } catch (Exception e)
            {
                Console.WriteLine("Error");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblStatus.Visible = false;
            if (!isSuccess)
            {
                lblStatus.Visible = true;
                displayStatus();
            }
        }

        private void displayStatus()
        {
            int count = Convert.ToInt32(DateTime.Now.ToString("ss"));

            lblStatus.BackColor = System.Drawing.Color.White;
            lblStatus.ForeColor = System.Drawing.Color.Red;
            lblStatus.Text = getMessage();

            if (count % 2 == 0)
            {
                lblStatus.BackColor = System.Drawing.Color.Red;
                lblStatus.ForeColor = System.Drawing.Color.White;
            }

        }


        protected int interval = 0;
        private string getMessage()
        {
            string msg = "";
            btnRegister.Enabled = isConnected;
            btnRun.Enabled = isConnected;
            if (!isConnected)
            {
                base.Text = companyName + " - Unable to connect to the Server.";
                msg = "Error: Please close this app and try again later.";
            }
            else
            {
                msg = "Please check Username and/or Password.";
            };

            return msg;
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();

            Environment.Exit(0);
        }

        private void txtUserName_MouseClick(object sender, MouseEventArgs e)
        {
            isSuccess = true;
            this.txtUserName.Text = "";
        }

        private void txtPassword_MouseClick(object sender, MouseEventArgs e)
        {
            isSuccess = true;
            this.txtPassword.Text = "";
        }
    }
}

