using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enrollement
{
    public class AttendanceModel : Model
    {
        protected DataManager dataManager = new DataManager();
        
        public int id { get; set; }
        public int employee_id { get; set; }
        public string time_in { get; set; }
        public string time_out { get; set; }

        public override string getClassName()
        {
            return this.GetType().Name;
        }

        public override Model[] getData()
        {
            return this.dataManager.getAttendancesWithinNDays(4);
        }

        public override string getFileName()
        {
            return "atttendances.json";
        }
    }
}
