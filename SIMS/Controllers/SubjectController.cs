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
    public class SubjectController : BaseController
    {
        #region User Index
        [Authorize]
        [CustomFilter(PageName = "Subject")]
        public ActionResult Index()
        {

            return View("Subject");
        }
        #endregion

        #region Get Subject List
        [Authorize]
        [CustomFilter(PageName = "Subject")]
        public JsonResult GetSubjectList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<SubjectList> org = new List<SubjectList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Subjects
                           //join subd in entity.SubjectDetails on new { subjectid = o.Id, orgid = o.OrganizationID } equals new { subjectid = subd.SubjectId, orgid = subd.OrganizationID }
                           //join course in entity.Courses on new { courseid = subd.CourseId, orgid = o.OrganizationID } equals new { courseid = course.Id, orgid = course.OrganizationID }
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new SubjectList
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

        #region Create Subject 
        [Authorize]
        [CustomFilter(PageName = "Subject")]
        public JsonResult SaveSubject(EPortal.Models.Subject Subjectinfo, List<Courselist> Courseinfo)
        {
            string errormsg = "";
            int result = 0;

            //if ((role.Code != "" || role.Code != null) && (role.Name != "" || role.Name != null))
            {
                //string orgid = Session["OrgId"].ToString();

                string orgid = User.OrgId;


                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (Subjectinfo.Operation == "Create")
                    {

                        var checkrolecode = (from r in entity.Subjects
                                             where r.OrganizationID == orgid
                                             && r.Code == Subjectinfo.Code
                                             select r).FirstOrDefault();
                        if (checkrolecode == null)
                        {
                            #region For Course but comented
                            //List<Courselist> seelctedcourse = Courseinfo.Where(x => x.Selected == true).ToList();

                            //if (seelctedcourse.Count() > 0)
                            //{


                            //    Subjectinfo.Id = Guid.NewGuid().ToString();
                            //    Subjectinfo.OrganizationID = orgid;
                            //    Subjectinfo.RowState = true;
                            //    Subjectinfo.CreateDateTime = System.DateTime.Now;
                            //    entity.Entry(Subjectinfo).State = System.Data.Entity.EntityState.Added;
                            //    entity.Subjects.Add(Subjectinfo);


                            //    foreach (Courselist item in seelctedcourse)
                            //    {
                            //        sibjectdetailinfo = new SubjectDetail();
                            //        sibjectdetailinfo.Id = Guid.NewGuid().ToString();
                            //        sibjectdetailinfo.SubjectId = Subjectinfo.Id;
                            //        sibjectdetailinfo.CourseId = item.Id;
                            //        sibjectdetailinfo.OrganizationID = orgid;
                            //        sibjectdetailinfo.RowState = true;
                            //        sibjectdetailinfo.CreateDateTime = System.DateTime.Now;
                            //        entity.Entry(sibjectdetailinfo).State = System.Data.Entity.EntityState.Added;
                            //        entity.SubjectDetails.Add(sibjectdetailinfo);
                            //    }

                            //}
                            #endregion


                            try
                            {
                                Subjectinfo.Id = Guid.NewGuid().ToString();
                                Subjectinfo.OrganizationID = User.OrgId;
                                Subjectinfo.CreateDateTime = DateTime.Now;
                                Subjectinfo.RowState = true;
                                

                                entity.Entry(Subjectinfo).State = System.Data.Entity.EntityState.Added;
                                result = entity.SaveChanges();
                            }
                            catch (Exception )
                            {

                            }
                        }

                        else
                        {
                            errormsg = "Subject already exist with same Code.";
                        }

                    }
                    else
                    {
                        EPortal.Models.Subject roledata = (from o in entity.Subjects
                                                           where o.OrganizationID == orgid
                                                           && o.Id == Subjectinfo.Id
                                                           select o
                               ).FirstOrDefault();

                        roledata.Code = Subjectinfo.Code;
                        roledata.Name = Subjectinfo.Name;
                        entity.Entry(roledata).State = System.Data.Entity.EntityState.Modified;


                        #region Couse BUt Comented
                        //foreach (Courselist item in Courseinfo)
                        //{
                        //    SubjectDetail subjectdetails = (from subjd in entity.SubjectDetails
                        //                                    where subjd.OrganizationID == orgid
                        //                                    && subjd.SubjectId == roledata.Id
                        //                                    && subjd.CourseId == item.Id
                        //                                    select subjd).FirstOrDefault();
                        //    if (subjectdetails != null)
                        //    {
                        //        if (item.Selected == false)
                        //        {
                        //            entity.Entry(subjectdetails).State = System.Data.Entity.EntityState.Deleted;

                        //        }
                        //        else
                        //        {
                        //            entity.Entry(subjectdetails).State = System.Data.Entity.EntityState.Modified;

                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (item.Selected == true)
                        //        {
                        //            sibjectdetailinfo = new SubjectDetail();
                        //            sibjectdetailinfo.Id = Guid.NewGuid().ToString();
                        //            sibjectdetailinfo.SubjectId = roledata.Id;
                        //            sibjectdetailinfo.CourseId = item.Id;
                        //            sibjectdetailinfo.OrganizationID = orgid;
                        //            sibjectdetailinfo.RowState = true;
                        //            sibjectdetailinfo.CreateDateTime = DateTime.Now;
                        //            entity.Entry(sibjectdetailinfo).State = System.Data.Entity.EntityState.Added;

                        //        }

                        //    }

                        //}
                        #endregion



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

        #region Edit Subject
        [Authorize]
        [CustomFilter(PageName = "Subject")]
        public JsonResult GetSubjectInfo(EPortal.Models.Subject Subject)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;

            SubjectList roleinforole = new SubjectList();
            //List<SubjectDetail> subjectdetails = new List<SubjectDetail>();
            List<Courselist> courselist = new List<Courselist>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                roleinforole = (from o in entity.Subjects
                                where o.Id == Subject.Id
                                && o.OrganizationID == orgid
                                select new SubjectList
                                {
                                    Id = o.Id,
                                    Code = o.Code,
                                    Name = o.Name,
                                    Operation = "Edit",
                                }).FirstOrDefault();



                courselist = GetCourseListinfo();
                //var subjectlist = (from subjd in entity.SubjectDetails
                //                   where subjd.OrganizationID == orgid
                //                   && subjd.SubjectId == roleinforole.Id
                //                   select subjd).ToList();
                //foreach (Courselist item in courselist)
                //{
                //    var checkselectedornot = subjectlist.Where(x => x.CourseId.Contains(item.Id)).FirstOrDefault();
                //    if (checkselectedornot != null)
                //    {

                //        item.Selected = true;
                //    }


                //}



            }
            return Json(new { subjectdata = roleinforole, coursedata = courselist }, JsonRequestBehavior.AllowGet);
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
    public class SubjectList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string CourseName { get; set; }
        //public List<SubjectDetail> SubjectDetailsLIst { get; set; }
        public List<Courselist> CourseList { get; set; }
    }
    public class Courselist
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }


}