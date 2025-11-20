using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

/// <summary>
/// История измерений прогресса клиента (вес, % жира и т.д.)
/// </summary>
public partial class Clientprogress
{
    public int Clientprogressid { get; set; }

    public int Clientid { get; set; }

    public DateOnly Date { get; set; }

    public decimal? Weight { get; set; }

    public decimal? Bodyfatpercentage { get; set; }

    public string? Notes { get; set; }

    public virtual Client Client { get; set; } = null!;
}
