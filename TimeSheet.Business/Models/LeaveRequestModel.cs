using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheet.Business.Models
{
    public class LeaveRequestModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string TypeOfLeave { get; set; }
        public object Reason { get; set; }
        public string osdStatus { get; set; }
        public object Comment { get; set; }
        public object Note { get; set; }
        public double TotalDay { get; set; }
        public string Team { get; set; }
        public int ID { get; set; }
        public DateTime Modified { get; set; }
    }

}
