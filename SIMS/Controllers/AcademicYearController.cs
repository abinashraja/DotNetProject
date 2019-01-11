using EPortal.App_Start;
using EPortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace EPortal.Controllers
{
    public class AcademicYearController : BaseController
    {
        private const string V = "AcademicYear";
        #region User Index
        [Authorize]
        [CustomFilter(PageName = V)]
        public ActionResult Index()
        {

            return View("AcademicYear");
        }
        #endregion

        #region Get AcademicYear List
        [Authorize]
        [CustomFilter(PageName = V)]
        public JsonResult GetAcademicYearList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<AcademicYearList> org = new List<AcademicYearList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.AcademicYears
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new AcademicYearList
                       {
                           Id = o.Id,
                           Code = o.Code,
                           Name = o.Name,
                           PeriodFrom = o.AcademicYearFrom.Value,
                           PeriodTo = o.AcademicYearTo.Value,
                           Operation = "Create",
                           DeleteConformation = false,
                           CreateDateTime = o.CreateDateTime
                       }).OrderByDescending(x => x.CreateDateTime).ToList();
            }
            string dateformat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion        

        #region Create AcademicYear 
        [Authorize]
        [CustomFilter(PageName = V)]
        public JsonResult SaveAcademicYear(EPortal.Models.AcademicYear academicyear)
        {
            string errormsg = "";
            int result = 0;

            //if ((role.Code != "" || role.Code != null) && (role.Name != "" || role.Name != null))
            {
                //string orgid = Session["OrgId"].ToString();

                string orgid = User.OrgId;


                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (academicyear.Operation == "Create")
                    {

                        var checkrolecode = (from r in entity.AcademicYears
                                             where r.OrganizationID == orgid
                                             && r.Code == academicyear.Code
                                             select r).FirstOrDefault();
                        if (checkrolecode == null)
                        {
                            academicyear.Id = Guid.NewGuid().ToString();
                            academicyear.RowState = true;
                            academicyear.CreateDateTime = DateTime.Now;
                            academicyear.OrganizationID = orgid;
                            entity.Entry(academicyear).State = System.Data.Entity.EntityState.Added;

                        }
                        else
                        {
                            errormsg = "Subject already exist with same Code.";
                        }
                        try
                        {

                            result = entity.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    else
                    {
                        EPortal.Models.AcademicYear roledata = (from o in entity.AcademicYears
                                                                where o.OrganizationID == orgid
                                                                && o.Id == academicyear.Id
                                                                select o
                               ).FirstOrDefault();

                        roledata.Code = academicyear.Code;
                        roledata.Name = academicyear.Name;
                        roledata.AcademicYearFrom = academicyear.AcademicYearFrom;
                        roledata.AcademicYearTo = academicyear.AcademicYearTo;
                        entity.Entry(roledata).State = System.Data.Entity.EntityState.Modified;
                        try
                        {

                            result = entity.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                }
            }
            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete AcademicYear
        [Authorize]
        [CustomFilter(PageName = V)]
        public JsonResult DeleteAcademicYear(EPortal.Models.AcademicYear academicyear)
        {

            int result = 0;
            string orgid = User.OrgId;

            string errormsg = string.Empty;

            using (EPortalEntities entity = new EPortalEntities())
            {
                //var checkref = (from r in entity.Subjects
                //                where r.OrganizationID == orgid
                //                && r.Id == Subject.Id
                //                select r).FirstOrDefault();
                //if (checkref != null)
                //{
                //    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                //}
                //else
                //{

                //Subject subjectinfo = (from sub in entity.Subjects
                //                       where sub.Id == Subject.Id
                //                       && sub.OrganizationID == orgid
                //                       select sub).FirstOrDefault();
                entity.Entry(academicyear).State = System.Data.Entity.EntityState.Deleted;


                //List<SubjectDetail> subjectdetails = (from detsub in entity.SubjectDetails
                //                                      where detsub.SubjectId == Subject.Id
                //                                      && detsub.OrganizationID == orgid
                //                                      select detsub).ToList();
                //if (subjectdetails.Count() > 0)
                //{
                //    foreach (SubjectDetail item in subjectdetails)
                //    {
                //        entity.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                //    }

                //}

                result = entity.SaveChanges();
                //}
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit AcademicYear
        [Authorize]
        [CustomFilter(PageName = V)]
        public JsonResult GetAcademicYearInfo(EPortal.Models.AcademicYear academicyear)
        {

            string orgid = User.OrgId;

            AcademicYearList roleinforole = new AcademicYearList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                roleinforole = (from o in entity.AcademicYears
                                where o.Id == academicyear.Id
                                && o.OrganizationID == orgid
                                select new AcademicYearList
                                {
                                    Id = o.Id,
                                    Code = o.Code,
                                    Name = o.Name,
                                    PeriodFrom = o.AcademicYearFrom.Value,
                                    PeriodTo = o.AcademicYearTo.Value,
                                    Operation = "Edit",
                                }).FirstOrDefault();
            }
            return Json(roleinforole, JsonRequestBehavior.AllowGet);
        }
        #endregion

        private List<Courselist> GetCourseListinfo()
        {
            string orgid = User.OrgId;
            List<Courselist> courselist = new List<Courselist>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                courselist = (from c in entity.Courses
                              where c.OrganizationID == orgid
                              select new Courselist
                              {
                                  Id = c.Id,
                                  Code = c.Code,
                                  Name = c.Name,
                                  Selected = false

                              }).ToList();
            }
            return courselist;
        }

        #region GetCourseList
        public JsonResult GetCourseList()
        {
            string orgid = User.OrgId;

            List<Courselist> courselist = GetCourseListinfo();


            return Json(courselist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Configuration Page
        [Authorize]
        [CustomFilter(PageName = V)]
        public ActionResult GetConfig()
        {

            return View("AcademicYearConfiguration");
        }

        public JsonResult GetStructure(string academicyear)
        {

            string orgid = User.OrgId;


            using (EPortalEntities entities = new EPortalEntities())
            {


                var data = (from c in entities.Courses
                            where c.OrganizationID == User.OrgId
                            select new
                            {
                                id = c.Id,
                                text = c.Name,
                                children = (from cl in entities.Classes
                                            where cl.OrganizationID == User.OrgId
                                            select new
                                            {
                                                id = c.Id + "_" + cl.Id,
                                                text = cl.Name,
                                                children = (from sec in entities.Sections
                                                            where sec.OrganizationID == User.OrgId
                                                            select new
                                                            {
                                                                id = c.Id + "_" + cl.Id + "_" + sec.Id,
                                                                text = sec.Name,
                                                                children = (from sub in entities.Subjects
                                                                            where sub.OrganizationID == User.OrgId
                                                                            select new
                                                                            {

                                                                                id = c.Id + "_" + cl.Id + "_" + sec.Id + "_" + sub.Id,
                                                                                text = sub.Name

                                                                            }).ToList().OrderBy(x => x.text),


                                                            }).ToList().OrderBy(x => x.text)

                                            }).ToList().OrderBy(x => x.text)
                            }).ToList().OrderBy(x => x.text);


                var selectedsubject = (from s in entities.AcademicYearCourseClassSectionSubjects
                                       where s.OrganizationID == orgid
                                       && s.AcademicYearId == academicyear
                                       select s.CourseId + "_" + s.ClassID + "_" + s.SectionId + "-" + s.SubjectId).ToArray();
                var selectedvalue = string.Join(",", selectedsubject);



                return Json(new { data = data, id = User.OrgId, name = User.OrgName, selectedvalue = selectedvalue }, JsonRequestBehavior.AllowGet);


            }

        }

        #endregion

        #region Save Configuration Academic Year
        public JsonResult SaveAcYearConfiguration(string[] selecteddata, string acyearid)
        {
            string orgid = User.OrgId;
            int result = 0;

            EPortal.Models.AcademicYearCourse course = null;
            EPortal.Models.AcademicYearCourseClass courseclass = null;
            EPortal.Models.AcademicYearCourseClassSection courseclasssection = null;
            EPortal.Models.AcademicYearCourseClassSectionSubject courseclasssectionsubject = null;
            using (EPortalEntities entity = new EPortalEntities())
            {

                foreach (var item in selecteddata)
                {
                    string courseid = item.Split('_')[0].ToString();
                    string classid = item.Split('_')[1].ToString();
                    string sectionid = item.Split('_')[2].ToString();
                    string subjectid = item.Split('_')[3].ToString();

                    #region AcYearCourse Save
                    course = new AcademicYearCourse();
                    course.Id = Guid.NewGuid().ToString();
                    course.CourseId = courseid;
                    course.OrganizationID = orgid;
                    course.AcademicYearId = acyearid;
                    course.Code = "";
                    course.Name = "";
                    course.Name = "";
                    course.RowState = true;
                    course.CreateDateTime = DateTime.Now;
                    entity.Entry(course).State = System.Data.Entity.EntityState.Added;
                    #endregion

                    #region AcYearCourseclass Save


                    courseclass = new AcademicYearCourseClass();
                    courseclass.Id = Guid.NewGuid().ToString();
                    courseclass.Code = "";
                    courseclass.Name = "";
                    courseclass.CourseId = courseid;
                    courseclass.ClassID = classid;
                    courseclass.OrganizationID = orgid;
                    courseclass.AcademicYearId = acyearid;
                    courseclass.RowState = true;
                    courseclass.CreateDateTime = DateTime.Now;
                    entity.Entry(courseclass).State = System.Data.Entity.EntityState.Added;


                    #endregion

                    #region AcYearCourseClassSection

                    courseclasssection = new AcademicYearCourseClassSection();
                    courseclasssection.Id = Guid.NewGuid().ToString();
                    courseclasssection.Code = "";
                    courseclasssection.Name = "";
                    courseclasssection.CourseId = courseid;
                    courseclasssection.ClassID = classid;
                    courseclasssection.SectionId = sectionid;
                    courseclasssection.OrganizationID = orgid;
                    courseclasssection.AcademicYearId = acyearid;
                    courseclasssection.RowState = true;
                    courseclasssection.CreateDateTime = DateTime.Now;
                    entity.Entry(courseclasssection).State = System.Data.Entity.EntityState.Added;

                    #endregion

                    #region AcYearCourseClassSectionSubject

                    courseclasssectionsubject = new AcademicYearCourseClassSectionSubject();
                    courseclasssectionsubject.Id = Guid.NewGuid().ToString();
                    courseclasssectionsubject.Code = "";
                    courseclasssectionsubject.Name = "";
                    courseclasssectionsubject.OrganizationID = orgid;
                    courseclasssectionsubject.CourseId = courseid;
                    courseclasssectionsubject.ClassID = classid;
                    courseclasssectionsubject.SectionId = sectionid;
                    courseclasssectionsubject.SubjectId = subjectid;
                    courseclasssectionsubject.AcademicYearId = acyearid;
                    courseclasssectionsubject.RowState = true;
                    courseclasssectionsubject.CreateDateTime = DateTime.Now;
                    entity.Entry(courseclasssectionsubject).State = System.Data.Entity.EntityState.Added;


                    #endregion
                }

                try
                {
                    result = entity.SaveChanges();

                }
                catch (Exception ex)
                {


                }


            }


            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);

        }
        #endregion
    }

    public class TreeClass
    {

        public string Id { get; set; }
        public string Text { get; set; }
        public List<TreeClass> Children { get; set; }
    }

    public class AcademicYearList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Operation { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public bool DeleteConformation { get; set; }
        public bool IsApplicant { get; set; }
        public DateTime CreateDateTime { get; set; }

    }
}