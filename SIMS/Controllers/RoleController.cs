using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Models;
using EPortal.Utility;
using EPortal.App_Start;

namespace EPortal.Controllers
{
    public class RoleController : BaseController
    {

        #region User Index
        [Authorize]
        [CustomFilter(PageName = "Role")]
        public ActionResult Index()
        {

            return View("Role");
        }
        #endregion

        #region Get User List
        [Authorize]
        [CustomFilter(PageName = "Role")]
        public JsonResult GetRoleList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<RoleList> rolelist = new List<RoleList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                rolelist = (from o in entity.RoleMasters
                            where o.OrganizationID == orgid
                            && o.OrganizationID != "1"
                            && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       ))
                            select new RoleList
                            {
                                Id = o.Id,
                                Code = o.Code,
                                Name = o.Name,
                                Operation = "Create",
                                DeleteConformation = false,
                                CreatedDatetime = o.CreateDateTime
                            }).OrderByDescending(x => x.CreatedDatetime).ToList();
            }
            return Json(rolelist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create User 
        [Authorize]
        [CustomFilter(PageName = "Role")]
        public JsonResult SaveRole(EPortal.Models.RoleMaster role)
        {


            string errormsg = "";
            int result = 0;

            //if ((role.Code != "" || role.Code != null) && (role.Name != "" || role.Name != null))
            {
                //string orgid = Session["OrgId"].ToString();

                string orgid = User.OrgId;

                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (role.Operation == "Create")
                    {

                        var checkrolecode = (from r in entity.RoleMasters
                                             where r.OrganizationID == orgid
                                             && r.Code == role.Code
                                             select r).FirstOrDefault();
                        if (checkrolecode == null)
                        {

                            role.Id = Guid.NewGuid().ToString();
                            role.OrganizationID = orgid;
                            role.RowState = true;
                            role.CreateDateTime = System.DateTime.Now;

                            entity.Entry(role).State = System.Data.Entity.EntityState.Added;
                            entity.RoleMasters.Add(role);
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
                            errormsg = "Role already exist with same Code.";
                        }

                    }
                    else
                    {
                        EPortal.Models.RoleMaster roledata = (from o in entity.RoleMasters
                                                              where o.OrganizationID == orgid
                                                              && o.Id == role.Id
                                                              select o
                               ).FirstOrDefault();

                        roledata.Code = role.Code;
                        roledata.Name = role.Name;
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
            //else
            //{
            //    if (role.Code != "" || role.Code != null)
            //    {
            //        errormsg = "Please enter Code.";
            //    }
            //    if (role.Name != "" || role.Name != null)
            //    {
            //        errormsg = "Please enter Name.";
            //    }
            //}

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Delete User
        [Authorize]
        [CustomFilter(PageName = "Role")]
        public JsonResult DeleteRole(EPortal.Models.RoleMaster rolemaster)
        {

            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            string errormsg = string.Empty;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {
                var checkref = (from r in entity.UserRoles
                                where r.OrganizationID == orgid
                                && r.RoleId == rolemaster.Id
                                select r).FirstOrDefault();
                if (checkref != null)
                {
                    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                }
                else
                {

                    entity.Entry(rolemaster).State = System.Data.Entity.EntityState.Deleted;
                    result = entity.SaveChanges();
                }
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit User
        [Authorize]
        [CustomFilter(PageName = "Role")]
        public JsonResult GetRoleInfo(EPortal.Models.RoleMaster roleinfo)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;

            RoleList roleinforole = new RoleList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                roleinforole = (from o in entity.RoleMasters
                                where o.Id == roleinfo.Id
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

        #region Get MOdule Name Info
        [Authorize]
        [CustomFilter(PageName = "Role")]
        public JsonResult GetprivilegeInfo(EPortal.Models.RoleMaster role)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;
            List<ModuleNameList> modulenamelist = new List<ModuleNameList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                var modulepage = (from orgp in entity.OrganizationPages
                                  join p in entity.Pages on orgp.PageId equals p.Id
                                  join mop in entity.ModulePages on p.Id equals mop.PageId
                                  join m in entity.Modules on mop.ModuleId equals m.Id
                                  where orgp.OrganizationID == orgid
                                  select new
                                  {
                                      ModuleidId = m.Id,
                                      ModuleCode = m.Code,
                                      ModuleName = m.Name,
                                      moduleseq = m.SequenceNo,
                                      PageId = p.Id,
                                      PageCode = p.Code,
                                      pageName = p.Name,
                                      pageseq = p.SequenceNo
                                  }).ToList();

                //var modulepage = (from mp in entity.ModulePages
                //                  join p in entity.OrganizationPages on mp.PageId equals p.PageId
                //                  join m in entity.Modules on mp.ModuleId equals m.Id
                //                  where m.SequenceNo != null
                //                  select new
                //                  {
                //                      ModuleidId = m.Id,
                //                      ModuleCode = m.Code,
                //                      ModuleName = m.Name,
                //                      moduleseq = m.SequenceNo,
                //                      PageId = p.Id,
                //                      PageCode = p.Code,
                //                      pageName = p.Name,
                //                      pageseq = p.SequenceNo
                //                  }).ToList();

                if (modulepage.Count() > 0)
                {

                    modulenamelist = (from m in modulepage
                                      group m by new { sequ = m.moduleseq, moduleid = m.ModuleidId, modulecode = m.ModuleCode, modulename = m.ModuleName }
                                     into j1
                                      select new ModuleNameList
                                      {
                                          Id = j1.Key.moduleid,
                                          Code = j1.Key.modulecode,
                                          Name = j1.Key.modulename,
                                          SequenceNo = j1.Key.sequ,
                                          modulepagelist = (from pa in modulepage
                                                            join prev in entity.Previleages on
                                                            new
                                                            {
                                                                orgid = orgid,
                                                                pageid = pa.PageId,
                                                                roleid = role.Id
                                                            }
                                                            equals new
                                                            {
                                                                orgid = prev.OrganizationID,
                                                                pageid = prev.PageId,
                                                                roleid = prev.RoleId
                                                            } into j2
                                                            from prev in j2.DefaultIfEmpty()
                                                            where pa.ModuleidId == j1.Key.moduleid
                                                            select new ModulePagelist
                                                            {
                                                                Id = pa.PageId,
                                                                Code = pa.PageCode,
                                                                Name = pa.pageName,
                                                                SequenceNo = pa.pageseq,
                                                                Create = prev == null ? false : prev.PCreate,
                                                                Update = prev == null ? false : prev.PUpdate,
                                                                Delete = prev == null ? false : prev.PDelete,
                                                                View = prev == null ? false : prev.PView
                                                            }).OrderBy(x => x.SequenceNo).ToList()
                                      }).OrderBy(x => x.SequenceNo).ToList();
                }
            }
            return Json(modulenamelist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save Privileges
        [CustomFilter(PageName = "Role")]
        public JsonResult SavePrivileges(List<ModuleNameList> moduleprevlist, string roleid)
        {
            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            EPortal.Models.Previleage prev = null;
            using (EPortalEntities entity = new EPortalEntities())
            {
                foreach (ModuleNameList item in moduleprevlist)
                {
                    foreach (ModulePagelist modulepagelistitem in item.modulepagelist)
                    {

                        EPortal.Models.Previleage forupdaterecord = null;
                        forupdaterecord = (from p in entity.Previleages
                                           where p.RoleId == roleid
                                           && p.PageId == modulepagelistitem.Id
                                           select p).FirstOrDefault();
                        if (forupdaterecord == null)
                        {

                            prev = new Previleage();
                            prev.Id = Guid.NewGuid().ToString();
                            prev.RoleId = roleid;
                            prev.PageId = modulepagelistitem.Id;
                            prev.OrganizationID = orgid;
                            prev.OperationId = string.Empty;
                            prev.CreateDateTime = System.DateTime.Now;
                            prev.RowState = true;
                            prev.PCreate = modulepagelistitem.Create;
                            prev.PUpdate = modulepagelistitem.Update;
                            prev.PDelete = modulepagelistitem.Delete;
                            prev.PView = modulepagelistitem.View;

                            entity.Entry(prev).State = System.Data.Entity.EntityState.Added;
                            entity.Previleages.Add(prev);
                        }
                        else
                        {
                            forupdaterecord.PCreate = modulepagelistitem.Create;
                            forupdaterecord.PUpdate = modulepagelistitem.Update;
                            forupdaterecord.PDelete = modulepagelistitem.Delete;
                            forupdaterecord.PView = modulepagelistitem.View;
                            entity.Entry(forupdaterecord).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
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
    public class RoleList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public DateTime CreatedDatetime { get; set; }
    }
    public class ModuleNameList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? SequenceNo { get; set; }
        public List<ModulePagelist> modulepagelist { get; set; }
    }
    public class ModulePagelist
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? SequenceNo { get; set; }
        public bool? Create { get; set; }
        public bool? Update { get; set; }
        public bool? Delete { get; set; }
        public bool? View { get; set; }

    }

}