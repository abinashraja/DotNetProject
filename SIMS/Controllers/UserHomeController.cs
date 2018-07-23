using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Models;
using EPortal.Utility;
using EPortal.App_Start;
using OpenPop.Pop3;
namespace EPortal.Controllers
{
    public class UserHomeController : BaseController
    {
        HomeController homecontroller = new HomeController();

        #region Get OrgData
        [Authorize]
        public JsonResult GetOrgData()
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;
            DeshBoardData org = new DeshBoardData();
            if (User.ISApplicant != null && User.ISApplicant != "")
            {
                //if (((bool)Session["ISApplicant"]) == true)
                //{
                //    org.IsApplicant = true;
                //}
                //else
                //{
                //    org.IsApplicant = true;
                //}
                org.IsApplicant = true;

            }
            else if (orgid == "1")
            {
                org.IsApplicant = false;
                org.IsSuper = true;
            }
            else
            {
                using (EPortalEntities entity = new EPortalEntities())
                {
                    org.TotalUserCount = (from u in entity.UserInfoes
                                          where u.OrganizationID == orgid
                                          //&& u.IsApplicant == true
                                          select u).Count();
                    org.TotalAssignUserCount = (from u in entity.ApplicantTests
                                                where u.OrganizationID == orgid
                                                select u.ApplicantId).Distinct().Count();
                    org.TotalNotAssignUserCount = org.TotalUserCount - org.TotalAssignUserCount;
                    org.TotalTestCount = (from t in entity.Tests
                                          where t.OrganizationID == orgid
                                          select t).Count();
                    org.TotalAssignTestCount = (from t in entity.ApplicantTests
                                                where t.OrganizationID == orgid
                                                select t.TestId).Distinct().Count();

                    org.TotalNotAssignTestCount = org.TotalTestCount - org.TotalAssignTestCount;
                    org.TotalSubject = (from sub in entity.Subjects where sub.OrganizationID == orgid select sub).Count();
                    org.TotalCourse = (from sub in entity.Courses where sub.OrganizationID == orgid select sub).Count();
                    org.TotalClasss = (from sub in entity.Classes where sub.OrganizationID == orgid select sub).Count();
                    org.TotalSection = (from sub in entity.Sections where sub.OrganizationID == orgid select sub).Count();
                    org.TotalStudent = (from sub in entity.UserInfoes where sub.OrganizationID == orgid select sub).Count();
                    org.TotalFaculty = (from sub in entity.UserInfoes where sub.OrganizationID == orgid select sub).Count();


                }
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get mail
        public JsonResult GetMail()
        {
            Pop3Client client = new Pop3Client();
            client.Connect("pop.asia.secureserver.net", 110, false);
            client.Authenticate("abinashpujhari@e-assessment.in", "passward");
            var count = client.GetMessageCount();
            List<MailData> maildata = new List<MailData>();
            MailData mail = null;
            //var datamail1 = client.GetMessage(1);
            //var datamail2= client.GetMessage(2);
            //var datamail3 = client.GetMessage(3);
            //var datamail4 = client.GetMessage(4);
            for (int i = 2; i <= count; i++)
            {
                mail = new MailData();
                mail.MailHeading = client.GetMessage(i).Headers.Subject.ToString();
                //mail.MailBody = (client.GetMessage(i)).MessagePart.GetBodyAsText().ToString();
                maildata.Add(mail);
            }
            if (count > 0)
            {
                for (int i = 1; i < count; i++)
                {
                    mail = new MailData();
                    //var datamail  =  client.GetMessage(i);
                    // mail.MailHeading = datamail.RawMessage.ToString();

                }
            }

            var message = client.GetMessage(count);

            return Json(message, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Sand Mail
        public JsonResult SandMail(MailData maildata)
        {
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string orgid = User.OrgId;
            string userid = User.UserId;

            string frommailid = string.Empty;
            bool samdmail = false;
            string msg = string.Empty;
            string usermailpassword = string.Empty;
            using (EPortalEntities entity = new EPortalEntities())
            {
                var userdata = (from u in entity.UserInfoes
                                where u.Id == userid
                                && u.OrganizationID == orgid
                                select u).FirstOrDefault();
                if (userdata != null)
                {
                    //frommailid = userdata.Email;
                    //usermailpassword = userdata.UserPassword;
                }
            }
            //if (!String.IsNullOrWhiteSpace(frommailid))
            //{
            samdmail = homecontroller.SendMail(maildata.MailTo, maildata.MailHeading, maildata.MailBody, null, frommailid, usermailpassword);
            //}
            //else
            //{
            //    msg = "Mail id not configure for login user.";
            //}
            return Json(new { samdmail = samdmail, msg = msg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region  GetAllEvent
        public JsonResult GetAllEvent()
        {
            List<OrgEventList> orgeventlist = new List<OrgEventList>();
            string orgid = User.OrgId;
            using (EPortalEntities entity = new EPortalEntities())
            {
                orgeventlist = (from ev in entity.OrgEvents
                                where ev.OganizationId == orgid
                                && ev.EventDate >= System.DateTime.Now
                                select new OrgEventList
                                {
                                    EventCode = ev.EventCode,
                                    EventName = ev.EventName,
                                    EventDate = ev.EventDate.Value,
                                    EventDescription = ev.EventDescription,
                                    Eventmonth = "",
                                    EventDay = ""
                                }).OrderBy(x => x.EventDate).Take(10).ToList();
            }
            if (orgeventlist.Count() > 0)
            {
                foreach (var item in orgeventlist)
                {
                    item.Eventmonth = item.EventDate.ToString("MMMM");
                    item.EventDay = item.EventDate.Day.ToString();
                }
            }

            return Json(orgeventlist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save Event
        public JsonResult SaveEvent(OrgEventList eventt)
        {
            string orgid = User.OrgId;
            int result = 0;
            EPortal.Models.OrgEvent orgevent = new OrgEvent();
            using (EPortalEntities entity = new EPortalEntities())
            {
                orgevent.Id = Guid.NewGuid().ToString();
                orgevent.EventCode = "New Event";
                orgevent.EventName = eventt.EventName;
                var theDate = new DateTime(eventt.EventDate.Year, eventt.EventDate.Month, eventt.EventDate.Day, eventt.Hour, eventt.Min, 00);
                orgevent.EventDate = theDate;
                orgevent.EventDescription = eventt.EventDescription;
                orgevent.OganizationId = orgid;
                entity.Entry(orgevent).State = System.Data.Entity.EntityState.Added;
                entity.OrgEvents.Add(orgevent);
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

        #region Show Fullcalander
        [Authorize]
        [CustomFilter(PageName = "UserHome")]
        public ActionResult fullcalendar()
        {

            return View("OrgEvent");
        }
        #endregion

        #region  GetAllEvent
        public JsonResult GetAllEventCalender()
        {
            List<OrgEventList> orgeventlist = new List<OrgEventList>();
            string orgid = User.OrgId;
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            using (EPortalEntities entity = new EPortalEntities())
            {
                orgeventlist = (from ev in entity.OrgEvents
                                where ev.OganizationId == orgid                                
                                select new OrgEventList
                                {
                                    EventCode = ev.EventCode,
                                    EventName = ev.EventName,
                                    EventDate = ev.EventDate.Value,
                                    EventDescription = ev.EventDescription,
                                    Eventmonth = "",
                                    EventDay = ""
                                }).OrderBy(x => x.EventDate).ToList();
            }
            if (orgeventlist.Count() > 0)
            {
                foreach (var item in orgeventlist)
                {
                    //item.EventDate = item.EventDate.AddDays(1);
                    item.EventDate = item.EventDate.AddHours(5);
                    item.EventDate = item.EventDate.AddMinutes(30);
                }
            }

            return Json(orgeventlist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        




    }
    public class DeshBoardData
    {
        public int TotalTestCount { get; set; }
        public int TotalAssignTestCount { get; set; }
        public int TotalNotAssignTestCount { get; set; }
        public int TotalUserCount { get; set; }
        public int TotalAssignUserCount { get; set; }
        public int TotalNotAssignUserCount { get; set; }
        public int TotalClasss { get; set; }
        public int TotalSection { get; set; }
        public int TotalStudent { get; set; }
        public int TotalFaculty { get; set; }
        public int TotalSubject { get; set; }
        public int TotalCourse { get; set; }
        public bool IsApplicant { get; set; }
        public bool IsSuper { get; set; }


    }
    public class MailData
    {
        public string MailHeading { get; set; }
        public string MailBody { get; set; }
        public string MailTo { get; set; }
    }
    public class OrgEventList
    {
        public string EventCode { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string EventDateStr { get; set; }
        public string EventDescription { get; set; }
        public string Eventmonth { get; set; }
        public string EventDay { get; set; }
        public int Hour { get; set; }
        public int Min { get; set; }
    }



}