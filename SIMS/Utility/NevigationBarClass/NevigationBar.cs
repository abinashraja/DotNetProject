using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using EPortal.Models;
using EPortal.App_Start;
using System.Security.Principal;
using System.Web.Script.Serialization;
using System.Security.Cryptography;

namespace EPortal.Utility.NevigationBarClass
{
    public class NevigationBar 
    {
        //#region Nevigation Bar Markup
        //public string GetMarkup()
        //{
        //    StringBuilder bulder = new StringBuilder();
        //    //string orgid = HttpContext.Current.Session["OrgId"].ToString();
        //    //string roleid = HttpContext.Current.Session["RoleId"].ToString();
        //    //string Userid = HttpContext.Current.Session["UserId"].ToString();
        //    string orgid = User.UserId;
        //    //string roleid = User.
        //    //string Userid =
        //    bool isapplicant = false;
        //    if (HttpContext.Current.Session["ISApplicant"] != null)
        //    {
        //        isapplicant = true;
        //    }            
        //    using (EPortalEntities entity = new EPortalEntities())
        //    {
        //        var data = (from p in entity.Previleages
        //                    join u in entity.UserRoles on new
        //                    {
        //                        roleid = p.RoleId,
        //                        Userid = Userid
        //                    }
        //                    equals new
        //                    {
        //                        roleid = u.RoleId,
        //                        Userid = u.UserId
        //                    }
        //                    join mp in entity.ModulePages on p.PageId equals mp.PageId
        //                    join m in entity.Modules on mp.ModuleId equals m.Id
        //                    join pa in entity.Pages on p.PageId equals pa.Id
        //                    join orgp in entity.OrganizationPages on new
        //                    {
        //                        pageid = p.PageId,
        //                        Orgid=p.OrganizationID
        //                    } equals new
        //                    {
        //                        pageid = orgp.PageId,
        //                        Orgid = orgp.OrganizationID
        //                    }
        //                    where p.OrganizationID == orgid
        //                    && p.RoleId == roleid
        //                    && p.RowState == true
        //                    && u.UserId == Userid
        //                    && (isapplicant==false?true:pa.ForAdmin==false)
        //                    && (orgid == "1" ? true : (p.PCreate == true || p.PUpdate == true || p.PDelete == true || p.PView == true))
        //                    //group m by new { }into g
        //                    select new
        //                    {
        //                        ModuleName = m.Name,
        //                        MOduleCode = m.Code,
        //                        ModuleId = m.Id,
        //                        ModuleSequence = m.SequenceNo,
        //                        PageId = pa.Id,
        //                        PageName = pa.Name,
        //                        PageCode = pa.Code,
        //                        pagesequence = pa.SequenceNo

        //                    }).ToList();

        //        var data1 = (from p in data
        //                     group p by new { ModuleName = p.ModuleName, ModuleSequence = p.ModuleSequence, ModuleId = p.ModuleId, ModuleCode = p.MOduleCode } into g1
        //                     select new
        //                     {
        //                         ModuleName = g1.Key.ModuleName,
        //                         ModuleId = g1.Key.ModuleId,
        //                         ModuleCode = g1.Key.ModuleCode,
        //                         ModuleSequence = g1.Key.ModuleSequence,
        //                         PageList = (from pa in data
        //                                     where pa.ModuleId == g1.Key.ModuleId
        //                                     select new
        //                                     {
        //                                         PageId = pa.PageId,
        //                                         PageName = pa.PageName,
        //                                         PageCode = pa.PageCode,
        //                                         pagesequence = pa.pagesequence
        //                                     }).OrderBy(x => x.pagesequence).ToList()


        //                     }).OrderBy(x => x.ModuleSequence).ToList();




        //        #region New Nav
        //        //foreach (var item in data1)
        //        //{
        //        //    bulder.AppendFormat("<md-subheader class='md-no-sticky' style='height: 54px;'>{0}</md-subheader>", item.ModuleName);
        //        //    foreach (var pageitem in item.PageList)
        //        //    {
        //        //        bulder.AppendFormat("<md-list-item class='pagecolor margindownpage' ng-href='{0}'>", "/" + pageitem.PageCode + "/Index");
        //        //        bulder.AppendFormat("<p>{0}</p>", pageitem.PageName);
        //        //        bulder.Append("</md-list-item>");
        //        //    }
        //        //}

        //        #endregion


        //        #region New Navigation
        //        foreach (var item in data1)
        //        {
        //            bulder.Append("<li>");
        //            bulder.AppendFormat("<a><i class='fa fa-home'></i>{0}<span class='fa fa-chevron-down'></span></a>", item.ModuleName);
        //            bulder.Append("<ul class='nav child_menu'>");                    
        //            foreach (var pageitem in item.PageList)
        //            {
        //                bulder.AppendFormat("<li><a href='{0}'>{1}</a></li>", "/" + pageitem.PageCode + "/Index", pageitem.PageName);
        //            }
        //            bulder.Append("</ul>");
        //            bulder.Append("</li>");
        //        }
        //        #endregion



        //        //#region New Navigation
        //        //foreach (var item in data1)
        //        //{
        //        //    bulder.Append("<li style='background-color:#009688;margin-bottom:1%;'>");
        //        //    bulder.AppendFormat("<div class='link' style='color: white;' id='clickeassessment'><i class='fa fa-database'></i>{0}<i class='fa fa-chevron-down'></i></div>", item.ModuleName);
        //        //    bulder.Append("<ul class='submenu'>");
        //        //    foreach (var pageitem in item.PageList)
        //        //    {
        //        //        bulder.AppendFormat("<li><a href='{0}'>{1}</a></li>", "/" + pageitem.PageCode + "/Index", pageitem.PageName);
        //        //    }
        //        //    bulder.Append("</ul>");
        //        //    bulder.Append("</li>");
        //        //}
        //        //#endregion

        //        return bulder.ToString(); ;
        //    }



        //}
        //#endregion
    }
}