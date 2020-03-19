using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;


namespace ServerAPI.Services
{
    public interface IFulcrumService
    {
        Object GetInmateNumber(FulcrumIdentifyRequest value);
        Object Verify(FulcrumVerifyRequest value);
        Object Enroll(FulcrumEnrollRequest value);
        Object Delete(FulcrumDeleteRequest value);
    }
}
