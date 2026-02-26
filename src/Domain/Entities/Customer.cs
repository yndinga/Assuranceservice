namespace AssuranceService.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = default!;
    public string? Email { get; private set; }

    private Customer() { }

    public Customer(string fullName, string? email = null)
    {
        Id = Guid.NewGuid();
        FullName = fullName;
        Email = email;
    }
}
