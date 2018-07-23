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
    
    public partial class Test
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Test()
        {
            this.TestInstructions = new HashSet<TestInstruction>();
            this.TestQuestions = new HashSet<TestQuestion>();
            this.TestSections = new HashSet<TestSection>();
        }
    
        public string Id { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public Nullable<System.DateTime> PeriodFrom { get; set; }
        public Nullable<System.DateTime> PeriodTo { get; set; }
        public bool IsPublish { get; set; }
        public bool Islocked { get; set; }
        public string OrganizationID { get; set; }
        public bool RowState { get; set; }
        public System.DateTime CreateDateTime { get; set; }
        public Nullable<int> ExamTIme { get; set; }
        public Nullable<int> HourTime { get; set; }
        public Nullable<int> MinTime { get; set; }
    
        public virtual Organization Organization { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestInstruction> TestInstructions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestQuestion> TestQuestions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestSection> TestSections { get; set; }
    }
}