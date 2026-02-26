namespace AssuranceService.Domain.Entities;

public class Policy
{
    public Guid Id { get; private set; }
    public string Number { get; private set; } = default!;
    public Guid CustomerId { get; private set; }
    public decimal Premium { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public AssuranceService.Domain.Enums.PolicyStatus Status { get; private set; }

    private Policy() { }

    public Policy(string number, Guid customerId, decimal premium, DateTime start, DateTime end)
    {
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Policy number is required", nameof(number));
        if (premium <= 0) throw new ArgumentException("Premium must be positive", nameof(premium));
        if (end <= start) throw new ArgumentException("End date must be after start date");

        Id = Guid.NewGuid();
        Number = number;
        CustomerId = customerId;
        Premium = premium;
        StartDate = start;
        EndDate = end;
        Status = AssuranceService.Domain.Enums.PolicyStatus.Active;
    }

    public void Cancel(string reason)
    {
        Status = AssuranceService.Domain.Enums.PolicyStatus.Cancelled;
    }
}
