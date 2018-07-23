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
using Newtonsoft.Json;

namespace EPortal.Controllers
{
    public class AcademicYearController : BaseController
    {
        #region User Index
        [Authorize]
        [CustomFilter(PageName = "AcademicYear")]
        public ActionResult Index()
        {

            return View("AcademicYear");
        }
        #endregion

        #region Get AcademicYear List
        [Authorize]
        [CustomFilter(PageName = "AcademicYear")]
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
                           PeriodFrom=o.PeriodFrom,
                           PeriodTo=o.PeriodTo,
                           Operation = "Create",
                           DeleteConformation = false,
                           CreateDateTime = o.CreateDateTime
                       }).OrderByDescending(x => x.CreateDateTime).ToList();
            }
            string dateformat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion        

        #region Create Subject 
        [Authorize]
        [CustomFilter(PageName = "AcademicYear")]
        public JsonResult SaveAcademicYear(EPortal.Models.AcademicYear academicyear)
        {
            AcademicYear academicyearinfo = null;

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
                            academicyear.Creator = User.UserId;
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
                        roledata.PeriodFrom = academicyear.PeriodFrom;
                        roledata.PeriodTo = academicyear.PeriodTo;
                        roledata.Modifier = User.UserId;
                        roledata.UpdatedDateTime = DateTime.Now;
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

        #region Delete Subject
        [Authorize]
        [CustomFilter(PageName = "Subject")]
        public JsonResult DeleteSubject(EPortal.Models.Subject Subject)
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
                entity.Entry(Subject).State = System.Data.Entity.EntityState.Deleted;


                List<SubjectDetail> subjectdetails = (from detsub in entity.SubjectDetails
                                                      where detsub.SubjectId == Subject.Id
                                                      && detsub.OrganizationID == orgid
                                                      select detsub).ToList();
                if (subjectdetails.Count() > 0)
                {
                    foreach (SubjectDetail item in subjectdetails)
                    {
                        entity.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                    }

                }

                result = entity.SaveChanges();
                //}
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit Subject
        [Authorize]
        [CustomFilter(PageName = "AcademicYear")]
        public JsonResult GetAcademicYearInfo(EPortal.Models.AcademicYear academicyear)
        {
            //string orgid = Session["OrgId"].ToString();

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
                                    PeriodFrom=o.PeriodFrom,
                                    PeriodTo=o.PeriodTo,
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