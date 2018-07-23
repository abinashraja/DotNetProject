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
    public class UserController : BaseController
    {
        HomeController homecontroller = new HomeController();
        #region User Index
        [Authorize]
        [CustomFilter(PageName = "User")]
        public ActionResult Index()
        {

            return View("User");
        }
        #endregion

        #region Get User List
        [Authorize]
        [CustomFilter(PageName = "Role")]
        public JsonResult GetUserList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<UserList> org = new List<UserList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.UserInfoes
                       where o.OrganizationID == orgid
                       && o.LogInId != "admin"
                       && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       || o.LogInId.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new UserList
                       {
                           Id = o.Id,
                           Code = o.Code,
                           Name = o.Name,
                           LogInId = o.LogInId,
                           Operation = "Create",
                           Email = o.Email,
                           DeleteConformation = false,
                           CreatedDateTime = o.CreateDateTime
                       }).OrderByDescending(x => x.CreatedDateTime).ToList();
            }
            string dateformat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            return Json(new { org = org, dateformat = dateformat }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region validation
        public bool StringEmptyValidate(string field)
        {
            bool returnvalue = true;

            if (string.IsNullOrWhiteSpace(field))
            {
                returnvalue = false;
            }

            return returnvalue;
        }
        public bool NumberValidate(string field)
        {
            bool returnvalue = true;
            int i;
            if (!(int.TryParse(field, out i)))
            {
                returnvalue = false;
            }

            return returnvalue;
        }
        public bool EmailValidate(string field)
        {
            bool returnvalue = true;
            bool isEmail = Regex.IsMatch(field, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            if (!isEmail)
                returnvalue = false;
            return returnvalue;

        }

        #endregion

        #region Create User 
        [Authorize]
        public JsonResult SaveUser(EPortal.Models.UserInfo UserInfo)
        {
            string errormsg = "";
            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;



            #region Operation save

            EPortal.Models.Previleage Previleageforadmin = null;
            EPortal.Models.UserRole Userroleforadmin = new UserRole();
            List<EPortal.Models.Previleage> Previleageforadminlist = new List<Previleage>();
            List<EPortal.Models.Page> getPage = new List<Page>();




            using (EPortalEntities entity = new EPortalEntities())
            {
                if (UserInfo.Operation == "Create")
                {

                    var checkforloginid = (from u in entity.UserInfoes
                                           where u.OrganizationID == orgid
                                           && (u.LogInId == UserInfo.LogInId || u.Code == UserInfo.Code)
                                           select u).FirstOrDefault();
                    if (checkforloginid == null)
                    {
                        UserInfo.Id = Guid.NewGuid().ToString();
                        UserInfo.OrganizationID = orgid;
                        UserInfo.RowState = true;
                        UserInfo.CreateDateTime = System.DateTime.Now;
                        // string password = UserInfo.DateOfBirth.Value.Day + "-" + UserInfo.DateOfBirth.Value.Month + "-" + UserInfo.DateOfBirth.Value.Year;
                        UserInfo.UserPassword = UserInfo.LogInId;
                        UserInfo.IsApplicant = UserInfo.IsApplicant;
                        UserInfo.MobileNo = null;
                        if (UserInfo.Email == null || UserInfo.Email == "")
                        {

                            UserInfo.Email = null;
                        }
                        else
                        {
                            UserInfo.Email = UserInfo.Email;
                        }

                        UserInfo.PhotoPath = null;
                        UserInfo.NoOfLogin = null;
                        UserInfo.UserType = "40";
                        entity.Entry(UserInfo).State = System.Data.Entity.EntityState.Added;
                        entity.UserInfoes.Add(UserInfo);

                        if (UserInfo.IsApplicant == true)
                        {
                            try
                            {

                                #region For Applicant
                                var roleid = (from r in entity.RoleMasters
                                              where r.OrganizationID == orgid
                                              && r.Code == "Applicant"
                                              select r).FirstOrDefault();

                                Userroleforadmin = new UserRole();
                                Userroleforadmin.Id = Guid.NewGuid().ToString();
                                Userroleforadmin.UserId = UserInfo.Id;
                                Userroleforadmin.RoleId = roleid.Id;
                                Userroleforadmin.OrganizationID = orgid;
                                Userroleforadmin.RowState = true;
                                Userroleforadmin.CreateDateTime = System.DateTime.Now;
                                entity.UserRoles.Add(Userroleforadmin);


                                var checkroleexist = (from r in entity.Previleages
                                                      where r.OrganizationID == orgid
                                                      && r.RoleId == roleid.Id
                                                      select r).ToList();

                                if (checkroleexist.Count() == 0)
                                {

                                    #region Get All Page 

                                    var getorgpage = (from p in entity.OrganizationPages
                                                      where p.OrganizationID == orgid
                                                      select p).ToList();

                                    getPage = (from p in entity.Pages
                                               where p.Code != "Organization"
                                               && p.ForAdmin == false
                                               select p).ToList();
                                    if (getPage.Count() > 0)
                                    {
                                        foreach (var item in getorgpage)
                                        {
                                            Previleageforadmin = new Previleage();
                                            Previleageforadmin.Id = Guid.NewGuid().ToString();
                                            Previleageforadmin.RoleId = roleid.Id;
                                            Previleageforadmin.PageId = item.PageId;
                                            Previleageforadmin.OperationId = string.Empty;
                                            Previleageforadmin.PCreate = true;
                                            Previleageforadmin.PUpdate = true;
                                            Previleageforadmin.PDelete = true;
                                            Previleageforadmin.PView = true;
                                            Previleageforadmin.OrganizationID = orgid;
                                            Previleageforadmin.RowState = true;
                                            Previleageforadmin.CreateDateTime = System.DateTime.Now;
                                            Previleageforadminlist.Add(Previleageforadmin);
                                        }
                                    }
                                    #endregion

                                    foreach (Previleage item in Previleageforadminlist)
                                    {
                                        entity.Entry(item).State = System.Data.Entity.EntityState.Added;
                                        entity.Previleages.Add(item);
                                    }
                                }
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                errormsg = ex.Message;
                            }

                        }
                    }
                    else
                    {

                        errormsg = "User already exist with same details.";
                    }
                    try
                    {
                        if (errormsg == "")
                        {
                            result = entity.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        errormsg = ex.Message;
                    }

                }
                else
                {
                    EPortal.Models.UserInfo usedata = (from o in entity.UserInfoes
                                                       where o.OrganizationID == orgid
                                                       && o.Id == UserInfo.Id
                                                       select o
                           ).FirstOrDefault();
                    usedata.Code = UserInfo.Code;
                    usedata.Name = UserInfo.Name;
                    usedata.LogInId = UserInfo.LogInId;
                    usedata.IsApplicant = UserInfo.IsApplicant;
                    usedata.MobileNo = UserInfo.MobileNo;
                    usedata.Email = UserInfo.Email;
                    entity.Entry(usedata).State = System.Data.Entity.EntityState.Modified;
                    try
                    {

                        result = entity.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        errormsg = ex.Message;
                    }

                }

            }
            #endregion


            #region Send Mail
            if (UserInfo.Operation == "Create" && (UserInfo.Email != null || UserInfo.Email != ""))
            {
                bool sendmailper = false;
                using (EPortalEntities entity = new EPortalEntities())
                {
                    var checkformail = (from mc in entity.EMailConfigurations
                                        where mc.OrganizationId == orgid
                                        select mc).FirstOrDefault();
                    if (checkformail != null)
                    {
                        if (checkformail.UserCreationMail == true)
                        {
                            sendmailper = true;
                        }
                    }
                }
                if (sendmailper == true)
                {
                    string body = "please find your UserName and Password below for E-Assessment.in ,UserName:" + UserInfo.LogInId + " and Password :" + UserInfo.UserPassword + "";
                    string heading = "Applicant " + UserInfo.Name + " created";
                    bool sendmail = homecontroller.SendMail(UserInfo.Email, heading, body, null);
                }
            }
            #endregion


            return Json(new { result = result > 0 ? true : false, errormsg = errormsg, id = UserInfo.Id }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Delete User
        [Authorize]
        public JsonResult DeleteUser(EPortal.Models.UserInfo Userinfo)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;
            string errormsg = string.Empty;
            int result = 0;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {

                var checkreferance = (from r in entity.ApplicantTests
                                      where r.OrganizationID == orgid
                                      && r.ApplicantId == Userinfo.Id
                                      select r).FirstOrDefault();
                if (checkreferance != null)
                {
                    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";

                }
                else
                {
                    entity.Entry(Userinfo).State = System.Data.Entity.EntityState.Deleted;
                    result = entity.SaveChanges();
                }


            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit User
        [Authorize]
        public JsonResult GetUserInfo(EPortal.Models.UserInfo Userinfo)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;


            UserList UserinfoUser = new UserList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                UserinfoUser = (from o in entity.UserInfoes
                                where o.Id == Userinfo.Id
                                && o.OrganizationID == orgid
                                select new UserList
                                {
                                    Id = o.Id,
                                    Code = o.Code,
                                    Name = o.Name,
                                    LogInId = o.LogInId,
                                    IsApplicant = o.IsApplicant.Value,
                                    MobileNo = o.MobileNo,
                                    Email = o.Email,
                                    Operation = "Edit",
                                    DateOfBirth = o.DateOfBirth.Value,
                                    ImageUrl = "/Home/GetFile?fileid=" + o.Id
                                }).FirstOrDefault();
            }
            return Json(UserinfoUser, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region upload FIle 
        [HttpPost]
        public JsonResult fileUpload(HttpPostedFileBase filedata)
        {

            EPortal.Models.Previleage Previleageforadmin = null;
            EPortal.Models.UserRole Userroleforadmin = new UserRole();
            List<EPortal.Models.Previleage> Previleageforadminlist = new List<Previleage>();
            List<EPortal.Models.Page> getPage = new List<Page>();


            string errormsg = string.Empty;
            int resultforsave = 0;
            var data = Request.Files[0];
            bool fileerror = false;
            if (!Request.Files[0].ContentType.Contains("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
            {
                errormsg = "Please select Excel file only.";
                fileerror = true;
            }
            if (fileerror == false)
            {
                #region Excel FIle upload
                //string orgid = Session["OrgId"].ToString();

                string orgid = User.OrgId;

                string path = string.Empty;
                //FileStream stream = new FileStream(data.FileName, FileMode.Open, FileAccess.Read);
                Excel.IExcelDataReader excelReader;
                excelReader = Excel.ExcelReaderFactory.CreateOpenXmlReader(data.InputStream);
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet result = excelReader.AsDataSet();
                List<EPortal.Models.UserInfo> useinfolist = new List<UserInfo>();
                EPortal.Models.UserInfo useinfo = null;

                using (EPortalEntities entity = new EPortalEntities())
                {

                    if (result.Tables.Count > 0)
                    {
                        foreach (var item in result.Tables[0].Rows)
                        {
                            string loginid = ((System.Data.DataRow)item).ItemArray[2].ToString();
                            string code = ((System.Data.DataRow)item).ItemArray[0].ToString();

                            var checkexist = (from u in entity.UserInfoes
                                              where u.OrganizationID == orgid
                                              && (u.LogInId == loginid || u.Code == code)
                                              select u).ToList();
                            if (checkexist.Count() > 0)
                            {
                                errormsg = "One or more User already exist with same Details.";
                                break;
                            }
                            if (!EmailValidate(((System.Data.DataRow)item).ItemArray[4].ToString()))
                            {
                                errormsg = "One or more User have invalid emailid.";
                                break;
                            }
                            if (errormsg == "")
                            {
                                useinfo = new UserInfo();
                                useinfo.Id = Guid.NewGuid().ToString();
                                useinfo.Code = ((System.Data.DataRow)item).ItemArray[0].ToString();
                                useinfo.Name = ((System.Data.DataRow)item).ItemArray[1].ToString();
                                useinfo.LogInId = loginid;
                                DateTime password = Convert.ToDateTime(((System.Data.DataRow)item).ItemArray[3].ToString());
                                useinfo.UserPassword = loginid;
                                useinfo.DateOfBirth = Convert.ToDateTime(((System.Data.DataRow)item).ItemArray[3].ToString());
                                useinfo.OrganizationID = orgid;
                                useinfo.RowState = true;
                                useinfo.CreateDateTime = System.DateTime.Now;
                                useinfo.Email = ((System.Data.DataRow)item).ItemArray[4].ToString();
                                useinfo.IsApplicant = Convert.ToBoolean(Convert.ToInt16(((System.Data.DataRow)item).ItemArray[5].ToString()));
                                useinfo.MobileNo = null;
                                useinfo.PhotoPath = null;
                                useinfo.NoOfLogin = null;




                                if (Convert.ToBoolean(Convert.ToInt16(((System.Data.DataRow)item).ItemArray[5].ToString())))
                                {
                                    useinfo.UserType = "40";
                                    #region For Applicant
                                    var roleid = (from r in entity.RoleMasters
                                                  where r.OrganizationID == orgid
                                                  && r.Code == "Applicant"
                                                  select r).FirstOrDefault();

                                    Userroleforadmin = new UserRole();
                                    Userroleforadmin.Id = Guid.NewGuid().ToString();
                                    Userroleforadmin.UserId = useinfo.Id;
                                    Userroleforadmin.RoleId = roleid.Id;
                                    Userroleforadmin.OrganizationID = orgid;
                                    Userroleforadmin.RowState = true;
                                    Userroleforadmin.CreateDateTime = System.DateTime.Now;
                                    entity.UserRoles.Add(Userroleforadmin);


                                    var checkroleexist = (from r in entity.Previleages
                                                          where r.OrganizationID == orgid
                                                          && r.RoleId == roleid.Id
                                                          select r).ToList();

                                    if (checkroleexist.Count() == 0)
                                    {

                                        #region Get All Page 
                                        getPage = (from p in entity.Pages
                                                   where p.Code != "Organization"
                                                   && p.ForAdmin == false
                                                   select p).ToList();
                                        if (getPage.Count() > 0)
                                        {
                                            foreach (Page itemp in getPage)
                                            {
                                                Previleageforadmin = new Previleage();
                                                Previleageforadmin.Id = Guid.NewGuid().ToString();
                                                Previleageforadmin.RoleId = roleid.Id;
                                                Previleageforadmin.PageId = itemp.Id;
                                                Previleageforadmin.OperationId = string.Empty;
                                                Previleageforadmin.PCreate = true;
                                                Previleageforadmin.PUpdate = true;
                                                Previleageforadmin.PDelete = true;
                                                Previleageforadmin.PView = true;
                                                Previleageforadmin.OrganizationID = orgid;
                                                Previleageforadmin.RowState = true;
                                                Previleageforadmin.CreateDateTime = System.DateTime.Now;
                                                Previleageforadminlist.Add(Previleageforadmin);
                                            }
                                        }
                                        #endregion

                                        foreach (Previleage itempre in Previleageforadminlist)
                                        {
                                            entity.Entry(itempre).State = System.Data.Entity.EntityState.Added;
                                            entity.Previleages.Add(itempre);
                                        }
                                    }
                                    #endregion
                                }
                                entity.Entry(useinfo).State = System.Data.Entity.EntityState.Added;
                                entity.UserInfoes.Add(useinfo);
                            }

                        }

                    }
                    else
                    {
                        errormsg = "Selected file is empty.No Data found.";
                    }

                    if (errormsg == string.Empty)
                    {
                        try
                        {
                            resultforsave = entity.SaveChanges();
                        }
                        catch (Exception ex)
                        {


                        }
                    }
                }
                #endregion
            }
            return Json(new { result = resultforsave > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region User Image FIle 
        [HttpPost]
        public JsonResult fileUploadImage(string userid)
        {

            string errormsg = string.Empty;
            var data = Request.Files[0];
            bool fileerror = false;
            int result = 0;
            if (!Request.Files[0].ContentType.Contains("image"))
            {
                errormsg = "Please select Image file only.";
                fileerror = true;
                result = 0;
            }
            if (fileerror == false)
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(data.InputStream);
                img.Save(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + userid + ".jpg");
                result = 1;
            }
            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    public class UserList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LogInId { get; set; }
        public string Email { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public bool IsApplicant { get; set; }
        public string MobileNo { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string ImageUrl { get; set; }
    }

}