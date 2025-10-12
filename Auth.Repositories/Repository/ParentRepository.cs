using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories.Repository
{
    public class ParentRepository : IParentRepository
    {
        private readonly pesContext _context;

        public ParentRepository(pesContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Parent parent)
        {
            await _context.Parents.AddAsync(parent);
            await _context.SaveChangesAsync();
        }

        public async Task<Parent?> GetByAccountIdAsync(int accountId)
        {
            return await _context.Parents.FirstOrDefaultAsync(p => p.AccountId == accountId);
        }

        public async Task<Parent?> GetByIdAsync(int id)
        {
            return await _context.Parents.FindAsync(id);
        }

        public async Task<IEnumerable<Parent>> GetAllAsync()
        {
            return await _context.Parents.ToListAsync();
        }

        public async Task UpdateAsync(Parent parent)
        {
            _context.Parents.Update(parent);
            await _context.SaveChangesAsync();
        }
    }
}
