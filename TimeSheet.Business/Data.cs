using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace TimeSheet.Business
{
    public class Data
    {
        public List<TimeSheetInfo> GetData()
        {
            DateTime baseDate = DateTime.Today;
           
            var today = baseDate;
            var thisWeekStart = baseDate.AddDays(-(int)baseDate.DayOfWeek);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
            var fromDate = thisWeekStart.ToString("yyyy-MM-dd");
            var toDate = thisWeekEnd.ToString("yyyy-MM-dd");
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://portaladmin.orientsoftware.net/api/");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = httpClient.GetAsync($"EmployeeTimeSheet/get?FromDate={fromDate}&ToDate={today}&EmployeeID=00013")
                    .GetAwaiter().GetResult();

                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var data = JsonConvert.DeserializeObject<List<TimeSheetInfo>>(json, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
                //dataGrid.ItemsSource = data;
                //dataGrid.SelectedIndex = data.FindIndex(x => x.IsCurrentDate);
                return data;
            }
            //var totalHour = TimeSpan.FromHours(data.Sum(x => x.TotalHour));
        }
    }
}
