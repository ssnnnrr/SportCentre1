using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Equipment
{
    public int Equipmentid { get; set; }

    public string Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int Quantity { get; set; }

    public DateOnly? Lastmaintenancedate { get; set; }
}
