using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Agency
{
    public long AgencyId { get; set; }

    public string? AgencyName { get; set; }

    public int AgentType { get; set; }

    public int? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CommissionId { get; set; }

    public long? Representative { get; set; }

    public virtual Commission? Commission { get; set; }

    public virtual User? RepresentativeNavigation { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
}
