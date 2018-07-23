using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Models;
using EPortal.Utility;
using EPortal.App_Start;
using System.Data;

namespace EPortal.Controllers
{
    public class QuestionController : BaseController
    {

        #region Question Index
        [Authorize]
        [CustomFilter(PageName = "Question")]
        public ActionResult Index()
        {
            EPortal.Models.Question question = new Question();
            return View("Question");
        }

        #endregion

        #region Save Question
        public JsonResult SaveQuestion(List<QuestionOptionList> optionlist, Questionobj question)
        {
            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            EPortal.Models.Question questionmodel = new Question();
            EPortal.Models.QuestionOption questionoptionmodel = null;
            List<EPortal.Models.QuestionOption> questionoptionmodellist = new List<QuestionOption>();
            EPortal.Models.QuestionAnswar questionanswer = null;
            EPortal.Models.QuestionsourceGroup questionsourcegroup = null;
            List<EPortal.Models.QuestionAnswar> questionanswerlist = new List<QuestionAnswar>();

            using (EPortalEntities entity = new EPortalEntities())
            {
                if (question.QuestionId == "" || question.QuestionId == null)
                {

                    #region For Insert
                    questionmodel.Id = Guid.NewGuid().ToString();
                    questionmodel.Question1 = question.QuestionInfo;
                    questionmodel.QuestionTypeId = question.QuestionTyeId;
                    questionmodel.QuestionMarks = question.QuestionMark;
                    questionmodel.HaveMultiAns = question.HaveMultiAns;
                    if (question.QuestionSourceId != "0")
                    {
                        questionmodel.SourceId = question.QuestionSourceId;
                    }
                    else
                    {
                        questionmodel.SourceId = null;
                    }
                    questionmodel.OrganizationID = orgid;
                    questionmodel.RowState = true;
                    questionmodel.CreateDateTime = DateTime.Now;
                    entity.Entry(questionmodel).State = System.Data.Entity.EntityState.Added;
                    entity.Questions.Add(questionmodel);

                    if (question.QuestionSourceId != "0" && question.QuestionSourceId != null)
                    {
                        questionsourcegroup = new QuestionsourceGroup();
                        questionsourcegroup.Id = Guid.NewGuid().ToString();
                        questionsourcegroup.QuestionId = questionmodel.Id;
                        questionsourcegroup.QuestionSourceId = question.QuestionSourceId;
                        questionsourcegroup.OrganizationID = orgid;
                        questionsourcegroup.RowState = true;
                        questionsourcegroup.CreateDateTime = DateTime.Now;
                        entity.Entry(questionsourcegroup).State = System.Data.Entity.EntityState.Added;
                        entity.QuestionsourceGroups.Add(questionsourcegroup);
                    }

                    int count = 0;
                    foreach (QuestionOptionList item in optionlist)
                    {
                        questionoptionmodel = new QuestionOption();
                        questionoptionmodel.Id = Guid.NewGuid().ToString();
                        questionoptionmodel.QuestionId = questionmodel.Id;
                        questionoptionmodel.QuestionOption1 = item.OptionText;
                        questionoptionmodel.SequenceNo = count;
                        questionoptionmodel.OrganizationID = orgid;
                        questionoptionmodel.RowState = true;
                        questionoptionmodel.CreateDateTime = DateTime.Now;
                        entity.Entry(questionoptionmodel).State = System.Data.Entity.EntityState.Added;
                        entity.QuestionOptions.Add(questionoptionmodel);
                        if (item.Selected == true)
                        {
                            questionanswer = new QuestionAnswar();
                            questionanswer.Id = Guid.NewGuid().ToString();
                            questionanswer.QuestionId = questionmodel.Id;
                            questionanswer.QuestionAnswarId = questionoptionmodel.Id;
                            questionanswer.OrganizationID = orgid;
                            questionanswer.RowState = true;
                            questionanswer.CreateDateTime = DateTime.Now;
                            entity.Entry(questionanswer).State = System.Data.Entity.EntityState.Added;
                            entity.QuestionAnswars.Add(questionanswer);
                        }
                        count = count + 1;

                    }
                    #endregion
                }
                else
                {
                    questionmodel = (from q in entity.Questions
                                     where q.OrganizationID == orgid
                                     && q.Id == question.QuestionId
                                     select q).FirstOrDefault();
                    if (questionmodel != null)
                    {
                        questionmodel.Question1 = question.QuestionInfo;
                        questionmodel.QuestionMarks = question.QuestionMark;
                        questionmodel.HaveMultiAns = question.HaveMultiAns;
                        entity.Entry(questionmodel).State = System.Data.Entity.EntityState.Modified;
                    }
                    int count = 0;
                    foreach (var item in optionlist)
                    {
                        var existdata = (from qo in entity.QuestionOptions
                                         where qo.OrganizationID == orgid
                                         && qo.QuestionId == question.QuestionId
                                         && qo.Id == item.OptionId
                                         select qo).FirstOrDefault();
                        if (existdata != null)
                        {
                            count = existdata.SequenceNo.HasValue ? existdata.SequenceNo.Value : 0;
                            existdata.QuestionOption1 = item.OptionText;
                            entity.Entry(existdata).State = System.Data.Entity.EntityState.Modified;

                            var checkans = (from qa in entity.QuestionAnswars
                                            where qa.OrganizationID == orgid
                                            && qa.QuestionId == question.QuestionId
                                            && qa.QuestionAnswarId == item.OptionId
                                            select qa).FirstOrDefault();
                            if (checkans != null)
                            {
                                if (item.Selected == true)
                                {
                                    entity.Entry(checkans).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    entity.Entry(checkans).State = System.Data.Entity.EntityState.Deleted;
                                }
                            }
                            else
                            {
                                if (item.Selected == true)
                                {
                                    questionanswer = new QuestionAnswar();
                                    questionanswer.Id = Guid.NewGuid().ToString();
                                    questionanswer.QuestionId = question.QuestionId;
                                    questionanswer.QuestionAnswarId = item.OptionId;
                                    questionanswer.OrganizationID = orgid;
                                    questionanswer.RowState = true;
                                    questionanswer.CreateDateTime = DateTime.Now;
                                    entity.Entry(questionanswer).State = System.Data.Entity.EntityState.Added;
                                    entity.QuestionAnswars.Add(questionanswer);
                                }
                            }
                        }
                        else
                        {

                            questionoptionmodel = new QuestionOption();
                            questionoptionmodel.Id = Guid.NewGuid().ToString();
                            questionoptionmodel.QuestionId = questionmodel.Id;
                            questionoptionmodel.QuestionOption1 = item.OptionText;
                            questionoptionmodel.SequenceNo = count + 1;
                            questionoptionmodel.OrganizationID = orgid;
                            questionoptionmodel.RowState = true;
                            questionoptionmodel.CreateDateTime = DateTime.Now;
                            entity.Entry(questionoptionmodel).State = System.Data.Entity.EntityState.Added;
                            entity.QuestionOptions.Add(questionoptionmodel);
                            if (item.Selected == true)
                            {
                                questionanswer = new QuestionAnswar();
                                questionanswer.Id = Guid.NewGuid().ToString();
                                questionanswer.QuestionId = questionmodel.Id;
                                questionanswer.QuestionAnswarId = questionoptionmodel.Id;
                                questionanswer.OrganizationID = orgid;
                                questionanswer.RowState = true;
                                questionanswer.CreateDateTime = DateTime.Now;
                                entity.Entry(questionanswer).State = System.Data.Entity.EntityState.Added;
                                entity.QuestionAnswars.Add(questionanswer);
                            }
                            count = count + 1;

                        }

                    }


                }


                result = entity.SaveChanges();
            }
            return Json(result > 0 ? true : false, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Source
        public JsonResult GetSource(string questiontypeid)
        {
            //string orgid = Session["OrgId"].ToString();

            string orgid = User.OrgId;
            List<QuestionTypeliSt> questiontypelist = new List<QuestionTypeliSt>();
            List<Sourcelist> sourcelist = new List<Sourcelist>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                sourcelist = (from qt in entity.Questionsources
                              where qt.OrganizationID == orgid
                              && qt.QuestionTypeId == questiontypeid
                              select new Sourcelist
                              {
                                  SourceId = qt.Id,
                                  SourceCode = qt.SourceCode,
                                  SourceName = qt.SourceName
                              }).ToList();
            }
            sourcelist.Insert(0, new Sourcelist { SourceId = "0", SourceCode = "Select", SourceName = "Select" });
            return Json(sourcelist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get AllQuestion Type
        public JsonResult GetAllQuestionType()
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<QuestionTypeliSt> questiontypelist = new List<QuestionTypeliSt>();
            List<Sourcelist> sourcelist = new List<Sourcelist>();
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
            sourcelist.Insert(0, new Sourcelist { SourceId = "0", SourceCode = "Select", SourceName = "Select" });
            return Json(new { qtype = questiontypelist, sourlist = sourcelist }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get All Question
        public JsonResult GetAllQuestion(string searchtext)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<QuestionList> questionlist = new List<QuestionList>();
            using (EPortalEntities entity = new EPortalEntities())
            {
                questionlist = (from q in entity.Questions
                                where q.OrganizationID == orgid
                                //&& ((searchtext == null || searchtext == "") ? true : (q.Question1.ToLower().Contains(searchtext.ToLower())
                                //))
                                select new QuestionList
                                {
                                    Id = q.Id,
                                    QuestionText = q.Question1,
                                    Operation = "Create",
                                    DeleteConformation = false
                                }).ToList();
            }
            return Json(questionlist, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Question detail
        public JsonResult GetQuestionDetail(string quesid)
        {
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            List<QuestionOptionList> optionlist = new List<QuestionOptionList>();
            List<Sourcelist> sourcelist = new List<Sourcelist>();
            string question = string.Empty;
            string questiontypeid = string.Empty;
            string sourceid = string.Empty;
            int questionmarks = 0;
            using (EPortalEntities entity = new EPortalEntities())
            {
                optionlist = (from qo in entity.QuestionOptions
                              where qo.OrganizationID == orgid
                              && qo.QuestionId == quesid
                              select new QuestionOptionList
                              {
                                  OptionText = qo.QuestionOption1,
                                  OptionId = qo.Id,
                                  sequenceno = qo.SequenceNo.HasValue ? qo.SequenceNo.Value : 0,
                                  Selected = (from qa in entity.QuestionAnswars
                                              where qa.OrganizationID == orgid
                                              && qa.QuestionId == quesid
                                              && qa.QuestionAnswarId == qo.Id
                                              select qo).FirstOrDefault() == null ? false : true

                              }).OrderBy(x => x.sequenceno).ToList();
                var questiondata = (from q in entity.Questions
                                    where q.OrganizationID == orgid
                                    && q.Id == quesid
                                    select q).FirstOrDefault();
                question = questiondata.Question1;
                questionmarks = questiondata.QuestionMarks;

                sourcelist = (from so in entity.Questionsources
                              where so.OrganizationID == orgid
                              && so.QuestionTypeId == questiondata.QuestionTypeId
                              select new Sourcelist
                              {
                                  SourceId = so.Id,
                                  SourceCode = so.SourceCode,
                                  SourceName = so.SourceName
                              }).ToList();
                questiontypeid = questiondata.QuestionTypeId;
                sourceid = questiondata.SourceId;

            }

            return Json(new { optionlist = optionlist, questiontxt = question, sourcelist = sourcelist, questiontype = questiontypeid, sourceid = sourceid, Questionmarks = questionmarks }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Delete Question
        public JsonResult DeleteQuestion(QuestionList question)
        {
            int result = 0;
            //string orgid = Session["OrgId"].ToString();
            string orgid = User.OrgId;
            string errormsg = string.Empty;
            using (EPortalEntities entity = new EPortalEntities())
            {

                var checkquestionuse = (from tq in entity.TestQuestions
                                        where tq.OrganizationID == orgid
                                        && tq.QuestionId == question.Id
                                        select tq).FirstOrDefault();
                if (checkquestionuse == null)
                {

                    var getquestion = (from q in entity.Questions
                                       where q.OrganizationID == orgid
                                       && q.Id == question.Id
                                       select q).FirstOrDefault();
                    if (getquestion != null)
                    {
                        var questionoptionlist = (from qo in entity.QuestionOptions
                                                  where qo.OrganizationID == orgid
                                                  && qo.QuestionId == getquestion.Id
                                                  select qo).ToList();
                        if (questionoptionlist.Count() > 0)
                        {
                            foreach (var item in questionoptionlist)
                            {
                                entity.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                            }
                        }

                        var questionoptionanslist = (from qo in entity.QuestionAnswars
                                                     where qo.OrganizationID == orgid
                                                     && qo.QuestionId == getquestion.Id
                                                     select qo).ToList();
                        if (questionoptionanslist.Count() > 0)
                        {
                            foreach (var item in questionoptionanslist)
                            {
                                entity.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                            }
                        }

                        var questiongroup = (from qo in entity.QuestionsourceGroups
                                             where qo.OrganizationID == orgid
                                             && qo.QuestionId == getquestion.Id
                                             select qo).ToList();
                        if (questiongroup.Count() > 0)
                        {
                            foreach (var item in questiongroup)
                            {
                                entity.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                            }
                        }

                        entity.Entry(getquestion).State = System.Data.Entity.EntityState.Deleted;
                        try
                        {
                            result = entity.SaveChanges();
                        }
                        catch (Exception ex)
                        {

                        }
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

        #region upload FIle 
        [HttpPost]
        public JsonResult fileUpload(string questiontypeid)
        {

            string errormsg = string.Empty;
            int resultforsave = 0;
            var data = Request.Files[0];            
            bool fileerror = false;
            if (!Request.Files[0].ContentType.Contains("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"))
            {
                errormsg = "Please select Excel file only.";
                fileerror = true;
            }
            if (fileerror == false)
            {
                #region Excel FIle upload
                //string orgid = Session["OrgId"].ToString();
                string orgid = User.OrgId;
                string path = string.Empty;
                //FileStream stream = new FileStream(data.FileName, FileMode.Open, FileAccess.Read);
                Excel.IExcelDataReader excelReader;
                excelReader = Excel.ExcelReaderFactory.CreateOpenXmlReader(data.InputStream);
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet result = excelReader.AsDataSet();

                EPortal.Models.Question question = null;
                EPortal.Models.QuestionOption questionoption = null;
                EPortal.Models.QuestionAnswar questionanswar = null;
                EPortal.Models.QuestionsourceGroup questionsourcegroup = null;
                List<EPortal.Models.Question> questionlist = new List<Question>();
                List<EPortal.Models.QuestionOption> questionoptionlist = new List<QuestionOption>();
                List<EPortal.Models.QuestionAnswar> questionanswarlist = new List<QuestionAnswar>();
                List<EPortal.Models.QuestionsourceGroup> questionsourcegoruplist = new List<QuestionsourceGroup>();

                using (EPortalEntities entity = new EPortalEntities())
                {
                    if (result.Tables.Count > 0)
                    {
                        string sourceid = string.Empty;
                        string questiontext = string.Empty;
                        foreach (var item in result.Tables[0].Rows)
                        {
                            var source = ((System.Data.DataRow)item).ItemArray[0].ToString();


                            if (source != "")
                            {
                                var sourceobj = (from s in entity.Questionsources
                                                 where s.QuestionTypeId == questiontypeid
                                                 && s.OrganizationID == orgid
                                                 && s.SourceCode == source
                                                 select s).FirstOrDefault();
                                if (sourceobj != null)
                                {
                                    sourceid = sourceobj.Id;


                                }
                                else
                                {
                                    sourceid = "";
                                    errormsg = "One or more source is invalid.";
                                }
                            }




                            #region Question

                            questiontext = ((System.Data.DataRow)item).ItemArray[1].ToString();
                            question = new Question();
                            question.Id = Guid.NewGuid().ToString();
                            question.Question1 = questiontext;
                            question.QuestionTypeId = questiontypeid;
                            question.SourceId = sourceid == "" ? null : sourceid;

                            int questionmarksindex = ((System.Data.DataRow)item).ItemArray.Count() - 1;
                            var questionmarks = ((System.Data.DataRow)item).ItemArray[questionmarksindex].ToString();


                            question.QuestionMarks = questionmarks == "" ? 1 : Convert.ToInt16(questionmarks);
                            question.OrganizationID = orgid;
                            question.RowState = true;
                            question.CreateDateTime = System.DateTime.Now;
                            entity.Questions.Add(question);
                            //questionlist.Add(question);
                            #endregion



                            #region Question Source 
                            if (sourceid != "")
                            {
                                questionsourcegroup = new QuestionsourceGroup();
                                questionsourcegroup.Id = Guid.NewGuid().ToString();
                                questionsourcegroup.QuestionId = question.Id;
                                questionsourcegroup.QuestionSourceId = sourceid;
                                questionsourcegroup.OrganizationID = orgid;
                                questionsourcegroup.RowState = true;
                                questionsourcegroup.CreateDateTime = System.DateTime.Now;
                                entity.QuestionsourceGroups.Add(questionsourcegroup);
                                //questionsourcegoruplist.Add(questionsourcegroup);
                            }

                            #endregion


                            if (questiontext == "")
                            {
                                errormsg = "One or more Question is invalid.";
                            }
                            int columncount = ((System.Data.DataRow)item).ItemArray.Count() - 2;
                            int count = 1;
                            for (int i = 2; i <= columncount - 1; i++)
                            {
                                
                                string optiontext = ((System.Data.DataRow)item).ItemArray[i].ToString();
                                if(optiontext!="")
                                {
                                    #region Question Option 
                                    questionoption = new QuestionOption();
                                    questionoption.Id = Guid.NewGuid().ToString();
                                    questionoption.QuestionId = question.Id;
                                    questionoption.QuestionOption1 = optiontext;
                                    questionoption.OrganizationID = orgid;
                                    questionoption.RowState = true;
                                    questionoption.CreateDateTime = System.DateTime.Now;
                                    questionoption.SequenceNo = count;
                                    entity.QuestionOptions.Add(questionoption);
                                    //questionoptionlist.Add(questionoption);
                                    #endregion

                                    #region Question Answar
                                    int questionanansindex = ((System.Data.DataRow)item).ItemArray.Count() - 2;
                                    string anstext = ((System.Data.DataRow)item).ItemArray[questionanansindex].ToString();
                                    if ((Convert.ToInt16(anstext)+1) == i)
                                    {
                                        questionanswar = new QuestionAnswar();
                                        questionanswar.Id = Guid.NewGuid().ToString();
                                        questionanswar.QuestionId = question.Id;
                                        questionanswar.QuestionAnswarId = questionoption.Id;
                                        questionanswar.OrganizationID = orgid;
                                        questionanswar.RowState = true;
                                        questionanswar.CreateDateTime = System.DateTime.Now;
                                        entity.QuestionAnswars.Add(questionanswar);
                                        //questionanswarlist.Add(questionanswar);
                                    }
                                    #endregion

                                    count = count + 1;

                                }
                               
                            }


                        }
                    }
                    else
                    {
                        errormsg = "Selected file is empty.No Data found.";
                    }

                    if (errormsg == string.Empty)
                    {
                        resultforsave = entity.SaveChanges();
                    }
                }
                #endregion
            }
            return Json(new { result = resultforsave > 0 ? true : false, errormsg = errormsg }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        





    }
    public class QuestionOptionList
    {
        public string OptionId { get; set; }
        public string OptionText { get; set; }
        public bool Selected { get; set; }
        public int? sequenceno { get; set; }
    }
    public class Questionobj
    {
        public string QuestionInfo { get; set; }
        public string QuestionTyeId { get; set; }
        public string QuestionSourceId { get; set; }
        public int QuestionMark { get; set; }
        public bool HaveMultiAns { get; set; }
        public string QuestionId { get; set; }
    }
    public class Sourcelist
    {
        public string SourceId { get; set; }
        public string SourceCode { get; set; }
        public string SourceName { get; set; }
    }
    public class QuestionList
    {
        public string Id { get; set; }
        public string QuestionText { get; set; }
        public string Operation { get; set; }        
        public bool Selected { get; set; }
        public bool DeleteConformation { get; set; }
    }

}