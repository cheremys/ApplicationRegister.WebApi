using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApplicationRegister.WebApi.Models
{
    public class ApplicationModel
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public string DepartmentAddress { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }
    }
}
