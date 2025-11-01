using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Workouttype
{
    public int Workouttypeid { get; set; }

    public string Typename { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
