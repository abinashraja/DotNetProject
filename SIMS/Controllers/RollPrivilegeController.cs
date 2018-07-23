using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Models;
using EPortal.Utility;
using EPortal.App_Start;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using System.IO;

namespace EPortal.Controllers
{


    public class RollPrivilegeController : BaseController
    {
        HomeController homecontroller = new HomeController();

        #region User Index
        [Authorize]
        [CustomFilter(PageName = "UserHome")]
        public ActionResult Index()
        {

            return View("RolePrevilage");
        }
        #endregion        

        #region Get User Type User List
        [Authorize]
        public JsonResult GetUserTypeUserList(string searchtext)
        {
            List<UserTypeUserList> UserTypeUserList = new List<Controllers.UserTypeUserList>();
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            using (EPortalEntities entity = new EPortalEntities())
            {
                UserTypeUserList = (from u in entity.UserInfoes
                                    join ro in entity.UserRoles
                                    on new
                                    {
                                        orgdid = u.OrganizationID,
                                        Userid = u.Id
                                    }
                                    equals new
                                    {
                                        orgdid = ro.OrganizationID,
                                        Userid = ro.UserId
                                    }
                                    into j1
                                    from ro in j1.DefaultIfEmpty()
                                    where u.UserType != "1" && u.UserType != "50"
                                    && u.UserType == "40"
                                    && u.OrganizationID == orgid
                                    && ((searchtext == null || searchtext == "") ? true : (u.Code.ToLower().Contains(searchtext.ToLower())
                       || u.Name.ToLower().Contains(searchtext.ToLower())
                       || ((u.IsApplicant == true ? "Applicant" : "User").ToLower().Contains(searchtext.ToLower()))
                       || ((ro != null && ro.RowState == true) ? "Yes" : "No").ToLower().Contains(searchtext.ToLower())
                       ))

                                    select new UserTypeUserList
                                    {
                                        Id = u.Id,
                                        Code = u.Code,
                                        Name = u.Name,
                                        Selected = false,
                                        LoginExist = (ro != null && ro.RowState == true) ? "Yes" : "No",
                                        ApplicantOrUser = u.IsApplicant == true ? "Applicant" : "User",
                                        Email = u.Email
                                    }).ToList();
            }

            return Json(UserTypeUserList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Role List
        public JsonResult GetRoleList(UserTypeUserList selectedUser)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            List<RoleListInfo> rolelist = new List<RoleListInfo>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                rolelist = (from r in entity.RoleMasters
                            join Userro in entity.UserRoles on new
                            {
                                orgid = r.OrganizationID,
                                Userid = selectedUser.Id,
                                roleid = r.Id

                            }
                            equals new
                            {
                                orgid = Userro.OrganizationID,
                                Userid = Userro.UserId,
                                roleid = Userro.RoleId
                            }
                            into j1
                            from Userro in j1.DefaultIfEmpty()
                            where r.OrganizationID == orgid
                            select new RoleListInfo
                            {
                                Id = r.Id,
                                Code = r.Code,
                                Name = r.Name,
                                Selected = (Userro == null ? false : true)

                            }).ToList();
            }
            return Json(rolelist, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Save Selected Role
        public JsonResult SaveSelectedRole(string selectedUser, string selectedrole)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;
            EPortal.Models.UserRole Userrorle = null;
            int result = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {

                Userrorle = (from usr in entity.UserRoles
                             where usr.OrganizationID == orgid
                             && usr.RoleId == selectedrole
                             && usr.UserId == selectedUser
                             select usr).FirstOrDefault();
                if (Userrorle == null)
                {
                    Userrorle = new UserRole();
                    Userrorle.Id = Guid.NewGuid().ToString();
                    Userrorle.UserId = selectedUser;
                    Userrorle.RoleId = selectedrole;
                    Userrorle.OrganizationID = orgid;
                    Userrorle.RowState = false;
                    Userrorle.CreateDateTime = System.DateTime.Now;
                    entity.Entry(Userrorle).State = System.Data.Entity.EntityState.Added;
                }
                else
                {
                    entity.Entry(Userrorle).State = System.Data.Entity.EntityState.Modified;
                }

                result = entity.SaveChanges();

            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Activate User
        public JsonResult ActivateUser(string Userid)
        {
            int result = 0;
            string msg = string.Empty;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            EPortal.Models.UserRole Userrole = null;
            EPortal.Models.UserInfo userinfo = null;
            using (EPortalEntities entity = new EPortalEntities())
            {
                Userrole = (from us in entity.UserRoles
                            where us.OrganizationID == orgid
                            && us.UserId == Userid
                            select us).FirstOrDefault();
                userinfo = (from u in entity.UserInfoes
                            where u.OrganizationID == orgid
                            && u.Id == Userid
                            select u).FirstOrDefault();
                if (userinfo != null)
                {
                    if (userinfo.NoOfLogin.HasValue && userinfo.NoOfLogin.Value == 1)
                    {
                        userinfo.NoOfLogin = null;
                        entity.Entry(userinfo).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                if (Userrole != null)
                {
                    Userrole.RowState = true;

                    entity.Entry(Userrole).State = System.Data.Entity.EntityState.Modified;
                    result = entity.SaveChanges();

                }
                else
                {
                    msg = "Please assign role for selected user.";
                }
            }

            return Json(new { result = result > 0 ? true : false, msg = msg }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Get Test List
        [Authorize]
        public JsonResult GetTestList(UserTypeUserList user)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            List<TestList> org = new List<TestList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Tests
                       join at in entity.ApplicantTests
                       on new
                       {
                           orgid = o.OrganizationID,
                           testid = o.Id,
                           Applicantid = user.Id,
                           activetest = true,
                       } equals new
                       {
                           orgid = at.OrganizationID,
                           testid = at.TestId,
                           Applicantid = at.ApplicantId,
                           activetest = at.RowState
                       } into j1
                       from at in j1.DefaultIfEmpty()
                       where o.OrganizationID == orgid
                       && o.IsPublish == true
                       select new TestList
                       {
                           Id = o.Id,
                           TestCode = o.TestCode,
                           TestName = o.TestName,
                           Selected = at == null ? false : true,
                           AlreadyApplied = (from aa in entity.UserAnswers
                                             where aa.OrganizationID == orgid
                                             && aa.ApplicantId == user.Id
                                             && aa.TestId == o.Id
                                             select aa).FirstOrDefault() == null ? false : true

                       }).ToList();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Assign Test User
        public JsonResult AssignTestUser(string userid, string testid)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            bool sendmailper = false;
            string usermail = string.Empty;
            EPortal.Models.ApplicantTest apptest = null;
            int result = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {
                var checkuserexam = (from at in entity.ApplicantTests
                                     where at.OrganizationID == orgid
                                     && at.ApplicantId == userid
                                     select at).ToList();
                if (checkuserexam.Count() > 0)
                {
                    foreach (var item in checkuserexam)
                    {
                        item.RowState = false;
                        entity.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }
                }


                apptest = new ApplicantTest();
                apptest.Id = Guid.NewGuid().ToString();
                apptest.TestId = testid;
                apptest.ApplicantId = userid;
                apptest.OrganizationID = orgid;
                apptest.RowState = true;
                apptest.CreateDateTime = System.DateTime.Now;
                entity.Entry(apptest).State = System.Data.Entity.EntityState.Added;
                entity.ApplicantTests.Add(apptest);
                result = entity.SaveChanges();
                if (result > 0)
                {
                    var checkformail = (from mc in entity.EMailConfigurations
                                        where mc.OrganizationId == orgid
                                        select mc).FirstOrDefault();
                    if (checkformail != null)
                    {
                        if (checkformail.TestAssignMail == true)
                        {
                            sendmailper = true;
                            var getusermail = (from u in entity.UserInfoes
                                               where u.OrganizationID == orgid
                                               && u.Id == userid
                                               select u).FirstOrDefault();
                            if (getusermail != null)
                            {
                                usermail = getusermail.Email;
                            }
                        }
                    }
                }
            }
            if (sendmailper == true && (usermail != null || usermail != ""))
            {
                string body = "One new Test is assign to you,please check it once.";
                string heading = "New test assign";
                bool sendmail = homecontroller.SendMail(usermail, heading, body, null);
            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Send mail Q&A
        public JsonResult SendMailQA(UserTypeUserList selectedUser)
        {
            bool sendmail = false;
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;

            //string userid = Session["UserId"].ToString();
            string usermailid = string.Empty;
            List<TestApplyQuestionList> question = null;
            List<TotalQuestionList> totalnoofquestion = new List<TotalQuestionList>();
            int result = 0;
            string errmsg = string.Empty;
            string testname = string.Empty;
            List<byte[]> pdfname = new List<byte[]>();
            using (EPortalEntities entity = new EPortalEntities())
            {

                var getusertest = (from ut in entity.ApplicantTests
                                   where ut.OrganizationID == orgid
                                   && ut.ApplicantId == selectedUser.Id
                                   select ut).ToList();
                if (getusertest.Count() > 0)
                {
                    foreach (var item in getusertest)
                    {
                        testname = (from t in entity.Tests
                                    where t.OrganizationID == orgid
                                    && t.Id == item.TestId
                                    select t.TestName).FirstOrDefault();

                        #region Get All question And Answer              

                        question = (from tq in entity.TestQuestions
                                    join q in entity.Questions
                                    on new
                                    {
                                        orgid = tq.OrganizationID,
                                        Questid = tq.QuestionId
                                    } equals new
                                    {
                                        orgid = q.OrganizationID,
                                        Questid = q.Id
                                    }
                                    join qs in entity.Questionsources
                                on new
                                {
                                    orgid = q.OrganizationID,
                                    questionsource = q.SourceId

                                } equals new
                                {
                                    orgid = qs.OrganizationID,
                                    questionsource = qs.Id
                                } into j1
                                    from qs in j1.DefaultIfEmpty()
                                    where tq.OrganizationID == orgid
                                    && tq.TestId == item.TestId
                                    select new TestApplyQuestionList
                                    {
                                        QuestionId = tq.QuestionId,
                                        QuestionText = q.Question1,
                                        SourceText = qs != null ? qs.ResourceText : "",
                                        QuestionNo = tq.SequenceNo,
                                        TestQuestionoptionList = (from qo in entity.QuestionOptions
                                                                  join an in entity.QuestionAnswars
                                                          on new
                                                          {
                                                              orgid = qo.OrganizationID,
                                                              questionid = tq.QuestionId,
                                                              optionid = qo.Id

                                                          }
                                                          equals new
                                                          {

                                                              orgid = an.OrganizationID,
                                                              questionid = an.QuestionId,
                                                              optionid = an.QuestionAnswarId
                                                          }
                                                          into j2
                                                                  from an in j2.DefaultIfEmpty()
                                                                  where qo.OrganizationID == orgid
                                                          && qo.QuestionId == tq.QuestionId
                                                                  select new TestQuestionoptionList
                                                                  {
                                                                      OptionId = qo.Id,
                                                                      OptionText = qo.QuestionOption1,
                                                                      QuestionAns = an == null ? false : true,
                                                                      Selected = (from useran in entity.UserAnswers
                                                                                  where useran.OrganizationID == orgid
                                                                          && useran.TestId == item.TestId
                                                                          && useran.QuestionId == tq.QuestionId
                                                                          && useran.optionId == qo.Id
                                                                          && useran.ApplicantId == selectedUser.Id
                                                                                  select useran).FirstOrDefault() == null ? false : true
                                                                  }).ToList(),
                                    }).ToList();

                        #endregion

                        StringBuilder markup = PreparedMarkupforPDF(question, testname);
                        pdfname.Add(ConvertHtmlToPdf(markup, testname, selectedUser.Id));
                    }
                }
                else
                {
                    errmsg = "User don't have any test.";
                }
            }
            if (errmsg == string.Empty)
            {
                string body = "Please find the attached file regarding user's Test Question and Answer.";
                string heading = "Test " + testname + " Question and Answer";
                sendmail = homecontroller.SendMail(selectedUser.Email, heading, body, pdfname);
            }
            return Json(new { sendmail = sendmail, errmsg = errmsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Prepared Markup for PDF
        private StringBuilder PreparedMarkupforPDF(List<TestApplyQuestionList> question, string testname)
        {
            StringBuilder bulder = new StringBuilder();
            string orgname = Session["OrgName"].ToString();
            bulder.Append("<html>");
            bulder.Append("<head>");
            bulder.Append("</head>");
            bulder.Append("<body>");
            bulder.Append("<div>");
            bulder.AppendFormat("<h2 style='text-align: center;'>{0}</h2>", orgname);
            bulder.AppendFormat("<p style='text-align: center;'>Question And Answer for the test :{0}</p>", testname);
            int questioncount = 1;
            foreach (TestApplyQuestionList item in question)
            {
                bulder.Append("<div style='margin-bottom: 5%;'>");
                bulder.AppendFormat("<p style='font-weight: bold;'>Question:{0}</p>", questioncount);
                bulder.AppendFormat("<p>{0}</p>", item.QuestionText);
                int count = 1;
                foreach (var option in item.TestQuestionoptionList)
                {
                    bulder.AppendFormat("<p style='margin-left:3%;font-weight: bold;color:{0};'>option-{1}</p>", option.QuestionAns == true ? "Green" : option.Selected == true ? "red" : "black", count);
                    bulder.AppendFormat("<p style='margin-left:3%;color:{0};'>{1}</p>", option.QuestionAns == true ? "Green" : option.Selected == true ? "red" : "black", option.OptionText);

                    count = count + 1;
                }
                questioncount = questioncount + 1;
                bulder.Append("</div>");
                //bulder.Append("<hr/>");

            }
            bulder.Append("</div>");
            bulder.Append("</body>");
            bulder.Append("</html>");

            return bulder;
        }
        #endregion

        #region Generate Pdf  Mathod



        private byte[] ConvertHtmlToPdf(StringBuilder sbHtmlText, string testname, string userid)
        {
            byte[] bPDF = null;
            string returnvalue = string.Empty;
            returnvalue = Request.PhysicalApplicationPath + "\\Upload\\" + userid + testname + ".pdf";


            MemoryStream ms = new MemoryStream();
            TextReader txtReader = new StringReader(sbHtmlText.ToString());

            // 1: create object of a itextsharp document class
            Document doc = new Document(PageSize.A4, 25, 25, 25, 25);

            // 2: we create a itextsharp pdfwriter that listens to the document and directs a XML-stream to a file
            PdfWriter oPdfWriter = PdfWriter.GetInstance(doc, ms);

            // 3: we create a worker parse the document
            iTextSharp.text.html.simpleparser.HTMLWorker htmlWorker = new iTextSharp.text.html.simpleparser.HTMLWorker(doc);

            // 4: we open document and start the worker on the document
            doc.Open();
            htmlWorker.StartDocument();

            // 5: parse the html into the document
            htmlWorker.Parse(txtReader);

            // 6: close the document and the worker
            htmlWorker.EndDocument();
            htmlWorker.Close();
            doc.Close();
            ms.Close();

            bPDF = ms.ToArray();

            //System.IO.File.WriteAllBytes(returnvalue, bPDF);

            return bPDF;
        }
        #endregion
    }

    public class UserTypeUserList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool Selected { get; set; }
        public string LoginExist { get; set; }
        public string ApplicantOrUser { get; set; }

    }
    public class RoleListInfo
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
        public bool? IsApplicant { get; set; }
    }

}