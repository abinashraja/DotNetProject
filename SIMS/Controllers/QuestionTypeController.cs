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
    public class QuestionTypeController : BaseController
    {

        #region QuestionType Index
        [Authorize]
        [CustomFilter(PageName = "QuestionType")]
        public ActionResult Index()
        {

            return View("QuestionType");
        }
        #endregion

        #region Get QuestionType List
        [Authorize]
        public JsonResult GetQuestionTypeList(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;

            List<QuestionTypeList> org = new List<QuestionTypeList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                org = (from o in entity.QuestionTypes
                       where o.OrganizationID == orgid
                       && ((searchtext == null || searchtext == "") ? true : (o.TypeCode.ToLower().Contains(searchtext.ToLower())
                       || o.TypeName.ToLower().Contains(searchtext.ToLower())                       
                       ))
                       select new QuestionTypeList
                       {
                           Id = o.Id,
                           TypeCode = o.TypeCode,
                           TypeName = o.TypeName,                           
                           Operation = "Create",                           
                           DeleteConformation = false,
                           CreatedDateTime=o.CreateDateTime
                       }).OrderByDescending(x=>x.CreatedDateTime).ToList();
            }
            return Json(org, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Create QuestionType 
        [Authorize]
        public JsonResult SaveQuestionType(EPortal.Models.QuestionType QuestionTypeInfo)
        {
            string errormsg = "";
            int result = 0;

            //if ((QuestionTypeInfo.TypeCode != "" || QuestionTypeInfo.TypeCode != null) && (QuestionTypeInfo.TypeName != "" || QuestionTypeInfo.TypeName != null))
            {

                //string orgid = Session["OrgId"].ToString();
                string orgid = User.OrgId;

                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (QuestionTypeInfo.Operation == "Create")
                    {

                        var checkquestiontype = (from qt in entity.QuestionTypes
                                                 where qt.OrganizationID == orgid
                                                 && qt.TypeCode == QuestionTypeInfo.TypeCode
                                                 select qt).FirstOrDefault();
                        if (checkquestiontype == null)
                        {
                            QuestionTypeInfo.Id = Guid.NewGuid().ToString();
                            QuestionTypeInfo.OrganizationID = orgid;
                            QuestionTypeInfo.RowState = true;
                            QuestionTypeInfo.CreateDateTime = System.DateTime.Now;
                            entity.Entry(QuestionTypeInfo).State = System.Data.Entity.EntityState.Added;
                            entity.QuestionTypes.Add(QuestionTypeInfo);

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
                            errormsg = "Question Type already exist with same Code.";
                        }
                    }
                    else
                    {
                        EPortal.Models.QuestionType usedata = (from o in entity.QuestionTypes
                                                           where o.OrganizationID == orgid
                                                           && o.Id == QuestionTypeInfo.Id
                                                           select o
                               ).FirstOrDefault();
                        usedata.TypeCode = QuestionTypeInfo.TypeCode;
                        usedata.TypeName = QuestionTypeInfo.TypeName;                       
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
            //    if (QuestionTypeInfo.TypeCode != "" || QuestionTypeInfo.TypeCode != null)
            //    {
            //        errormsg = "Please enter TypeCode.";
            //    }
            //    if (QuestionTypeInfo.TypeName != "" || QuestionTypeInfo.TypeName != null)
            //    {
            //        errormsg = "Please enter Name.";
            //    }                
            //}

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Delete QuestionType
        [Authorize]
        public JsonResult DeleteQuestionType(EPortal.Models.QuestionType QuestionTypeinfo)
        {

            int result = 0;
            string errormsg = string.Empty;
            // validation = EPortal.Utility.Utility.ValidateProperty(orgdata.Code, "Required");
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;

            using (EPortalEntities entity = new EPortalEntities())
            {
                var checkisusedornot = (from ques in entity.Questions
                                        where ques.OrganizationID == orgid
                                        && ques.QuestionTypeId == QuestionTypeinfo.Id
                                        select ques).FirstOrDefault();
                if (checkisusedornot == null)
                {
                    var checkquestionsoirce = (from qs in entity.Questionsources
                                               where qs.OrganizationID == orgid
                                               && qs.QuestionTypeId == QuestionTypeinfo.Id
                                               select qs).FirstOrDefault();
                    if (checkquestionsoirce == null)

                    {
                        entity.Entry(QuestionTypeinfo).State = System.Data.Entity.EntityState.Deleted;
                        result = entity.SaveChanges();
                    }
                    else
                    {
                        errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                    }

                }
                else
                {
                    errormsg = "Operation conflict:Operation cannot be performed.Record already in Used.";
                }



               
            }

            return Json(new { result = result > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit QuestionType
        [Authorize]
        public JsonResult GetQuestionTypeInfo(EPortal.Models.QuestionType QuestionTypeinfo)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;
            QuestionTypeList QuestionTypeinfoQuestionType = new QuestionTypeList();
            using (EPortalEntities entity = new EPortalEntities())
            {
                QuestionTypeinfoQuestionType = (from o in entity.QuestionTypes
                                where o.Id == QuestionTypeinfo.Id
                                && o.OrganizationID == orgid
                                select new QuestionTypeList
                                {
                                    Id = o.Id,
                                    TypeCode = o.TypeCode,
                                    TypeName = o.TypeName,  
                                    Operation = "Edit"
                                }).FirstOrDefault();
            }
            return Json(QuestionTypeinfoQuestionType, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
    public class QuestionTypeList
    {
        public string Id { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }        
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }
        public DateTime CreatedDateTime { get; set; }

    }

}