using System;
using System.Collections.Generic;

namespace backend_ProjectmanagementV2.Models;

public partial class Activity
{
    public int Id { get; set; }

    public int? ProjectId { get; set; }

    public int? ActivityHeaderId { get; set; }

    public string? Name { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Activity? ActivityHeader { get; set; }

    public virtual ICollection<Activity> InverseActivityHeader { get; set; } = new List<Activity>();

    public virtual Project? Project { get; set; }
}
