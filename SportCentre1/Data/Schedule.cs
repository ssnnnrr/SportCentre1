using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Schedule
{
    public int Scheduleid { get; set; }

    public int Workouttypeid { get; set; }

    public int Trainerid { get; set; }

    public DateTime Starttime { get; set; }

    public DateTime Endtime { get; set; }

    public int Maxcapacity { get; set; }

    public int? Currentenrollment { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Trainer Trainer { get; set; } = null!;

    public virtual Workouttype Workouttype { get; set; } = null!;
}
