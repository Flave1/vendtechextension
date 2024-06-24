using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class PlatformPacParam
{
    public long Id { get; set; }

    public int PlatformId { get; set; }

    public int PlatformApiConnectionId { get; set; }

    public string? Config { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Platform Platform { get; set; } = null!;

    public virtual PlatformApiConnection PlatformApiConnection { get; set; } = null!;
}
