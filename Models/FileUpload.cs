namespace backend_ProjectmanagementV2.Models;

public partial class FileUpload
{
    public int Id { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<ProjectXfile> ProjectXfiles { get; set; } = new List<ProjectXfile>();
}
