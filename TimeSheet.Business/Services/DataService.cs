﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using TimeSheet.Business.Models;

namespace TimeSheet.Business.Services
{
    public class DataService : IDataService
    {
        public List<TimeSheetInfo> GetData(string empId, bool getRawData = false)
        {
            DateTime baseDate = DateTime.Today;
            var data = new List<TimeSheetInfo>();
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
                data = JsonConvert.DeserializeObject<List<TimeSheetInfo>>(json, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });

            }

            var uri = new Uri("https://portal.orientsoftware.net/_api/");
            var leaveData = new List<LeaveRequestModel>();
            var credentialsCache = new CredentialCache { { uri, "NTLM", CredentialCache.DefaultNetworkCredentials } };
            var handler = new HttpClientHandler { Credentials = credentialsCache };
            using (var httpClient1 = new HttpClient(handler))
            {
                httpClient1.BaseAddress = uri;
                httpClient1.Timeout = new TimeSpan(0, 0, 10);
                httpClient1.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = httpClient1.GetAsync("web/lists/getbytitle('OsdLeaveRequest')/items?$filter=Author%2FEMail eq 'lam.nguyen%40orientsoftware.net'").Result;
                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var jsonValue = JObject.Parse(json).GetValue("value");
                leaveData = JsonConvert.DeserializeObject<List<LeaveRequestModel>>(jsonValue.ToString(), new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });

            }
            ProcessData(data, leaveData, getRawData);
            return data;
        }

        private void ProcessData(List<TimeSheetInfo> data, List<LeaveRequestModel> leaveData, bool getRawData)
        {
            foreach (var item in data)
            {

                if (!item.osdTimeIn.HasValue)
                    item.IsCurrentDate = false;
                else
                    item.IsCurrentDate = DateTime.Now.DayOfWeek == item.osdTimeIn.Value.DayOfWeek;

                if (!item.osdTimeIn.HasValue)
                    item.TotalHour = 0;

                if (!item.osdTimeIn.HasValue && !item.osdTimeOut.HasValue)
                {
                    item.AnnualLeave = 1;
                }
                foreach (var leave in leaveData)
                {
                    if (item.osdTimeIn.Value.Date >= leave.FromDate.Date && item.osdTimeIn.Value.Date <= leave.ToDate.Date)
                    {
                        if (leave.FromDate.Date == leave.ToDate.Date)
                            item.AnnualLeave = leave.TotalDay;
                    }
                }

                if (!getRawData)
                {
                    var now = DateTime.Now;
                    if (!item.osdTimeOut.HasValue || item.osdHoursPerDay == 0 || (item.IsCurrentDate && item.osdTimeOut.Value.Hour < 17))
                    {
                        var noonTime = new DateTime(now.Year, now.Month, now.Day, 13, 0, 0);
                        if (now < noonTime)
                        {
                            item.TotalHour = (DateTime.Now - item.osdTimeIn).Value.TotalHours;
                        }
                        else
                            item.TotalHour = (DateTime.Now - item.osdTimeIn).Value.TotalHours - 1.5;
                    }
                    else
                        item.TotalHour = item.AnnualLeave > 0 ? item.osdFullHoursPerday : item.osdHoursPerDay - 0.5;
                }
                else
                {
                    item.TotalHour = item.osdHoursPerDay;
                }
                item.Missing = 8 - item.TotalHour - 8 * item.AnnualLeave;
                item.Expected = item.osdTimeIn.Value.AddHours((item.AnnualLeave > 0 ? 8 * item.AnnualLeave : 9.5));


                if (!item.osdTimeIn.HasValue)
                    item.DisplayTimeOut = "--:--";
                else
                    item.DisplayTimeOut = (item.osdTimeOut == item.osdTimeIn) ? "--:--" : item.osdTimeOut.Value.ToString("t");

                if (!item.osdTimeIn.HasValue)
                    item.DisplayTotalHour = "--:--";
                else
                {
                    var ts1 = TimeSpan.FromHours(item.TotalHour);
                    item.DisplayTotalHour = ts1.Hours.ToString("D2") + ":" + ts1.Minutes.ToString("D2");
                }
                if (!item.osdTimeIn.HasValue)
                    item.DisplayDayOfWeek = "--:--";
                else
                    item.DisplayDayOfWeek = item.osdTimeIn.Value.DayOfWeek.ToString();

                if (!item.osdTimeIn.HasValue)
                    item.DisplayMissing = "--:--";
                var ts = TimeSpan.FromHours(item.Missing);
                item.DisplayMissing = ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2");

                if (!item.osdTimeIn.HasValue)
                    item.DisplayExpected = "--:--";
                else
                    item.DisplayExpected = (item.TotalHour < 12 && item.TotalHour > -0.5) ? item.Expected.ToString("t") : "--:--";
            }
        }
    }
}

