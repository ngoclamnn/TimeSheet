using System.Collections.Generic;
using TimeSheet.Business.Models;

namespace TimeSheet.Business.Services
{
    public interface IDataService
    {
        List<TimeSheetInfo> GetData(string empId, bool useCheckOutDataForCurrentData = true);
    }
}
