using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class PlatformApiLog
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public string ApiLog { get; set; } = null!;

    public int LogType { get; set; }

    public DateTime LogDate { get; set; }
}
