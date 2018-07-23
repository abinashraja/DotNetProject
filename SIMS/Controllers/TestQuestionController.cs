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
    public class TestQuestionController : BaseController
    {

        #region TestQuestion Index
        [Authorize]
        [CustomFilter(PageName = "TestQuestion")]
        public ActionResult Index()
        {
            return View("TestQuestion");
        }
        #endregion

        #region Get Test List
        [Authorize]
        public JsonResult GetTestList()
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<TestList> testlist = new List<TestList>();
            List<TestSectionList> testsection = new List<TestSectionList>();
            List<QuestionTypeList> QuestionTypelIst = new List<QuestionTypeList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                testlist = (from o in entity.Tests
                            where o.OrganizationID == orgid
                            && o.IsPublish==false
                            select new TestList
                            {
                                Id = o.Id,
                                TestCode = o.TestCode,
                                TestName = o.TestName,
                            }).ToList();

                QuestionTypelIst = (from o in entity.QuestionTypes
                                    where o.OrganizationID == orgid
                                    select new QuestionTypeList
                                    {
                                        Id = o.Id,
                                        TypeCode = o.TypeCode,
                                        TypeName = o.TypeName,
                                    }).ToList();
            }
            testlist.Insert(0, new TestList { Id = "0", TestCode = "Select", TestName = "Select" });
            QuestionTypelIst.Insert(0, new QuestionTypeList { Id = "0", TypeCode = "Select", TypeName = "Select" });
            testsection.Insert(0, new TestSectionList { Id = "0", TestSectionCode = "Select", TestSectionName = "Select" });
            return Json(new { testlist = testlist, QuestionTypelIst = QuestionTypelIst, testsection = testsection }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get TestSection List
        [Authorize]
        public JsonResult GetTestSectionList(string testid)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<TestSectionList> org = new List<TestSectionList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.TestSections
                       join p in entity.Tests on o.ParentId equals p.Id
                       where o.OrganizationID == orgid
                       && o.ParentId == testid
                       select new TestSectionList
                       {
                           Id = o.Id,
                           TestSectionCode = o.TestSectionCode,
                           TestSectionName = o.TestSectionName,
                       }).ToList();
            }
            org.Insert(0, new TestSectionList { Id = "0", TestSectionCode = "Select", TestSectionName = "Select" });
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Unique Question List
        public JsonResult GetUniqueQuestionList(string questiontypeid, string testid, string testsectionid)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            int noofquestionsel = 0;
            List<UniqueQuestionlist> questionlist = new List<UniqueQuestionlist>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                questionlist = (from q in entity.Questions
                                where q.OrganizationID == orgid
                                && q.QuestionTypeId == questiontypeid
                                && q.SourceId == null
                                select new UniqueQuestionlist
                                {
                                    Id = q.Id,
                                    QuestionText = q.Question1,
                                    Selected = (from tq in entity.TestQuestions
                                                where tq.OrganizationID == orgid
                                                && tq.TestId == testid
                                                && tq.TestSectionId == testsectionid
                                                && tq.QuestionId == q.Id
                                                select tq).FirstOrDefault() == null ? false : true,
                                    Operation = (from tq in entity.TestQuestions
                                                 where tq.OrganizationID == orgid
                                                 && tq.TestId == testid
                                                 && tq.TestSectionId == testsectionid
                                                 && tq.QuestionId == q.Id
                                                 select tq).FirstOrDefault() == null ? "Create" : "Edit",
                                    CreatDate = q.CreateDateTime
                                }).OrderBy(x => x.CreatDate).ToList();
                noofquestionsel = questionlist.Where(x => x.Selected == true).Count();
            }
            return Json(new { questionlist = questionlist, noofquestionsel = noofquestionsel }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Group Question List
        public JsonResult GetGroupQuestionList(string questiontypeid, string testid, string testsectionid)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<GroupQuestionlist> questionlist = new List<GroupQuestionlist>();
            int noofgroupquestion = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {
                questionlist = (from qs in entity.Questionsources
                                where qs.OrganizationID == orgid
                                && qs.QuestionTypeId == questiontypeid
                                select new GroupQuestionlist
                                {
                                    Id = qs.Id,
                                    QuestionText = qs.ResourceText,
                                    Selected = (from q in entity.QuestionsourceGroups
                                                join tq in entity.TestQuestions
                                                 on new
                                                 {
                                                     orgid = q.OrganizationID,
                                                     testid = testid,
                                                     testsectionid = testsectionid,
                                                     questionid = q.QuestionId,

                                                 } equals new
                                                 {
                                                     orgid = tq.OrganizationID,
                                                     testid = tq.TestId,
                                                     testsectionid = tq.TestSectionId,
                                                     questionid = tq.QuestionId
                                                 }
                                                where q.OrganizationID == orgid
                                                 && q.QuestionSourceId == qs.Id
                                                select q.Id).FirstOrDefault() == null ? false : true,
                                    Operation = (from q in entity.QuestionsourceGroups
                                                 join tq in entity.TestQuestions
                                                  on new
                                                  {
                                                      orgid = q.OrganizationID,
                                                      testid = testid,
                                                      testsectionid = testsectionid,
                                                      questionid = q.QuestionId,

                                                  } equals new
                                                  {
                                                      orgid = tq.OrganizationID,
                                                      testid = tq.TestId,
                                                      testsectionid = tq.TestSectionId,
                                                      questionid = tq.QuestionId
                                                  }
                                                 where q.OrganizationID == orgid
                                                  && q.QuestionSourceId == qs.Id
                                                 select q.Id).FirstOrDefault() == null ? "Create" : "Edit",
                                    CreatDate = qs.CreateDateTime,
                                    NoOfQuestion = (from noofq in entity.QuestionsourceGroups
                                                    where noofq.OrganizationID == orgid
                                                    && noofq.QuestionSourceId == qs.Id
                                                    select noofq).Count()


                                }).OrderBy(x => x.CreatDate).ToList();

                noofgroupquestion = questionlist.Where(x => x.Selected == true).Sum(x => x.NoOfQuestion);
            }
            return Json(new { questionlist = questionlist, noofgroupquestion = noofgroupquestion }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save Test QUestion
        public JsonResult SaveTestQuestion(List<UniqueQuestionIds> usniids, List<GroupQuestionIds> groupid, string testid, string testsection)
        {

            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            int result = 0;
            EPortal.Models.TestQuestion testquestion = null;
            List<EPortal.Models.QuestionsourceGroup> questiongroupsou = null;

            using (EPortalEntities entity = new EPortalEntities())
            {
                int itera = 0;
                int count = 1;                
                if (usniids != null)
                {
                    #region Unique 
                    foreach (var item in usniids)
                    {
                        if (itera == 0)
                        {
                            var totalquestion = (from tq in entity.TestQuestions
                                                 where tq.OrganizationID == orgid
                                                 && tq.TestId == testid
                                                 //&& tq.TestSectionId == testsection
                                                 select tq).ToList();
                            if (totalquestion.Count() > 0)
                            {
                                count = totalquestion.Max(x => x.SequenceNo.Value)+1;
                            }
                            itera = 1;
                        }
                        var checkexist = (from tq in entity.TestQuestions
                                          where tq.OrganizationID == orgid
                                          && tq.TestId == testid
                                          && tq.TestSectionId == testsection
                                          && tq.QuestionId == item.Id
                                          select tq).FirstOrDefault();
                        if (checkexist == null)
                        {
                            testquestion = new TestQuestion();
                            testquestion.Id = Guid.NewGuid().ToString();
                            testquestion.TestId = testid;
                            testquestion.TestSectionId = testsection;
                            testquestion.QuestionId = item.Id;
                            testquestion.SequenceNo = count;
                            testquestion.OrganizationID = orgid;
                            testquestion.RowState = true;
                            testquestion.CreateDateTime = System.DateTime.Now;
                            entity.Entry(testquestion).State = System.Data.Entity.EntityState.Added;
                            entity.TestQuestions.Add(testquestion);
                            count = count + 1;
                        }
                        else
                        {
                            entity.Entry(checkexist).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                    #endregion
                }
                if (groupid != null)
                {
                    #region Group Question
                    foreach (var item in groupid)
                    {
                        if (usniids == null)
                        {
                            var totalquestion = (from tq in entity.TestQuestions
                                                 where tq.OrganizationID == orgid
                                                 && tq.TestId == testid
                                                 //&& tq.TestSectionId == testsection
                                                 select tq).ToList();
                            if (totalquestion.Count() > 0)
                            {
                                count = totalquestion.Max(x => x.SequenceNo.Value);
                            }
                        }
                        questiongroupsou = (from gqs in entity.QuestionsourceGroups
                                            where gqs.OrganizationID == orgid
                                            && gqs.QuestionSourceId == item.Id
                                            select gqs).ToList();
                        if (questiongroupsou.Count() > 0)
                        {
                            foreach (var questionsource in questiongroupsou)
                            {

                                var checkexist = (from tq in entity.TestQuestions
                                                  where tq.OrganizationID == orgid
                                                  && tq.TestId == testid
                                                  && tq.TestSectionId == testid
                                                  && tq.QuestionId == questionsource.QuestionId
                                                  select tq).FirstOrDefault();

                                if (checkexist == null)
                                {


                                    testquestion = new TestQuestion();
                                    testquestion.Id = Guid.NewGuid().ToString();
                                    testquestion.TestId = testid;
                                    testquestion.TestSectionId = testsection;
                                    testquestion.QuestionId = questionsource.QuestionId;
                                    testquestion.SequenceNo = count;
                                    testquestion.OrganizationID = orgid;
                                    testquestion.RowState = true;
                                    testquestion.CreateDateTime = System.DateTime.Now;
                                    entity.Entry(testquestion).State = System.Data.Entity.EntityState.Added;
                                    entity.TestQuestions.Add(testquestion);
                                    count = count + 1;
                                }
                                else
                                {

                                    entity.Entry(checkexist).State = System.Data.Entity.EntityState.Modified;
                                }
                            }
                        }
                    }
                    #endregion
                }
                result = entity.SaveChanges();
            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion


    }
    public class TestQuestionList
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
    }
    public class UniqueQuestionlist
    {
        public string Id { get; set; }
        public string QuestionText { get; set; }
        public bool Selected { get; set; }
        public DateTime CreatDate { get; set; }
        public string Operation { get; set; }
    }
    public class GroupQuestionlist
    {
        public string Id { get; set; }
        public string QuestionText { get; set; }
        public bool Selected { get; set; }
        public int NoOfQuestion { get; set; }
        public DateTime CreatDate { get; set; }
        public string Operation { get; set; }
    }
    public class UniqueQuestionIds
    {
        public string Id { get; set; }
    }
    public class GroupQuestionIds
    {
        public string Id { get; set; }
    }

}