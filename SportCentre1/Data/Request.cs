using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Request
{
    public int Requestid { get; set; }

    public int Clientid { get; set; }

    public string Subject { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime Creationdate { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
