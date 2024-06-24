using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Meter
{
    public long MeterId { get; set; }

    public long UserId { get; set; }

    public string Number { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? MeterMake { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsVerified { get; set; }

    public string? Allias { get; set; }

    public bool IsSaved { get; set; }

    public int NumberType { get; set; }

    public virtual User User { get; set; } = null!;
}
