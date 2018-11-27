using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheet.Business.Models
{
    public class TimeSheetInfo
    {
        public string osdTimesheetId { get; set; }
        public DateTime? osdTimeIn { get; set; }
        public DateTime? osdTimeOut { get; set; }
        public string osdFullNameEN { get; set; }
        public string osdFullNameVN { get; set; }
        public double osdHoursPerDay { get; set; }
        public double osdHoursPerDayUntilNow { get; set; }
        public double osdFullHoursPerday { get; set; }
        public DateTime osdWorkingDate { get; set; }
        public string LastName { get; set; }
        public string MidName { get; set; }
        public string FirstName { get; set; }
        public int LimitedTime { get; set; }
        public bool IsCurrentDate
        {
            get;set;
        }
        public string DisplayTimeOut
        {
            get;set;
        }
        public string DisplayTotalHour
        {
            get;set;
        }
        public string DisplayDayOfWeek
        {
            get;set;
        }
        public double TotalHour
        {
            get;set;
        }

        public string DisplayMissing
        {
            get;set;
        }
        public double Missing
        {
            get
            {
                return 8 - TotalHour - 8 * AnnualLeave;
            }
        }
        public DateTime Expected
        {
            get
            {
                return osdTimeIn.Value.AddHours((AnnualLeave > 0 ? 8 * AnnualLeave : 9.5));
            }
        }

        public string DisplayExpected
        {
            get
            {
                if (!osdTimeIn.HasValue)
                    return "--:--";
                return (TotalHour < 12 && TotalHour > -0.5) ? Expected.ToString("t") : "--:--";
            }
        }

        public double AnnualLeave
        {
            get; set;
        }
    }
}
