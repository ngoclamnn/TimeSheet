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
        public DateTime osdTimeOut { get; set; }

        public bool IsCurrentDate
        {
            get
            {
                return DateTime.Now.DayOfWeek == osdTimeIn.DayOfWeek;
            }
        }
        public string DisplayTimeOut
        {
            get
            {
                return (osdTimeOut == osdTimeIn) ? "--:--" : osdTimeOut.ToString("t");
            }
        }
        public string DisplayTotalHour
        {
            get
            {
                var ts = TimeSpan.FromHours(TotalHour);
                return ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2");
            }
        }
        public string DisplayDayOfWeek
        {
            get
            {
                return osdTimeIn.DayOfWeek.ToString();
            }
        }
        public double TotalHour
        {
            get
            {
                // forgot checkin
                if (osdHoursPerDay == 0 || (osdHoursPerDay - 0.5 < 6))
                {
                    var now = DateTime.Now;
                    var noonTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0);
                    if (now < noonTime)
                    {
                        return (DateTime.Now - osdTimeIn).TotalHours;
                    }
                    return (DateTime.Now - osdTimeIn).TotalHours - 1.5;
                }

                return osdHoursPerDay - 0.5;
            }
        }
        public DateTime osdTimeIn { get; set; }
        public string DisplayMissing
        {
            get
            {
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

        public DateTime Expected
        {
            get
            {
                return osdTimeIn.AddHours(9.5);
            }
        }

        public string DisplayExpected
        {
            get
            {
                return (TotalHour < 12 && TotalHour > -0.5) ? Expected.ToString("t") : "--:--";
            }
        }

    }
}
