using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enrollement
{
    public class PendingAttendance : Model
    {

        public int emp_id { get; set; }
        public DateTime date { get; set; }

        public override string getClassName()
        {
            return this.GetType().Name;
        }

        public override Model[] getData()
        {
            throw new NotImplementedException();
        }

        public override string getFileName()
        {
            return "attendance.json";
        }
    }
}
