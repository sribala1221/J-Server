using System;
using System.Collections.Generic;
using GenerateTables.Models;

namespace ServerAPI.ViewModels
{
    public class ProgramInstructor
    {
        public List<Instructor> Assigned { get; set; }
        public List<Instructor> Certified { get; set; }
        public List<Instructor> NotCertified { get; set; }
    }

    public class Instructor
    {
        public PersonnelVm Personnel { get; set; }
        public int? DeleteFlag { get; set; }
        public int? ProgramInstructorCertId { get; set; }
        public string ProgramInstructorCerificationName { get; set; }
        public string CertificateNote { get; set; }
        public int PersonnelId { get; set; }
    }
    public class ProgramCourseInstructorVm
    {
        public int PersonnelId { get; set; }
        public int PersonId { get; set; }
        public PersonnelVm Personnel { get; set; }
        public List<ScheduleVm> Certified { get; set; }
        public List<ScheduleVm> AssignedCourses { get; set; }
        public string PersonDlNumber { get; set; }
        public string PersonOtherIdNumber { get; set; }
    }

    public class ProgramInstructionCerificationVm
    {
        public List<ProgramCourseInstructorVm> CourseInstructor { get; set; }
        public List<ProgramInstructorCert> ProgamClassCertificateList { get; set; }
        public List<ScheduleVm> Instructor { get; set; }
    }

    public class ProgramInstructorCertVm
    {
        public int ProgramInstructorCertId { get; set; }
        public string CertNote { get; set; }
        public int CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public int? DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool DeleteFlag { get; set; }
        public int PersonnelId { get; set; }
        public int ProgramClassId { get; set; }
        public bool IsChecked { get; set; }
    }

    public class ProgramInstructorAssignVm
    {
        public int ProgramInstructorAssignId { get; set; }
        public int ScheduleId { get; set; }
        public int CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool DeleteFlag { get; set; }
        public int PersonnelId { get; set; }
    }


}
