using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPortal.Models;

namespace EPortal.Models
{
    public partial class UserInfo
    {
        public string OrganizationName { get; set; }
        public string ErrorMsg { get; set; }

    }
    public partial class Organization
    {
        public string Operation { get; set; }
    }
    public partial class UserInfo
    {
        public string Operation { get; set; }
    }
    public partial class Previleage
    {
        public string Operation { get; set; }
    }
    public partial class RoleMaster
    {
        public string Operation { get; set; }
    }
    public partial class Test
    {
        public string Operation { get; set; }
    }
    public partial class TestSection
    {
        public string Operation { get; set; }
    }
    public partial class QuestionType
    {
        public string Operation { get; set; }
    }
    public partial class Questionsource
    {
        public string Operation { get; set; }
    }
    public partial class TestInstruction
    {
        public string Operation { get; set; }
    }
    public partial class Course
    {
        public string Operation { get; set; }
    }
    public partial class Class
    {
        public string Operation { get; set; }
    }
    public partial class Section
    {
        public string Operation { get; set; }
    }
    public partial class Subject
    {
        public string Operation { get; set; }
        
    }
    public partial class AcademicYear
    {
        public string Operation { get; set; }
        public bool DeleteConformation { get; set; }

    }
}