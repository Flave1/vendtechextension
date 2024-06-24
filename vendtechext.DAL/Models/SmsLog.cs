using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class SmsLog
{
    public int Id { get; set; }

    public DateTime? DateTime { get; set; }

    public long? UserId { get; set; }

    public long? Posid { get; set; }

    public string? TransactionId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? MeterNumber { get; set; }

    public long? AgentId { get; set; }
}
