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
    
    public partial class QuestionOption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public QuestionOption()
        {
            this.QuestionAnswars = new HashSet<QuestionAnswar>();
        }
    
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public string QuestionOption1 { get; set; }
        public string OrganizationID { get; set; }
        public bool RowState { get; set; }
        public System.DateTime CreateDateTime { get; set; }
        public Nullable<int> SequenceNo { get; set; }
    
        public virtual Organization Organization { get; set; }
        public virtual Question Question { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QuestionAnswar> QuestionAnswars { get; set; }
    }
}
