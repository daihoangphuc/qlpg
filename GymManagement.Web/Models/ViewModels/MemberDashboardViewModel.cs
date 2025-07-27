using GymManagement.Web.Data.Models;

namespace GymManagement.Web.Models.ViewModels
{
    public class MemberDashboardViewModel
    {
        public int TotalActiveRegistrations { get; set; }
        public int MonthlyAttendanceCount { get; set; }
        public int TotalClassesJoined { get; set; }
        public string CurrentPackage { get; set; } = "Chưa có";
        public List<DangKy> RecentRegistrations { get; set; } = new List<DangKy>();
    }
}
