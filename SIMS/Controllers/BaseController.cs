using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPortal.App_Start;
using System.Security.Principal;
namespace EPortal.Controllers
{
    public class BaseController : Controller
    {
        protected virtual new CustomPrincipal User
        {
            get { return System.Web.HttpContext.Current.User as CustomPrincipal; }
        }
    }
}