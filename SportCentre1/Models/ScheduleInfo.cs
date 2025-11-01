using System;

namespace SportCentre1.Models
{

    public class ScheduleInfo
    {
        public int ScheduleId { get; set; }
        public string WorkoutName { get; set; } = string.Empty;
        public string TrainerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string EnrollmentInfo { get; set; } = string.Empty;
    }
}