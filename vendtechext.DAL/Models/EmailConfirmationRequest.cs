using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class EmailConfirmationRequest
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
