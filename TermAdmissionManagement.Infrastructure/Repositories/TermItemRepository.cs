using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Infrastructure.DBContext;
using TermAdmissionManagement.Infrastructure.Entities;
using TermAdmissionManagement.Infrastructure.Repositories.IRepository;

namespace TermAdmissionManagement.Infrastructure.Repositories
{
    public class TermItemRepository : ITermItemRepository
    {
        private readonly PesTermManagementContext _context;

        public TermItemRepository(PesTermManagementContext context)
        {
            _context = context;
        }
        
        public async Task<List<TermItem>> GetTermItemsToProcessAsync(DateTime now, CancellationToken ct)
        {
            return await _context.TermItems
                .Include(t => t.AdmissionTerm)
                .Where(t => t.Status == "awaiting" && t.AdmissionTerm.StartDate <= now)
                .ToListAsync(ct);
        }

    }
}
