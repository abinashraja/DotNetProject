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
    
    public partial class UserInfo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserInfo()
        {
            this.GroupUserApplicants = new HashSet<GroupUserApplicant>();
        }
    
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LogInId { get; set; }
        public string UserPassword { get; set; }
        public string OrganizationID { get; set; }
        public bool RowState { get; set; }
        public System.DateTime CreateDateTime { get; set; }
        public string UserType { get; set; }
        public string Email { get; set; }
        public Nullable<bool> IsApplicant { get; set; }
        public string MobileNo { get; set; }
        public string PhotoPath { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public Nullable<int> NoOfLogin { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GroupUserApplicant> GroupUserApplicants { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual UserType UserType1 { get; set; }
    }
}
