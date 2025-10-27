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
        
        // AI Support Methods
        Task<IEnumerable<Parent>> GetParentsWithLimitAsync(int limit = 10);
        Task<int> GetTotalParentsCountAsync();
        
        // AI Dynamic Query Methods
        Task<IEnumerable<Parent>> GetParentsByJobAsync(string job, int limit = 10);
        Task<IEnumerable<Parent>> GetParentsByRelationshipAsync(string relationship, int limit = 10);
        Task<IEnumerable<Parent>> SearchParentsAsync(string searchTerm, int limit = 10);
    }
}

