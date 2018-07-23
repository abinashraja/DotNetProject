using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;
using EPortal.App_Start;
namespace EPortal
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Server.ClearError();
            Response.Redirect("/Home/Error");
        }
        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    CustomPrincipalSerializeModel serializeModel = serializer.Deserialize<CustomPrincipalSerializeModel>(authTicket.UserData);
                    CustomPrincipal newUser = new CustomPrincipal(authTicket.Name);
                    newUser.Id = serializeModel.Id;
                    newUser.FirstName = serializeModel.FirstName;
                    newUser.LastName = serializeModel.LastName;
                    newUser.Email = serializeModel.Email;
                    newUser.RoleId = serializeModel.RoleId;
                    newUser.OrgId = serializeModel.OrgId;
                    newUser.OrgName = serializeModel.OrgName;
                    newUser.UserId = serializeModel.UserId;
                    newUser.UserName = serializeModel.UserName;
                    newUser.ISApplicant = serializeModel.ISApplicant;

                    HttpContext.Current.User = (System.Security.Principal.IPrincipal)newUser;
                }
                catch (Exception ex)
                {
                    FormsAuthentication.SignOut();
                }


            }
          
        }
    }
}
