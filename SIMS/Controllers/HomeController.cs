using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Models;
using System.Web.Security;
using EPortal.App_Start;
using System.IO;
using System.Net.Mail;
using System.Security.Principal;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace EPortal.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            EPortal.Models.UserInfo Usermodel = new UserInfo();
            if (User != null && User.ISApplicant != null)
            {
                Usermodel.ErrorMsg = "Please enter valid User details.";

            }
            else
            {
                Usermodel.ErrorMsg = "";
            }
            return View("Index", Usermodel);
        }

        [Authorize]
        public JsonResult GetMessageList()
        {
            List<MessageList> messagelist = new List<MessageList>();
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();
            string orgid = User.OrgId;
            string userid = User.UserId;

            using (EPortalEntities entity = new EPortalEntities())
            {
                messagelist = (from m in entity.UserFeedBacks
                               join p in entity.UserInfoes on m.ApplicantId equals p.Id
                               where m.OrganizationID == orgid
                               && m.ApplicantTo == userid
                               select new MessageList
                               {
                                   Message = m.MessageInfo,
                                   MessageDate = m.CreateDateTime,
                                   MessageDatestr = "",
                                   PhotoPath = "/Home/GetFile?fileid=" + p.Id,
                                   ApplicantName = p.Name,
                                   IsNew = false
                               }).OrderByDescending(x => x.MessageDate).ToList();
                foreach (var item in messagelist)
                {
                    item.ApplicantName = item.ApplicantName.ToUpper();
                    item.Message = item.Message.Count() > 100 ? item.Message.Substring(0, 100) : item.Message;
                    item.MessageDatestr = (System.DateTime.Now - item.MessageDate).Days.ToString();
                    if ((System.DateTime.Now - item.MessageDate).Days < 7)
                    {
                        item.IsNew = true;
                    }
                }
                messagelist = messagelist.Where(x => x.IsNew == true).OrderBy(x => x.MessageDate).ToList();



            }
            return Json(new { msgcount = messagelist.Count(), messagelist = messagelist.Take(5).ToList() }, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [CustomFilter(PageName = "UserHome")]
        public ActionResult AllMessage()
        {


            return PartialView("AllMessage");
        }
        [HttpPost]
        public ActionResult Login(EPortal.Models.UserInfo Userinfo)
        {
            CustomPrincipalSerializeModel serializeModel = new CustomPrincipalSerializeModel();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            EPortal.Models.Organization org = null;
            EPortal.Models.UserInfo Userdata = null;
            EPortal.Models.UserRole Userrole = null;
            bool sendmailper = false;
            using (EPortalEntities entity = new EPortalEntities())
            {
                try
                {
                    org = (from o in entity.Organizations
                           where o.Code == Userinfo.OrganizationName
                           select o).FirstOrDefault();
                }
                catch (Exception ex)
                {

                }
                if (org != null)
                {
                    Userdata = (from u in entity.UserInfoes
                                where u.OrganizationID == org.Id
                                && u.LogInId == Userinfo.LogInId
                                && u.UserPassword == Userinfo.UserPassword
                                select u).FirstOrDefault();
                    if (Userdata != null)
                    {
                        Userrole = (from ro in entity.UserRoles
                                    where ro.OrganizationID == org.Id
                                    && ro.UserId == Userdata.Id
                                    && ro.RowState == true
                                    select ro).FirstOrDefault();

                    }
                    var checkformail = (from mc in entity.EMailConfigurations
                                        where mc.OrganizationId == org.Id
                                        select mc).FirstOrDefault();
                    if (checkformail != null)
                    {
                        if (checkformail.AfterLoginMail == true)
                        {
                            sendmailper = true;
                        }
                    }
                }
            }
            if (Userdata != null && Userrole != null)
            {
                //FormsAuthentication.SetAuthCookie(Userdata.LogInId, true);

                serializeModel.OrgId = org.Id;
                //Session["OrgId"] = org.Id;

                // Session["OrgName"] = org.Name;
                serializeModel.OrgName = org.Name;

                //Session["UserId"] = Userdata.Id;
                serializeModel.UserId = Userdata.Id;

                //Session["UserName"] = Userdata.Name;
                serializeModel.UserName = Userdata.Name;

                //Session["ISApplicant"] = Userdata.IsApplicant;
                serializeModel.ISApplicant = Userdata.IsApplicant.ToString();

                if (Userrole != null)
                {
                    //Session["RoleId"] = Userrole.RoleId;                    
                    serializeModel.RoleId = Userrole.RoleId;
                }
                string userData = serializer.Serialize(serializeModel);
                //FormsAuthentication.SetAuthCookie(Userdata.LogInId, true);
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                     1,
                     Userinfo.LogInId,
                     DateTime.Now,
                     DateTime.Now.AddMinutes(15),
                     false,
                     userData.ToString(),
                     FormsAuthentication.FormsCookiePath);
                string encTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                Response.Cookies.Add(faCookie);
                int cookieSize = System.Text.UTF8Encoding.UTF8.GetByteCount(faCookie.Values.ToString());
                if (sendmailper == true)
                {
                    //getting client ip address
                    string ipAddress = Request.UserHostAddress.ToString();

                    //getting client browser name
                    string browserName = Request.Browser.Browser.ToString();

                    //getting client browser version
                    string browserVersion = Request.Browser.Version.ToString();



                    string body = "Hi,Just now someone login to youe acocunt with IP:" + ipAddress + ",Browser:" + browserName + browserVersion + ",If not you please contact us.";
                    string heading = "User login Details:";
                    if (Userdata.Email != null || Userdata.Email != "")
                    {
                        bool sendmail = SendMail(Userdata.Email, heading, body, null);
                    }
                }


                return RedirectToAction("UserHome");
            }
            else
            {
                Session["InvalidUser"] = true;
                return Redirect("/Home/Index");
            }


        }
        [Authorize]
        [CustomFilter(PageName = "UserHome")]

        public ActionResult UserHome()
        {




            return View("UserHome");
        }

        [Authorize]

        public ActionResult Signout()
        {
            FormsAuthentication.SignOut();

            //Session["nevigationBar"] = null;
            //Session["refressornot"] = null;
            //Session["OrgId"] = null;
            //Session["OrgName"] = null;
            //Session["UserId"] = null;
            //Session["UserName"] = null;
            //Session["RoleId"] = null;
            //Session["InvalidUser"] = null;
            return Redirect("/Home/Index");



        }
        [Authorize]
        public ActionResult Error()
        {
            return View("Error");
        }

        #region Change Password
        public JsonResult ChangePassword(changePassword password)
        {
            string orgid = User.OrgId.ToString();
            string userid = User.UserId.ToString();
            int result = 0;
            string msg = "";
            bool sendmailper = false;
            EPortal.Models.UserInfo model = null;
            using (EPortalEntities entity = new EPortalEntities())
            {
                model = (from u in entity.UserInfoes
                         where u.OrganizationID == orgid
                         && u.Id == userid
                         select u).FirstOrDefault();
                if (model != null)
                {
                    if (model.UserPassword == password.oldpassword)
                    {
                        model.UserPassword = password.newpassword;
                        entity.Entry(model).State = System.Data.Entity.EntityState.Modified;
                        result = entity.SaveChanges();

                        if (result > 0)
                        {


                            var checkformail = (from mc in entity.EMailConfigurations
                                                where mc.OrganizationId == orgid
                                                select mc).FirstOrDefault();
                            if (checkformail != null)
                            {
                                if (checkformail.AfterChangePasswordMail == true)
                                {
                                    sendmailper = true;
                                }
                            }
                        }


                    }
                    else
                    {
                        msg = "Please enter valid current password.";
                    }
                }
            }

            if (sendmailper == true && (model.Email != null || model.Email != ""))
            {
                string body = "Hi " + model.Name + ",just now your password is change.if not you ,please contact us.";
                string heading = model.Name + " your password change";
                bool sendmail = SendMail(model.Email, heading, body, null);
            }

            return Json(new { result = result > 0 ? true : false, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Org Logo
        public ActionResult GetOrgLogo(string orgid)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgcode = string.Empty;
            using (EPortalEntities entity = new EPortalEntities())
            {
                var orgdata = (from o in entity.Organizations
                               where o.Id == orgid
                               select o).FirstOrDefault();
                if (orgdata != null)
                {
                    orgcode = orgdata.Code;
                }

            }
            if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + orgcode + ".jpg"))
            {
                byte[] imageByteData = System.IO.File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + orgcode + ".jpg");
                return File(imageByteData, "image/jpg");
            }
            else
            {
                byte[] imageByteData = System.IO.File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + "DefaultOrg" + ".jpg");
                return File(imageByteData, "image/jpg");
            }

        }
        #endregion

        #region Get Image FIle
        public ActionResult GetFile(string fileid)
        {
            //string orgid = Session["OrgId"].ToString();


            if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + fileid + ".jpg"))
            {
                byte[] imageByteData = System.IO.File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + fileid + ".jpg");
                return File(imageByteData, "image/jpg");
            }
            else
            {
                byte[] imageByteData = System.IO.File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + "DefaultOrg" + ".jpg");
                return File(imageByteData, "image/jpg");
            }

        }
        #endregion


        #region Get User Image
        public ActionResult GetUserImage()
        {

            // string fileid = Session["UserId"].ToString();
            string fileid = User.UserId;
            if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + fileid + ".jpg"))
            {
                byte[] imageByteData = System.IO.File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + fileid + ".jpg");
                return File(imageByteData, "image/jpg");
            }
            else
            {
                byte[] imageByteData = System.IO.File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "\\Upload\\" + "DefaultOrg" + ".jpg");
                return File(imageByteData, "image/jpg");
            }

        }
        #endregion

        #region Get USer LIst
        public JsonResult GetUserList(string take, string searchkey)
        {
            int takedata = Convert.ToInt16(take);
            bool moredisabled = false;
            List<MessageUserList> messageluserlist = new List<MessageUserList>();
            //string orgid = Session["OrgId"].ToString();
            //string fileid = Session["UserId"].ToString();
            string orgid = User.OrgId;
            string fileid = User.UserId;

            using (EPortalEntities entity = new EPortalEntities())
            {
                messageluserlist = (from u in entity.UserInfoes
                                    where u.OrganizationID == orgid
                                    && ((searchkey == "" || searchkey == null) ? true : u.Name.ToLower().Contains(searchkey.ToLower()))
                                    && (u.Id != fileid)
                                    select new MessageUserList
                                    {
                                        Name = u.Name,
                                        UserId = u.Id
                                    }).OrderBy(x => x.Name).ToList();
            }

            if (messageluserlist.Count() < takedata)
            {
                moredisabled = true;
            }

            return Json(new { messagelistuser = messageluserlist.Take(takedata).ToList(), moredisabled = moredisabled }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Get User Message List
        public JsonResult GetUserMessageList(string userid, string megcount)
        {
            List<MessageList> usermessagelist = new List<MessageList>();
            int msgcount = Convert.ToInt16(megcount);
            //string orgid = Session["OrgId"].ToString();
            //string fileid = Session["UserId"].ToString();
            string orgid = User.OrgId;
            string fileid = User.UserId;

            using (EPortalEntities entity = new EPortalEntities())
            {
                usermessagelist = (from p in entity.UserFeedBacks
                                   join u in entity.UserInfoes on p.ApplicantId equals u.Id
                                   where p.OrganizationID == orgid
                                   && ((p.ApplicantTo == fileid && p.ApplicantId == userid) || (p.ApplicantTo == userid && p.ApplicantId == fileid))
                                   select new MessageList
                                   {
                                       Message = p.MessageInfo,
                                       MessageDate = p.CreateDateTime,
                                       ApplicantName = u.Name,
                                       MessageDatestr = "",
                                       PhotoPath = "/Home/GetFile?fileid=" + u.Id,
                                       UserId = u.Id

                                   }).OrderByDescending(x => x.MessageDate).Take(msgcount).ToList();
                if (usermessagelist.Count() > 0)
                {
                    foreach (var item in usermessagelist)
                    {
                        item.ApplicantName = item.ApplicantName.ToUpper();
                        if (fileid == item.UserId)
                        {
                            item.ApplicantName = "Me";
                        }
                        item.MessageDatestr = (System.DateTime.Now - item.MessageDate).Days.ToString();
                    }
                }

            }
            return Json(usermessagelist.OrderBy(x => x.MessageDate).ToList(), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Save User Message List
        public JsonResult SaveUserMessageList(string userid, string message)
        {
            List<MessageList> usermessagelist = new List<MessageList>();
            int resule = 0;
            //string orgid = Session["OrgId"].ToString();
            //string fileid = Session["UserId"].ToString();
            string orgid = User.OrgId;
            string fileid = User.UserId;

            EPortal.Models.UserFeedBack userfeedback = null;
            using (EPortalEntities entity = new EPortalEntities())
            {
                userfeedback = new UserFeedBack();
                userfeedback.Id = Guid.NewGuid().ToString();
                userfeedback.ApplicantId = fileid;
                userfeedback.MessageHeader = "Header-Message";
                userfeedback.MessageInfo = message;
                userfeedback.CreateDateTime = System.DateTime.Now;
                userfeedback.OrganizationID = orgid;
                userfeedback.ApplicantTo = userid;
                entity.Entry(userfeedback).State = System.Data.Entity.EntityState.Added;
                entity.UserFeedBacks.Add(userfeedback);
                resule = entity.SaveChanges();
            }
            return Json(resule > 0 ? true : false, JsonRequestBehavior.AllowGet);

        }
        #endregion


        #region Check Page Previlage

        public void SetPagePrvileage(string pagename)
        {
            string orgid = User.OrgId;
            string roleid = User.RoleId;
            using (EPortalEntities entity = new EPortalEntities())
            {
                string pageid = (from p in entity.Pages
                                 where p.Code == pagename
                                 select p.Id).FirstOrDefault();
                if (pageid != null)
                {
                    var prevdata = (from prev in entity.Previleages
                                    where prev.OrganizationID == orgid
                                    && prev.RoleId == roleid
                                    && prev.PageId == pageid
                                    select prev).FirstOrDefault();
                    if (prevdata != null)
                    {
                        EPortal.Utility.Utility.Create = prevdata.PCreate.HasValue ? prevdata.PCreate.Value : false;
                        EPortal.Utility.Utility.Update = prevdata.PUpdate.HasValue ? prevdata.PUpdate.Value : false;
                        EPortal.Utility.Utility.Delete = prevdata.PDelete.HasValue ? prevdata.PDelete.Value : false;
                        EPortal.Utility.Utility.View = prevdata.PView.HasValue ? prevdata.PView.Value : false;
                    }
                }
            }




        }
        #endregion


        #region Check Security
        public bool CheckSecurity(string pagename)
        {


            string orgid = User.OrgId;
            string roleid = User.RoleId;
            bool returnvalue = false;

            using (EPortalEntities entity = new EPortalEntities())
            {
                EPortal.Models.Page pagesobj = (from p in entity.Pages
                                                where p.Code == pagename
                                                select p).FirstOrDefault();
                if (pagesobj != null)
                {
                    EPortal.Models.Previleage prevobj = (from pre in entity.Previleages
                                                         where pre.PageId == pagesobj.Id
                                                         && pre.OrganizationID == orgid
                                                         && pre.RoleId == roleid
                                                         select pre).FirstOrDefault();
                    if (prevobj != null)
                    {
                        returnvalue = true;
                    }


                }
            }

            return returnvalue;
        }

        #endregion

        #region Send Mail 
        public bool SendMail(string to, string subject, string msg, List<byte[]> attchment = null, string frommail = null, string frommailpassword = null)
        {
            string MailId = string.Empty;
            int MailPort = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["MailPort"]);
            string MailSmtpServer = System.Configuration.ConfigurationManager.AppSettings["MailSmtpServer"];
            string MailSmtpMailPassword = System.Configuration.ConfigurationManager.AppSettings["MailSmtpMailPassword"];
            if (frommail == null || frommail == string.Empty)
            {
                MailId = System.Configuration.ConfigurationManager.AppSettings["MailId"];
            }
            else
            {
                MailId = frommail;
                MailSmtpMailPassword = frommailpassword;
            }



            SmtpClient mailServer = new SmtpClient(MailSmtpServer, MailPort);
            //mailServer.Timeout = 10000;
            //mailServer.EnableSsl = true;
            mailServer.Credentials = new System.Net.NetworkCredential(MailId, MailSmtpMailPassword);
            string from = MailId;
            MailMessage mailmsg = new MailMessage(from, to);
            mailmsg.Subject = subject;
            mailmsg.Body = GetMailTemplate(msg);
            mailmsg.IsBodyHtml = true;
            //mailServer.EnableSsl = true;
            if (attchment != null)
            {
                foreach (var item in attchment)
                {
                    Attachment att = new Attachment(new MemoryStream(item), "Test QAndA");
                    mailmsg.Attachments.Add(att);
                }
            }
            try
            {

                mailServer.Send(mailmsg);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;


        }
        #endregion

        #region Mail Template

        private string GetMailTemplate(string body)
        {
            StringBuilder bulder = new StringBuilder();

            bulder.Append("<table width='100%' bgcolor='#f6f8f1' border='0' cellpadding='0' cellspacing='0'>");
            bulder.Append("<tr>");
            bulder.Append("<td>");
            bulder.Append("<table style='width:100%; max-width:600px;align='center';cellpadding='0';cellspacing='0' border='0''>");
            bulder.Append("<tr style='background-color: #009688;'>");
            bulder.Append("<td style='height: 60px; border: solid 1px #009688;color:white;text-align: center;font-family: serif;font-size: 2pc;'>");
            bulder.Append("<span>E-assessment.in</span>");
            bulder.Append("</td>");
            bulder.Append("</tr>");
            bulder.Append("<tr>");
            bulder.Append("<td style='height: 100px;'>");
            bulder.Append("<table>");
            bulder.Append("<tr>");
            bulder.Append("<td style='min-height:50px;'>");
            bulder.Append("Dear User,");
            bulder.Append("<br><br>");
            bulder.AppendFormat("{0}", body);
            bulder.Append("</tr>");
            bulder.Append("<tr style='height:150px;'>");
            bulder.Append("<td>");
            bulder.Append("thanks");
            bulder.Append("<br>");
            bulder.Append("<a href='www.e-assessment.in'>E-assessment.in</a>");
            bulder.Append("</td>");
            bulder.Append("</tr>");
            bulder.Append("<tr>");
            bulder.Append("<td>");
            bulder.Append("Note: This is a system generated mail please do not reply");
            bulder.Append("</td></tr></table></td></tr>");
            bulder.Append("<tr style='background-color: #908C8C;'>");
            bulder.Append("<td style='height: 20px; border: solid 1px #908C8C;text-align:center;font-family: serif;color:white;'>");
            bulder.Append("<h4>Preparing for tomorrow</h4>");
            bulder.Append("</td>");
            bulder.Append("</tr></table></td></tr></table>");
            return bulder.ToString();
        }

        #endregion


        #region Nevigation Bar Markup
        public string GetMarkup()
        {
            StringBuilder bulder = new StringBuilder();
            string orgid = User.OrgId;
            string roleid = User.RoleId;
            string Userid = User.UserId;
            //string roleid = User.
            //string Userid =
            bool isapplicant = false;
            if (User.ISApplicant != null && User.ISApplicant != "")
            {
                isapplicant = true;
            }
            using (EPortalEntities entity = new EPortalEntities())
            {
                var data = (from p in entity.Previleages
                            join u in entity.UserRoles on new
                            {
                                roleid = p.RoleId,
                                Userid = Userid
                            }
                            equals new
                            {
                                roleid = u.RoleId,
                                Userid = u.UserId
                            }
                            join mp in entity.ModulePages on p.PageId equals mp.PageId
                            join m in entity.Modules on mp.ModuleId equals m.Id
                            join pa in entity.Pages on p.PageId equals pa.Id
                            join orgp in entity.OrganizationPages on new
                            {
                                pageid = p.PageId,
                                Orgid = p.OrganizationID
                            } equals new
                            {
                                pageid = orgp.PageId,
                                Orgid = orgp.OrganizationID
                            }
                            where p.OrganizationID == orgid
                            && p.RoleId == roleid
                            && p.RowState == true
                            && u.UserId == Userid
                            && (isapplicant == false ? true : pa.ForAdmin == false)
                            && (orgid == "1" ? true : (p.PCreate == true || p.PUpdate == true || p.PDelete == true || p.PView == true))
                            //group m by new { }into g
                            select new
                            {
                                ModuleName = m.Name,
                                MOduleCode = m.Code,
                                ModuleId = m.Id,
                                ModuleSequence = m.SequenceNo,
                                PageId = pa.Id,
                                PageName = pa.Name,
                                PageCode = pa.Code,
                                pagesequence = pa.SequenceNo

                            }).ToList();

                var data1 = (from p in data
                             group p by new { ModuleName = p.ModuleName, ModuleSequence = p.ModuleSequence, ModuleId = p.ModuleId, ModuleCode = p.MOduleCode } into g1
                             select new
                             {
                                 ModuleName = g1.Key.ModuleName,
                                 ModuleId = g1.Key.ModuleId,
                                 ModuleCode = g1.Key.ModuleCode,
                                 ModuleSequence = g1.Key.ModuleSequence,
                                 PageList = (from pa in data
                                             where pa.ModuleId == g1.Key.ModuleId
                                             select new
                                             {
                                                 PageId = pa.PageId,
                                                 PageName = pa.PageName,
                                                 PageCode = pa.PageCode,
                                                 pagesequence = pa.pagesequence
                                             }).OrderBy(x => x.pagesequence).ToList()


                             }).OrderBy(x => x.ModuleSequence).ToList();




                #region New Nav
                //foreach (var item in data1)
                //{
                //    bulder.AppendFormat("<md-subheader class='md-no-sticky' style='height: 54px;'>{0}</md-subheader>", item.ModuleName);
                //    foreach (var pageitem in item.PageList)
                //    {
                //        bulder.AppendFormat("<md-list-item class='pagecolor margindownpage' ng-href='{0}'>", "/" + pageitem.PageCode + "/Index");
                //        bulder.AppendFormat("<p>{0}</p>", pageitem.PageName);
                //        bulder.Append("</md-list-item>");
                //    }
                //}

                #endregion


                #region New Navigation
                foreach (var item in data1)
                {
                    bulder.Append("<li>");
                    bulder.AppendFormat("<a><i class='fa fa-home'></i>{0}<span class='fa fa-chevron-down'></span></a>", item.ModuleName);
                    bulder.Append("<ul class='nav child_menu'>");
                    foreach (var pageitem in item.PageList)
                    {
                        bulder.AppendFormat("<li><a href='{0}'>{1}</a></li>", "/" + pageitem.PageCode + "/Index", pageitem.PageName);
                    }
                    bulder.Append("</ul>");
                    bulder.Append("</li>");
                }
                #endregion



                //#region New Navigation
                //foreach (var item in data1)
                //{
                //    bulder.Append("<li style='background-color:#009688;margin-bottom:1%;'>");
                //    bulder.AppendFormat("<div class='link' style='color: white;' id='clickeassessment'><i class='fa fa-database'></i>{0}<i class='fa fa-chevron-down'></i></div>", item.ModuleName);
                //    bulder.Append("<ul class='submenu'>");
                //    foreach (var pageitem in item.PageList)
                //    {
                //        bulder.AppendFormat("<li><a href='{0}'>{1}</a></li>", "/" + pageitem.PageCode + "/Index", pageitem.PageName);
                //    }
                //    bulder.Append("</ul>");
                //    bulder.Append("</li>");
                //}
                //#endregion

                return bulder.ToString();
            }



        }
        #endregion

        #region User Lost Password
        public ActionResult UserLostPassword()
        {

            EPortal.Models.UserInfo UserForgotPassword = new UserInfo();
            return View("LostPassword", UserForgotPassword);
        }
        public ActionResult LostPasswordCheck(EPortal.Models.UserInfo lostpassword)
        {


            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion


    }
    public class changePassword
    {
        public string oldpassword { get; set; }
        public string newpassword { get; set; }
        public string renewpassword { get; set; }
    }
    public class UserForgotPassword
    {
        public string EmailId { get; set; }
    }
    public class MessageList
    {
        public string Message { get; set; }
        public DateTime MessageDate { get; set; }
        public string MessageDatestr { get; set; }
        public string PhotoPath { get; set; }
        public string ApplicantName { get; set; }
        public bool IsNew { get; set; }
        public string UserId { get; set; }
    }
    public class MessageUserList
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public int NewMsgCount { get; set; }
    }
}