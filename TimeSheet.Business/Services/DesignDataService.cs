using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using TimeSheet.Business.Models;

namespace TimeSheet.Business.Services
{
    public class DesignDataService : IDataService
    {
        public List<TimeSheetInfo> GetData(string empId, bool useCheckOutDataForCurrentData = true)
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
                var response = httpClient.GetAsync($"EmployeeTimeSheet/get?FromDate={fromDate}&ToDate={today}&EmployeeID={empId}")
                    .GetAwaiter().GetResult();

                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var data = JsonConvert.DeserializeObject<List<TimeSheetInfo>>(json, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
                return data;
            }
        }
    }
}
