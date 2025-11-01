using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class ClientMembership
{
    public int ClientMembershipId { get; set; }
    public int Clientid { get; set; }
    public int Membershiptypeid { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public virtual Client Client { get; set; } = null!;
    public virtual Membershiptype Membershiptype { get; set; } = null!;
}