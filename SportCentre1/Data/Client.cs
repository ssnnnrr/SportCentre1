using System;
using System.Collections.Generic;

namespace SportCentre1.Data;

public partial class Client
{
    public int Clientid { get; set; }

    public int? Userid { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public string? Email { get; set; }

    public DateOnly Registrationdate { get; set; }

    /// <summary>
    /// Рост клиента в сантиметрах
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Целевой вес клиента в кг
    /// </summary>
    public decimal? Targetweight { get; set; }

    /// <summary>
    /// Целевой процент жира клиента
    /// </summary>
    public decimal? Targetbodyfatpercentage { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<ClientChallenge> ClientChallenges { get; set; } = new List<ClientChallenge>();


    public virtual ICollection<Clientmembership> Clientmemberships { get; set; } = new List<Clientmembership>();

    public virtual ICollection<Clientprogress> Clientprogresses { get; set; } = new List<Clientprogress>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User? User { get; set; }
}
