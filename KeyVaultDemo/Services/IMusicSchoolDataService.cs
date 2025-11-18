using Azure.Identity;
using KeyVaultDemo.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace KeyVaultDemo.Services;

public interface IMusicSchoolDataService
{
    Task<Student> CreateStudentAsync(Student student, CancellationToken cancellationToken = default);
    Task<Student?> AddInstrumentsAsync(Guid studentId, IEnumerable<Instrument> instruments, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default);
}

public sealed class MusicSchoolDataService : IMusicSchoolDataService, IDisposable
{
    private const string DatabaseName = "musicschool";
    private const string StudentsContainerName = "students";
    private readonly IConfiguration _configuration;

    private readonly CosmosClient _client;
    private readonly Container _studentsContainer;

    public MusicSchoolDataService(IConfiguration configuration)
    {
        _configuration = configuration;
        // Use Default Azure Credential
        // Add RBAC  to Cosmos DB
        // User & Function App
        var cosmosUri = _configuration["COSMOSDB_URI"]
            ?? throw new InvalidOperationException("CosmosUri setting is missing.");

        _client = new CosmosClient(cosmosUri, new DefaultAzureCredential());

        _studentsContainer = _client.GetContainer(DatabaseName, StudentsContainerName);
    }

    public async Task<Student> CreateStudentAsync(Student student, CancellationToken cancellationToken = default)
    {
        student.Instruments ??= new List<Instrument>();

        var response = await _studentsContainer.CreateItemAsync(
            student,
            new PartitionKey(student.id.ToString()),
            cancellationToken: cancellationToken);

        return response.Resource;
    }

    public async Task<Student?> AddInstrumentsAsync(Guid studentId, IEnumerable<Instrument> instruments, CancellationToken cancellationToken = default)
    {
        var instrumentsList = instruments?.ToList() ?? new List<Instrument>();

        try
        {
            var response = await _studentsContainer.ReadItemAsync<Student>(
                studentId.ToString(),
                new PartitionKey(studentId.ToString()),
                cancellationToken: cancellationToken);

            var student = response.Resource;
            student.Instruments ??= new List<Instrument>();
            student.Instruments.AddRange(instrumentsList);

            var replaceResponse = await _studentsContainer.ReplaceItemAsync(
                student,
                studentId.ToString(),
                new PartitionKey(studentId.ToString()),
                cancellationToken: cancellationToken);

            return replaceResponse.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Student>> GetAllStudentsAsync(CancellationToken cancellationToken = default)
    {
        var students = new List<Student>();
        var iterator = _studentsContainer.GetItemQueryIterator<Student>("SELECT * FROM c");

        while (iterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            students.AddRange(response.Resource);
        }

        return students;
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
