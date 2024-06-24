using System;
using System.Collections.Generic;

namespace vendtechext.DAL.Models;

public partial class Module
{
    public int ModuleId { get; set; }

    public string? ModuleName { get; set; }

    public string? ControllerName { get; set; }

    public string? Description { get; set; }

    public bool? IsAdmin { get; set; }

    public int? SubMenuOf { get; set; }

    public virtual ICollection<Module> InverseSubMenuOfNavigation { get; set; } = new List<Module>();

    public virtual Module? SubMenuOfNavigation { get; set; }

    public virtual ICollection<UserAssignedModule> UserAssignedModules { get; set; } = new List<UserAssignedModule>();
}
