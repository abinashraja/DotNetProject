using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.Utility;
using System.Web.Routing;
using EPortal.Controllers;

namespace EPortal.App_Start
{
    public class CustomFilter : ActionFilterAttribute, IActionFilter
    {

        public string PageName { get; set; }

        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {

            HomeController bar = new HomeController();
            filterContext.Controller.ViewBag.nevigationBar = bar.GetMarkup();
            if (this.PageName != "UserHome")
            {
                HomeController homecont = new HomeController();

                bool checksecurity = homecont.CheckSecurity(this.PageName);
                if (checksecurity == false)
                {

                    filterContext.Result = new RedirectToRouteResult(
                 new RouteValueDictionary {{ "Controller", "Home" },
                                      { "Action", "Error" } });


                }
            }
            this.OnActionExecuting(filterContext);
        }
        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {



            if (!string.IsNullOrWhiteSpace(this.PageName))
            {

                HomeController homecont = new HomeController();
                homecont.SetPagePrvileage(this.PageName);
            }
            this.OnActionExecuted(filterContext);
        }

    }
}