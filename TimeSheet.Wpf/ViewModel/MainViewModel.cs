using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Notifications.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using TimeSheet.Business.Models;
using TimeSheet.Business.Services;

namespace TimeSheet.Wpf.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private NotificationManager _notificationManager;
        private DispatcherTimer timer = null;
        private string _userId;
        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
                RaisePropertyChanged("UserId");
            }
        }
        private string _userFullName;
        public string UserFullName
        {
            get
            {
                return _userFullName;
            }
            set
            {
                _userFullName = value;
                RaisePropertyChanged("UserFullName");
            }
        }
        private string _displayTotalHour;
        public string DisplayTotalHour
        {
            get
            {
                return _displayTotalHour;
            }
            set
            {
                _displayTotalHour = value;
                RaisePropertyChanged("DisplayTotalHour");
            }
        }

        public string DisplayMissingTotalHour
        {
            get
            {
                return _displayMissingTotalHour;
            }
            set
            {
                _displayMissingTotalHour = value;
                RaisePropertyChanged("DisplayMissingTotalHour");
            }
        }

        IDataService _serviceProxy;
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService serviceProxy)
        {
            if (IsInDesignMode)
            {
                Title = "Conexus timesheet monitor (Design)";
            }
            else
            {
                Title = "Conexus timesheet monitor";
            }
            _serviceProxy = serviceProxy;
            
            TimeSheetInfos = new ObservableCollection<TimeSheetInfoRow>();
            ReadAllCommand = new RelayCommand(() => GetData(_userId, _useCheckOutDataForCurrentDate));
            _notificationManager = new NotificationManager();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += new EventHandler(TimerTick);
            timer.Start();
            UserId = "00013";
            ReadAllCommand.Execute(null);
        }

        private bool _useCheckOutDataForCurrentDate = false;
        public bool UseCheckOutDataForCurrentDate
        {
            get
            {
                return _useCheckOutDataForCurrentDate;
            }
            set
            {
                _useCheckOutDataForCurrentDate = value;
                RaisePropertyChanged("UseCheckOutDataForCurrentDate");
                ReadAllCommand.Execute(null);
            }
        }
        private bool PostPone = false;
        private void TimerTick(object send, EventArgs e)
        {
            ReadAllCommand.Execute(null);
            if (!IsInDesignMode)
            {
                var currentDateTimeSheet = _timeSheetInfos.FirstOrDefault(x => x.Info.IsCurrentDate);
                if (currentDateTimeSheet != null && !PostPone && currentDateTimeSheet.Info.TotalHour >= 7)
                {
                    _notificationManager.Show(new NotificationContent
                    {
                        Title = "Conexus timesheet",
                        Message = "Can go home. Remember to check your finger.",
                        Type = NotificationType.Information,
                    }, onClose: () => PostPone = true);
                }
            }
        }

        public string Title { get; set; }

        ObservableCollection<TimeSheetInfoRow> _timeSheetInfos;
        private string _displayMissingTotalHour;

        public ObservableCollection<TimeSheetInfoRow> TimeSheetInfos
        {
            get { return _timeSheetInfos; }
            set
            {
                _timeSheetInfos = value;
                RaisePropertyChanged("TimeSheetInfos");
            }
        }
        public void GetData(string empId, bool useCheckOutDataForCurrentData)
        {
            var data = _serviceProxy.GetData(empId, useCheckOutDataForCurrentData);
            var container = new ObservableCollection<TimeSheetInfoRow>();
            foreach (var item in data)
            {
                container.Add(new TimeSheetInfoRow { Info = item });
            }
            TimeSheetInfos = container;
            UserFullName = "Name: " + TimeSheetInfos[0].Info.osdFullNameVN;
            var ts = TimeSpan.FromHours(TimeSheetInfos.Sum(x => x.Info.TotalHour));
            var ts1 = TimeSpan.FromHours(TimeSheetInfos.Sum(x => x.Info.Missing));
            DisplayTotalHour = "Total until now: " + ((int)ts.TotalHours).ToString("D2") + ":" + ts.Minutes.ToString("D2");
            DisplayMissingTotalHour = "Missing: " + ((int)ts1.TotalHours).ToString("D2") + ":" + ts1.Minutes.ToString("D2");
        }
        public RelayCommand ReadAllCommand { get; set; }



    }

    public class TimeSheetInfoRow : ViewModelBase
    {
        public TimeSheetInfo Info { get; set; }
        public string OutTooltip
        {
            get
            {
                if (Info.Missing > 0 && Info.TotalHour < 8)
                    return "Unusual check out time";
                else
                    return null;
            }
        }
        public SolidColorBrush OutForeColor
        {
            get
            {
                var now = DateTime.Now;
                if (Info.osdTimeOut.HasValue && Info.osdTimeOut.Value.Hour < 17)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                else
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                }
            }
        }

        public SolidColorBrush MissingBackgroundColor
        {
            get
            {
                if (Info.Missing <= 0)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
                else
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                }
            }
        }
        public SolidColorBrush MissingForeColor
        {
            get
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
            }
        }
    }
}