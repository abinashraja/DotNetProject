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
    public class TestSectionController : BaseController
    {

        #region TestSection Index
        [Authorize]
        [CustomFilter(PageName = "TestSection")]
        public ActionResult Index()
        {

            return View("TestSection");
        }
        #endregion

        #region Get TestSection List
        [Authorize]
        public JsonResult GetTestSectionList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            List<TestSectionList> org = new List<TestSectionList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.TestSections
                       join p in entity.Tests on o.ParentId equals p.Id
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.TestSectionCode.ToLower().Contains(searchtext.ToLower())
                       || o.TestSectionName.ToLower().Contains(searchtext.ToLower())
                       || p.TestName.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new TestSectionList
                       {
                           Id = o.Id,
                           TestSectionCode = o.TestSectionCode,
                           TestSectionName = o.TestSectionName,
                           parentId = o.ParentId,
                           parentName = p.TestName,
                           Operation = "Create",
                           DeleteConformation = false,
                           IsTestPublish=p.IsPublish,
                           CreatedDateTime=p.CreateDateTime
                       }).OrderByDescending(x=>x.CreatedDateTime).ToList();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create TestSection 
        [Authorize]
        public JsonResult SaveTestSection(EPortal.Models.TestSection TestSectionInfo)
        {
            string errormsg = "";
            int result = 0;

           // if ((TestSectionInfo.TestSectionCode != "" || TestSectionInfo.TestSectionCode != null) && (TestSectionInfo.TestSectionName != "" || TestSectionInfo.TestSectionName != null))
            {
                //string orgid = Session["OrgId"].ToString();
                string orgid = User.OrgId;

                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (TestSectionInfo.Operation == "Create")
                    {
                        var checkdublivate = (from ts in entity.TestSections
                                              where ts.OrganizationID == orgid
                                              && ts.TestSectionCode == TestSectionInfo.TestSectionCode
                                              && ts.ParentId == TestSectionInfo.ParentId
                                              select ts).FirstOrDefault();

                        if (checkdublivate == null)
                        {

                            TestSectionInfo.Id = Guid.NewGuid().ToString();
                            TestSectionInfo.OrganizationID = orgid;
                            TestSectionInfo.RowState = true;
                            TestSectionInfo.CreateDateTime = System.DateTime.Now;
                            entity.Entry(TestSectionInfo).State = System.Data.Entity.EntityState.Added;
                            entity.TestSections.Add(TestSectionInfo);
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
                            errormsg = "Test Section already exist with same Code.";
                        }

                    }
                    else
                    {
                        EPortal.Models.TestSection usedata = (from o in entity.TestSections
                                                              where o.OrganizationID == orgid
                                                              && o.Id == TestSectionInfo.Id
                                                              select o
                               ).FirstOrDefault();
                        usedata.TestSectionCode = TestSectionInfo.TestSectionCode;
                        usedata.TestSectionName = TestSectionInfo.TestSectionName;
                        usedata.ParentId = TestSectionInfo.ParentId;

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
            //    if (TestSectionInfo.TestSectionCode != "" || TestSectionInfo.TestSectionCode != null)
            //    {
            //        errormsg = "Please enter Code.";
            //    }
            //    if (TestSectionInfo.TestSectionName != "" || TestSectionInfo.TestSectionName != null)
            //    {
            //        errormsg = "Please enter Name.";
            //    }
            //}

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete TestSection
        [Authorize]
        public JsonResult DeleteTestSection(EPortal.Models.TestSection TestSectioninfo)
        {

            int result = 0;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            string errormsg = string.Empty;
            using (EPortalEntities entity = new EPortalEntities())
            {
                var checkreferance = (from r in entity.TestQuestions
                                      where r.OrganizationID == orgid
                                      && r.TestSectionId == TestSectioninfo.Id
                                      select r).FirstOrDefault();
                if (checkreferance != null)
                {
                    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                }
                else
                {
                    entity.Entry(TestSectioninfo).State = System.Data.Entity.EntityState.Deleted;
                    result = entity.SaveChanges();
                }
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit TestSection
        [Authorize]
        public JsonResult GetTestSectionInfo(EPortal.Models.TestSection TestSectioninfo)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;


            TestSectionList TestSectioninfoTestSection = new TestSectionList();
            List<TestListInfo> testlistinfo = null;
            using (EPortalEntities entity = new EPortalEntities())
            {
                TestSectioninfoTestSection = (from o in entity.TestSections
                                              join p in entity.Tests on o.ParentId equals p.Id
                                              where o.Id == TestSectioninfo.Id
                                                    && o.OrganizationID == orgid
                                              select new TestSectionList
                                              {
                                                  Id = o.Id,
                                                  TestSectionCode = o.TestSectionCode,
                                                  TestSectionName = o.TestSectionName,
                                                  parentId = o.ParentId,
                                                  parentName = p.TestName,
                                                  Operation = "Edit",
                                                  IsTestPublish=p.IsPublish
                                              }).FirstOrDefault();

                testlistinfo = (from t in entity.Tests
                                                   where t.OrganizationID == orgid
                                                   select new TestListInfo
                                                   {
                                                       TestId = t.Id,
                                                       Code = t.TestCode,
                                                       Name = t.TestName,
                                                       Selected = (TestSectioninfoTestSection.parentId == t.Id) ? true : false
                                                   }).ToList();

            }
            return Json(new { editdta = TestSectioninfoTestSection, testlist = testlistinfo }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get All Test
        public JsonResult GetAllTest()
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            List<TestListInfo> testlistinfo = new List<TestListInfo>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                testlistinfo = (from t in entity.Tests
                                where t.OrganizationID == orgid
                                && t.IsPublish==false
                                select new TestListInfo
                                {
                                    TestId = t.Id,
                                    Code = t.TestCode,
                                    Name = t.TestName,
                                    Selected = false
                                }).ToList();

            }

            return Json(testlistinfo, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
    public class TestSectionList
    {
        public string Id { get; set; }
        public string TestSectionCode { get; set; }
        public string TestSectionName { get; set; }
        public string parentId { get; set; }
        public string parentName { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public bool IsTestPublish { get; set; }
        public DateTime CreatedDateTime { get; set; }

    }
    public class TestListInfo
    {
        public string TestId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }

    }

}