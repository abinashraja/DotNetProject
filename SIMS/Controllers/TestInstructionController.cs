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
    public class TestInstructionController : BaseController
    {

        #region TestInstruction Index
        [Authorize]
        [CustomFilter(PageName = "TestInstruction")]
        public ActionResult Index()
        {

            return View("TestInstruction");
        }
        #endregion

        #region Get TestInstruction List
        [Authorize]
        public JsonResult GetTestInstructionList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<TestInstructionList> org = new List<TestInstructionList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.TestInstructions
                       join t in entity.Tests on new
                       {
                           orgid = o.OrganizationID,
                           testid = o.TestId
                       } equals new
                       {
                           orgid = t.OrganizationID,
                           testid = t.Id
                       }
                       where o.OrganizationID == orgid
                      && ((searchtext == null || searchtext == "") ? true : (o.Code.ToLower().Contains(searchtext.ToLower())
                       || o.Name.ToLower().Contains(searchtext.ToLower())
                       || t.TestName.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new TestInstructionList
                       {
                           Id = o.Id,
                           Code = o.Code,
                           Name = o.Name,
                           Operation = "Create",
                           DeleteConformation = false,
                           TestName = t.TestName,
                           IsTestPublish = t.IsPublish
                       }).ToList();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create TestInstruction 
        [Authorize]
        public JsonResult SaveTestInstruction(EPortal.Models.TestInstruction TestInstructionInfo, string sourcetestinfo)
        {
            string errormsg = "";
            int result = 0;

            //if ((TestInstructionInfo.Code != "" || TestInstructionInfo.Code != null) && (TestInstructionInfo.Name != "" || TestInstructionInfo.Name != null))
            {

                //string orgid = Session["OrgId"].ToString();
                string orgid = User.OrgId;
                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (TestInstructionInfo.Operation == "Create")
                    {
                        var checkdub = (from qi in entity.TestInstructions
                                        where qi.OrganizationID == orgid
                                        && qi.TestId == TestInstructionInfo.TestId
                                        && qi.Code == TestInstructionInfo.Code
                                        select qi).FirstOrDefault();
                        if (checkdub == null)
                        {
                            TestInstructionInfo.InstructionText = sourcetestinfo;
                            TestInstructionInfo.Id = Guid.NewGuid().ToString();
                            TestInstructionInfo.OrganizationID = orgid;
                            TestInstructionInfo.RowState = true;
                            TestInstructionInfo.CreateDateTime = System.DateTime.Now;
                            entity.Entry(TestInstructionInfo).State = System.Data.Entity.EntityState.Added;
                            entity.TestInstructions.Add(TestInstructionInfo);

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
                            errormsg = "Code already exist with same Code for selected Test.";

                        }


                        

                    }
                    else
                    {
                        EPortal.Models.TestInstruction usedata = (from o in entity.TestInstructions
                                                                  where o.OrganizationID == orgid
                                                                  && o.Id == TestInstructionInfo.Id
                                                                  select o
                               ).FirstOrDefault();
                        usedata.Code = TestInstructionInfo.Code;
                        usedata.Name = TestInstructionInfo.Name;
                        usedata.InstructionText = sourcetestinfo;
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
            //    if (TestInstructionInfo.Code != "" || TestInstructionInfo.Code != null)
            //    {
            //        errormsg = "Please enter Code.";
            //    }
            //    if (TestInstructionInfo.Name != "" || TestInstructionInfo.Name != null)
            //    {
            //        errormsg = "Please enter Name.";
            //    }
            //    if (TestInstructionInfo.InstructionText != "" || TestInstructionInfo.InstructionText != null)
            //    {
            //        errormsg = "Please enter Instruction.";
            //    }
            //}

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete TestInstruction
        [Authorize]
        public JsonResult DeleteTestInstruction(EPortal.Models.TestInstruction TestInstructioninfo)
        {

            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            string errormsg = string.Empty;
            string testid = string.Empty;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {

                var gettestid = (from t in entity.TestInstructions
                                 where t.OrganizationID == orgid
                                 && t.Id == TestInstructioninfo.Id
                                 select t).FirstOrDefault();
                if (gettestid != null)
                {
                    testid = gettestid.TestId;
                }
                var checkreferance = (from r in entity.Tests
                                      where r.OrganizationID == orgid
                                      && r.Id == testid
                                      select r).FirstOrDefault();
                if (checkreferance != null && checkreferance.IsPublish == true)
                {
                    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                }
                else
                {
                    entity.Entry(TestInstructioninfo).State = System.Data.Entity.EntityState.Deleted;
                    result = entity.SaveChanges();
                }
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit TestInstruction
        [Authorize]
        public JsonResult GetTestInstructionInfo(EPortal.Models.TestInstruction TestInstructioninfo)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<TestList> testlist = new List<TestList>();
            TestInstructionList TestInstructioninfoTestInstruction = new TestInstructionList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                TestInstructioninfoTestInstruction = (from o in entity.TestInstructions
                                                      join t in entity.Tests on new
                                                      {
                                                          orgid = o.OrganizationID,
                                                          testid = o.TestId
                                                      } equals new
                                                      {
                                                          orgid = t.OrganizationID,
                                                          testid = t.Id
                                                      }

                                                      where o.Id == TestInstructioninfo.Id
                                                      && o.OrganizationID == orgid
                                                      select new TestInstructionList
                                                      {
                                                          Id = o.Id,
                                                          Code = o.Code,
                                                          Name = o.Name,
                                                          ResourceText = o.InstructionText,
                                                          TestId = o.TestId,
                                                          Operation = "Edit",
                                                          IsTestPublish = t.IsPublish
                                                      }).FirstOrDefault();

                testlist = (from o in entity.Tests
                            where o.OrganizationID == orgid
                            select new TestList
                            {
                                Id = o.Id,
                                TestCode = o.TestCode,
                                TestName = o.TestName,
                            }).ToList();



            }
            return Json(new { sourcedata = TestInstructioninfoTestInstruction, atypelist = testlist }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Test List
        [Authorize]
        public JsonResult GetTestList()
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<TestList> testlist = new List<TestList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                testlist = (from o in entity.Tests
                            where o.OrganizationID == orgid
                            && o.IsPublish == false
                            select new TestList
                            {
                                Id = o.Id,
                                TestCode = o.TestCode,
                                TestName = o.TestName,
                            }).ToList();
            }
            testlist.Add(new TestList { Id = "0", TestCode = "Select", TestName = "Select" });
            return Json(testlist, JsonRequestBehavior.AllowGet);
        }
        #endregion



    }
    public class TestInstructionList
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ResourceText { get; set; }
        public string TestId { get; set; }
        public string Operation { get; set; }
        public string TestName { get; set; }
        public bool IsTestPublish { get; set; }
        public bool DeleteConformation { get; set; }

    }

}