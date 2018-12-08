using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPortal.Models;
using System.Net.Mail;
using System.IO;
using System.Text;
using System.Web.Mvc;
using EPortal.App_Start;
using EPortal.Controllers;
using System.Security.Principal;

namespace EPortal.Utility
{
    public static class Utility
    {
        public static string UserId { get; set; }
        public static string UserName { get; set; }
        public static string TestId { get; set; }
        public static string UserRoleId { get; set; }
        public static string Organizationid { get; set; }
        public static string OrganizationName { get; set; }
        public static string PeriodId { get; set; }
        public static string PeriodName { get; set; }
        public static bool Create { get; set; }
        public static bool Update { get; set; }
        public static bool Delete { get; set; }
        public static bool View { get; set; }
        public static string CurrentPageName { get; set; }
        public static bool ValidateProperty(string propertyName, string validationtype)
        {
            bool returnvalue = true;
            switch (validationtype)
            {
                case "Required":
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        returnvalue = false;
                    }
                    break;
            }
            return returnvalue;
        }


        




    }
}