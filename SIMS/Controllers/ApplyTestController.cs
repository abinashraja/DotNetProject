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
    public class ApplyTestController : BaseController
    {

        #region User Index
        [Authorize]
        [CustomFilter(PageName = "ApplyTest")]
        public ActionResult Index()
        {

            //string userid = Session["UserId"].ToString();
            //string orgid = Session["OrgId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            bool isvalid = true;
            using (EPortalEntities entity = new EPortalEntities())
            {
                isvalid = (from u in entity.UserInfoes
                           where u.OrganizationID == orgid
                           && u.Id == userid
                           select u.NoOfLogin).FirstOrDefault() == null ? true : false;
            }
            if (isvalid == true)
            {
                return View("ApplyTest");
            }
            else
            {
                return View("PageRefresh");
            }
        }
        #endregion

        #region Test Page OPen
        [Authorize]
        [CustomFilter(PageName = "ApplyTest")]
        public ActionResult TestPage()
        {
            int result = 0;
            //string userid = Session["UserId"].ToString();
            //string orgid = Session["OrgId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            using (EPortalEntities entity = new EPortalEntities())
            {
                var userloginupdate = (from u in entity.UserInfoes
                                       where u.OrganizationID == orgid
                                       && u.Id == userid
                                       select u).FirstOrDefault();
                if (userloginupdate != null)
                {
                    if (userloginupdate.NoOfLogin == null)
                    {
                        userloginupdate.NoOfLogin = 1;
                        entity.Entry(userloginupdate).State = System.Data.Entity.EntityState.Modified;
                        result = entity.SaveChanges();
                        if (result > 0)
                        {
                            return View("TestPage");
                        }
                        else
                        {
                            return View("Error");
                        }
                    }
                    else
                    {
                        return View("PageRefresh");
                    }
                }
                else
                {
                    return View("Error");
                }
            }



        }
        #endregion

        #region Get Test Instruction
        public JsonResult GetTestInstruction()
        {
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            bool havetest = false;
            string testinstruction = string.Empty;
            using (EPortalEntities entity = new EPortalEntities())
            {
                var usertest = (from at in entity.ApplicantTests
                                where at.OrganizationID == orgid
                                && at.ApplicantId == userid
                                && at.RowState == true
                                select at).FirstOrDefault();
                if (usertest != null)
                {
                    havetest = true;
                    testinstruction = (from tin in entity.TestInstructions
                                       where tin.OrganizationID == orgid
                                       && tin.TestId == usertest.TestId
                                       select tin.InstructionText).FirstOrDefault();
                }



            }
            return Json(new { havetest = havetest, testinstruction = testinstruction }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Test Section
        public JsonResult GetTestSection()
        {

            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            int hourtime = 0;
            int mintime = 0;
            int totalminute = 0;
            List<TestSectionList> testsectionlist = new List<TestSectionList>();
            using (EPortalEntities entity = new EPortalEntities())
            {

                var gettest = (from t in entity.ApplicantTests
                               where t.OrganizationID == orgid
                               && t.ApplicantId == userid
                               && t.RowState == true
                               select t).FirstOrDefault();
                if (gettest != null)
                {
                    testsectionlist = (from t in entity.TestSections
                                       where t.OrganizationID == orgid
                                       && t.ParentId == gettest.TestId
                                       select new TestSectionList
                                       {
                                           Id = t.Id,
                                           TestSectionCode = t.TestSectionCode,
                                           TestSectionName = t.TestSectionName
                                       }).OrderBy(x => x.TestSectionName).ToList();

                    var usertime = (from t in entity.UserTestTImes
                                    where t.OrganizationID == orgid
                                    && t.TestId == gettest.TestId
                                    && t.ApplicantId == userid
                                    select t).FirstOrDefault();
                    if (usertime == null)
                    {
                        var testtime = (from t in entity.Tests
                                        where t.OrganizationID == orgid
                                        && t.Id == gettest.TestId
                                        select t).FirstOrDefault();
                        if (testtime != null)
                        {
                            hourtime = testtime.HourTime.Value;
                            mintime = testtime.MinTime.Value;
                            totalminute = (60 * (hourtime * 60)) + (60 * mintime);
                        }
                    }
                    else
                    {
                        totalminute = Convert.ToInt16(usertime.QuestionRemainTime);
                    }
                }



            }

            return Json(new { testsectionlist = testsectionlist, totalminute = totalminute }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Test Section Detail
        public JsonResult GetTestSectionDetail(TestSectionList section)
        {
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            TestApplyQuestionList question = null;
            List<TotalQuestionList> totalnoofquestion = new List<TotalQuestionList>();
            using (EPortalEntities entity = new EPortalEntities())
            {

                var gettest = (from t in entity.ApplicantTests
                               where t.OrganizationID == orgid
                               && t.ApplicantId == userid
                               && t.RowState == true
                               select t).FirstOrDefault();

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
                            && tq.TestId == gettest.TestId
                            && tq.SequenceNo == 1
                            && tq.TestSectionId == section.Id
                            select new TestApplyQuestionList
                            {
                                QuestionId = tq.QuestionId,
                                QuestionText = q.Question1,
                                QuestionNo = 1,
                                SourceText = qs != null ? qs.ResourceText : "",
                                HaveMultiAns = (q.HaveMultiAns == null || q.HaveMultiAns == false) ? false : true,
                                TestQuestionoptionList = (from qo in entity.QuestionOptions
                                                          where qo.OrganizationID == orgid
                                                          && qo.QuestionId == tq.QuestionId
                                                          select new TestQuestionoptionList
                                                          {
                                                              OptionId = qo.Id,
                                                              OptionText = qo.QuestionOption1,
                                                              Selected = (from useran in entity.UserAnswers
                                                                          where useran.OrganizationID == orgid
                                                                          && useran.TestId == gettest.TestId
                                                                          && useran.QuestionId == tq.QuestionId
                                                                          && useran.optionId == qo.Id
                                                                          && useran.ApplicantId == userid
                                                                          select useran).FirstOrDefault() == null ? false : true
                                                          }).ToList(),
                            }).FirstOrDefault();

                totalnoofquestion = (from tq in entity.TestQuestions
                                         //join useran in entity.UserAnswers
                                         //on new
                                         //{
                                         //    orgid = tq.OrganizationID,
                                         //    testid = tq.TestId,
                                         //    questionid = tq.QuestionId,
                                         //    userid = userid
                                         //} equals new
                                         //{
                                         //    orgid = useran.OrganizationID,
                                         //    testid = useran.TestId,
                                         //    questionid = useran.QuestionId,
                                         //    userid = useran.ApplicantId
                                         //} into j1
                                         //from useran in j1.DefaultIfEmpty()

                                     join useranmark in entity.UserAnswerMarks
                                   on new
                                   {
                                       orgid = tq.OrganizationID,
                                       testid = tq.TestId,
                                       questionid = tq.QuestionId,
                                       userid = userid
                                   } equals new
                                   {
                                       orgid = useranmark.OrganizationID,
                                       testid = useranmark.TestId,
                                       questionid = useranmark.QuestionId,
                                       userid = useranmark.ApplicantId
                                   } into j2
                                     from useranmark in j2.DefaultIfEmpty()

                                     where tq.OrganizationID == orgid
                                     && tq.TestId == gettest.TestId
                                     && tq.TestSectionId == section.Id
                                     //select new TotalQuestionList
                                     //{
                                     //    Qno = tq.SequenceNo.Value,
                                     //    MarkForView = useran == null ? (useranmark == null ? false : true) : useranmark == null ? false : true,
                                     //    Attended = useran == null ? false : useranmark == null ? true : false,
                                     //    NotAttended = useran == null ? useranmark == null ? true : false : false
                                     //}).OrderBy(x => x.Qno).ToList();

                                     select new TotalQuestionList
                                     {
                                         Qno = tq.SequenceNo.Value,
                                         MarkForView = (from useran in entity.UserAnswers
                                                        where useran.OrganizationID == tq.OrganizationID
                                                        && useran.TestId == tq.TestId
                                                        && useran.QuestionId == tq.QuestionId
                                                        && useran.ApplicantId == userid
                                                        select useran).ToList().Count() == 0 ?
                                                       (useranmark == null ? false : true) :
                                                       (from useranmark in entity.UserAnswerMarks
                                                        where useranmark.OrganizationID == tq.OrganizationID
                                                        && useranmark.TestId == tq.TestId
                                                        && useranmark.QuestionId == tq.QuestionId
                                                        && useranmark.ApplicantId == userid
                                                        select useranmark).ToList().Count() == 0 ? false : true,
                                         Attended = (from useran in entity.UserAnswers
                                                     where useran.OrganizationID == tq.OrganizationID
                                                     && useran.TestId == tq.TestId
                                                     && useran.QuestionId == tq.QuestionId
                                                     && useran.ApplicantId == userid
                                                     select useran).ToList().Count() == 0 ? false : (from useranmark in entity.UserAnswerMarks
                                                                                                     where useranmark.OrganizationID == tq.OrganizationID
                                                                                                     && useranmark.TestId == tq.TestId
                                                                                                     && useranmark.QuestionId == tq.QuestionId
                                                                                                     && useranmark.ApplicantId == userid
                                                                                                     select useranmark).ToList().Count() == 0 ? true : false,
                                         NotAttended = (from useran in entity.UserAnswers
                                                        where useran.OrganizationID == tq.OrganizationID
                                                        && useran.TestId == tq.TestId
                                                        && useran.QuestionId == tq.QuestionId
                                                        && useran.ApplicantId == userid
                                                        select useran).ToList().Count() == 0 ? (from useranmark in entity.UserAnswerMarks
                                                                                                where useranmark.OrganizationID == tq.OrganizationID
                                                                                                && useranmark.TestId == tq.TestId
                                                                                                && useranmark.QuestionId == tq.QuestionId
                                                                                                && useranmark.ApplicantId == userid
                                                                                                select useranmark).ToList().Count() == 0 ? true : false : false
                                     }).OrderBy(x => x.Qno).ToList();
            }
            return Json(new { question = question, totalnoofquestion = totalnoofquestion }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region  Get Question No
        public JsonResult GetQuestionNo(TotalQuestionList qno, TestSectionList section)
        {
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            TestApplyQuestionList question = null;
            List<TotalQuestionList> totalnoofquestion = new List<TotalQuestionList>();
            using (EPortalEntities entity = new EPortalEntities())
            {

                var gettest = (from t in entity.ApplicantTests
                               where t.OrganizationID == orgid
                               && t.ApplicantId == userid
                               && t.RowState == true
                               select t).FirstOrDefault();

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
                            && tq.TestId == gettest.TestId
                            && tq.SequenceNo == qno.Qno
                            && tq.TestSectionId == section.Id
                            select new TestApplyQuestionList
                            {
                                QuestionId = tq.QuestionId,
                                QuestionText = q.Question1,
                                SourceText = qs != null ? qs.ResourceText : "",
                                HaveMultiAns = (q.HaveMultiAns == null || q.HaveMultiAns == false) ? false : true,
                                QuestionNo = tq.SequenceNo,
                                TestQuestionoptionList = (from qo in entity.QuestionOptions
                                                          where qo.OrganizationID == orgid
                                                          && qo.QuestionId == tq.QuestionId
                                                          select new TestQuestionoptionList
                                                          {
                                                              OptionId = qo.Id,
                                                              OptionText = qo.QuestionOption1,
                                                              Selected = (from useran in entity.UserAnswers
                                                                          where useran.OrganizationID == orgid
                                                                          && useran.TestId == gettest.TestId
                                                                          && useran.QuestionId == tq.QuestionId
                                                                          && useran.optionId == qo.Id
                                                                          && useran.ApplicantId == userid
                                                                          select useran).FirstOrDefault() == null ? false : true
                                                          }).OrderBy(x => Guid.NewGuid()).ToList(),
                            }).FirstOrDefault();

                totalnoofquestion = (from tq in entity.TestQuestions
                                         //join useran in entity.UserAnswers
                                         //on new
                                         //{
                                         //    orgid = tq.OrganizationID,
                                         //    testid = tq.TestId,
                                         //    questionid = tq.QuestionId,
                                         //    userid = userid
                                         //} equals new
                                         //{
                                         //    orgid = useran.OrganizationID,
                                         //    testid = useran.TestId,
                                         //    questionid = useran.QuestionId,
                                         //    userid = useran.ApplicantId
                                         //} into j1
                                         //from useran in j1.DefaultIfEmpty()

                                     join useranmark in entity.UserAnswerMarks
                                   on new
                                   {
                                       orgid = tq.OrganizationID,
                                       testid = tq.TestId,
                                       questionid = tq.QuestionId,
                                       userid = userid
                                   } equals new
                                   {
                                       orgid = useranmark.OrganizationID,
                                       testid = useranmark.TestId,
                                       questionid = useranmark.QuestionId,
                                       userid = useranmark.ApplicantId
                                   } into j2
                                     from useranmark in j2.DefaultIfEmpty()

                                     where tq.OrganizationID == orgid
                                     && tq.TestId == gettest.TestId
                                     && tq.TestSectionId == section.Id
                                     //select new TotalQuestionList
                                     //{
                                     //    Qno = tq.SequenceNo.Value,
                                     //    MarkForView = useran == null ? (useranmark == null ? false : true) : useranmark == null ? false : true,
                                     //    Attended = useran == null ? false : useranmark == null ? true : false,
                                     //    NotAttended = useran == null ? useranmark == null ? true : false : false
                                     //}).OrderBy(x => x.Qno).ToList();

                                     select new TotalQuestionList
                                     {
                                         Qno = tq.SequenceNo.Value,
                                         MarkForView = (from useran in entity.UserAnswers
                                                        where useran.OrganizationID == tq.OrganizationID
                                                        && useran.TestId == tq.TestId
                                                        && useran.QuestionId == tq.QuestionId
                                                        && useran.ApplicantId == userid
                                                        select useran).ToList().Count() == 0 ?
                                                       (useranmark == null ? false : true) :
                                                       (from useranmark in entity.UserAnswerMarks
                                                        where useranmark.OrganizationID == tq.OrganizationID
                                                        && useranmark.TestId == tq.TestId
                                                        && useranmark.QuestionId == tq.QuestionId
                                                        && useranmark.ApplicantId == userid
                                                        select useranmark).ToList().Count() == 0 ? false : true,
                                         Attended = (from useran in entity.UserAnswers
                                                     where useran.OrganizationID == tq.OrganizationID
                                                     && useran.TestId == tq.TestId
                                                     && useran.QuestionId == tq.QuestionId
                                                     && useran.ApplicantId == userid
                                                     select useran).ToList().Count() == 0 ? false : (from useranmark in entity.UserAnswerMarks
                                                                                                     where useranmark.OrganizationID == tq.OrganizationID
                                                                                                     && useranmark.TestId == tq.TestId
                                                                                                     && useranmark.QuestionId == tq.QuestionId
                                                                                                     && useranmark.ApplicantId == userid
                                                                                                     select useranmark).ToList().Count() == 0 ? true : false,
                                         NotAttended = (from useran in entity.UserAnswers
                                                        where useran.OrganizationID == tq.OrganizationID
                                                        && useran.TestId == tq.TestId
                                                        && useran.QuestionId == tq.QuestionId
                                                        && useran.ApplicantId == userid
                                                        select useran).ToList().Count() == 0 ? (from useranmark in entity.UserAnswerMarks
                                                                                                where useranmark.OrganizationID == tq.OrganizationID
                                                                                                && useranmark.TestId == tq.TestId
                                                                                                && useranmark.QuestionId == tq.QuestionId
                                                                                                && useranmark.ApplicantId == userid
                                                                                                select useranmark).ToList().Count() == 0 ? true : false : false
                                     }).OrderBy(x => x.Qno).ToList();
            }
            return Json(new { question = question, totalnoofquestion = totalnoofquestion }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save Question Option
        public JsonResult SaveQuestionOption(TestApplyQuestionList Question, TestSectionList section, string type, int ExamTIme)
        {
            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            EPortal.Models.UserAnswer userans = null;
            EPortal.Models.UserAnswerMark useransmark = null;
            EPortal.Models.UserTestTIme usertime = null;
            string questionid = Question.QuestionId;
            using (EPortalEntities entity = new EPortalEntities())
            {
                var gettest = (from t in entity.ApplicantTests
                               where t.OrganizationID == orgid
                               && t.ApplicantId == userid
                               && t.RowState == true
                               select t).FirstOrDefault();

                usertime = (from t in entity.UserTestTImes
                            where t.OrganizationID == orgid
                            && t.TestId == gettest.TestId
                            && t.ApplicantId == userid
                            select t).FirstOrDefault();
                if (usertime == null)
                {
                    usertime = new UserTestTIme();
                    usertime.Id = Guid.NewGuid().ToString();
                    usertime.ApplicantId = userid;
                    usertime.TestId = gettest.TestId;
                    usertime.QuestionRemainTime = ExamTIme.ToString();
                    usertime.OrganizationID = orgid;
                    usertime.RowState = true;
                    usertime.CreateDateTime = System.DateTime.Now;
                    entity.Entry(usertime).State = System.Data.Entity.EntityState.Added;
                    entity.UserTestTImes.Add(usertime);
                }
                else
                {
                    usertime.QuestionRemainTime = ExamTIme.ToString();
                    entity.Entry(usertime).State = System.Data.Entity.EntityState.Modified;
                }
                var checkmark = (from urm in entity.UserAnswerMarks
                                 where urm.OrganizationID == orgid
                                 && urm.QuestionId == Question.QuestionId
                                 && urm.TestId == gettest.TestId
                                 && urm.ApplicantId == userid
                                 select urm).FirstOrDefault();
                if (type == "2")
                {
                    if (checkmark == null)
                    {

                        useransmark = new UserAnswerMark();
                        useransmark.Id = Guid.NewGuid().ToString();
                        useransmark.QuestionId = Question.QuestionId;
                        useransmark.TestId = gettest.TestId;
                        useransmark.ApplicantId = userid;
                        useransmark.OrganizationID = orgid;
                        useransmark.RowState = true;
                        useransmark.CreateDateTime = System.DateTime.Now;
                        entity.Entry(useransmark).State = System.Data.Entity.EntityState.Added;
                        entity.UserAnswerMarks.Add(useransmark);
                    }
                    else
                    {
                        entity.Entry(checkmark).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                else
                {
                    if (checkmark != null)
                    {
                        entity.Entry(checkmark).State = System.Data.Entity.EntityState.Deleted;
                    }
                }
                foreach (var item in Question.TestQuestionoptionList)
                {

                    var checkextst = (from u in entity.UserAnswers
                                      where u.OrganizationID == orgid
                                      && u.TestId == gettest.TestId
                                      && u.QuestionId == questionid
                                      && u.optionId == item.OptionId
                                      && u.ApplicantId == userid
                                      select u).FirstOrDefault();
                    if (checkextst == null)
                    {
                        if (item.Selected == true)
                        {
                            userans = new UserAnswer();
                            userans.Id = Guid.NewGuid().ToString();
                            userans.ApplicantId = userid;
                            userans.TestId = gettest.TestId;
                            userans.QuestionId = Question.QuestionId;
                            userans.optionId = item.OptionId;
                            userans.OrganizationID = orgid;
                            userans.RowState = true;
                            userans.CreateDateTime = System.DateTime.Now;
                            entity.Entry(userans).State = System.Data.Entity.EntityState.Added;
                            entity.UserAnswers.Add(userans);
                        }

                    }
                    else
                    {
                        if (item.Selected == true)
                        {

                            entity.Entry(checkextst).State = System.Data.Entity.EntityState.Modified;

                        }
                        else
                        {
                            entity.Entry(checkextst).State = System.Data.Entity.EntityState.Deleted;
                        }
                    }
                }
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

        #region Reset Question
        public JsonResult ResetQuestionOption(TestApplyQuestionList Question, TestSectionList section, int ExamTIme)
        {
            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            EPortal.Models.UserAnswer userans = null;
            EPortal.Models.UserAnswerMark useransmark = null;
            EPortal.Models.UserTestTIme usertime = null;
            string questionid = Question.QuestionId;
            using (EPortalEntities entity = new EPortalEntities())
            {

                var gettest = (from t in entity.ApplicantTests
                               where t.OrganizationID == orgid
                               && t.ApplicantId == userid
                               && t.RowState == true
                               select t).FirstOrDefault();

                usertime = (from t in entity.UserTestTImes
                            where t.OrganizationID == orgid
                            && t.TestId == gettest.TestId
                            && t.ApplicantId == userid
                            select t).FirstOrDefault();
                if (usertime == null)
                {
                    usertime = new UserTestTIme();
                    usertime.Id = Guid.NewGuid().ToString();
                    usertime.ApplicantId = userid;
                    usertime.TestId = gettest.TestId;
                    usertime.QuestionRemainTime = ExamTIme.ToString();
                    usertime.OrganizationID = orgid;
                    usertime.RowState = true;
                    usertime.CreateDateTime = System.DateTime.Now;
                    entity.Entry(usertime).State = System.Data.Entity.EntityState.Added;
                    entity.UserTestTImes.Add(usertime);
                }
                else
                {
                    usertime.QuestionRemainTime = ExamTIme.ToString();
                    entity.Entry(usertime).State = System.Data.Entity.EntityState.Modified;
                }


                var checkextst = (from u in entity.UserAnswers
                                  where u.OrganizationID == orgid
                                  && u.TestId == gettest.TestId
                                  && u.QuestionId == questionid
                                  && u.ApplicantId == userid
                                  select u).ToList();

                var checkmark = (from urm in entity.UserAnswerMarks
                                 where urm.OrganizationID == orgid
                                 && urm.QuestionId == Question.QuestionId
                                 && urm.TestId == gettest.TestId
                                 && urm.ApplicantId == userid
                                 select urm).FirstOrDefault();

                if (checkextst.Count() > 0)
                {
                    foreach (var item in checkextst)
                    {
                        entity.Entry(item).State = System.Data.Entity.EntityState.Deleted;

                    }
                }
                if (checkmark != null)
                {
                    entity.Entry(checkmark).State = System.Data.Entity.EntityState.Deleted;
                }
                result = entity.SaveChanges();

            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Submit Test
        public JsonResult SubmitTestNew()
        {
            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            List<EPortal.Models.UserAnswer> userans = null;
            EPortal.Models.UserMark usermark = null;
            using (EPortalEntities entity = new EPortalEntities())
            {

                var gettest = (from t in entity.ApplicantTests
                               where t.OrganizationID == orgid
                               && t.ApplicantId == userid
                               && t.RowState == true
                               select t).FirstOrDefault();
                Utility.Utility.TestId = gettest.TestId;


                #region Get TOtal No of  Section for test
                List<TestSectionMarkList> testsectuonlist = new List<TestSectionMarkList>();
                testsectuonlist = (from ts in entity.TestSections
                                   where ts.OrganizationID == orgid
                                   && ts.ParentId == gettest.TestId
                                   select new TestSectionMarkList
                                   {
                                       TestSectionId = ts.Id,
                                       TestSectionMark = 0
                                   }).ToList();
                #endregion


                userans = (from p in entity.UserAnswers
                           where p.OrganizationID == orgid
                           && p.ApplicantId == userid
                           && p.TestId == gettest.TestId
                           select p).ToList();
                if (userans.Count() > 0)
                {
                    decimal mark = 0;
                    foreach (UserAnswer item in userans)
                    {

                        var checkans = (from a in entity.QuestionAnswars
                                        where a.OrganizationID == orgid
                                        && a.QuestionId == item.QuestionId
                                        && a.QuestionAnswarId == item.optionId
                                        select a).FirstOrDefault();

                        var questionmarks = (from qm in entity.Questions
                                             where qm.OrganizationID == orgid
                                             && qm.Id == item.QuestionId
                                             select qm.QuestionMarks).FirstOrDefault();

                        if (checkans != null)
                        {
                            var getquestionsection = (from qts in entity.TestQuestions
                                                      where qts.OrganizationID == orgid
                                                      && qts.TestId == gettest.TestId
                                                      && qts.QuestionId == item.QuestionId
                                                      select qts.TestSectionId).FirstOrDefault();

                            if (getquestionsection != null)
                            {
                                var setmarks = (from s in testsectuonlist
                                                where s.TestSectionId == getquestionsection
                                                select s).FirstOrDefault();
                                if (setmarks != null)
                                {
                                    setmarks.TestSectionMark = setmarks.TestSectionMark + questionmarks;
                                }
                            }
                        }
                    }
                    foreach (var item in testsectuonlist)
                    {
                        usermark = new UserMark();
                        usermark.Id = Guid.NewGuid().ToString();
                        usermark.ApplicantId = userid;
                        usermark.TestId = gettest.TestId;
                        usermark.TestSectionId = item.TestSectionId;
                        usermark.Mark = item.TestSectionMark.ToString();
                        usermark.OrganizationID = orgid;
                        usermark.RowState = true;
                        usermark.CreateDateTime = System.DateTime.Now;
                        entity.Entry(usermark).State = System.Data.Entity.EntityState.Added;
                        entity.UserMarks.Add(usermark);
                    }

                    //Modified Applicant  Test Record after submit the test
                    gettest.RowState = false;
                    entity.Entry(gettest).State = System.Data.Entity.EntityState.Modified;
                }
                result = entity.SaveChanges();
            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Result
        public JsonResult GetResult()
        {
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            string testname = string.Empty;
            List<TestSectionMarkList> useranslist = null;

            using (EPortalEntities entity = new EPortalEntities())
            {

                var gettest = (from t in entity.ApplicantTests
                               where t.OrganizationID == orgid
                               && t.ApplicantId == userid
                               //&& t.RowState==true
                               select t).FirstOrDefault();
                var testnameinfo = (from t in entity.Tests
                                    where t.OrganizationID == orgid
                                    && t.Id == gettest.TestId
                                    //&& t.RowState==true
                                    select t).FirstOrDefault();
                if (testnameinfo != null)
                {
                    testname = testnameinfo.TestName;
                }
                if (gettest != null)
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
                                   && um.TestId == gettest.TestId
                                   && um.ApplicantId == userid
                                   select new TestSectionMarkList
                                   {
                                       TestSectionId = um.TestSectionId,
                                       TestSectionName = s.TestSectionName,
                                       TestSectionMarkStr = um.Mark
                                   }).ToList();
                }
            }
            useranslist.Add(new TestSectionMarkList { TestSectionId = "Total", TestSectionName = "Total", TestSectionMarkStr = useranslist.Sum(x => Convert.ToDecimal(x.TestSectionMarkStr)).ToString() });
            return Json(new { useranslist = useranslist, testname = testname, usename = Session["UserName"].ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Submit Test
        public JsonResult SubmitTest()
        {
            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            //string userid = Session["UserId"].ToString();

            string userid = User.UserId;
            string orgid = User.OrgId;

            List<EPortal.Models.UserAnswer> userans = null;
            EPortal.Models.UserMark usermark = null;
            List<EPortal.Models.TestQuestion> testquestion = new List<TestQuestion>();
            using (EPortalEntities entity = new EPortalEntities())
            {

                var gettest = (from t in entity.ApplicantTests
                               where t.OrganizationID == orgid
                               && t.ApplicantId == userid
                               && t.RowState == true
                               select t).FirstOrDefault();
                Utility.Utility.TestId = gettest.TestId;


                #region Get TOtal No of  Section for test
                List<TestSectionMarkList> testsectuonlist = new List<TestSectionMarkList>();
                testsectuonlist = (from ts in entity.TestSections
                                   where ts.OrganizationID == orgid
                                   && ts.ParentId == gettest.TestId
                                   select new TestSectionMarkList
                                   {
                                       TestSectionId = ts.Id,
                                       TestSectionMark = 0
                                   }).ToList();
                #endregion


                testquestion = (from tq in entity.TestQuestions
                                where tq.OrganizationID == orgid
                                && tq.TestId == gettest.TestId
                                select tq).ToList();

                if (testquestion.Count() > 0)
                {
                    foreach (var item in testquestion)
                    {
                        List<string> questionanslist = (from qa in entity.QuestionAnswars
                                                        where qa.OrganizationID == orgid
                                                        && qa.QuestionId == item.QuestionId
                                                        select qa.QuestionAnswarId).ToList();
                        if (questionanslist.Count() > 0)
                        {
                            int count = 0;
                            foreach (var ans in questionanslist)
                            {
                                var data = (from ua in entity.UserAnswers
                                            where ua.OrganizationID == orgid
                                            && ua.QuestionId == item.QuestionId
                                            && ua.optionId == ans
                                            select ua).FirstOrDefault();
                                if (data != null)
                                {
                                    count = count + 1;
                                }

                            }
                            if (count == (questionanslist.Count()))
                            {

                                var questionmarks = (from qm in entity.Questions
                                                     where qm.OrganizationID == orgid
                                                     && qm.Id == item.QuestionId
                                                     select qm.QuestionMarks).FirstOrDefault();


                                var getquestionsection = (from qts in entity.TestQuestions
                                                          where qts.OrganizationID == orgid
                                                          && qts.TestId == gettest.TestId
                                                          && qts.QuestionId == item.QuestionId
                                                          select qts.TestSectionId).FirstOrDefault();


                                var setmarks = (from s in testsectuonlist
                                                where s.TestSectionId == getquestionsection
                                                select s).FirstOrDefault();
                                if (setmarks != null)
                                {
                                    setmarks.TestSectionMark = setmarks.TestSectionMark + questionmarks;
                                }
                            }
                        }
                    }
                    foreach (var item in testsectuonlist)
                    {
                        usermark = new UserMark();
                        usermark.Id = Guid.NewGuid().ToString();
                        usermark.ApplicantId = userid;
                        usermark.TestId = gettest.TestId;
                        usermark.TestSectionId = item.TestSectionId;
                        usermark.Mark = item.TestSectionMark.ToString();
                        usermark.OrganizationID = orgid;
                        usermark.RowState = true;
                        usermark.CreateDateTime = System.DateTime.Now;
                        entity.Entry(usermark).State = System.Data.Entity.EntityState.Added;
                        entity.UserMarks.Add(usermark);
                    }
                    //Modified Applicant  Test Record after submit the test
                    gettest.RowState = false;
                    entity.Entry(gettest).State = System.Data.Entity.EntityState.Modified;
                }
                result = entity.SaveChanges();

            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
    public class TestApplyQuestionList
    {
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int? QuestionNo { get; set; }
        public string SourceText { get; set; }
        public List<TestQuestionoptionList> TestQuestionoptionList { get; set; }
        public bool HaveMultiAns { get; set; }
    }
    public class TestQuestionoptionList
    {
        public string OptionId { get; set; }
        public string OptionText { get; set; }
        public bool Selected { get; set; }
        public bool QuestionAns { get; set; }
    }
    public class TotalQuestionList
    {
        public int? Qno { get; set; }
        public bool MarkForView { get; set; }
        public bool Attended { get; set; }
        public bool NotAttended { get; set; }

    }
    public class TestSectionMarkList
    {
        public string TestSectionId { get; set; }
        public string TestSectionName { get; set; }
        public decimal TestSectionMark { get; set; }
        public string TestSectionMarkStr { get; set; }

    }



}
