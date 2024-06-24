using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class AccountVerificationOtp
{
    public long AccountVerificationOtpid { get; set; }

    public long UserId { get; set; }

    public string Otp { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
