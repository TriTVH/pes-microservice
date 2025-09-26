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
        private PesTermManagementContext _context;
        public TermItemRepository(PesTermManagementContext context ) 
        {
            _context = context; 
        }

        public async Task<int> CreateTermItemAsync(TermItem termItem)
        {
            _context.TermItems.Add(termItem);
            return await _context.SaveChangesAsync();
        }

        public async Task<TermItem?> GetByIdsAsync(int id)
        {
            return await _context.TermItems
                .Include(t => t.AdmissionTerm)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<int> UpdateTermItem(TermItem termItem)
        {
            _context.TermItems.Update(termItem);
            return await _context.SaveChangesAsync();
        }

    }
}
