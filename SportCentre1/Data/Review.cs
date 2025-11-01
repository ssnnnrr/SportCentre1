using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Review
{
    public int Reviewid { get; set; }

    public int Clientid { get; set; }

    public int? Trainerid { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateOnly Reviewdate { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Trainer? Trainer { get; set; }
}
