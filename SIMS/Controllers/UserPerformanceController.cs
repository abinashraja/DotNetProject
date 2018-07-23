using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Models;
using EPortal.Utility;
using EPortal.App_Start;
using System.Security.Principal;
using System.Web.Script.Serialization;
using System.Security.Cryptography;

namespace EPortal.Controllers
{
    public class UserPerformanceController : BaseController
    {

        #region TestQuestion Index
        [Authorize]
        [CustomFilter(PageName = "UserPerformance")]
        public ActionResult Index()
        {
            return View("UserPerformance");
        }
        #endregion


        #region Get All Test List
        public JsonResult GetAllTestList()
        {
            //string orgid = Session["Orgid"].ToString();
            //string userid = Session["UserId"].ToString();
            string orgid = User.OrgId;
            string userid = User.UserId;

            List<GraphTest> graphtest = new List<GraphTest>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                var testdata = (from t in entity.UserMarks
                                join tn in entity.Tests on t.TestId equals tn.Id
                                where t.OrganizationID == orgid
                                && t.ApplicantId == userid
                                group t by new { testid = t.TestId, testname = tn.TestName }
                           into j1
                                select new
                                {
                                    TestId = j1.Key.testid,
                                    TestName = j1.Key.testname,
                                    TestMaxmarks = (from tq in entity.TestQuestions
                                                    join q in entity.Questions on new
                                                    {
                                                        orgid = tq.OrganizationID,
                                                        questionid = tq.QuestionId
                                                    } equals new
                                                    {
                                                        orgid = q.OrganizationID,
                                                        questionid = q.Id
                                                    }
                                                    where tq.OrganizationID == orgid
                                                   && tq.TestId == j1.Key.testid
                                                    select q.QuestionMarks).Sum(),
                                    TestScore = (from ts in entity.UserMarks
                                                 where ts.OrganizationID == orgid
                                                 && ts.ApplicantId == userid
                                                 && ts.TestId == j1.Key.testid
                                                 select ts).ToList()


                                }).ToList();

                foreach (var item in testdata)
                {
                    decimal percentage = 0;
                    decimal score = 0;
                    foreach (var mar in item.TestScore)
                    {
                        score = score + Convert.ToInt32(mar.Mark);
                    }
                    percentage = (score / item.TestMaxmarks) * 100;
                    graphtest.Add(new GraphTest { TestId = item.TestId, TestName = item.TestName, TestScore = percentage });
                }

            }
            return Json(new { OveralPerformance = graphtest }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Test Data
        public JsonResult GetTestData(string testid)
        {
            //string orgid = Session["Orgid"].ToString();
            //string userid = Session["UserId"].ToString();
            string orgid = User.OrgId;
            string userid = User.UserId;
            List<GraphTestSection> testsectiondata = new List<GraphTestSection>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                var data = (from t in entity.UserMarks
                            join ts in entity.TestSections on t.TestSectionId equals ts.Id
                            where t.OrganizationID == orgid
                            && t.TestId == testid
                            && t.ApplicantId == userid
                            select new
                            {
                                testsectionid = ts.Id,
                                testsectionname = ts.TestSectionName,
                                testsectionscore = t.Mark
                            }).ToList();
                if (data.Count() > 0)
                {
                    foreach (var item in data)
                    {
                        testsectiondata.Add(new GraphTestSection { TestSectionId = item.testsectionid, TestSectionName = item.testsectionname, TestSectionScore = Convert.ToInt32(item.testsectionscore) });
                    }
                }
            }
            return Json(testsectiondata, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
    public class GraphTest
    {
        public string TestId { get; set; }
        public string TestName { get; set; }
        public decimal TestScore { get; set; }
    }
    public class GraphTestSection
    {
        public string TestSectionId { get; set; }
        public string TestSectionName { get; set; }
        public int TestSectionScore { get; set; }
    }
}