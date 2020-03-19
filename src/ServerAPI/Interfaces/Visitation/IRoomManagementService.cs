using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
public interface IRoomManagementService
{
  VisitRoomManagementDetails GetRoomManagementDetails(VisitRoomManagementDetails objVisitParam);
  Task<int> UpdateScheduleBooth(BoothInfo boothinfo);  
}
}