using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Message
{
    public int Messageid { get; set; }

    public int Requestid { get; set; }

    public int Senderuserid { get; set; }

    public string Messagetext { get; set; } = null!;

    public DateTime Sentdate { get; set; }

    public virtual Request Request { get; set; } = null!;

    public virtual User Senderuser { get; set; } = null!;
}
