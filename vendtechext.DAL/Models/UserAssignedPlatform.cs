using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class UserAssignedPlatform
{
    public long AssignUserPlatformId { get; set; }

    public long UserId { get; set; }

    public int PlatformId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Platform Platform { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
