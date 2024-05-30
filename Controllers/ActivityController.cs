

using backend_ProjectmanagementV2.Data;
using backend_ProjectmanagementV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("activities")]
[Authorize]
public class ActivityController : ControllerBase
{

    private DatabaseContext _db = new DatabaseContext();
    private readonly ILogger<ActivityController> _logger;

    public ActivityController(ILogger<ActivityController> logger)
    {
        _logger = logger;
    }

    public class ActivityCreateRequest
    {
       

        public int? ProjectId { get; set; } 

        public int? ActivityHeaderId { get; set; }

        public string? Name { get; set; } 

        public virtual ICollection<Activity> InverseActivityHeader { get; set; } = new List<Activity>(); 
    }

    [HttpPost("Create" , Name = "CreateActivities")]

    public static Activity Create(DatabaseContext db, Activity activity)
    {
        activity.CreateDate = DateTime.Now;
        activity.UpdateDate = DateTime.Now;
        activity.IsDeleted = false;
        db.Activities.Add(activity);
        db.SaveChanges(); // บันทึกการเปลี่ยนแปลงลงในฐานข้อมูล
        return activity;
    }

     [HttpGet("GetBy/{id}", Name = "GetAllActivity")]

    public ActionResult GetAllActivity(int id)
    {
        Activity activity = Activity.GetByActivityId(_db, id);
        if (activity == null)
        {
        return NotFound(); // ถ้าไม่พบโปรเจคให้ส่งคำตอบ 404 Not Found
        }

        return Ok(activity);
        
    }
}

    
    

    