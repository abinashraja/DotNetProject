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
    
    public partial class TestInstruction
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string InstructionText { get; set; }
        public string OrganizationID { get; set; }
        public bool RowState { get; set; }
        public System.DateTime CreateDateTime { get; set; }
        public string TestId { get; set; }
    
        public virtual Organization Organization { get; set; }
        public virtual Test Test { get; set; }
    }
}
