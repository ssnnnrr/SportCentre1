using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Payment
{
    public int Paymentid { get; set; }

    public int? Clientid { get; set; }

    public decimal? Amount { get; set; }

    public DateOnly? Paymentdate { get; set; }

    public string? Description { get; set; }

    public int? Bookingid { get; set; }

    public int? Membershiptypeid { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Membershiptype? Membershiptype { get; set; }
}
