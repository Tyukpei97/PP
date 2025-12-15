using System;

namespace MiniEmployeeDatabase;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }

    public Employee Clone() => new()
    {
        Id = Id,
        Name = Name,
        Position = Position,
        Salary = Salary
    };
}
