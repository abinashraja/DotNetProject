using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Models;
using EPortal.Utility;
using EPortal.App_Start;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using System.Globalization;

namespace EPortal.Controllers
{
    public class CourseController : BaseController
    {
        #region User Index
        [Authorize]
        [CustomFilter(PageName = "Course")]
        public ActionResult Index()
        {

            return View("Course");
        }
        #endregion

        #region Get Course List
        [Authorize]
        [CustomFilter(PageName = "Course")]
        public JsonResult GetCourseList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<CourseList> org = new List<CourseList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Courses
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new CourseList
                       {
                           Id = o.Id,
                           Code = o.Code,
                           Name = o.Name,
                           Operation = "Create",
                           DeleteConformation = false,
                           CreatedDateTime = o.CreateDateTime
                       }).OrderByDescending(x => x.CreatedDateTime).ToList();
            }
            string dateformat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion        

        #region Create Course 
        [Authorize]
        [CustomFilter(PageName = "Course")]
        public JsonResult SaveCourse(EPortal.Models.Course course)
        {


            string errormsg = "";
            int result = 0;

            //if ((role.Code != "" || role.Code != null) && (role.Name != "" || role.Name != null))
            {
                //string orgid = Session["OrgId"].ToString();

                string orgid = User.OrgId;

                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (course.Operation == "Create")
                    {

                        var checkrolecode = (from r in entity.RoleMasters
                                             where r.OrganizationID == orgid
                                             && r.Code == course.Code
                                             select r).FirstOrDefault();
                        if (checkrolecode == null)
                        {

                            course.Id = Guid.NewGuid().ToString();
                            course.OrganizationID = orgid;
                            course.RowState = true;
                            course.CreateDateTime = System.DateTime.Now;

                            entity.Entry(course).State = System.Data.Entity.EntityState.Added;
                            entity.Courses.Add(course);
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
                            errormsg = "Course already exist with same Code.";
                        }

                    }
                    else
                    {
                        EPortal.Models.Course roledata = (from o in entity.Courses
                                                              where o.OrganizationID == orgid
                                                              && o.Id == course.Id
                                                              select o
                               ).FirstOrDefault();

                        roledata.Code = course.Code;
                        roledata.Name = course.Name;
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

        #region Delete Course
        [Authorize]
        [CustomFilter(PageName = "Course")]
        public JsonResult DeleteCourse(EPortal.Models.Course course)
        {

            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            string errormsg = string.Empty;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {
                var checkref = (from r in entity.SubjectDetails
                                where r.OrganizationID == orgid
                                && r.CourseId == course.Id
                                select r).FirstOrDefault();
                if (checkref != null)
                {
                    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                }
                else
                {

                    entity.Entry(course).State = System.Data.Entity.EntityState.Deleted;
                    result = entity.SaveChanges();
                }
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit Course
        [Authorize]
        [CustomFilter(PageName = "Course")]
        public JsonResult GetCourseInfo(EPortal.Models.Course course)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;

            RoleList roleinforole = new RoleList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                roleinforole = (from o in entity.Courses
                                where o.Id == course.Id
                                && o.OrganizationID == orgid
                                select new RoleList
                                {
                                    Id = o.Id,
                                    Code = o.Code,
                                    Name = o.Name,
                                    Operation = "Edit"
                                }).FirstOrDefault();
            }
            return Json(roleinforole, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    public class CourseList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }

}