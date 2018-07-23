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
    public class MailConfigController : BaseController
    {

        #region TestSection Index
        [Authorize]
        [CustomFilter(PageName = "MailConfig")]
        public ActionResult Index()
        {

            return View("MailConfig");
        }
        #endregion

        #region Get Mail Configuration
        public JsonResult GetMailConfiguration()
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            MailCOnfigurationClass mailconfig = new MailCOnfigurationClass();
            using (EPortalEntities entity = new EPortalEntities())
            {
                mailconfig = (from m in entity.EMailConfigurations
                              where m.OrganizationId == orgid
                              select new MailCOnfigurationClass
                              {
                                  UserCreation = m.UserCreationMail,
                                  TestAssign = m.TestAssignMail,
                                  Login = m.AfterLoginMail,
                                  ChangePassword = m.AfterChangePasswordMail,
                                  ResultAfterTest = m.SendResultAfterTestMail,
                                  Questionpaper = false
                              }).FirstOrDefault();
            }

            return Json(mailconfig, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save Mail Config
        public JsonResult SaveMailConfig(MailCOnfigurationClass mailconfigdata)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            EPortal.Models.EMailConfiguration mailconfig = null;
            int result = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {
                mailconfig = (from m in entity.EMailConfigurations
                              where m.OrganizationId == orgid
                              select m).FirstOrDefault();
                if (mailconfig == null)
                {
                    mailconfig = new EMailConfiguration();
                    mailconfig.Id = Guid.NewGuid().ToString();
                    mailconfig.OrganizationId = orgid;
                    mailconfig.UserCreationMail = mailconfigdata.UserCreation;
                    mailconfig.TestAssignMail = mailconfigdata.TestAssign;
                    mailconfig.AfterLoginMail = mailconfigdata.Login;
                    mailconfig.SendResultAfterTestMail = mailconfigdata.ResultAfterTest;
                    mailconfig.AfterChangePasswordMail = mailconfigdata.ChangePassword;
                    entity.Entry(mailconfig).State = System.Data.Entity.EntityState.Added;
                    entity.EMailConfigurations.Add(mailconfig);

                }
                else
                {
                    mailconfig.UserCreationMail = mailconfigdata.UserCreation;
                    mailconfig.TestAssignMail = mailconfigdata.TestAssign;
                    mailconfig.AfterLoginMail = mailconfigdata.Login;
                    mailconfig.SendResultAfterTestMail = mailconfigdata.ResultAfterTest;
                    mailconfig.AfterChangePasswordMail = mailconfigdata.ChangePassword;
                    entity.Entry(mailconfig).State = System.Data.Entity.EntityState.Modified;
                }
                result = entity.SaveChanges();

            }

            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    public class MailCOnfigurationClass
    {
        public bool UserCreation { get; set; }
        public bool TestAssign { get; set; }
        public bool Login { get; set; }
        public bool ChangePassword { get; set; }
        public bool ResultAfterTest { get; set; }
        public bool Questionpaper { get; set; }
    }
}