using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class User
{
    public int Userid { get; set; }

    public int Roleid { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual Client? Client { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Role Role { get; set; } = null!;

    public virtual Trainer? Trainer { get; set; }
}
