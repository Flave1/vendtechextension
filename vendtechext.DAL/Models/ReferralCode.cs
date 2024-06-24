using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class ReferralCode
{
    public long ReferralCodeId { get; set; }

    public long FkUserId { get; set; }

    public string Code { get; set; } = null!;

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User FkUser { get; set; } = null!;
}
