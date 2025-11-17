using KeyVaultDemo.Models;
using KeyVaultDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace KeyVaultDemo.Features;

public class CreateStudent
{
    private readonly ILogger<CreateStudent> _logger;
    private readonly IMusicSchoolDataService _dataService;

    public CreateStudent(ILogger<CreateStudent> logger, IMusicSchoolDataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [Function("CreateStudent")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "students")] HttpRequest req,
        [FromBody] Student student)
    {
        _logger.LogInformation("CreateStudent HTTP trigger processed a request.");

        if (student is null)
        {
            return new BadRequestObjectResult("Invalid student payload.");
        }

        if (student.id == Guid.Empty)
        {
            student.id = Guid.NewGuid();
        }

        var created = await _dataService.CreateStudentAsync(student);

        return new ObjectResult(created) { StatusCode = (int)HttpStatusCode.Created };
    }
}