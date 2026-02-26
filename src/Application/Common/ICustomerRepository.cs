using AssuranceService.Domain.Entities;

namespace AssuranceService.Application.Common;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Customer?> GetByEmailAsync(string email);
}


