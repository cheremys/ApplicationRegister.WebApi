using ApplicationRegister.WebApi.Interfaces;
using ApplicationRegister.WebApi.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace ApplicationRegister.WebApi.Services
{
    internal class ApplicationService : IApplicationService
    {
        private const string createMessange = "create";
        private const string getMessange = "get";
        private readonly IWorker worker;
        public ApplicationService(IWorker worker)
        {
            this.worker = worker;
        }

        public int? CreateApplication(ApplicationExtendedModel application)
        {
            string json = JsonSerializer.Serialize<ApplicationExtendedModel>(application);
            var response = worker.SendMessage(createMessange + json);
            worker.Close();

            int id;
            return !string.IsNullOrEmpty(response) && int.TryParse(response, out id) ? id : (int?)null;
        }

        public IEnumerable<ApplicationModel> GetApplications(int id, string address = null)
        {
            string json = JsonSerializer.Serialize(new { Id = id, Address = address });
            string response = worker.SendMessage(getMessange + json);
            worker.Close();

            return !string.IsNullOrEmpty(response) ? 
                JsonSerializer.Deserialize<List<ApplicationModel>>(response) : null;
        }
    }
}
