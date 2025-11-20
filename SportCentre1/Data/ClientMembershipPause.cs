using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

/// <summary>
/// История заморозки абонементов клиентов
/// </summary>
public partial class ClientMembershipPause
{
    public int Pauseid { get; set; }

    public int Clientmembershipid { get; set; }

    public DateOnly Startdate { get; set; }

    public DateOnly Enddate { get; set; }

    public virtual Clientmembership Clientmembership { get; set; } = null!;
}
