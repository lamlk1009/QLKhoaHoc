namespace DemoApp.ViewModels
{
    public class CourseRegistrationStat
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int Registrations { get; set; }
        public double Percentage { get; set; }
    }
}
