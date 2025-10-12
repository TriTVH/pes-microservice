using Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Repositories
{
    public interface IParentRepository
    {
        Task AddAsync(Parent parent);
        Task<Parent?> GetByAccountIdAsync(int accountId);
        Task<Parent?> GetByIdAsync(int id);
        Task<IEnumerable<Parent>> GetAllAsync();
        Task UpdateAsync(Parent parent);
    }
}
