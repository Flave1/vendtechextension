using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class City
{
    public int CityId { get; set; }

    public string Name { get; set; } = null!;

    public int CountryId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
