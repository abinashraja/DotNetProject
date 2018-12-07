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
    public class OrganizationController : BaseController
    {

        #region Organization Index
        [Authorize]
        [CustomFilter(PageName = "Organization")]
        public ActionResult Index()
        {

            return View("Organization");
        }
        #endregion

        #region Get Organization List
        [Authorize]
        public JsonResult GetOrganizationList(string searchtext)
        {
            List<OrgList> org = new List<OrgList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Organizations
                       where o.Id != "1"
                       && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new OrgList
                       {
                           Id = o.Id,
                           Code = o.Code,
                           Name = o.Name,
                           Email = o.Email,
                           ContactNo = o.ContactNo,
                           Operation = "Create",
                           DeleteConformation = false,


                       }).ToList();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create Organization 
        [Authorize]
        public JsonResult SaveOrganization(EPortal.Models.Organization orgdata)
        {
            string errormsg = "";
            int result = 0;
            if ((orgdata.Code != "" || orgdata.Code != null) && (orgdata.Name != "" || orgdata.Name != null))
            {

                // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");
                EPortal.Models.RoleMaster roleadmin = new RoleMaster();
                EPortal.Models.RoleMaster roleapplicant = new RoleMaster();
                EPortal.Models.UserInfo Userforadmin = new UserInfo();
                EPortal.Models.UserRole Userroleforadmin = new UserRole();
                EPortal.Models.Previleage Previleageforadmin = null;
                List<EPortal.Models.Previleage> Previleageforadminlist = new List<Previleage>();
                List<EPortal.Models.Page> getPage = new List<Page>();




                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (orgdata.Operation == "Create")
                    {
                        orgdata.Id = Guid.NewGuid().ToString();

                        #region Create AdminUser for This Organization
                        roleadmin.Id = Guid.NewGuid().ToString();
                        roleadmin.Code = "admin";
                        roleadmin.Name = "admin";
                        roleadmin.OrganizationID = orgdata.Id;
                        roleadmin.RowState = true;
                        roleadmin.CreateDateTime = System.DateTime.Now;




                        #region Create ROle for This User
                        roleapplicant.Id = Guid.NewGuid().ToString();
                        roleapplicant.Code = "Applicant";
                        roleapplicant.Name = "Applicant";
                        roleapplicant.OrganizationID = orgdata.Id;
                        roleapplicant.RowState = true;
                        roleapplicant.CreateDateTime = System.DateTime.Now;




                        #endregion

                        Userforadmin.Id = Guid.NewGuid().ToString();
                        Userforadmin.Code = "Admin";
                        Userforadmin.Name = "Admin";
                        Userforadmin.LogInId = "admin";
                        Userforadmin.UserPassword = "admin";
                        Userforadmin.UserType = "50";
                        Userforadmin.OrganizationID = orgdata.Id;
                        Userforadmin.RowState = true;
                        Userforadmin.CreateDateTime = System.DateTime.Now;

                        Userroleforadmin.Id = Guid.NewGuid().ToString();
                        Userroleforadmin.UserId = Userforadmin.Id;
                        Userroleforadmin.RoleId = roleadmin.Id;
                        Userroleforadmin.OrganizationID = orgdata.Id;
                        Userroleforadmin.RowState = true;
                        Userroleforadmin.CreateDateTime = System.DateTime.Now;




                        #endregion

                        #region Get All Page 
                        getPage = (from p in entity.Pages
                                   where p.Code != "Organization"
                                   && p.ForAdmin == true
                                   select p).ToList();
                        if (getPage.Count() > 0)
                        {
                            //foreach (Page item in getPage)
                            //{
                            //    Previleageforadmin = new Previleage();
                            //    Previleageforadmin.Id = Guid.NewGuid().ToString();
                            //    Previleageforadmin.RoleId = roleadmin.Id;
                            //    Previleageforadmin.PageId = item.Id;
                            //    Previleageforadmin.OperationId = string.Empty;
                            //    Previleageforadmin.PCreate = true;
                            //    Previleageforadmin.PUpdate = true;
                            //    Previleageforadmin.PDelete = true;
                            //    Previleageforadmin.PView = true;
                            //    Previleageforadmin.OrganizationID = orgdata.Id;
                            //    Previleageforadmin.RowState = true;
                            //    Previleageforadmin.CreateDateTime = System.DateTime.Now;
                            //    Previleageforadminlist.Add(Previleageforadmin);
                            //}
                        }
                        #endregion

                        entity.Entry(orgdata).State = System.Data.Entity.EntityState.Added;
                        entity.Entry(roleadmin).State = System.Data.Entity.EntityState.Added;
                        entity.Entry(roleapplicant).State = System.Data.Entity.EntityState.Added;
                        entity.Entry(Userforadmin).State = System.Data.Entity.EntityState.Added;
                        entity.Entry(Userroleforadmin).State = System.Data.Entity.EntityState.Added;

                        entity.Organizations.Add(orgdata);
                        entity.RoleMasters.Add(roleadmin);
                        entity.RoleMasters.Add(roleapplicant);
                        entity.UserInfoes.Add(Userforadmin);
                        entity.UserRoles.Add(Userroleforadmin);
                        //foreach (Previleage item in Previleageforadminlist)
                        //{
                        //    entity.Entry(item).State = System.Data.Entity.EntityState.Added;
                        //    entity.Previleages.Add(item);
                        //}
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
                        orgdata.Code = orgdata.Code;
                        orgdata.Name = orgdata.Name;
                        orgdata.ContactNo = orgdata.ContactNo;
                        orgdata.ESTDate = orgdata.ESTDate;
                        orgdata.PhoneNo = orgdata.PhoneNo;
                        orgdata.Email = orgdata.Email;
                        orgdata.Address = orgdata.Address;
                        orgdata.Country = orgdata.Country;
                        orgdata.OrgState = orgdata.OrgState;
                        orgdata.Location = orgdata.Location;
                        orgdata.Pin = orgdata.Pin;

                        entity.Entry(orgdata).State = System.Data.Entity.EntityState.Modified;
                        result = entity.SaveChanges();

                    }

                }
            }
            else
            {
                if (orgdata.Code != "" || orgdata.Code != null)
                {
                    errormsg = "Please enter Code.";
                }
                if (orgdata.Name != "" || orgdata.Name != null)
                {
                    errormsg = "Please enter Name.";
                }

            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Delete Organization
        [Authorize]
        public JsonResult DeleteOrganization(EPortal.Models.Organization orgdata)
        {

            int result = 0;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {

                entity.Entry(orgdata).State = System.Data.Entity.EntityState.Deleted;
                result = entity.SaveChanges();
            }

            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit Organization
        [Authorize]
        public JsonResult GetOrganizationInfo(EPortal.Models.Organization orginfo)
        {

            OrgList org = new OrgList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Organizations
                       where o.Id == orginfo.Id
                       select new OrgList
                       {
                           Id = o.Id,
                           Code = o.Code,
                           Name = o.Name,
                           ContactNo = o.ContactNo,
                           Email = o.Email,
                           ESTDate = o.ESTDate,
                           PhoneNo = o.PhoneNo,
                           Address = o.Address,
                           Location = o.Location,
                           OrgState = o.OrgState,
                           Country = o.Country,
                           Pin = o.Pin,
                           Operation = "Edit",
                           ImageUrl = "/Home/GetOrgLogo?orgid=" + o.Id
                       }).FirstOrDefault();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region upload FIle 
        [HttpPost]
        public JsonResult fileUpload(string code)
        {

            string errormsg = string.Empty;
            var data = Request.Files[0];
            bool fileerror = false;
            if (!Request.Files[0].ContentType.Contains("image"))
            {
                errormsg = "Please select Image file only.";
                fileerror = true;
            }
            if (fileerror == false)
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(data.InputStream);
                img.Save(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + code + ".jpg");

            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Get MOdule Name Info
        [Authorize]
        [CustomFilter(PageName = "Organization")]
        public JsonResult GetprivilegeInfo(EPortal.Models.Organization org)
        {
            string orgid = org.Id;
            List<OrganizationPageList> organpagelist = new List<OrganizationPageList>();
            List<ModuleNameList> listmodule = new List<ModuleNameList>();
            using (EPortalEntities entity = new EPortalEntities())
            {


                organpagelist = (from m in entity.Modules
                                 join mp in entity.ModulePages on m.Id equals mp.ModuleId
                                 join p in entity.Pages on mp.PageId equals p.Id
                                 join op in entity.OrganizationPages on new
                                 {
                                     Orgid = orgid,
                                     pageid = p.Id
                                 }
                                 equals new
                                 {
                                     Orgid = op.OrganizationID,
                                     pageid = op.PageId
                                 } into j1
                                 from op in j1.DefaultIfEmpty()
                                 where m.SequenceNo != null
                                 select new OrganizationPageList
                                 {
                                     ModuleidId = m.Id,
                                     ModuleCode = m.Code,
                                     ModuleName = m.Name,
                                     moduleseq = m.SequenceNo,
                                     PageId = p.Id,
                                     PageCode = p.Code,
                                     pageName = p.Name,
                                     pageseq = p.SequenceNo,
                                     PageSelected = op == null ? false : true
                                 }).ToList();

                if (organpagelist.Count() > 0)
                {
                    listmodule = (from m in organpagelist
                                  group m by new { sequ = m.moduleseq, moduleid = m.ModuleidId, modulecode = m.ModuleCode, modulename = m.ModuleName }
                                     into j1
                                  select new ModuleNameList
                                  {

                                      Id = j1.Key.moduleid,
                                      Code = j1.Key.modulecode,
                                      Name = j1.Key.modulename,
                                      SequenceNo = j1.Key.sequ,
                                      modulepagelist = (from pa in organpagelist
                                                        where pa.ModuleidId == j1.Key.moduleid
                                                        select new ModulePagelist
                                                        {
                                                            Id = pa.PageId,
                                                            Code = pa.PageCode,
                                                            Name = pa.pageName,
                                                            SequenceNo = pa.pageseq,
                                                            Create = pa.PageSelected
                                                        }).OrderBy(x => x.SequenceNo).ToList()

                                  }).OrderBy(x => x.SequenceNo).ToList();
                }

            }
            return Json(listmodule, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Organization Page Save
        public JsonResult OrganizationSave(List<ModuleNameList> moduleprevlist, string orgid)
        {
            EPortal.Models.OrganizationPage orgpage = null;
            EPortal.Models.Previleage Previleageforadmin = null;
            int result = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {
                foreach (var module in moduleprevlist)
                {
                    foreach (var modulepage in module.modulepagelist)
                    {
                        var checkexistornot = (from op in entity.OrganizationPages
                                               where op.OrganizationID == orgid
                                               && op.PageId == modulepage.Id
                                               select op).FirstOrDefault();
                        string code = string.Empty;
                        if (module.Code == "OrganizationSetup" || module.Code == "UserManagement" || module.Code== "SchoolSetup" || module.Code== "Fee")
                        {
                            code = "Admin";
                        }
                        else
                        {
                            code = "Applicant";
                        }
                        var getroleid = (from r in entity.RoleMasters
                                         where r.OrganizationID == orgid
                                         && r.Code == code
                                         select r).FirstOrDefault();
                        if (checkexistornot != null)
                        {
                            if (modulepage.Create == false)
                            {
                                entity.Entry(checkexistornot).State = System.Data.Entity.EntityState.Deleted;

                                
                                if (getroleid != null)
                                {
                                    var prev = (from p in entity.Previleages
                                                where p.OrganizationID == orgid
                                                && p.PageId == modulepage.Id
                                                && p.RoleId == getroleid.Id
                                                select p).FirstOrDefault();
                                    if (prev != null)
                                    {
                                        entity.Entry(checkexistornot).State = System.Data.Entity.EntityState.Deleted;
                                    }
                                }

                            }
                            else
                            {
                                entity.Entry(checkexistornot).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        else
                        {
                            if (modulepage.Create == true)
                            {
                                orgpage = new OrganizationPage();
                                orgpage.Id = Guid.NewGuid().ToString();
                                orgpage.OrganizationID = orgid;
                                orgpage.PageId = modulepage.Id;
                                orgpage.CreateDateTime = System.DateTime.Now;
                                orgpage.RowState = true;
                                entity.Entry(orgpage).State = System.Data.Entity.EntityState.Added;

                                Previleageforadmin = new Previleage();
                                Previleageforadmin.Id = Guid.NewGuid().ToString();
                                Previleageforadmin.RoleId = getroleid.Id;
                                Previleageforadmin.PageId = modulepage.Id;
                                Previleageforadmin.OperationId = string.Empty;
                                Previleageforadmin.PCreate = true;
                                Previleageforadmin.PUpdate = true;
                                Previleageforadmin.PDelete = true;
                                Previleageforadmin.PView = true;
                                Previleageforadmin.OrganizationID = orgid;
                                Previleageforadmin.RowState = true;
                                Previleageforadmin.CreateDateTime = System.DateTime.Now;
                                entity.Entry(Previleageforadmin).State = System.Data.Entity.EntityState.Added;
                                entity.Previleages.Add(Previleageforadmin);

                            }
                        }
                    }
                }
                result = entity.SaveChanges();

            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion



    }
    public class OrgList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ContactNo { get; set; }
        public DateTime? ESTDate { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string Pin { get; set; }
        public string OrgState { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public string ImageUrl { get; set; }

    }
    public class OrganizationPageList
    {
        public string ModuleidId { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public int? moduleseq { get; set; }
        public string PageId { get; set; }
        public string PageCode { get; set; }
        public string pageName { get; set; }
        public int? pageseq { get; set; }
        public bool PageSelected { get; set; }
    }

}