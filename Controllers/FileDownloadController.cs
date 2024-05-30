using System.ComponentModel.DataAnnotations;
using backend_ProjectmanagementV2.Data;
using backend_ProjectmanagementV2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("FileDownload")]

public class FileDownloadController : ControllerBase
{

    private DatabaseContext _db = new DatabaseContext();

    private IHostEnvironment _hostingEnvironment;

    private readonly ILogger<FileUploadController> _logger;

    public FileDownloadController(ILogger<FileUploadController> logger, IHostEnvironment? environment)
    {
        _logger = logger;
        _hostingEnvironment = environment;
    }

[HttpGet("DownloadFile/{id}")]
    public IActionResult DownloadFile(int id)
    {
        // ดึงข้อมูลไฟล์จากฐานข้อมูล
        var file = _db.FileUploads.FirstOrDefault(f => f.Id == id);
        if (file == null)
        {
            return NotFound(new Response
            {
                Code = 404,
                Message = "File Not Found"
            });
        }

        // ระบุเส้นทางของไฟล์ที่ต้องการดาวน์โหลด
        string filePath = Path.Combine(_hostingEnvironment.ContentRootPath, file.FilePath, file.Id.ToString(), file.FileName);

        // ตรวจสอบว่าไฟล์มีอยู่จริง
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new Response
            {
                Code = 404,
                Message = "File Not Found"
            });
        }

        // อ่านไฟล์เป็น byte array
        var memory = new MemoryStream();
        using (var stream = new FileStream(filePath, FileMode.Open))
        {
            stream.CopyTo(memory);
        }
        memory.Position = 0;

        // คืนไฟล์ให้กับผู้ใช้
        return File(memory, GetContentType(filePath), file.FileName);
    }
      private string GetContentType(string path)
    {
        var types = GetMimeTypes();
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return types[ext];
    }

    private Dictionary<string, string> GetMimeTypes()
    {
        return new Dictionary<string, string>
        {
            { ".txt", "text/plain" },
            { ".pdf", "application/pdf" },
            { ".doc", "application/vnd.ms-word" },
            { ".docx", "application/vnd.ms-word" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats.officedocument.spreadsheetml.sheet" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".gif", "image/gif" },
            { ".csv", "text/csv" }
        };
    }
}