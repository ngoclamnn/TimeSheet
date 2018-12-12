using Newtonsoft.Json;
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
        private List<TimeSheetInfo> _data;
        private List<LeaveRequestModel> _leaveData;
        private string _empId;
        private string _email;

        public List<TimeSheetInfo> GetData(string empId, bool useCheckOutDataForCurrentData = true)
        {
            if (_empId == empId && _data != null && _leaveData != null)
            {
                ProcessData(_data, _leaveData, useCheckOutDataForCurrentData);
                return _data;
            }
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
                _data = data;
                _empId = empId;
            }
            var uri = new Uri("https://portal.orientsoftware.net/_api/");
            var leaveData = new List<LeaveRequestModel>();
            var credentialsCache = new CredentialCache { { uri, "NTLM", CredentialCache.DefaultNetworkCredentials } };
            var handler = new HttpClientHandler { Credentials = credentialsCache };

            if (string.IsNullOrEmpty(_email))
            {
                using (var httpClient2 = new HttpClient(handler))
                {
                    httpClient2.BaseAddress = uri;
                    httpClient2.Timeout = new TimeSpan(0, 0, 10);
                    httpClient2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = httpClient2.GetAsync($"web/lists/getbytitle('OsdUserProfile')/items?$filter=osdTimeSheetId eq '{empId}'").Result;
                    string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var jsonValue = JObject.Parse(json).GetValue("value");
                    var responseData = JsonConvert.DeserializeObject<List<UserInfoModel>>(jsonValue.ToString(), new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
                    _email = responseData[0].osdEmail;
                }
            }


            credentialsCache = new CredentialCache { { uri, "NTLM", CredentialCache.DefaultNetworkCredentials } };
            handler = new HttpClientHandler { Credentials = credentialsCache };
            using (var httpClient1 = new HttpClient(handler))
            {
                httpClient1.BaseAddress = uri;
                httpClient1.Timeout = new TimeSpan(0, 0, 10);
                httpClient1.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = httpClient1.GetAsync($"web/lists/getbytitle('OsdLeaveRequest')/items?$filter=Author%2FEMail eq '{_email}'").Result;
                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var jsonValue = JObject.Parse(json).GetValue("value");
                leaveData = JsonConvert.DeserializeObject<List<LeaveRequestModel>>(jsonValue.ToString(), new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local });
                _leaveData = leaveData;
            }
            ProcessData(data, leaveData, useCheckOutDataForCurrentData);
            return data;
        }

        private void ProcessData(List<TimeSheetInfo> data, List<LeaveRequestModel> leaveData, bool useCheckOutDataForCurrentData)
        {
            foreach (var item in data)
            {

                if (!item.osdTimeIn.HasValue)
                    item.IsCurrentDate = false;
                else
                    item.IsCurrentDate = DateTime.Now.DayOfWeek == item.osdTimeIn.Value.DayOfWeek;

                if (!item.osdTimeIn.HasValue)
                    item.TotalHour = 0;
                //no checkin and checkout 

                foreach (var leave in leaveData)
                {
                    if (!item.osdTimeIn.HasValue && !item.osdTimeOut.HasValue)
                    {
                        item.AnnualLeave = 1;
                    }
                    //if(item.osdWorkingDate.Date == leave.)
                    if (item.osdWorkingDate.Date >= leave.FromDate.Date && item.osdWorkingDate.Date <= leave.ToDate.Date)
                    {
                        if (leave.FromDate.Date == leave.ToDate.Date)
                            item.AnnualLeave = leave.TotalDay;
                    }
                }



                if (useCheckOutDataForCurrentData)
                {
                    var now = DateTime.Now;
                    if (item.AnnualLeave > 0)
                        item.TotalHour = 0;
                    else
                    {
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
                }
                else
                {
                    item.TotalHour = item.osdHoursPerDay - 0.5;
                }
                item.Missing = 8 - item.TotalHour - 8 * item.AnnualLeave;
                if (item.osdTimeIn.HasValue)
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

                item.DisplayDayOfWeek = item.osdWorkingDate.DayOfWeek.ToString();

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

