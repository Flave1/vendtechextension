using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Notification
{
    public long NotificationId { get; set; }

    public long UserId { get; set; }

    public int? Type { get; set; }

    public string? Text { get; set; }

    public string? Title { get; set; }

    public int Status { get; set; }

    public long? RowId { get; set; }

    public bool MarkAsRead { get; set; }

    public DateTime SentOn { get; set; }

    public virtual User User { get; set; } = null!;
}
