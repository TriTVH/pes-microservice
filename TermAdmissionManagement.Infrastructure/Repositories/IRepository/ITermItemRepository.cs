using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Infrastructure.Entities;

namespace TermAdmissionManagement.Infrastructure.Repositories.IRepository
{
    public interface ITermItemRepository
    {
        Task<int> CreateTermItemAsync(TermItem termItem);
        Task<TermItem?> GetByIdsAsync(int id);
        Task<int> UpdateTermItem(TermItem termItem);
    }
}
