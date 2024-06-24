using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class UserAssignedWidget
{
    public long AssignWidgetId { get; set; }

    public long UserId { get; set; }

    public int WidgetId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool? IsAddedFromAgency { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Widget Widget { get; set; } = null!;
}
