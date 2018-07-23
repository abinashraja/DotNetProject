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
    public class SectionController : BaseController
    {
        #region User Index
        [Authorize]
        [CustomFilter(PageName = "Section")]
        public ActionResult Index()
        {

            return View("Section");
        }
        #endregion

        #region Get Section List
        [Authorize]
        [CustomFilter(PageName = "Section")]
        public JsonResult GetSectionList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<SectionList> org = new List<SectionList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Sections
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new SectionList
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

        #region Create Section 
        [Authorize]
        [CustomFilter(PageName = "Section")]
        public JsonResult SaveSection(EPortal.Models.Section Sectioninfo)
        {


            string errormsg = "";
            int result = 0;

            //if ((role.Code != "" || role.Code != null) && (role.Name != "" || role.Name != null))
            {
                //string orgid = Session["OrgId"].ToString();

                string orgid = User.OrgId;

                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (Sectioninfo.Operation == "Create")
                    {

                        var checkrolecode = (from r in entity.Subjects
                                             where r.OrganizationID == orgid
                                             && r.Code == Sectioninfo.Code
                                             select r).FirstOrDefault();
                        if (checkrolecode == null)
                        {

                            Sectioninfo.Id = Guid.NewGuid().ToString();
                            Sectioninfo.OrganizationID = orgid;
                            Sectioninfo.RowState = true;
                            Sectioninfo.CreateDateTime = System.DateTime.Now;

                            entity.Entry(Sectioninfo).State = System.Data.Entity.EntityState.Added;
                            entity.Sections.Add(Sectioninfo);
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
                            errormsg = "Section already exist with same Code.";
                        }

                    }
                    else
                    {
                        EPortal.Models.Section roledata = (from o in entity.Sections
                                                              where o.OrganizationID == orgid
                                                              && o.Id == Sectioninfo.Id
                                                              select o
                               ).FirstOrDefault();

                        roledata.Code = Sectioninfo.Code;
                        roledata.Name = Sectioninfo.Name;
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

        #region Delete Section
        [Authorize]
        [CustomFilter(PageName = "Section")]
        public JsonResult DeleteSection(EPortal.Models.Section Section)
        {

            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            string errormsg = string.Empty;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {
                //var checkref = (from r in entity.Sectiones
                //                where r.OrganizationID == orgid
                //                && r.Id == Section.Id
                //                select r).FirstOrDefault();
                //if (checkref != null)
                //{
                //    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                //}
                //else
                //{

                    entity.Entry(Section).State = System.Data.Entity.EntityState.Deleted;
                    result = entity.SaveChanges();
                //}
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit Section
        [Authorize]
        [CustomFilter(PageName = "Section")]
        public JsonResult GetSectionInfo(EPortal.Models.Section Section)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;

            RoleList roleinforole = new RoleList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                roleinforole = (from o in entity.Sections
                                where o.Id == Section.Id
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
    public class SectionList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }

}