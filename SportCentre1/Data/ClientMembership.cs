using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Clientmembership
{
    public int Clientmembershipid { get; set; }

    public int Clientid { get; set; }

    public int Membershiptypeid { get; set; }

    public DateOnly Startdate { get; set; }

    public DateOnly Enddate { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<ClientMembershipPause> ClientMembershipPauses { get; set; } = new List<ClientMembershipPause>();


    public virtual Membershiptype Membershiptype { get; set; } = null!;
}
