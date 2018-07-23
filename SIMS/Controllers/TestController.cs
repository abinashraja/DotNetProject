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
    public class TestController : BaseController
    {

        #region Test Index
        [Authorize]
        [CustomFilter(PageName = "Test")]
        public ActionResult Index()
        {

            return View("Test");
        }
        #endregion

        #region Get Test List
        [Authorize]
        public JsonResult GetTestList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<TestList> org = new List<TestList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Tests
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.TestCode.ToLower().Contains(searchtext.ToLower())
                       || o.TestName.ToLower().Contains(searchtext.ToLower())
                       || (o.IsPublish == true ? "Yes" : "No").ToLower().Contains(searchtext.ToLower())
                       || (o.Islocked == true ? "Yes" : "No").ToLower().Contains(searchtext.ToLower())
                       ))
                       select new TestList
                       {
                           Id = o.Id,
                           TestCode = o.TestCode,
                           TestName = o.TestName,
                           PeriodFrom = o.PeriodFrom,
                           PeriodTo = o.PeriodTo,
                           Operation = "Create",
                           Islocked = o.Islocked,
                           IsPublish = o.IsPublish,
                           DeleteConformation = false,
                           IsPublishTxt = o.IsPublish == true ? "Yes" : "No",
                           IslockedTxt = o.Islocked == true ? "Yes" : "No",
                           CreatedDateTime = o.CreateDateTime
                       }).OrderByDescending(x => x.CreatedDateTime).ToList();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create Test 
        [Authorize]
        public JsonResult SaveTest(EPortal.Models.Test TestInfo)
        {
            string errormsg = "";
            int result = 0;

            //if ((TestInfo.TestCode != "" || TestInfo.TestCode != null) && (TestInfo.TestName != "" || TestInfo.TestName != null))
            {
                //string orgid = Session["OrgId"].ToString();

                string orgid = User.OrgId;
                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (TestInfo.Operation == "Create")
                    {

                        var checktestcodeexist = (from t in entity.Tests
                                                  where t.OrganizationID == orgid
                                                  && t.TestCode == TestInfo.TestCode
                                                  select t).FirstOrDefault();
                        if (checktestcodeexist == null)
                        {

                            TestInfo.Id = Guid.NewGuid().ToString();
                            TestInfo.OrganizationID = orgid;
                            TestInfo.RowState = true;
                            TestInfo.CreateDateTime = System.DateTime.Now;
                            TestInfo.IsPublish = false;
                            TestInfo.Islocked = false;
                            entity.Entry(TestInfo).State = System.Data.Entity.EntityState.Added;
                            entity.Tests.Add(TestInfo);
                            try
                            {
                                result = entity.SaveChanges();
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            errormsg = "Test already exist with same Code.";
                        }

                    }
                    else
                    {
                        EPortal.Models.Test usedata = (from o in entity.Tests
                                                       where o.OrganizationID == orgid
                                                       && o.Id == TestInfo.Id
                                                       select o
                               ).FirstOrDefault();
                        usedata.TestCode = TestInfo.TestCode;
                        usedata.TestName = TestInfo.TestName;
                        usedata.PeriodFrom = TestInfo.PeriodFrom;
                        usedata.PeriodTo = TestInfo.PeriodTo;
                        usedata.HourTime = TestInfo.HourTime;
                        usedata.MinTime = TestInfo.MinTime;
                        entity.Entry(usedata).State = System.Data.Entity.EntityState.Modified;
                        try
                        {

                            result = entity.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                }
            }
            //else
            //{
            //    if (TestInfo.TestCode != "" || TestInfo.TestCode != null)
            //    {
            //        errormsg = "Please enter Code.";
            //    }
            //    if (TestInfo.TestName != "" || TestInfo.TestName != null)
            //    {
            //        errormsg = "Please enter Name.";
            //    }
            //}

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Delete Test
        [Authorize]
        public JsonResult DeleteTest(EPortal.Models.Test Testinfo)
        {

            int result = 0;
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;
            string errormsg = string.Empty;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {
                var checkreferance = (from r in entity.ApplicantTests
                                      where r.OrganizationID == orgid
                                      && r.TestId == Testinfo.Id
                                      select r).FirstOrDefault();
                if (checkreferance != null)
                {
                    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                }
                else
                {
                    var checktestsectionref = (from tsr in entity.TestSections
                                               where tsr.OrganizationID == orgid
                                               && tsr.ParentId == Testinfo.Id
                                               select tsr).FirstOrDefault();
                    if (checktestsectionref != null)
                    {
                        errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                    }
                    else
                    {
                        entity.Entry(Testinfo).State = System.Data.Entity.EntityState.Deleted;
                        result = entity.SaveChanges();
                    }
                }
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit Test
        [Authorize]
        public JsonResult GetTestInfo(EPortal.Models.Test Testinfo)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            TestList TestinfoTest = new TestList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                TestinfoTest = (from o in entity.Tests
                                where o.Id == Testinfo.Id
                                && o.OrganizationID == orgid
                                select new TestList
                                {
                                    Id = o.Id,
                                    TestCode = o.TestCode,
                                    TestName = o.TestName,
                                    PeriodFrom = o.PeriodFrom,
                                    PeriodTo = o.PeriodTo,
                                    Operation = "Edit",
                                    HourTime = o.HourTime,
                                    MinTime = o.MinTime,
                                    IsPublish = o.IsPublish,
                                    Islocked = o.Islocked

                                }).FirstOrDefault();
            }
            return Json(TestinfoTest, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Publish Test
        public JsonResult PublishTest(TestList test)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            int result = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {
                var testmodel = (from t in entity.Tests
                                 where t.OrganizationID == orgid
                                 && t.Id == test.Id
                                 select t).FirstOrDefault();
                if (testmodel != null)
                {
                    testmodel.IsPublish = true;
                    entity.Entry(testmodel).State = System.Data.Entity.EntityState.Modified;
                }

                result = entity.SaveChanges();
            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Lock Test
        public JsonResult LockTest(TestList test)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            int result = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {
                var testmodel = (from t in entity.Tests
                                 where t.OrganizationID == orgid
                                 && t.Id == test.Id
                                 select t).FirstOrDefault();
                if (testmodel != null)
                {
                    testmodel.Islocked = true;
                    entity.Entry(testmodel).State = System.Data.Entity.EntityState.Modified;
                }

                result = entity.SaveChanges();
            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);

        }
        #endregion
    }
    public class TestList
    {
        public string Id { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public bool IsPublish { get; set; }
        public bool Islocked { get; set; }
        public string IsPublishTxt { get; set; }
        public string IslockedTxt { get; set; }
        public int? ExamTIme { get; set; }
        public int? HourTime { get; set; }
        public int? MinTime { get; set; }
        public bool Selected { get; set; }
        public bool AlreadyApplied { get; set; }
        public string URl { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }

}