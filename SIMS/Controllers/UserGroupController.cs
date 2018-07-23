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
    public class UserGroupController : BaseController
    {
        HomeController homecontroller = new HomeController();

        #region TestSection Index
        [Authorize]
        [CustomFilter(PageName = "UserGroup")]
        public ActionResult Index()
        {

            return View("UserGroup");
        }
        #endregion

        #region Get USer Group List
        [Authorize]
        public JsonResult GetUSerGroupList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            List<UserGroupList> org = new List<UserGroupList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.GroupUsers
                       where o.OganizationId == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.GroupUserCode.ToLower().Contains(searchtext.ToLower())
                       || o.GroupUserName.ToLower().Contains(searchtext.ToLower())))
                       select new UserGroupList
                       {
                           Id = o.Id,
                           UserGroupCode = o.GroupUserCode,
                           UserGroupName = o.GroupUserName,
                           Operation = "Create",
                           DeleteConformation = false,
                           CreatedDateTime = o.createdDate
                       }).OrderByDescending(x => x.CreatedDateTime).ToList();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Applicant List
        public JsonResult GetSourceApplicantList()
        {
            List<ApplicantList> ApplicantList = new List<Controllers.ApplicantList>();
            string orgid = User.OrgId;
            using (EPortalEntities entity = new EPortalEntities())
            {
                ApplicantList = (from o in entity.UserInfoes
                                 where o.OrganizationID == orgid
                                 && o.LogInId != "admin"
                                 select new ApplicantList
                                 {
                                     Id = o.Id,
                                     ApplicantId = o.Id,
                                     ApplicantName = o.Name
                                 }).ToList();
            }


            return Json(ApplicantList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save User Group
        public JsonResult SaveUserGroup(UserGroupList usergroup, List<ApplicantList> applicantlist)
        {

            string orgid = User.OrgId;
            EPortal.Models.GroupUser groupuser = null;
            EPortal.Models.GroupUserApplicant groupuserapplicant = null;
            List<EPortal.Models.GroupUserApplicant> groupuserapplicantlist = new List<GroupUserApplicant>();
            int result = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {
                if (usergroup.Operation != "Edit")
                {
                    #region Save User group
                    groupuser = new GroupUser();
                    groupuser.Id = Guid.NewGuid().ToString();
                    groupuser.GroupUserCode = usergroup.UserGroupCode;
                    groupuser.GroupUserName = usergroup.UserGroupName;
                    groupuser.OganizationId = orgid;
                    groupuser.createdDate = System.DateTime.Now;
                    entity.Entry(groupuser).State = System.Data.Entity.EntityState.Added;
                    entity.GroupUsers.Add(groupuser);
                    foreach (ApplicantList item in applicantlist.ToList())
                    {
                        groupuserapplicant = new GroupUserApplicant();
                        groupuserapplicant.Id = Guid.NewGuid().ToString();
                        groupuserapplicant.GroupUserId = groupuser.Id;
                        groupuserapplicant.ApplicantId = item.ApplicantId;
                        groupuserapplicant.OganizationId = orgid;
                        groupuserapplicant.createdDate = System.DateTime.Now;
                        entity.Entry(groupuserapplicant).State = System.Data.Entity.EntityState.Added;
                        entity.GroupUserApplicants.Add(groupuserapplicant);
                    }
                    #endregion

                }
                else
                {
                    #region Update User Group 
                    groupuser = (from ug in entity.GroupUsers
                                 where ug.OganizationId == orgid
                                 && ug.Id == usergroup.Id
                                 select ug).FirstOrDefault();
                    if (groupuser != null)
                    {
                        groupuserapplicantlist = (from gap in entity.GroupUserApplicants
                                                  where gap.GroupUserId == usergroup.Id
                                                  select gap).ToList();
                        if (groupuserapplicantlist.Count() > 0)
                        {
                            foreach (var item in groupuserapplicantlist)
                            {
                                var itemdata = (from apl in applicantlist
                                                where apl.ApplicantId == item.ApplicantId
                                                select apl).FirstOrDefault();
                                if (itemdata == null)
                                {
                                    entity.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                                }
                                else
                                {
                                    entity.Entry(item).State = System.Data.Entity.EntityState.Modified;
                                }
                            }
                            foreach (var item in applicantlist)
                            {
                                var itemdata = (from apl in groupuserapplicantlist
                                                where apl.ApplicantId == item.ApplicantId
                                                select apl).FirstOrDefault();
                                if (itemdata == null)
                                {
                                    groupuserapplicant = new GroupUserApplicant();
                                    groupuserapplicant.Id = Guid.NewGuid().ToString();
                                    groupuserapplicant.GroupUserId = groupuser.Id;
                                    groupuserapplicant.ApplicantId = item.ApplicantId;
                                    groupuserapplicant.OganizationId = orgid;
                                    groupuserapplicant.createdDate = System.DateTime.Now;
                                    entity.Entry(groupuserapplicant).State = System.Data.Entity.EntityState.Added;
                                    entity.GroupUserApplicants.Add(groupuserapplicant);
                                }
                            }
                        }
                    }
                    #endregion
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

        #region Edit User Group
        public JsonResult EditUserGroup(string usergroupid)
        {
            string orgid = User.OrgId;
            List<ApplicantList> destapplicantlist = new List<ApplicantList>();
            List<ApplicantList> sourceapplicantlist = new List<ApplicantList>();
            List<string> applicantlistids = new List<string>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                destapplicantlist = (from da in entity.GroupUserApplicants
                                     join p in entity.UserInfoes on da.ApplicantId equals p.Id
                                     where da.OganizationId == orgid
                                     && da.GroupUserId == usergroupid
                                     select new ApplicantList
                                     {
                                         Id = da.Id,
                                         ApplicantName = p.Name,
                                         ApplicantId = da.ApplicantId
                                     }).ToList();
                if (destapplicantlist.Count() > 0)
                {
                    applicantlistids = destapplicantlist.Select(x => x.ApplicantId).ToList();
                }

                sourceapplicantlist = (from o in entity.UserInfoes
                                       where o.OrganizationID == orgid
                                       && o.LogInId != "admin"
                                       && (!applicantlistids.Contains(o.Id))
                                       select new ApplicantList
                                       {
                                           Id = o.Id,
                                           ApplicantId = o.Id,
                                           ApplicantName = o.Name
                                       }).ToList();
            }

            return Json(new { dest = destapplicantlist, source = sourceapplicantlist }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete User Group
        public JsonResult DeleteUserGroup(string usergroupid)
        {
            string orgid = User.OrgId;
            int result = 0;
            EPortal.Models.GroupUser groupuser = new GroupUser();
            List<EPortal.Models.GroupUserApplicant> groupuserapplicant = new List<GroupUserApplicant>();
            List<string> applicantids = new List<string>();
            string msg = "";
            using (EPortalEntities entity = new EPortalEntities())
            {
                groupuser = (from da in entity.GroupUsers
                             where da.OganizationId == orgid
                             && da.Id == usergroupid
                             select da).FirstOrDefault();
                if (groupuser != null)
                {
                    groupuserapplicant = (from apl in entity.GroupUserApplicants
                                          where apl.OganizationId == orgid
                                          && apl.GroupUserId == groupuser.Id
                                          select apl).ToList();
                }
                entity.Entry(groupuser).State = System.Data.Entity.EntityState.Deleted;
                if (groupuserapplicant.Count() > 0)
                {
                    applicantids = groupuserapplicant.Select(x => x.ApplicantId).ToList();
                    var checkrecordexistornot = (from t in entity.ApplicantTests
                                                 where t.OrganizationID == orgid
                                                 && (applicantids.Contains(t.ApplicantId))
                                                 select t).ToList();
                    if (checkrecordexistornot.Count() == 0)
                    {
                        foreach (var item in groupuserapplicant)
                        {
                            entity.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                        }
                    }
                    else
                    {
                        msg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                    }


                }
                if (msg == "")
                {
                    result = entity.SaveChanges();
                }

            }

            return Json(new { result = result > 0 ? true : false, errormsg = msg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Test List
        [Authorize]
        public JsonResult GetTestList(UserGroupList usergroup)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            string userid = string.Empty;
            List<string> applicantlist = new List<string>();
            List<TestList> org = new List<TestList>();
            using (EPortalEntities entity = new EPortalEntities())
            {


                org = (from o in entity.Tests
                       where o.OrganizationID == orgid
                       && o.IsPublish == true
                       select new TestList
                       {
                           Id = o.Id,
                           TestCode = o.TestCode,
                           TestName = o.TestName,
                           Selected = false,
                           AlreadyApplied = false
                       }).ToList();
                applicantlist = (from u in entity.GroupUserApplicants
                                 where u.OganizationId == orgid
                                 && u.GroupUserId == usergroup.Id
                                 select u.ApplicantId).ToList();

                if (org.Count() > 0)
                {
                    List<string> checkselected = null;
                    List<string> checkapplied = null;
                    foreach (var item in org)
                    {
                        checkselected = new List<string>();
                        checkapplied = new List<string>();

                        foreach (var applicant in applicantlist)
                        {
                            var checkselectedapp = (from at in entity.ApplicantTests
                                                    where at.OrganizationID == orgid
                                                    && at.TestId == item.Id
                                                    && at.ApplicantId == applicant
                                                    //&& at.RowState == true
                                                    select at.ApplicantId).FirstOrDefault();
                            if (checkselectedapp != null)
                            {
                                checkselected.Add(checkselectedapp);
                            }
                            var checkappliedapp = (from aa in entity.UserAnswers
                                                   where aa.OrganizationID == orgid
                                                   && aa.ApplicantId == applicant
                                                   && aa.TestId == item.Id
                                                   select aa.ApplicantId).FirstOrDefault();
                            if (checkappliedapp != null)
                            {
                                checkapplied.Add(checkappliedapp);
                            }
                        }

                        if (checkselected.Count() == applicantlist.Count())
                        {
                            item.Selected = true;

                        }
                        if (checkapplied.Count() == applicantlist.Count())
                        {
                            item.AlreadyApplied = true;
                        }

                    }
                }






            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Assign Test User
        public JsonResult AssignTestUser(string groupid, string testid)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            bool sendmailper = false;
            string usermail = string.Empty;
            List<string> applicantlist = new List<string>();
            EPortal.Models.ApplicantTest apptest = null;
            int result = 0;
            List<string> usermaillist = new List<string>();
            using (EPortalEntities entity = new EPortalEntities())
            {

                applicantlist = (from u in entity.GroupUserApplicants
                                 where u.OganizationId == orgid
                                 && u.GroupUserId == groupid
                                 select u.ApplicantId).ToList();
                if (applicantlist.Count() > 0)
                {
                    foreach (var applicantid in applicantlist)
                    {
                        var checkuserexam = (from at in entity.ApplicantTests
                                             where at.OrganizationID == orgid
                                             && at.ApplicantId == applicantid
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
                        apptest.ApplicantId = applicantid;
                        apptest.OrganizationID = orgid;
                        apptest.RowState = true;
                        apptest.CreateDateTime = System.DateTime.Now;
                        entity.Entry(apptest).State = System.Data.Entity.EntityState.Added;
                        entity.ApplicantTests.Add(apptest);
                        result = entity.SaveChanges();
                        if (result > 0)
                        {
                            result = 0;
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
                                                       && u.Id == applicantid
                                                       select u).FirstOrDefault();
                                    if (getusermail.Email != null && getusermail.Email != string.Empty)
                                    {
                                        usermaillist.Add(getusermail.Email);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            if (sendmailper == true && usermaillist.Count() > 0)
            {
                string body = "One new Test is assign to you,please check.";
                string heading = "New test assign";
                foreach (var item in usermaillist)
                {
                    bool sendmail = homecontroller.SendMail(item, heading, body, null);
                }

            }

            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
    public class UserGroupList
    {
        public string Id { get; set; }
        public string UserGroupCode { get; set; }
        public string UserGroupName { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public List<ApplicantList> ApplicantList { get; set; }


    }
    public class ApplicantList
    {
        public string Id { get; set; }
        public string ApplicantId { get; set; }
        public string ApplicantName { get; set; }

    }

}