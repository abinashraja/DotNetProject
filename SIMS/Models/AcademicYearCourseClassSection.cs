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
    
    public partial class AcademicYearCourseClassSection
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string OrganizationID { get; set; }
        public string AcademicYearId { get; set; }
        public string CourseId { get; set; }
        public string ClassID { get; set; }
        public string SectionId { get; set; }
        public bool RowState { get; set; }
        public System.DateTime CreateDateTime { get; set; }
    
        public virtual AcademicYear AcademicYear { get; set; }
        public virtual Class Class { get; set; }
        public virtual Course Course { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual Section Section { get; set; }
    }
}
