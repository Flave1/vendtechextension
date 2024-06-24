using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace vendtechext.DAL.Models;

public partial class Platform
{
    public int PlatformId { get; set; }

    public string Title { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public bool Enabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ShortName { get; set; }

    public string? Logo { get; set; }

    public decimal MinimumAmount { get; set; }

    public bool DisablePlatform { get; set; }

    public string? DisabledPlatformMessage { get; set; }

    public int PlatformType { get; set; }

    public int? PlatformApiConnId { get; set; }

    public int? PlatformApiConnBackupId { get; set; }
    [NotMapped]
    public virtual PlatformApiConnection PlatformApiConn { get; set; }
    [NotMapped]
    public virtual PlatformApiConnection PlatformApiConnBackup { get; set; }
    [NotMapped]
    public virtual ICollection<PlatformApiConnection> PlatformApiConnections { get; set; } = new List<PlatformApiConnection>();

    public virtual ICollection<PlatformPacParam> PlatformPacParams { get; set; } = new List<PlatformPacParam>();

    public virtual ICollection<PlatformTransaction> PlatformTransactions { get; set; } = new List<PlatformTransaction>();

    public virtual ICollection<PosassignedPlatform> PosassignedPlatforms { get; set; } = new List<PosassignedPlatform>();

    public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();

    public virtual ICollection<UserAssignedPlatform> UserAssignedPlatforms { get; set; } = new List<UserAssignedPlatform>();
}
