namespace DemoApp.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalRegistrations { get; set; }

        // Dùng cho biểu đồ tỉ lệ và top khóa học
        public List<CourseRegistrationStat> RegistrationStats { get; set; } = new();
    }
}
