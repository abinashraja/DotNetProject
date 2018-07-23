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
    public class QuestionSourceController : BaseController
    {

        #region QuestionSource Index
        [Authorize]
        [CustomFilter(PageName = "QuestionSource")]
        public ActionResult Index()
        {

            return View("QuestionSource");
        }
        #endregion

        #region Get QuestionSource List
        [Authorize]
        public JsonResult GetQuestionSourceList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<QuestionSourceList> org = new List<QuestionSourceList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.Questionsources
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.SourceCode.ToLower().Contains(searchtext.ToLower())
                       || o.SourceName.ToLower().Contains(searchtext.ToLower())
                       ))
                       select new QuestionSourceList
                       {
                           Id = o.Id,
                           SourceCode = o.SourceCode,
                           SourceName = o.SourceName,
                           Operation = "Create",
                           DeleteConformation = false,
                           CreatedDateTime = o.CreateDateTime
                       }).OrderByDescending(x => x.CreatedDateTime).ToList();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create QuestionSource 
        [Authorize]
        public JsonResult SaveQuestionSource(EPortal.Models.Questionsource QuestionSourceInfo, string sourcetestinfo)
        {
            string errormsg = "";
            int result = 0;

            //if ((QuestionSourceInfo.SourceCode != "" || QuestionSourceInfo.SourceCode != null) && (QuestionSourceInfo.SourceName != "" || QuestionSourceInfo.SourceName != null))
            {

                //string orgid = Session["OrgId"].ToString();
                string orgid = User.OrgId;
                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (QuestionSourceInfo.Operation == "Create")
                    {

                        var questionsourcedub = (from qs in entity.Questionsources
                                                 where qs.OrganizationID == orgid
                                                 && qs.QuestionTypeId == QuestionSourceInfo.QuestionTypeId
                                                 && qs.SourceCode == QuestionSourceInfo.SourceCode
                                                 select qs).FirstOrDefault();
                        if (questionsourcedub == null)
                        {
                            QuestionSourceInfo.ResourceText = sourcetestinfo;
                            QuestionSourceInfo.Id = Guid.NewGuid().ToString();
                            QuestionSourceInfo.OrganizationID = orgid;
                            QuestionSourceInfo.RowState = true;
                            QuestionSourceInfo.CreateDateTime = System.DateTime.Now;
                            entity.Entry(QuestionSourceInfo).State = System.Data.Entity.EntityState.Added;
                            entity.Questionsources.Add(QuestionSourceInfo);

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
                            errormsg = "Question Source  Code already exist Selected Question Type.";
                        }
                    }
                    else
                    {
                        EPortal.Models.Questionsource usedata = (from o in entity.Questionsources
                                                                 where o.OrganizationID == orgid
                                                                 && o.Id == QuestionSourceInfo.Id
                                                                 select o
                               ).FirstOrDefault();
                        usedata.SourceCode = QuestionSourceInfo.SourceCode;
                        usedata.SourceName = QuestionSourceInfo.SourceName;
                        usedata.ResourceText = sourcetestinfo;
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
            //    if (QuestionSourceInfo.SourceCode != "" || QuestionSourceInfo.SourceCode != null)
            //    {
            //        errormsg = "Please enter SourceCode.";
            //    }
            //    if (QuestionSourceInfo.SourceName != "" || QuestionSourceInfo.SourceName != null)
            //    {
            //        errormsg = "Please enter Name.";
            //    }
            //    if (QuestionSourceInfo.ResourceText != "" || QuestionSourceInfo.ResourceText != null)
            //    {
            //        errormsg = "Please enter Question Source Description.";
            //    }
            //}

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Delete QuestionSource
        [Authorize]
        public JsonResult DeleteQuestionSource(EPortal.Models.Questionsource QuestionSourceinfo)
        {

            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            string errormsg = string.Empty;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");

            using (EPortalEntities entity = new EPortalEntities())
            {
                var checkreferance = (from r in entity.Questions
                                      where r.OrganizationID == orgid
                                      && r.SourceId == QuestionSourceinfo.Id
                                      select r).FirstOrDefault();
                if (checkreferance != null)
                {
                    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                }
                else
                {
                    entity.Entry(QuestionSourceinfo).State = System.Data.Entity.EntityState.Deleted;
                    result = entity.SaveChanges();
                }
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit QuestionSource
        [Authorize]
        public JsonResult GetQuestionSourceInfo(EPortal.Models.Questionsource QuestionSourceinfo)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            List<QuestionTypeliSt> questiontypelist = new List<QuestionTypeliSt>();
            QuestionSourceList QuestionSourceinfoQuestionSource = new QuestionSourceList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                QuestionSourceinfoQuestionSource = (from o in entity.Questionsources

                                                    where o.Id == QuestionSourceinfo.Id
                                                    && o.OrganizationID == orgid
                                                    select new QuestionSourceList
                                                    {
                                                        Id = o.Id,
                                                        SourceCode = o.SourceCode,
                                                        SourceName = o.SourceName,
                                                        ResourceText = o.ResourceText,
                                                        QuestionTypeId = o.QuestionTypeId,
                                                        Operation = "Edit"
                                                    }).FirstOrDefault();
                questiontypelist = (from qt in entity.QuestionTypes
                                    where qt.OrganizationID == orgid
                                    select new QuestionTypeliSt
                                    {
                                        QuestionTypeId = qt.Id,
                                        QuestionTypeCode = qt.TypeCode,
                                        QuestionTypeName = qt.TypeName
                                    }).ToList();
            }
            questiontypelist.Insert(0, new QuestionTypeliSt { QuestionTypeId = "0", QuestionTypeCode = "Select", QuestionTypeName = "Select" });
            return Json(new { sourcedata = QuestionSourceinfoQuestionSource, atypelist = questiontypelist }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Get All Question Type 
        public JsonResult GetAllQuestionType()
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;
            List<QuestionTypeliSt> questiontypelist = new List<QuestionTypeliSt>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                questiontypelist = (from qt in entity.QuestionTypes
                                    where qt.OrganizationID == orgid
                                    select new QuestionTypeliSt
                                    {
                                        QuestionTypeId = qt.Id,
                                        QuestionTypeCode = qt.TypeCode,
                                        QuestionTypeName = qt.TypeName
                                    }).ToList();
            }

            questiontypelist.Insert(0, new QuestionTypeliSt { QuestionTypeId = "0", QuestionTypeCode = "Select", QuestionTypeName = "Select" });
            return Json(questiontypelist, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    public class QuestionSourceList
    {
        public string Id { get; set; }
        public string SourceCode { get; set; }
        public string SourceName { get; set; }
        public string ResourceText { get; set; }
        public string QuestionTypeId { get; set; }
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public DateTime CreatedDateTime { get; set; }

    }
    public class QuestionTypeliSt
    {
        public string QuestionTypeId { get; set; }
        public string QuestionTypeCode { get; set; }
        public string QuestionTypeName { get; set; }
    }

}