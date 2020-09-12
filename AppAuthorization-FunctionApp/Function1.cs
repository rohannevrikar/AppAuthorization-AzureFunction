using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.Azure.WebJobs.Host;
using System.Threading;

namespace FunctionADApp
{
    
    public class Function1 : BaseAuthorizedFunction
    {
        public Function1(IHttpContextAccessor a) : base(a) { }

        [RoleAuthorize("DNA.Read")]
        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
    public abstract class BaseAuthorizedFunction : IFunctionExceptionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseAuthorizedFunction(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            if (exceptionContext.Exception.InnerException != null && exceptionContext.Exception.InnerException is AuthorizationException)
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await _httpContextAccessor.HttpContext.Response.WriteAsync(exceptionContext.Exception.InnerException.Message);
            }
            if (exceptionContext.Exception.InnerException != null && exceptionContext.Exception.InnerException is ArgumentNullException)
            {
                _httpContextAccessor.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await _httpContextAccessor.HttpContext.Response.WriteAsync(exceptionContext.Exception.InnerException.Message);

            }
        }
    }
}
