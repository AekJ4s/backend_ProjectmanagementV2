using System;
using System.Collections.Generic;

namespace backend_ProjectmanagementV2.Models;

public partial class User
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
