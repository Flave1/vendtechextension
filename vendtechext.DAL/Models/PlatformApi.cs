using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class PlatformApi
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int ApiType { get; set; }

    public decimal Balance { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Currency { get; set; } = null!;

    public string? Config { get; set; }

    public virtual Currency CurrencyNavigation { get; set; } = null!;

    public virtual ICollection<PlatformApiConnection> PlatformApiConnections { get; set; } = new List<PlatformApiConnection>();
}
