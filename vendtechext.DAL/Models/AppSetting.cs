using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class AppSetting
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Value { get; set; }
}
