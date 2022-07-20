using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enrollment
{
    public class EmployeeModel : Model
    {
        protected DataManager dataManager = new DataManager();

        public int id { get; set; }
        public int emp_id { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public int user_status { get; set; }
        public string status { get; set; }
        public object user_roles { get; set; }
        public object emp_statuses { get; set; }
        public string user_statuses { get; set; }
        public string full_name { get; set; }

        public override string getClassName()
        {
            return this.GetType().Name;
        }

        public override Model[] getData()
        {
            return this.dataManager.getEmployees(1);
        }

        public override string getFileName()
        {
            return "employees.json";
        }
    }
}
