using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Booking
{
    public int Bookingid { get; set; }

    public int Clientid { get; set; }

    public int Scheduleid { get; set; }

    public DateTime Bookingtime { get; set; }

    public bool Ispaid { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Schedule Schedule { get; set; } = null!;
}
