using ApplicationRegister.WebApi.Models;
using System.Collections.Generic;

namespace ApplicationRegister.WebApi.Interfaces
{
    public interface IApplicationService
    {
        public IEnumerable<ApplicationModel> GetApplications(int id, string address);

        public int? CreateApplication(ApplicationExtendedModel application);
    }
}
