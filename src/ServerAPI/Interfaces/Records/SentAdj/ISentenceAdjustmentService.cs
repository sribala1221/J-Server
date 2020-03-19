using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface ISentenceAdjustmentService
    {
        List<AttendanceVm> GetAttendeeList(DateTime dateValue,int inmateId);
        AttendanceParam GetSentenceAdjDetail(int inmateId);
        Task<int> UpdateNoDayForDay(int[] attendanceIds);
        Task<int> UpdateArrestAttendance(AttendanceParam attendanceParams);
        DiscDaysVm GetDiscDaysCount(bool showRemoved);
        List<DisciplinaryDays> GetDiscDaysDetails(bool isApply, bool showRemoved);
        int UpdateDiscInmateRemoveFlag(int discInmateId, bool isUndo);
        int GetIncarcerationForSentence(int inmateId, int arrestId);

    }
}
