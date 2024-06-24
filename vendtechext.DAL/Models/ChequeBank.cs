using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class ChequeBank
{
    public int Id { get; set; }

    public string? BankCode { get; set; }

    public string? BankName { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createdon { get; set; }

    public bool? IsDeleted { get; set; }
}
