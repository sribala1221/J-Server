using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
	public class LogCountVm
	{
	public List<AttendanceAoVm> ClockIn{get; set;}  
    public List<AttendanceAoVm> ClockOut {get; set;}
    public List<AttendanceAoVm> SetHousing{get; set;}
    public List<AttendanceAoVm> SetLocation{get; set;}
    public List<AttendanceAoVm> SetStatus{get; set;}
    public List<AttendanceAoVm> Logs{get; set;}
    public int PersonnelId{get; set;}
    public PersonnelVm PersonnelDetails{get; set;}

    public DateTime? FromDate {get; set;}

    public DateTime? ToDate {get; set;}
    public string PersonnelDepartment { get; set; }
    public string PersonnelPosition { get; set; }

    public bool PersonnelTerminationFlag{get; set;}
    }
    public class LogCountDetails
    {
       public List<LogCountVm> LogCountList {get; set;}
       public List<LookupVm> DepartmentList{get; set;}
       public List<LookupVm> PositionList{get; set;}
    }

    public class AttendanceAoVm
    {   
        public int Count{get; set;}
        public int AttendanceAOHistoryId {get; set;}
        public DateTime? AttendanceAoStatusDate{get; set;}
        public DateTime? AttendanceAoLastHousingDate {get; set;}
        public string AttendanceAoLastHousingLocation{get; set;}
        public string AttendanceAoLastHousingNumber{get; set;}
        public string AttendanceAoLastHousingNote{get; set;}
        public DateTime? AttendanceAoLastLocDate {get; set;}
        public string AttendanceAoLastLocDesc {get; set;}
        public string AttendanceAoLastLocTrack { get; set; }
        public string AttendanceAoStatusNote { get; set; }
        public string Status{get; set;}
        public string AttendanceAoOfficerLog { get; set; }
    }


}