using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Vendor
{
    public long VendorId { get; set; }

    public string? Name { get; set; }

    public string? SurName { get; set; }

    public string? Vendor1 { get; set; }

    public string? Email { get; set; }

    public long AgencyId { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public int? AgentType { get; set; }

    public int? CommissionPercentage { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Agency Agency { get; set; } = null!;
}
