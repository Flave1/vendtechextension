using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class TempCountry
{
    public int CountryId { get; set; }

    public string? CountryName { get; set; }

    public string CurrencyName { get; set; } = null!;

    public string CurrencySymbol { get; set; } = null!;

    public string? CountryCode { get; set; }

    public bool Enabled { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? DomainUrl { get; set; }
}
