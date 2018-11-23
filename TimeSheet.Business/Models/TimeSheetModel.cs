using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheet.Models
{
    public class TimeSheetModel
    {

        public TimeSheetModel()
        {

        }

        public ObservableCollection<TimeSheetInfo> TimeSheetInfos
        {
            get; set;
        }

        public void LoadTimeSheetInfos()
        {

        }
    }

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
            get
            {
                if (!osdTimeIn.HasValue)
                    return false;
                return DateTime.Now.DayOfWeek == osdTimeIn.Value.DayOfWeek;
            }
        }
        public string DisplayTimeOut
        {
            get
            {
                if (!osdTimeIn.HasValue)
                    return "--:--";
                return (osdTimeOut == osdTimeIn) ? "--:--" : osdTimeOut.Value.ToString("t");
            }
        }
        public string DisplayTotalHour
        {
            get
            {
                if (!osdTimeIn.HasValue)
                    return "--:--";
                var ts = TimeSpan.FromHours(TotalHour);
                return ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2");
            }
        }
        public string DisplayDayOfWeek
        {
            get
            {
                if (!osdTimeIn.HasValue)
                    return "--:--";
                return osdTimeIn.Value.DayOfWeek.ToString();
            }
        }
        public double TotalHour
        {
            get
            {
                if (!osdTimeIn.HasValue)
                    return 0;
                // forgot checkin
                if (osdHoursPerDay == 0 || (osdHoursPerDay - 0.5 < 6))
                {
                    var now = DateTime.Now;
                    var noonTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0);
                    if (now < noonTime)
                    {
                        return (DateTime.Now - osdTimeIn).Value.TotalHours;
                    }
                    return (DateTime.Now - osdTimeIn).Value.TotalHours - 1.5;
                }

                return osdHoursPerDay - 0.5;
            }
        }
      
        public string DisplayMissing
        {
            get
            {
                if (!osdTimeIn.HasValue)
                    return "--:--";
                var ts = TimeSpan.FromHours(Missing);
                return ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2");
            }
        }
        public double Missing
        {
            get
            {
                return 8 - TotalHour;
            }
        }
        public DateTime Expected
        {
            get
            {
                return osdTimeIn.Value.AddHours(9.5);
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
      

        

    }
}
