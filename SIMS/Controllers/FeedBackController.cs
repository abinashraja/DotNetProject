using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Models;
using EPortal.Utility;
using EPortal.App_Start;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using System.IO;
using System.Net;

namespace EPortal.Controllers
{
    public class FeedBackController : BaseController
    {

        #region User Index
        [Authorize]
        [CustomFilter(PageName = "FeedBack")]
        public ActionResult Index()
        {

            return View("FeedBack");
        }
        public JsonResult SaveFeedback(UserFeedBack feedback)
        {

            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            using (EPortalEntities entity = new EPortalEntities())
            {

                var orgadminid = (from a in entity.UserInfoes
                                  where a.OrganizationID == orgid
                                  && a.Code == "Admin"
                                  select a.Id).FirstOrDefault();
                


                feedback.Id = Guid.NewGuid().ToString();
                feedback.OrganizationID = orgid;
                feedback.RowState = true;
                feedback.ApplicantId = userid;
                feedback.CreateDateTime = System.DateTime.Now;
                feedback.ApplicantTo = orgadminid;
                entity.Entry(feedback).State = System.Data.Entity.EntityState.Added;
                entity.UserFeedBacks.Add(feedback);
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


}