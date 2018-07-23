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
    public class UserReportController : BaseController
    {

        #region User Index
        [Authorize]
        [CustomFilter(PageName = "UserReport")]
        public ActionResult Index()
        {

            return View("UserReport");
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
                            group t by new { testid = t.Id, testcode = t.TestCode, testname = t.TestName }
                            into j1
                            select new TestList
                            {
                                Id = j1.Key.testid,
                                TestCode = j1.Key.testcode,
                                TestName = j1.Key.testname,
                                URl = "GetResult?testid=" + j1.Key.testid,
                            }).ToList();
            }
            return Json(testlist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Result
        public JsonResult GetResult(string testid)
        {
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            //string OrgaName = Session["OrgName"].ToString();
            //string userName = Session["UserName"].ToString();

            string OrgaName = User.OrgName;
            string userName = User.UserName;

            List<UserReportCLass> useranslist = null;
            string testname = string.Empty;
            OrgDateil orgdetail = new OrgDateil();
            using (EPortalEntities entity = new EPortalEntities())
            {

                testname = (from t in entity.Tests
                            where t.OrganizationID == orgid
                            && t.Id == testid
                            select t.TestName).FirstOrDefault();

                orgdetail = (from o in entity.Organizations
                             where o.Id == orgid
                             select new OrgDateil
                             {
                                 OrgaName = o.Name,
                                 OrgaAddress = o.Address,
                                 OrgaPin = o.Pin,
                                 OrgaCountry = o.Country,
                                 OrgaEst = o.ESTDate,
                                 OrgaState = o.OrgState,
                                 OrgLogo = "/Home/GetOrgLogo?orgid="+o.Id
                             }).FirstOrDefault();
                orgdetail.OrgaEststr = orgdetail.OrgaEst.HasValue?orgdetail.OrgaEst.Value.ToShortDateString():"";

                if (testname != null)
                {
                    useranslist = (from um in entity.UserMarks
                                   join s in entity.TestSections
                                   on new
                                   {
                                       orgid = um.OrganizationID,
                                       testid = um.TestId,
                                       sectionid = um.TestSectionId
                                   } equals new
                                   {
                                       orgid = s.OrganizationID,
                                       testid = s.ParentId,
                                       sectionid = s.Id
                                   }
                                   where um.OrganizationID == orgid
                                   && um.TestId == testid
                                   && um.ApplicantId == userid
                                   select new UserReportCLass
                                   {
                                       TestSectionId = um.TestSectionId,
                                       TestSectionName = s.TestSectionName,
                                       TestSectionMarkStr = um.Mark,
                                       TestName = testname,
                                       ApplicantName = userName
                                   }).ToList();
                }
            }
            useranslist.Add(new UserReportCLass { TestSectionId = "Total", TestSectionName = "Total", TestSectionMarkStr = useranslist.Sum(x => Convert.ToDecimal(x.TestSectionMarkStr)).ToString() });
            //StringBuilder markup = PreparedMarkupforPDF(useranslist, testname);
            //ConvertHtmlToPdf(markup, testname);
            return Json(new { useranslist = useranslist, orgdetail = orgdetail }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Prepared Markup for PDF
        private StringBuilder PreparedMarkupforPDF(List<TestSectionMarkList> useranslist, string testname)
        {
            StringBuilder bulder = new StringBuilder();
            string orgname = Session["OrgName"].ToString();
            bulder.Append("<html>");
            bulder.Append("<head>");
            bulder.Append("</head>");
            bulder.Append("<body>");
            bulder.Append("<div class='container' style='margin - top: 2 %;'>");
            bulder.AppendFormat("<h2 style='text-align: center;'>{0}</h2>", orgname);
            bulder.AppendFormat("<p style='text-align: center;'>{1} report card for Test :{0}</p>", testname, Session["UserName"].ToString());
            bulder.Append("<div style='margin-left: 20%;'>");
            bulder.Append(" <table style='width:100%;border: 1px solid black;border - collapse: collapse;'>");
            bulder.Append("<thead>");
            bulder.Append("<tr class='danger'>");
            foreach (var item in useranslist)
            {
                bulder.AppendFormat("<th style'font-weight:bold;border: 1px solid black;border-collapse:collapse;'>{0}</th>", item.TestSectionName);
            }
            bulder.Append("</tr>");
            bulder.Append("</thead>");

            bulder.Append("<tbody>");
            bulder.Append("<tr style='border: 1px solid black;border-collapse:collapse;'>");
            foreach (var item in useranslist)
            {
                bulder.AppendFormat("<th>{0}</th>", item.TestSectionMarkStr);
            }
            bulder.Append("</tr>");
            bulder.Append("</tbody>");

            bulder.Append("</table>");
            bulder.Append("</div>");
            bulder.Append("</div>");
            bulder.Append("</body>");
            bulder.Append("</html>");

            return bulder;
        }
        #endregion

        #region Generate Pdf  Mathod
        private bool ConvertHtmlToPdf(StringBuilder sbHtmlText, string testname)
        {
            //StringBuilder sbHtmlText
            //StringBuilder sbHtmlText = new StringBuilder();
            //sbHtmlText.Append("<html><head>Employee Info</head>");
            //sbHtmlText.Append("<body>Hi This is Employee Info</body></html>");
            //string userid = Session["UserId"].ToString();
            string userid = User.UserId;
            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(Request.PhysicalApplicationPath + "\\" + userid + testname + ".pdf", FileMode.Create));
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
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + userid + testname + ".pdf");
                Response.TransmitFile(Request.PhysicalApplicationPath + "\\" + userid + testname + ".pdf");
                Response.End();
            }
            catch (Exception ex)
            {

            }





            if (System.IO.File.Exists(Server.MapPath("\\" + userid + testname + ".pdf")))
            {
                // Use a try block to catch IOExceptions, to 
                // handle the case of the file already being 
                // opened by another process. 
                try
                {
                    System.IO.File.Delete(Server.MapPath("\\" + userid + testname + ".pdf"));
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
    public class UserReportCLass
    {
        public string TestSectionId { get; set; }
        public string TestSectionName { get; set; }
        public string TestSectionMarkStr { get; set; }
        public string TestName { get; set; }
        public string ApplicantName { get; set; }
        public string OrgaName { get; set; }
        public string OrgaAddress { get; set; }

    }
    public class OrgDateil
    {
        public string OrgaName { get; set; }
        public string OrgaAddress { get; set; }
        public string OrgaPin { get; set; }
        public DateTime? OrgaEst { get; set; }
        public string OrgaEststr { get; set; }
        public string OrgaState { get; set; }
        public string OrgaCountry { get; set; }
        public string OrgLogo { get; set; }
    }
}