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
    public class ProfileController : BaseController
    {

        #region Profile Index
        [Authorize]
        [CustomFilter(PageName = "Profile")]
        public ActionResult Index()
        {

            return View("Profile");
        }
        #endregion
    }
    
    

}