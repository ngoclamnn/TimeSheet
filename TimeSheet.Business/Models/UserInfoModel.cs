using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheet.Business.Models
{
    public class UserInfoModel
    {
        public int Id { get; set; }
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        public string osdJobTitle { get; set; }
        public object osdSkillset { get; set; }
        public string osdLoginId { get; set; }
        public string osdSkype { get; set; }
        public string osdEmail { get; set; }
        public string osdWorkPhone { get; set; }
        public string osdMobile { get; set; }
        public object osdStatus { get; set; }
        public string osdImage { get; set; }
        public double osdUserId { get; set; }
        public object osdEmployeeId { get; set; }
        public string osdTimeSheetId { get; set; }
        public string osdSummary { get; set; }
    }
}
