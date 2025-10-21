using Transactions.Domain.Entities;

namespace Transactions.Domain.Repositories;

public interface IImportRepository
{
    Task<Import?> GetByHashAsync(string hash);
    Task<Import?> GetByIdAsync(Guid id);
}