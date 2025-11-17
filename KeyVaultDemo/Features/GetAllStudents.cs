using KeyVaultDemo.Models;
using KeyVaultDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace KeyVaultDemo.Features;

public class GetAllStudents
{
    private readonly ILogger<GetAllStudents> _logger;
    private readonly IMusicSchoolDataService _dataService;

    public GetAllStudents(ILogger<GetAllStudents> logger, IMusicSchoolDataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [Function("GetAllStudents")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "students")] HttpRequest req)
    {
        var results = new List<Student>();
        await foreach (var student in _dataService.GetAllStudentsAsync())
        {
            results.Add(student);
        }

        return new OkObjectResult(results);
    }
}
