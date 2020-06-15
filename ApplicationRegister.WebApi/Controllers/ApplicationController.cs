using ApplicationRegister.WebApi.Interfaces;
using ApplicationRegister.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationRegister.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService service;
        private readonly IActionContextAccessor accessor;
        private readonly ILogger<ApplicationController> logger;

        public ApplicationController(IApplicationService service, IActionContextAccessor accessor, ILogger<ApplicationController> logger)
        {
            this.service = service;
            this.accessor = accessor;
            this.logger = logger;
        }


        [HttpGet()]
        public string Get()
        {
            //TODO: Remove this method after testing
            logger.LogDebug("Default controller");
            return "Default controller";
        }

        [HttpGet("{id}/{address?}")]
        public async Task<IActionResult> GetApplications(int id, string address)
        {
            var ip = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress;
            string departmentAddress = string.IsNullOrEmpty(address) ? "undefined" : address;

            logger.LogInformation("Got request from {ip} to get informtion about Application {id} for department {departmentAddress}", ip, id, departmentAddress);

            IEnumerable<ApplicationModel> applications = null;

            try
            {
                logger.LogInformation("Sent request to service to get information about Application {id} for department {departmentAddress}", id, departmentAddress);

                applications = await Task.Run(() => service.GetApplications(id, address));

                if (applications != null)
                {
                    logger.LogInformation("Received success response from service to get information about Application {id} for department {departmentAddress}", id, departmentAddress);
                }
                else
                {
                    logger.LogInformation("Cannot find information about Application {id} for department {departmentAddress}", id, departmentAddress);
                    return NotFound();
                }

            }
            catch (System.Exception exception)
            {
                logger.LogError(exception.Message);
                return NotFound();
            }

            return CreatedAtAction(nameof(GetApplications), applications);
        }

        [HttpPost]
        public async Task<IActionResult> PostApplication([FromBody] ApplicationModel application)
        {
            if (application.ClientId <= 0)
            {
                ModelState.AddModelError("ClientId", "Client Id should have a positive value");
                logger.LogError("Client Id doesn't have a positive value");
            }
            if (application.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount should have a positive value");
                logger.LogError("Amount doesn't have a positive value");
            }
            if (string.IsNullOrEmpty(application.Currency))
            {
                ModelState.AddModelError("Currency", "Currency is required");
                logger.LogError("Currency is not defined");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            int? applicationId = null;
            var ip = accessor.ActionContext.HttpContext.Connection.RemoteIpAddress;

            var applicationExtended = new ApplicationExtendedModel
            {
                Id = application.Id,
                ClientId = application.ClientId,
                DepartmentAddress = application.DepartmentAddress,
                Amount = application.Amount,
                Currency = application.Currency,
                Ip = ip.ToString()
            };

            logger.LogInformation("Got request to create Application from {ip}", ip);

            try
            {
                logger.LogInformation("Sent request to service to create Application {application}", application);

                applicationId = await Task.Run(() => service.CreateApplication(applicationExtended));

                if (applicationId != null)
                {
                    logger.LogInformation("Received success response from service to create Application");
                }
                else
                {
                    logger.LogInformation("Cannot create Application {application}", applicationExtended);
                    return NotFound();
                }
            }
            catch (System.Exception exception)
            {
                logger.LogError(exception.Message);
                return NotFound();
            }

            return CreatedAtAction(nameof(PostApplication), applicationId);
        }
    }
}
