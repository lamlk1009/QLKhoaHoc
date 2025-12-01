namespace DemoApp.ViewModels
{
    public class MyCourseItemViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public string? ShortDescription { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Level { get; set; } = "All level";
        public int TotalLessons { get; set; }
        public int ProgressPercent { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
