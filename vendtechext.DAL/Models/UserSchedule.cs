using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class UserSchedule
{
    public int UserScheduleId { get; set; }

    public long UserId { get; set; }

    public int ScheduleType { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Balance { get; set; }
}
