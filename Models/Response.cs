namespace backend_ProjectmanagementV2.Models;


public partial class Response
{
    public int Code{get; set; }

    public string? Message{get; set; }

    public object? Data{get; set; }
}