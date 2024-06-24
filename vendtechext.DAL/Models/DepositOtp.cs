using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class DepositOtp
{
    public long DepositOtpid { get; set; }

    public string Otp { get; set; } = null!;

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }
}
