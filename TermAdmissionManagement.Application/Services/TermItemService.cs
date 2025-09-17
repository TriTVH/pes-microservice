using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Application.Services.IService;
using TermAdmissionManagement.Infrastructure.Entities;
using TermAdmissionManagement.Infrastructure.Repositories.IRepository;

namespace TermAdmissionManagement.Application.Services
{
    public class TermItemService : ITermItemService
    {
        private ITermItemRepository _termItemRepo;
        public TermItemService(ITermItemRepository termItemRepository)
        {
            _termItemRepo = termItemRepository;
        }
        public async Task<List<TermItem>> GetTermItemsToProcessAsync(DateTime now, CancellationToken ct)
        {
           return await _termItemRepo.GetTermItemsToProcessAsync(now, ct);
        }
    }
}
