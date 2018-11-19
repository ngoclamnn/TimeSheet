using Newtonsoft.Json;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TimeSheet.JsonAnalyzer;
using TimeSheet.Model;

namespace TimeSheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer dispatcherTimer;
        private List<TimeSheetDto> data;
        private bool _acceptEnoughTime;

        public MainWindow()
        {
            InitializeComponent();

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
                var response = httpClient.GetAsync($"EmployeeTimeSheet/get?FromDate={fromDate}&ToDate={today}&EmployeeID={lbUserId.Text}")
                    .GetAwaiter().GetResult();

                string json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                data = JsonConvert.DeserializeObject<List<TimeSheetDto>>(json, new JsonSerializerSettings() {DateTimeZoneHandling = DateTimeZoneHandling.Local });
                dataGrid.ItemsSource = data;
                dataGrid.SelectedIndex = data.FindIndex(x => x.IsCurrentDate);
            }
            var totalHour = TimeSpan.FromHours(data.Sum(x => x.TotalHour));
            lbTotalTime.Content = ((int)totalHour.TotalHours).ToString("D2") + ":" + totalHour.Minutes.ToString("D2");
            //  DispatcherTimer setup
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();


        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current second
            var totalHour = TimeSpan.FromHours(data.Sum(x => x.osdHoursPerDay));
            lbTotalTime.Content = ((int)totalHour.TotalHours).ToString("D2") + ":" + totalHour.Minutes.ToString("D2");


            var notificationManager = new NotificationManager();

            if (!_acceptEnoughTime &&  data.FirstOrDefault(x=>x.IsCurrentDate).TotalHour >= 8.0)
            {
                notificationManager.Show(new NotificationContent
                {
                    //Title = "Working time is enough!",
                    Message = "Can go home now.",
                    Type = NotificationType.Information,

                }, onClose: () => { _acceptEnoughTime = true; });

            }
            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }


    }
}
