using ParentService.Infrastructure.Models;
using ParentService.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Infrastructure.Repositories
{
    public class TransactionRepo : ITransactionRepo
    {
        private PES_APP_FULL_DBContext _context;

        public TransactionRepo(PES_APP_FULL_DBContext context)
        {
            _context = context;
        }

        public async Task<int> CreateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            return await _context.SaveChangesAsync();
        }
    }
}
