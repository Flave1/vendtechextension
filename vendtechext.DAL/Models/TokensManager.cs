using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class TokensManager
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string TokenKey { get; set; } = null!;

    public string? DeviceToken { get; set; }

    public int? AppType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresOn { get; set; }

    public string? PosNumber { get; set; }

    public virtual User User { get; set; } = null!;
}
