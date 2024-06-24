using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Commission
{
    public int CommissionId { get; set; }

    public decimal Percentage { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Agency> Agencies { get; set; } = new List<Agency>();

    public virtual ICollection<Po> Pos { get; set; } = new List<Po>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
