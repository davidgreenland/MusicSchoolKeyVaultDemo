using Azure.Identity;
using KeyVaultDemo.Services;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration; // Added Azure.Extensions.AspNetCore.Configuration.Secrets package
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add Azure Key Vault to configuration sources

var configuration = builder.Configuration;
var keyVaultUri = configuration["KEY_VAULT_URI"];

configuration.AddAzureKeyVault(
    new Uri(keyVaultUri), 
    new DefaultAzureCredential());

//

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddSingleton<IMusicSchoolDataService, MusicSchoolDataService>();

builder.Build().Run();
