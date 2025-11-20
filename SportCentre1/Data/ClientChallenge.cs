using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

/// <summary>
/// Связь клиентов с челленджами, их прогресс и статус
/// </summary>
public partial class ClientChallenge
{
    public int Clientchallengeid { get; set; }

    public int Clientid { get; set; }

    public int Challengeid { get; set; }

    public DateTime Joindate { get; set; }

    public int Progress { get; set; }

    public string Status { get; set; } = null!;

    public virtual Challenge Challenge { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;
}
