using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Currency
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CountryName { get; set; }

    public string? CountryCode { get; set; }

    public virtual ICollection<PlatformApi> PlatformApis { get; set; } = new List<PlatformApi>();

    public virtual ICollection<PlatformTransaction> PlatformTransactions { get; set; } = new List<PlatformTransaction>();
}
