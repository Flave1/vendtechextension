using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Widget
{
    public int WidgetId { get; set; }

    public string Title { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public bool Enabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserAssignedWidget> UserAssignedWidgets { get; set; } = new List<UserAssignedWidget>();
}
