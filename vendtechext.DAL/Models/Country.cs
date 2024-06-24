using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Country
{
    public int CountryId { get; set; }

    public string CurrencyName { get; set; } = null!;

    public string CurrencySymbol { get; set; } = null!;

    public string? CountryName { get; set; }

    public string? CountryCode { get; set; }

    public bool Disabled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? DomainUrl { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
