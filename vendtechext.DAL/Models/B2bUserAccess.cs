using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class B2bUserAccess
{
    public long B2bUserAccessId { get; set; }

    public long UserId { get; set; }

    public string ClientToken { get; set; } = null!;

    public string Apitoken { get; set; } = null!;

    public string Clientkey { get; set; } = null!;

    public string Apikey { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
