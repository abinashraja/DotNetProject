using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace EPortal.App_Start
{
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role) { return false; }

        public CustomPrincipal(string username)
        {
            this.Identity = new GenericIdentity(username);

        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RoleId { get; set; }
        public string OrgId { get; set; }
        public string OrgName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ISApplicant { get; set; }
    }
    public class CustomPrincipalSerializeModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RoleId { get; set; }
        public string OrgId { get; set; }
        public string OrgName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ISApplicant { get; set; }
        
    }
}