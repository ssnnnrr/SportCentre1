using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

/// <summary>
/// Общеклубные челленджи и события
/// </summary>
public partial class Challenge
{
    public int Challengeid { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Startdate { get; set; }

    public DateTime Enddate { get; set; }

    public string? Reward { get; set; }

    /// <summary>
    /// Тип челленджа (например, Attendance, Manual)
    /// </summary>
    public string Challengetype { get; set; } = null!;

    public virtual ICollection<ClientChallenge> ClientChallenges { get; set; } = new List<ClientChallenge>();

}
