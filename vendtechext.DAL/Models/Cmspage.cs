using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;


public partial class Cmspage
{
    public int PageId { get; set; }

    public string PageName { get; set; } = null!;

    public string? PageTitle { get; set; }

    public string? PageContent { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaKeywords { get; set; }

    public string? MetaDescription { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
