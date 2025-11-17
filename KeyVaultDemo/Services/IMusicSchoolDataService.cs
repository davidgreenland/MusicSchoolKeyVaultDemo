using KeyVaultDemo.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace KeyVaultDemo.Services;

public interface IMusicSchoolDataService
{
    Task<Student> CreateStudentAsync(Student student, CancellationToken cancellationToken = default);
    Task<Student?> AddInstrumentsAsync(Guid studentId, IEnumerable<Instrument> instruments, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Student> GetAllStudentsAsync(CancellationToken cancellationToken = default);
}

public sealed class MusicSchoolDataService : IMusicSchoolDataService, IDisposable
{
    private const string DatabaseName = "musicschool";
    private const string StudentsContainerName = "students";

    private readonly CosmosClient _client;
    private readonly Container _studentsContainer;

    public MusicSchoolDataService(IConfiguration configuration)
    {
        var connectionString = configuration["COSMOSDB_CONNECTIONSTRING"]
            ?? throw new InvalidOperationException("CosmosConnection setting is missing.");

        _client = new CosmosClient(connectionString);

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

    public async IAsyncEnumerable<Student> GetAllStudentsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var iterator = _studentsContainer.GetItemQueryIterator<Student>("SELECT * FROM c");

        while (iterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            foreach (var student in response.Resource)
            {
                yield return student;
            }
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
