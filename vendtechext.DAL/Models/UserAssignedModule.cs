using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class UserAssignedModule
{
    public long AssignUserModuleId { get; set; }

    public long UserId { get; set; }

    public int ModuleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool? IsAddedFromAgency { get; set; }

    public virtual Module Module { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
