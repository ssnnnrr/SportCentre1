using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Trainer
{
    public int Trainerid { get; set; }

    public int? Userid { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string? Specialization { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual User? User { get; set; }
}
