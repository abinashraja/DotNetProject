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
    public class TestQAController : BaseController
    {

        #region User Index
        [Authorize]
        [CustomFilter(PageName = "TestQA")]
        public ActionResult Index()
        {

            return View("TestQA");
        }
        #endregion

        #region Get Test List
        [Authorize]
        public JsonResult GetTestList()
        {
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            List<TestList> testlist = new List<TestList>();
            using (EPortalEntities entity = new EPortalEntities())
            {

                testlist = (from um in entity.UserMarks
                            join t in entity.Tests on new
                            {
                                orgid = um.OrganizationID,
                                testid = um.TestId

                            } equals new
                            {
                                orgid = t.OrganizationID,
                                testid = t.Id
                            }

                            where um.ApplicantId == userid
                            && um.OrganizationID == orgid
                            && t.Islocked == true
                            group t by new { testid = t.Id, testcode = t.TestCode, testname = t.TestName }
                            into j1
                            select new TestList
                            {
                                Id = j1.Key.testid,
                                TestCode = j1.Key.testcode,
                                TestName = j1.Key.testname,
                                URl = "SendQuestionAnsPDf?testid=" + j1.Key.testid,
                            }).ToList();
            }
            return Json(testlist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Send Question ans PDF TO Applicant
        public ActionResult SendQuestionAnsPDf(string testid)
        {
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            string usermailid = string.Empty;
            List<TestApplyQuestionList> question = null;
            List<TotalQuestionList> totalnoofquestion = new List<TotalQuestionList>();
            int result = 0;
            string errmsg = string.Empty;
            string testname = string.Empty;
            using (EPortalEntities entity = new EPortalEntities())
            {

                testname = (from t in entity.Tests
                            where t.OrganizationID == orgid
                            && t.Id == testid
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
                            && tq.TestId == testid
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
                                                                          && useran.TestId == testid
                                                                          && useran.QuestionId == tq.QuestionId
                                                                          && useran.optionId == qo.Id
                                                                          && useran.ApplicantId == userid
                                                                          select useran).FirstOrDefault() == null ? false : true
                                                          }).ToList(),
                            }).ToList();

                #endregion


            }
            StringBuilder markup = PreparedMarkupforPDF(question, testname);
            ConvertHtmlToPdf(markup);
            return View("TestQA");

        }
        #endregion

        #region Prepared Markup for PDF
        private StringBuilder PreparedMarkupforPDF(List<TestApplyQuestionList> question,string testname)
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
        private bool ConvertHtmlToPdf(StringBuilder sbHtmlText)
        {
            //StringBuilder sbHtmlText
            //StringBuilder sbHtmlText = new StringBuilder();
            //sbHtmlText.Append("<html><head>Employee Info</head>");
            //sbHtmlText.Append("<body>Hi This is Employee Info</body></html>");
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            

            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(Request.PhysicalApplicationPath + "\\" + userid + ".pdf", FileMode.Create));
            document.Open();
            iTextSharp.text.html.simpleparser.HTMLWorker hw =
            new iTextSharp.text.html.simpleparser.HTMLWorker(document);
            try
            {


                hw.Parse(new StringReader(sbHtmlText.ToString()));
            }
            catch (Exception ex)
            {

            }
            document.Close();

            try
            {
                Response.ClearHeaders();
                Response.ContentType = "Application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + userid + ".pdf");
                Response.TransmitFile(Request.PhysicalApplicationPath + "\\" + userid + ".pdf");
                Response.End();
            }
            catch (Exception ex)
            {

            }





            if (System.IO.File.Exists(Server.MapPath("\\" + userid + ".pdf")))
            {
                // Use a try block to catch IOExceptions, to 
                // handle the case of the file already being 
                // opened by another process. 
                try
                {
                    System.IO.File.Delete(Server.MapPath("\\" + userid + ".pdf"));
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);

                }
            }
            return true;
        }
        #endregion









    }
}