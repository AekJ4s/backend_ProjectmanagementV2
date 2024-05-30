using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.IdentityModel.Tokens;
using backend_ProjectmanagementV2.Data;
using backend_ProjectmanagementV2.Models;
[ApiController]
[Route("projects")]
[Authorize]

public class ProjectController : ControllerBase
{

    private DatabaseContext _db = new DatabaseContext();
    private readonly ILogger<ProjectController> _logger;
    private IHostEnvironment _hostingEnvironment;
    public ProjectController(ILogger<ProjectController> logger, IHostEnvironment? environment)
    {
        _logger = logger;
        _hostingEnvironment = environment;
    }

    [HttpPost("CreateProject", Name = "CreateProject")]
    public ActionResult<Response> Create([FromForm] CreateProjectXfile request)
    {
        var projectCreate = request.ProjectCreate;
        var files = request.Files;
        var activitys = JsonConvert.DeserializeObject<List<Activity>>(projectCreate.Activities);
        Project newProject = new Project
        {
            Name = projectCreate.Name,
            OwnerId = projectCreate.OwnerId,
            Detail = projectCreate.Detail,
            StartDate = DateTime.Parse(projectCreate.StartDate),
            EndDate = DateTime.Parse(projectCreate.EndDate),
            Activities = new List<Activity>(),
            CreateDate = DateTime.Now,
            UpdateDate = DateTime.Now,
            IsDeleted = false
        };

        var samename = _db.Projects.Where(p => p.Name == projectCreate.Name && p.IsDeleted != true).ToList();

                 if (samename.Count > 0){
                    return BadRequest(new Response{
                        Code = 400,
                        Message = " มีโปรเจคอยู่แล้ว ",
                        Data = null 
                    });
                 }

       
            using (var transaction = _db.Database.BeginTransaction())
            {

                // เพิ่มกิจกรรมใหม่ในโปรเจ็ค
                Activity.SendActivities(newProject, newProject.Activities, activitys);
                _db.Projects.Add(newProject);
        
                _db.SaveChanges(); // บันทึก newProject ก่อนเพื่อให้ได้ Id

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        var newFile = new FileUpload
                        {
                            FileName = file.FileName,
                            FilePath = "UploadedFile/ProfileImg/",
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            IsDeleted = false
                        };

                        newFile = FileUpload.Create(_db, newFile);

                        if (file.Length > 0)
                        {
                            string upload = Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedFile/ProfileImg/", newFile.Id.ToString());
                            Directory.CreateDirectory(upload);
                            string filePath = Path.Combine(upload, file.FileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }
                        }
                        _db.SaveChanges(); // บันทึก newFile แต่ละไฟล์ก่อนเพื่อให้ได้ Id

                        ProjectXfile projectXfile = new ProjectXfile
                        {
                            ProjectId = newProject.Id, // ใช้ Id ของ newProject
                            FileUploadId = newFile.Id, // ใช้ Id ของ newFile
                            IsDeleted = false, // กำหนดค่าว่ายังไม่ถูก Delete
                        };

                        _db.ProjectXfiles.Add(projectXfile);
                    }

                }
                else files = null;
                _db.SaveChanges(); // บันทึกการเปลี่ยนแปลงทั้งหมด

                transaction.Commit(); // ยืนยัน transaction

