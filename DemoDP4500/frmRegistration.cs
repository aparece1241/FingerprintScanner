using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using Enrollement;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Enrollment
{

    public partial class frmRegistration : Form
    {
        private DPFP.Template Template;
        DataManager dal;
        Dictionary<string, int> columnNames = new Dictionary<string, int> { 
            {"ID", 75}, 
            {"Last Name", 120}, 
            { "First Name", 120}, 
            { "Middle Name", 120}, 
            { "Status", 120} 
        };

        public frmRegistration()
        {
            InitializeComponent();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            string _name = txtLastName.Text + " ";
            _name += txtFirstName.Text + " ";
            _name += txtMiddleName.Text;

            if (Validate(txtStatus.Text, _name))
            {
                frmEnrollment capturar = new frmEnrollment();
                capturar.OnTemplate += this.OnTemplate;
                capturar.ShowDialog();
            }
        }

        private void OnTemplate(DPFP.Template template)
        {
            this.Invoke(new Function(delegate ()
            {
                Template = template;
                btnSave.Enabled = (Template != null);
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] _stream = Template.Bytes;
                int emp_id = Int32.Parse(txtEmpID.Text);
                dal = new DataManager();

                /* string qryStr = "UPDATE employees SET fingerprint = @fingerprint WHERE emp_id = @empId;";

                 var fingerprint = new MySqlParameter("@fingerprint", MySqlDbType.Blob);
                 var empId = new MySqlParameter("@empId", MySqlDbType.Int32);

                 fingerprint.Value = _stream;
                 empId.Value = Int32.Parse(txtEmpID.Text);

                 MySqlParameter[] myParamArray = new MySqlParameter[]{
                   fingerprint,
                   empId
                 };*/

                // Testing
                dal.updateRecordAsync(Convert.ToBase64String(_stream), emp_id);

                Template = null;

                initFields();
                loadListView("");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void frmRegistration_Load(object sender, EventArgs e)
        {
            Config objt = new Config();
            this.Text = objt.company;

            initFields();
            loadListView("");
        }

        private void initFields()
        {
            lvEmpList.Items.Clear();
               
            lvEmpList.View = View.Details;
            lvEmpList.GridLines = true;
            lvEmpList.FullRowSelect = true;

            //Add column header
            if (lvEmpList.Columns.Count > 0)
            {
                foreach (ColumnHeader column in lvEmpList.Columns)
                {
                    lvEmpList.Columns.Remove(column);
                }
            }
            
            foreach(KeyValuePair<string, int> column in columnNames)
            {
                lvEmpList.Columns.Add(column.Key, column.Value);
            }

            if (Template == null)
            {
                btnSave.Enabled = false;
            }

            displayDetails(false);
        }

        private void loadListView(string param)
        {

            try
            {
                dal = new DataManager();
 
                var employees = dal.getEmployee(0, param);
                foreach (EmployeeModel emp in employees)
                {

                    ListViewItem listitem = new ListViewItem(emp.id.ToString());
                    listitem.SubItems.Add(emp.last_name.ToString());
                    listitem.SubItems.Add(emp.first_name.ToString());
                    listitem.SubItems.Add(emp.middle_name.ToString());

                    string _status = emp.status.ToString();
                    /*string status = "Not Register";

                    if (_status != "")
                    {
                        status = "Registered";
                    }*/

                    listitem.SubItems.Add(_status);

                    lvEmpList.Items.Add(listitem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //MessageBox.Show(ex);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            initFields();
            loadListView(txtSearch.Text);
        }

        private void displayDetails(bool flag) 
        {
            txtEmpID.Text = "";
            txtLastName.Text = "";
            txtFirstName.Text = "";
            txtMiddleName.Text = "";
            txtStatus.Text = "";

            lblEmpID.Visible = flag;
            lblName.Visible = flag;
            lblStatus.Visible = flag;
            btnScan.Enabled = flag;
        }

        private void lvEmpList_DoubleClick(object sender, EventArgs e)
        {
            if (((ListView)sender).SelectedItems != null)
            { 
                displayDetails(true);

                txtEmpID.Text = lvEmpList.Items[lvEmpList.SelectedIndices[0]].SubItems[0].Text;
                txtLastName.Text = lvEmpList.Items[lvEmpList.SelectedIndices[0]].SubItems[1].Text + ",";
                txtFirstName.Text = lvEmpList.Items[lvEmpList.SelectedIndices[0]].SubItems[2].Text;
                txtMiddleName.Text = lvEmpList.Items[lvEmpList.SelectedIndices[0]].SubItems[3].Text;
                txtStatus.Text = lvEmpList.Items[lvEmpList.SelectedIndices[0]].SubItems[4].Text;
                txtStatus.ForeColor = System.Drawing.Color.Red;

                if (txtStatus.Text != "Not Register")
                {
                    txtStatus.ForeColor = System.Drawing.Color.Green;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        static bool Validate(string x, string name)
        {

            if (x == "Registered")
            {
                if ((MessageBox.Show(name + " is already Registered.\nDo you want to Continue ?", "Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
                {
                    return true;
                }
            }

            if (name.Trim() != "")
            {
                return true;
            }

            return false;
        }
        
        private void txtSearch_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            { 
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";

            initFields();
            loadListView("");
        }
    }
}
