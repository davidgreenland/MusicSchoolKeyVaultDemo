using System.Text.Json.Serialization;

namespace KeyVaultDemo.Models;

public sealed class Instrument
{
    public required string Name { get; set; }
    public required string Family { get; set; }
    public string? SkillLevel { get; set; }
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}

public sealed class Student
{
    public Guid id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public Guid? TeacherId { get; set; }
    public string? Email { get; set; }
    public DateOnly EnrolmentDate { get; set; }
    public List<Instrument> Instruments { get; set; } = new();
}
