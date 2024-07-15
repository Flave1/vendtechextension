using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class PlatformApiConnection
{
    public int Id { get; set; }

    public int PlatformId { get; set; }

    public int? PlatformApiId { get; set; }

    public string Name { get; set; } = null!;

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Platform Platform { get; set; } = null!;

    public virtual PlatformApi? PlatformApi { get; set; }

    public virtual ICollection<PlatformPacParam> PlatformPacParams { get; set; } = new List<PlatformPacParam>();

    public virtual ICollection<Platform> PlatformPlatformApiConnBackups { get; set; } = new List<Platform>();

    public virtual ICollection<Platform> PlatformPlatformApiConns { get; set; } = new List<Platform>();

    public virtual ICollection<PlatformTransaction> PlatformTransactions { get; set; } = new List<PlatformTransaction>();
}
