using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Membershiptype
{
    public int Membershiptypeid { get; set; }

    public string Typename { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Durationdays { get; set; }

    public virtual ICollection<Clientmembership> Clientmemberships { get; set; } = new List<Clientmembership>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
