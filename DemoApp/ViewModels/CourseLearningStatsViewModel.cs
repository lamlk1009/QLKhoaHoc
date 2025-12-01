namespace DemoApp.ViewModels
{
    public class CourseLearningStatsViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? LastStudyDate { get; set; }
        public int StudyDays { get; set; }
        // % điểm danh (0–100)
        public int AttendanceRate { get; set; }

        // Điểm trung bình (0–100)
        public int AvgScore { get; set; }

        // Option: có thể thêm các field khác nếu cần
        public int TotalStudyMinutes { get; set; }
        public int CurrentStreakDays { get; set; }
        public int AttendancePresent { get; set; }
        public int TotalSessions { get; set; }
        public int StreakDays { get; set; }

        public List<DailyStudyStat> Last7DaysStats { get; set; } = new();
        public List<LearningActivityLog> RecentActivities { get; set; } = new();
    }
}
