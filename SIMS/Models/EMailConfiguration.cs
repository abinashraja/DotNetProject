//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EPortal.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EMailConfiguration
    {
        public string Id { get; set; }
        public bool UserCreationMail { get; set; }
        public bool TestAssignMail { get; set; }
        public bool SendResultAfterTestMail { get; set; }
        public bool AfterLoginMail { get; set; }
        public bool AfterChangePasswordMail { get; set; }
        public string OrganizationId { get; set; }
    }
}
