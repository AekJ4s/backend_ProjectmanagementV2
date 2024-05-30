using System;
using System.Collections.Generic;

namespace backend_ProjectmanagementV2.Models;

public partial class ProjectXfile
{
    public int Id { get; set; }

    public int? ProjectId { get; set; }

    public int? FileUploadId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual FileUpload? FileUpload { get; set; }

    public virtual Project? Project { get; set; }
}
