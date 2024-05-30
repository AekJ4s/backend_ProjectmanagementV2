using System.ComponentModel.DataAnnotations;
using backend_ProjectmanagementV2.Data;
using backend_ProjectmanagementV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("FileUpload")]
[Authorize]

public class FileUploadController : ControllerBase
{

    private DatabaseContext _db = new DatabaseContext();

    private IHostEnvironment _hostingEnvironment;

    private readonly ILogger<FileUploadController> _logger;

    public FileUploadController(ILogger<FileUploadController> logger, IHostEnvironment? environment)
    {
        _logger = logger;
        _hostingEnvironment = environment;
    }

    [HttpPost(Name = "UploadFile")]
    public ActionResult UploadFile([FromForm] List<IFormFile> formFiles)
    {
        if(formFiles == null || formFiles.Count == 0)
        {
            return BadRequest(new Response
            {
                Code = 400,
                Message = "File is Required"
                
            });
        }

        List<FileUpload> uploadedFiles = new List<FileUpload>();
        
        foreach(var formFile in formFiles)
        {
            var file = new FileUpload
            {
                FileName = formFile.FileName,
                FilePath = "UploadedFile/ProfileImg"
            };

            file = FileUpload.Create(_db,file);
            uploadedFiles.Add(file);

            if(formFile.Length > 0)
            {
                string upload = Path.Combine(_hostingEnvironment.ContentRootPath,"UploadedFile/ProfileImg/", file.Id.ToString());
                Directory.CreateDirectory(upload);
                string filePath = Path.Combine(upload, formFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    formFile.CopyTo(fileStream);
                }
            }
        }

        return Ok(new Response
        {
            Code = 200,
            Message = "Upload File Success",
            Data = uploadedFiles
        });
    }

     



     [HttpGet("GetBy/{id}", Name = "GetFileById")]
    public ActionResult<Response> GetFileId(int id)
    {
        try
        {
            List<FileUpload> fileOfProject = FileUpload.GetById(_db, id); // ได้ไฟล์ที่เกี่ยวข้องกับโปรเจคทุกอย่างแล้ว

            // ตรวจสอบว่าโปรเจคที่ค้นหาพบหรือไม่
            if (fileOfProject == null)
            {
                return NotFound(new Response
                {
                    Code = 404,
                    Message = "Project not found or has been deleted"
                });
            }

            // ส่งข้อมูลโปรเจคกลับไปยังไคลเอนต์
            return Ok(new Response
            {
                Code = 200,
                Message = "Success",
                Data = fileOfProject
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

    
}


      

       

//         if(formFile != null && formFile.Length >0){
//             string upload = Path.Combine(_hostingEnvironment.ContentRootPath, "UploadedFile/ProfileImg/" + file.Id);

//             Directory.CreateDirectory(upload);
//             string filePath = Path.Combine(upload,formFile.FileName);
//             using (Stream fileStream = new FileStream(filePath, FileMode.Create))
//             {
//                 formFile.CopyTo(fileStream);
//             }

           
//         }
//          return Ok(new Response
//             {
//                 Code = 200,
//                 Message = "Success",
//                 Data = file
//             });
//     }
// }