using ParentService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Infrastructure.Repositories.IRepositories
{
    public interface ITransactionRepo
    {
        Task<int> CreateTransactionAsync(Transaction transaction);
    }
}
