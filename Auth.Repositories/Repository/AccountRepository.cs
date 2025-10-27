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

        // AI Support Methods
        public async Task<IEnumerable<Account>> GetAccountsByRoleAsync(string role, int limit = 10)
        {
            return await _context.Accounts
                .Where(a => a.Role == role)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetAccountsByIdsAsync(IEnumerable<int> accountIds)
        {
            return await _context.Accounts
                .Where(a => accountIds.Contains(a.Id))
                .ToListAsync();
        }

        public async Task<int> GetTotalAccountsCountAsync()
        {
            return await _context.Accounts.CountAsync();
        }

        public async Task<int> GetAccountsCountByRoleAsync(string role)
        {
            return await _context.Accounts.CountAsync(a => a.Role == role);
        }

        // AI Dynamic Query Methods
        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<IEnumerable<Account>> SearchAccountsAsync(string searchTerm, int limit = 10)
        {
            return await _context.Accounts
                .Where(a => a.Name.Contains(searchTerm) || a.Email.Contains(searchTerm))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Account>> GetAccountsByStatusAsync(string status, int limit = 10)
        {
            return await _context.Accounts
                .Where(a => a.Status == status)
                .Take(limit)
                .ToListAsync();
        }
    }
}
