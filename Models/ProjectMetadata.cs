using backend_ProjectmanagementV2.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace backend_ProjectmanagementV2.Models

{
    public class ProjectMetadata
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Detail { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsDeleted { get; set; }

        public int? ActivitiesId { get; set; }
    }

    public class ProjectCreate
    {

        public string? Name { get; set; }

        public int? OwnerId { get; set; }

        public string? Detail { get; set; }

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }
        public string? Activities { get; set; }

        public virtual ICollection<ProjectXfile>? ProjectXfiles { get; set; } = new List<ProjectXfile>();


    }

    public class ProjectUpdate

    {
        public int? Id { get; set; }
        public string? Name { get; set; }

        public int? OwnerId { get; set; }

        public string? Detail { get; set; }

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }
        public string? Activities { get; set; }

        public string? ProjectXfiles { get; set; } 

    }

    public class UpdateProjectXfile {

        public required ProjectUpdate ProjectUpdate { get; set; }

        public List<IFormFile>? Files { get; set; }
    }

    public class CreateProjectXfile
    {

        public required ProjectCreate ProjectCreate { get; set; }

        public List<IFormFile>? Files { get; set; }

    }


    public class ProjectUpdateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public string Detail { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Activities { get; set; }
        public virtual ICollection<ProjectXfile>? ProjectXfiles { get; set; } = new List<ProjectXfile>();

    }


    [MetadataType(typeof(ProjectMetadata))]

    public partial class Project
    {
        public static Project Create(DatabaseContext db, Project project, FileUpload file)
        {
            // ตั้งค่าวันที่สร้างและวันที่อัพเดทสำหรับโปรเจ็ค
            project.CreateDate = DateTime.Now;
            project.UpdateDate = DateTime.Now;
            project.IsDeleted = false;

            // ตั้งค่าวันที่สร้างและวันที่อัพเดทสำหรับไฟล์
            file.CreateDate = DateTime.Now;
            file.UpdateDate = DateTime.Now;
            file.IsDeleted = false;

            // เพิ่มโปรเจ็คและไฟล์ลงในฐานข้อมูล
            db.Projects.Add(project);
            db.FileUploads.Add(file);

            // บันทึกการเปลี่ยนแปลงในฐานข้อมูล
            db.SaveChanges();

            return project;
        }

        public static List<Project> GetAll(DatabaseContext db)
        {
            List<Project> returnThis = db.Projects.Where(q => q.IsDeleted != true).ToList();
            return returnThis;
        }


        public static Project Update(DatabaseContext db, Project project)
        {
            project.UpdateDate = DateTime.Now;
            db.Entry(project).State = EntityState.Modified;
            db.SaveChanges();
            return project;
        }

        public static Project GetById(DatabaseContext db, int id)
        {
            Project? project = db.Projects
                                    .Include(p => p.Activities) // Eager loading ข้อมูลกิจกรรม
                                    .FirstOrDefault(q => q.Id == id && q.IsDeleted != true);
            return project ?? new Project(); // คืนค่าโปรเจคพร้อมกับข้อมูลกิจกรรมหรือสร้างโปรเจคใหม่หากไม่พบ
        }

        public static Project Delete(DatabaseContext db, int id)
        {
            Project project = GetById(db, id);

            project.IsDeleted = true;
            // db.Employees.Remove(employee); เป็นวิธีการลบแบบให้หายไปเลย
            db.Entry(project).State = EntityState.Modified; // Soft Delete
            db.SaveChanges();

            return project;
        }
    }

}