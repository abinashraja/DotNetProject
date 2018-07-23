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
    public class ClassController : BaseController
    {
        #region User Index
        [Authorize]
        [CustomFilter(PageName = "Class")]
        public ActionResult Index()
        {

            return View("Class");
        }
        #endregion

        #region Get Class List
        [Authorize]
        [CustomFilter(PageName = "Class")]
        public JsonResult GetClassList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<ClassList> org = new List<ClassList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Classes
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new ClassList
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

        #region Create Class 
        [Authorize]
        [CustomFilter(PageName = "Class")]
        public JsonResult SaveClass(EPortal.Models.Class classinfo)
        {


            string errormsg = "";
            int result = 0;

            //if ((role.Code != "" || role.Code != null) && (role.Name != "" || role.Name != null))
            {
                //string orgid = Session["OrgId"].ToString();

                string orgid = User.OrgId;

                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (classinfo.Operation == "Create")
                    {

                        var checkrolecode = (from r in entity.RoleMasters
                                             where r.OrganizationID == orgid
                                             && r.Code == classinfo.Code
                                             select r).FirstOrDefault();
                        if (checkrolecode == null)
                        {

                            classinfo.Id = Guid.NewGuid().ToString();
                            classinfo.OrganizationID = orgid;
                            classinfo.RowState = true;
                            classinfo.CreateDateTime = System.DateTime.Now;

                            entity.Entry(classinfo).State = System.Data.Entity.EntityState.Added;
                            entity.Classes.Add(classinfo);
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
                            errormsg = "Class already exist with same Code.";
                        }

                    }
                    else
                    {
                        EPortal.Models.Class roledata = (from o in entity.Classes
                                                              where o.OrganizationID == orgid
                                                              && o.Id == classinfo.Id
                                                              select o
                               ).FirstOrDefault();

                        roledata.Code = classinfo.Code;
                        roledata.Name = classinfo.Name;
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

        #region Delete Class
        [Authorize]
        [CustomFilter(PageName = "Class")]
        public JsonResult DeleteClass(EPortal.Models.Class Class)
        {

            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            string errormsg = string.Empty;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {
                //var checkref = (from r in entity.Classes
                //                where r.OrganizationID == orgid
                //                && r.Id == Class.Id
                //                select r).FirstOrDefault();
                //if (checkref != null)
                //{
                //    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                //}
                //else
                //{

                    entity.Entry(Class).State = System.Data.Entity.EntityState.Deleted;
                    result = entity.SaveChanges();
                //}
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit Class
        [Authorize]
        [CustomFilter(PageName = "Class")]
        public JsonResult GetClassInfo(EPortal.Models.Class Class)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;

            RoleList roleinforole = new RoleList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                roleinforole = (from o in entity.Classes
                                where o.Id == Class.Id
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
    public class ClassList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }

}