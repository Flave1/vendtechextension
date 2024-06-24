using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Nation
{
    public int Id { get; set; }

    public string Sortname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int IsdCode { get; set; }
}
