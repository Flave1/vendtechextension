using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class ErrorLog
{
    public int ErrorLogId { get; set; }

    public string? Message { get; set; }

    public string? StackTrace { get; set; }

    public string? InnerException { get; set; }

    public string? LoggedInDetails { get; set; }

    public string? QueryData { get; set; }

    public string? FormData { get; set; }

    public string? RouteData { get; set; }

    public DateTime? LoggedAt { get; set; }

    public long? UserId { get; set; }
}
