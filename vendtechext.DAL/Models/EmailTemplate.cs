using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class EmailTemplate
{
    public int TemplateId { get; set; }

    public string? TemplateName { get; set; }

    public string? EmailSubject { get; set; }

    public string? TemplateContent { get; set; }

    public bool TemplateStatus { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int TemplateType { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }

    public string? Desription { get; set; }

    public string? TargetUser { get; set; }
}
