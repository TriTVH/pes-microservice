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
    }
}
