using backend_ProjectmanagementV2.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace backend_ProjectmanagementV2.Models

{
    public class ActivityMetadata
    {
        public int Id { get; set; }

        public int? ActivityHeaderId { get; set; }

        public string? Name { get; set; }

        public string? Detail { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public bool? IsDeleted { get; set; }

        public int? ProjectId { get; set; }

        public virtual Activity? ActivityHeader { get; set; }

        public virtual ICollection<Activity> InverseActivityHeader { get; set; } = new List<Activity>();

        public virtual Project? Project { get; set; }


    }

    [MetadataType(typeof(ActivityMetadata))]

    public partial class Activity
    {
        public static Activity GetByActivityId(DatabaseContext db, int id)
        {
            Activity? returnThis = db.Activities.Where(q => q.Id == id && q.IsDeleted != true).FirstOrDefault();
            return returnThis ?? new Activity();
        }
        public static void Create(Activity parent, Project project, DatabaseContext db, Activity activity)
        // ฟังก์ชั่น Create จะรับค่า กิจกรรมตัวแม่ (ตัวที่แล้วอ่ะ) , กับตัวโปรเจคปัจจุบัน , DataBase , และ กิจกรรมย่อยตัวนี้
        {

            if(parent == null){
                activity.ActivityHeaderId = null;
            }
            // กิจกรรมย่อยตัวนี้ จะมี Activity Header เป็น แม่ หรือเอาข้อมูลของแม่มาใส่นั่นเอง
            activity.ActivityHeader = parent;
            // กิจกรรมย่อยตัวนี้ ถูกกำหนดให้อยู่ในโปรเจคตัวที่ส่งเข้ามา แบบชี้ว่าเป็นกิจกรรมย่อยของโปรเจคนี้นะไรงี้
            activity.Project = project;
            // อัพเดตเวลาปกติเลย แต่อันนี้เวลาที่เราสร้างกิจกรรมย่อยใหม่ก็ต้องไม่เท่ากับตัวโปรเจคเนอะในกรณีที่ไม่ได้สร้างมาพร้อมกัน
            activity.CreateDate = DateTime.Now;
            activity.UpdateDate = DateTime.Now;
            activity.IsDeleted = false;

            //อันนี้คือว่าเราจะไปเช็คว่าในกิจกรรมย่อยตัวนี้ ที่กำลังทำอยู่เนี่ย มีกิจกรรมย่อยของมันอีกไหมเนอะ เพราะว่าต้องทำตัวเล็กด้วยถ้าตัวเล็กถูกสร้าง
            //ก็คือเราจะวนโดยใช้ค่า newActivity ไปวนหาข้อมูลใน กิจกรรมย่อยตัวนี้ในฟีลล์ InverseActivityHeader (ก็คือฟีลล์ไว้สำหรับเก็บข้อมูลตัวลูกนั่นแหละ)
            foreach (Activity newActivity in activity.InverseActivityHeader)
            {
                //ก็ถ้ามีเราก็จะโยนกลับไปเพื่อให้สร้างลูกมันไปด้วยพร้อมๆกัน ก็จะโยนจนกว่าลูกจะหมดก็จะหลุด foreach อ่ะเพราะมันทำจนข้อมูลหมด
                Activity.Create(activity, project, db, newActivity);
            }

        }

        public static void SendActivities(Project project, ICollection<Activity> activityOrigin, ICollection<Activity> activityRecive)
        {
            if (activityRecive == null || activityOrigin == null)
            {
                return;
            }

            foreach (Activity Data in activityRecive)  // วนหาค่าภายใน activity 
            {
                Activity HeadData = new Activity           // สร้าง activity ใหม่
                {
                    Name = Data.Name,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    IsDeleted = false,
                    Project = project
                };

                // วนสร้างลูกใหม่เรื่อยๆ
                if (Data.InverseActivityHeader != null)
                {
                    HeadData.InverseActivityHeader = new List<Activity>();
                    SendActivities(project, HeadData.InverseActivityHeader, Data.InverseActivityHeader);
                }

                // สร้างลูกในแม่
                activityOrigin.Add(HeadData);
            }
        }


        public static void TakeActivity(Activity? parent, Project project, Activity activity, DatabaseContext _db)
        {
            //กิจกรรมย่อยนี้ถูกสร้างขึ้นมาหรือยัง? ถ้ายังไม่ถูกสร้าง ( activity Id จะเป็น 0 )
            if (activity.Id == 0)
            {
                
                // โยนไปให้ฟังก์ชั่น Create โดยส่งค่า กิจกรรมตัวแม่ , โปรเจคตัวแม่ , ส่งกิจกรรมย่อยไป , ส่ง DataBase ไป
                Activity.Create(parent, project, _db, activity);

                //เมื่อออกจากฟังก์ชั่น Create แล้วจะนำข้อมูล Activities ที่ถูกสร้างขึ้นมามาเพิ่มใน DataBase
                _db.Activities.Add(activity);
            }
            else //ถ้ากิจกรรมนี้เคยถูกสร้างขึ้นมาอยู่แล้ว เราจะส่งไปให้อัพเดตแทนที่จะสร้างใหม่
            {
                // โยนกิจกรรมตัวแม่ไปหาก่อนว่ามีลูกไหม เพราะว่าเราต้องอัพเดตตัวลูกก่อนถึงจะอัพเดตตัวแม่ได้
                foreach (Activity Data in activity.InverseActivityHeader)
                {
                    //ฟังก์ชั่นสำหรับหาลูกของกิจกรรม โดยเราจะส่งกิจกรรมย่อย , ตัวโปรเจค , ตัวข้อมูลในฟีวล์ข้อมูล InverseActivityHeader(หรือกิจกรรมตัวลูกนั่นเอง) เข้าไป และส่ง DataBase
                    TakeActivity(activity, project, Data, _db);
                }
                // เมื่อเสร็จในการอัพเดตทุกตัวจะออกมาทำบรรทัดต่อไปหลังจากนี้

                // ค้นหาข้อมูลในฟีลล์ Activities ตาม id ของตัวกิจกรรม
                Activity? dataUpdate = _db.Activities.Find(activity.Id);

                // ถ้าข้อมูลที่ไปหามามีข้อมูลอยู่ก็จะนำข้อมูลมาอัพเดต ชื่อ ,วันที่อัพเดต,และ IsDeleted โดยอิงจากข้อมูลปัจจุบัน
                if (dataUpdate != null)
                {
                    dataUpdate.Name = activity.Name;
                    dataUpdate.UpdateDate = DateTime.Now;
                    dataUpdate.IsDeleted = activity.IsDeleted;
                    _db.Update(dataUpdate);
                    _db.SaveChanges();
                }
            }
        }


        public static void GetALLActivityinside(ICollection<Activity> activity, DatabaseContext _db)
        {
            foreach (Activity Data in activity)
            {

                Data.InverseActivityHeader = _db.Activities.Where(i => i.ActivityHeaderId == Data.Id && i.IsDeleted != true).AsNoTracking().ToList();
                if (Data.InverseActivityHeader.Count > 0)
                {
                    GetALLActivityinside(Data.InverseActivityHeader, _db);
                }
            }
            return;
        }

        public static void DeleteActivityOfProject(ICollection<Activity> activity, DatabaseContext _db)
        {
            foreach (Activity Data in activity)
            {

                Data.InverseActivityHeader = _db.Activities.Where(i => i.ActivityHeaderId == Data.Id && i.IsDeleted != true).AsNoTracking().ToList();
                if (Data.InverseActivityHeader.Count > 0)
                {
                    DeleteActivityOfProject(Data.InverseActivityHeader, _db);
                }
                Data.IsDeleted = true;
            }
            return;
        }


    }


}