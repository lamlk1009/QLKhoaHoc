using DemoApp.Models;

namespace DemoApp.ViewModels
{
    public class CourseLearningViewModel
    {
        public KhoaHoc KhoaHoc { get; set; }

        // bài học
        public List<BaiHoc> Lessons { get; set; } = new();
        public List<int> CompletedLessonIds { get; set; } = new();
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int ProgressPercent { get; set; }

        // điểm danh
        public List<BuoiHoc> Sessions { get; set; } = new();
        public List<DiemDanh> Attendances { get; set; } = new();

        // thống kê
        public CourseLearningStatsViewModel Stats { get; set; } = new();
    }
}
