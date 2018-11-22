using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeSheet.Models;

namespace TimeSheet.Business.Services
{
    public interface IDataService
    {
        ObservableCollection<TimeSheetInfo> GetData(string empId);
    }
}
