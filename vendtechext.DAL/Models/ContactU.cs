using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class ContactU
{
    public long ContactUsId { get; set; }

    public long UserId { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public int Status { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
