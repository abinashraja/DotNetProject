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
    
    public partial class TestQuestion
    {
        public string Id { get; set; }
        public string TestId { get; set; }
        public string TestSectionId { get; set; }
        public string QuestionId { get; set; }
        public string OrganizationID { get; set; }
        public bool RowState { get; set; }
        public System.DateTime CreateDateTime { get; set; }
        public Nullable<int> SequenceNo { get; set; }
    
        public virtual Organization Organization { get; set; }
        public virtual Question Question { get; set; }
        public virtual Test Test { get; set; }
        public virtual TestSection TestSection { get; set; }
    }
}
