using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class PosassignedPlatform
{
    public long AssignPosplatformId { get; set; }

    public long Posid { get; set; }

    public int PlatformId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Platform Platform { get; set; } = null!;

    public virtual Po Pos { get; set; } = null!;
}
