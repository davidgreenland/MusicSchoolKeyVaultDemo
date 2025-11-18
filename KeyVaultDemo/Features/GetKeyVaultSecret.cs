using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace KeyVaultDemo.Features;

public class GetKeyVaultSecret
{
    private readonly IConfiguration _configuration;


    public GetKeyVaultSecret(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function("GetKeyVaultSecret")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "secret")] HttpRequest req)
    {
        var secretValue = _configuration["keyvaultsecret"];

        return new OkObjectResult(secretValue);
    }
}