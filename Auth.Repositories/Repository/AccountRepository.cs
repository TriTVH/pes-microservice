using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly pesContext _context;

        public AccountRepository(pesContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task<Account?> GetByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _context.Accounts.FindAsync(id);
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