                return Ok(new Response
                {
                    Code = 200,
                    Message = "Success",
                    Data = newProject
                });
            }
              
    }



    [HttpGet(Name = "ShowAllProjects")]

    public ActionResult GetAll()
    {
        // .OrderBy(q => q.Salary) เรียงจากน้อยไปมาก
        // .OrderByDescending(q => q.Salary) เรียงจากมากไปน้อย
        List<Project> projects = Project.GetAll(_db);
        return Ok(projects);
    }

    [HttpGet("GetBy/{id}", Name = "GetProjectID")]
    public ActionResult GetProjectID(int id)
    {
        // ค้นหาโปรเจคจากฐานข้อมูลโดยใช้ Id
        Project? dataOfProject = _db.Projects
            .Include(p => p.Activities)
            .Include(p => p.ProjectXfiles)
            .FirstOrDefault(p => p.Id == id && p.IsDeleted != true);

        if (dataOfProject == null)
        {
            // หากไม่พบโปรเจคในฐานข้อมูล คืนค่า NotFound
            return NotFound(new Response
            {
                Code = 404,
                Message = "Project not found"
            });
        }

        // ค้นหาข้อมูล ProjectXfiles ที่เกี่ยวข้อง
        var allFilesAboutProject = _db.ProjectXfiles
            .Where(pwf => pwf.ProjectId == id && pwf.IsDeleted != true)
            .Include(pwf => pwf.FileUpload) // Include the related File entity
            .ToList();

        var detailOfFileProject = new List<FileUpload>();
        foreach (var data in allFilesAboutProject)
        {
            var detail = _db.FileUploads.Where(x => data.FileUploadId == x.Id && x.IsDeleted != true).ToList();
            detailOfFileProject.AddRange(detail);
        }

        dataOfProject.Activities = _db.Activities
            .Where(act => act.ProjectId == id && act.ActivityHeaderId == null && act.IsDeleted != true)
            .ToList();

        // วนลูปผ่านกิจกรรมในโปรเจคและเรียกใช้เมธอด GetALLActivityinside
        foreach (var activity in dataOfProject.Activities)
        {
            Activity.GetALLActivityinside(dataOfProject.Activities, _db); // ไปวนหาว่ามีลูกอีกไหม
        }

        try
        {
            // ส่งข้อมูลโปรเจคกลับไปยังไคลเอนต์
            return Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = new
                {
                    Project = dataOfProject,
                    Files = detailOfFileProject,
                }
            });
        }
        catch (Exception e)
        {
            // หากเกิดข้อผิดพลาดในการส่งข้อมูล คืนค่า StatusCode 500 (Internal Server Error)
            return StatusCode(500, new Response
            {
                Code = 500,
                Message = "Internal server error: " + e.Message
            });
        }
    }

    [HttpPut("UpdateProject", Name = "ProjectUpdate")]

    public ActionResult<Response> UpdateProjectRequest([FromForm] UpdateProjectXfile NewData)
    {
        //ถ้ามีข้อมูล
        if (NewData.ProjectUpdate != null)
        {
            Project? projectUpdate = _db.Projects.Find(NewData.ProjectUpdate.Id);
            var Request = NewData.ProjectUpdate;
            var files = NewData.Files;
            var ProjectXFiles = JsonConvert.DeserializeObject<List<ProjectXfile>>(Request.ProjectXfiles);
            var activitys = JsonConvert.DeserializeObject<List<Activity>>(Request.Activities);
            
            var samename = _db.Projects.Where(db => db.Name == Request.Name && db.IsDeleted != true && db.Id != Request.Id).ToList();

                 if (samename.Count > 0){
                    return BadRequest(new Response{
                        Code = 400,
                        Message = " ชื่อนี้ถูกใช้อยู่แล้ว ไม่สามารถแก้ไขได้ ",
                    });
                 }

            if (projectUpdate.Activities != null)
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    
                    // ให้นำข้อมูลไปหากิจกรรมภายใน
                    foreach (Activity ThisAct in activitys)
                    {
                        //ส่งข้อมูลกิจกรรมให้ไปค้นหากินกรรมย่อย
                        Activity.TakeActivity(null, projectUpdate, ThisAct, _db);
                    }
                    
                    foreach( var ProxFile in ProjectXFiles){
                        if(ProxFile.IsDeleted == true){
                            ProjectXfile ThisShouldBeDelete = ProjectXfile.GetOnlyThis(_db,ProxFile.Id);
                            if(ThisShouldBeDelete.IsDeleted != true){
                            ThisShouldBeDelete.IsDeleted = true;
                            }
                        }
                    }

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        var newFile = new FileUpload
                        {
                            FileName = file.FileName,
                            FilePath = "UploadedFile/ProfileImg/",
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            IsDeleted = false
                        };

                        newFile = FileUpload.Create(_db, newFile);

                        if (file.Length > 0)
                        {
                            string upload = Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedFile/ProfileImg/", newFile.Id.ToString());
                            Directory.CreateDirectory(upload);
                            string filePath = Path.Combine(upload, file.FileName);
                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }
                        }
                        _db.SaveChanges(); // บันทึก newFile แต่ละไฟล์ก่อนเพื่อให้ได้ Id

                        ProjectXfile projectXfile = new ProjectXfile
                        {
                            ProjectId = projectUpdate.Id, // ใช้ Id ของ newProject
                            FileUploadId = newFile.Id, // ใช้ Id ของ newFile
                            IsDeleted = false, // กำหนดค่าว่ายังไม่ถูก Delete
                        };

                        _db.ProjectXfiles.Add(projectXfile);
                    }

                }
                else files = null;

            
                _db.SaveChanges(); // บันทึกการเปลี่ยนแปลงทั้งหมด

                transaction.Commit(); // ยืนยัน transaction
                }
            }


            // ถ้ามีข้อมูลโปรเจค
            if (projectUpdate != null)
            {
                // ตรวจสอบฟีลแต่ละฟีลและอัพเดตข้อมูล
                projectUpdate.Name = (string.IsNullOrEmpty(Request.Name) || Request.Name == "string") ? projectUpdate.Name : Request.Name;
                projectUpdate.Detail = (Request.Detail == "string") ? projectUpdate.Detail : Request.Detail;
                projectUpdate.StartDate = (Request.StartDate == null) ? projectUpdate.StartDate : DateTime.Parse(Request.StartDate);
                projectUpdate.EndDate = (Request.EndDate == null) ? projectUpdate.EndDate : DateTime.Parse(Request.EndDate);
                projectUpdate.UpdateDate = DateTime.Now;
                

                //     // บันทึกการอัพเดทลงในฐานข้อมูล
                Project.Update(_db, projectUpdate);
                _db.SaveChanges();
                //     // ส่งค่าสถานะการอัพเดทเป็น 200 OK

                return new Response
                {
                    Code = 200,
                    Message = "Success",
                    Data = projectUpdate
                };



            }
            else
                return new Response
                {
                    Code = 400,
                    Message = "Bad Request",
                    Data = null
                };


        }
        else
        {
            {
                // หากไม่พบโปรเจคที่ต้องการอัพเดท ส่งค่า BadRequest
                return new Response
                {
                    Code = 500,
                    Message = "Not Find Project",
                };
            }
        }
    }

    // เปลี่ยน IsDeleted ของโปรเจคเป็น true และ กิจกรรมย่อยทั้งหมดเป็น true
    [HttpDelete(Name = "DeleteProject")]
    public ActionResult<Response> DeleteProduct(int id)
    {
        // ค้นหาโปรเจคจากฐานข้อมูลโดยใช้ Id
        Project? DataOfProject = _db.Projects.Find(id);

        // ตรวจสอบว่าโปรเจคที่ค้นหาพบหรือไม่
        if (DataOfProject != null)
        {
            // ค้นหากิจกรรมที่เกี่ยวข้องกับโปรเจค
            DataOfProject.Activities = _db.Activities
                .Where(useinit => useinit.ProjectId == id && useinit.ActivityHeaderId == null && useinit.IsDeleted != true)
                .ToList();

            // วนลูปผ่านกิจกรรมในโปรเจคและเรียกใช้เมธอด TakeActivity
            foreach (Activity activity in DataOfProject.Activities)
            {
                Activity.DeleteActivityOfProject(DataOfProject.Activities, _db); //ไปวนหาว่ามีลูกอีกไหม
            }

            // ถ้ามีข้อมูลโปรเจค
            DataOfProject.IsDeleted = true;
            // บันทึกการอัพเดทลงในฐานข้อมูล
            Project.Update(_db, DataOfProject);
            _db.SaveChanges();
            // ส่งค่าสถานะการอัพเดทเป็น 200 OK
            return new Response
            {
                Code = 200,
                Message = "Success",
                Data = DataOfProject
            };



        }
        else
            return new Response
            {
                Code = 400,
                Message = "หาโปรเจคไม่เจอ หรือว่าถูกลบไปแล้ว",
                Data = null
            };


    }
}
