using System.ComponentModel.DataAnnotations;
using backend_ProjectmanagementV2.Data;
namespace backend_ProjectmanagementV2.Models

{
    public class FileUploadMetadata
    {
    public int Id { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsDeleted { get; set; }
    }

  

    
    [MetadataType(typeof(FileUploadMetadata))]

    public partial class FileUpload 
    {

        public static FileUpload Create(DatabaseContext db , FileUpload file)
        {
            file.CreateDate = DateTime.Now;
            file.UpdateDate = DateTime.Now;
            file.IsDeleted = false;
            db.FileUploads.Add(file);
            db.SaveChanges();

            return file;
        }

         public static List<FileUpload> GetById(DatabaseContext db, int id)
        {   
            List<FileUpload>? file = db.FileUploads
                                    .Where(q => q.Id == id && q.IsDeleted != true).ToList();
            return file; // คืนค่าโปรเจคพร้อมกับข้อมูลกิจกรรมหรือสร้างโปรเจคใหม่หากไม่พบ
        }

      
    }
}