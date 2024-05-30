using System.ComponentModel.DataAnnotations;
using backend_ProjectmanagementV2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ObjectPool;

namespace backend_ProjectmanagementV2.Models

{
    public class ProjectXfileMetadata
    {
        public int Id { get; set; }

        public string? FileName { get; set; }

        public string? FilePath { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsDeleted { get; set; }
    }

    public class ProjectXfileCreate
    {
        public int Id { get; set; }

        public string? FileName { get; set; }

        public string? FilePath { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsDeleted { get; set; }
    }

    public class ProjectXfileRequest
    {

        public int? ProjectId { get; set; }

        public int? FileUploadId { get; set; }

    }
    [MetadataType(typeof(ProjectXfileMetadata))]

    public partial class ProjectXfile
    {

        public static ProjectXfile Create(DatabaseContext db, ProjectXfile file)
        {
            file.IsDeleted = false;
            db.ProjectXfiles.Add(file);
            return file;
        }

        public static List<ProjectXfile> GetAll(DatabaseContext db)
        {
            List<ProjectXfile> returnThis = db.ProjectXfiles.Where(q => q.IsDeleted != true).ToList();
            return returnThis;
        }

          public static List<ProjectXfile> GetById(DatabaseContext db, int id)
        {   
            List<ProjectXfile>? file = db.ProjectXfiles
                                    .Where(q => q.ProjectId == id && q.IsDeleted != true).ToList();
            return file; // คืนค่าโปรเจคพร้อมกับข้อมูลกิจกรรมหรือสร้างโปรเจคใหม่หากไม่พบ
        }

        public static ProjectXfile GetOnlyThis(DatabaseContext db , int id){
            ProjectXfile? file = db.ProjectXfiles.Where(q => q.Id == id ).FirstOrDefault();
            return file;
        }

         

        
        public static ProjectXfile Update(DatabaseContext db, ProjectXfile project)
        {
            db.Entry(project).State = EntityState.Modified;
            db.SaveChanges();
            return project;
        }

    }
}