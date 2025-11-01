using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Employee
{
    public int Employeeid { get; set; }

    public int Userid { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string? Position { get; set; }

    public virtual User User { get; set; } = null!;
}
