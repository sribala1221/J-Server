using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Interfaces
{
    public interface IVisitAvailabilityService
    {
        VisitScheduledDetail GetVisitAvailability(VisitScheduledVm objParam);
    }
}
