using KeyVaultDemo.Models;
using KeyVaultDemo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace KeyVaultDemo.Features;

public class AddStudentInstruments
{
    private readonly ILogger<AddStudentInstruments> _logger;
    private readonly IMusicSchoolDataService _dataService;

    public AddStudentInstruments(ILogger<AddStudentInstruments> logger, IMusicSchoolDataService dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [Function("AddInstruments")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "students/{id:guid}/instruments")] HttpRequest req,
        Guid id,
        [FromBody] List<Instrument> instruments)
    {
        _logger.LogInformation("AddInstruments HTTP trigger processed a request for student {StudentId}.", id);

        var updated = await _dataService.AddInstrumentsAsync(id, instruments);
        if (updated is null)
        {
            return new NotFoundObjectResult($"Student {id} not found.");
        }

        return new OkObjectResult(updated);
    }
}