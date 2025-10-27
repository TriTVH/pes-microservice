using Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Repositories
{
    public interface IAccountRepository
    {
        Task AddAsync(Account account);
        Task<Account?> GetByEmailAsync(string email);
        Task<Account?> GetByIdAsync(int id);
        Task<IEnumerable<Account>> GetAllAsync();
        Task UpdateAsync(Account account);
        
        // AI Support Methods
        Task<IEnumerable<Account>> GetAccountsByRoleAsync(string role, int limit = 10);
        Task<IEnumerable<Account>> GetAccountsByIdsAsync(IEnumerable<int> accountIds);
        Task<int> GetTotalAccountsCountAsync();
        Task<int> GetAccountsCountByRoleAsync(string role);
        
        // AI Dynamic Query Methods
        Task<Account?> GetAccountByEmailAsync(string email);
        Task<IEnumerable<Account>> SearchAccountsAsync(string searchTerm, int limit = 10);
        Task<IEnumerable<Account>> GetAccountsByStatusAsync(string status, int limit = 10);
    }
}
