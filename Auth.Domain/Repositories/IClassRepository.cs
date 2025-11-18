using Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Repositories
{
    public interface IClassRepository
    {
        // AI Support Methods
        Task<IEnumerable<Class>> GetClassesWithSchedulesAsync(int limit = 10);
        Task<int> GetTotalClassesCountAsync();
        Task<int> GetActiveClassesCountAsync();
        
        // AI Dynamic Query Methods
        Task<Class?> GetClassByNameAsync(string name);
        Task<IEnumerable<Class>> GetClassesByStatusAsync(string status, int limit = 10);
        Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId, int limit = 10);
        Task<IEnumerable<Class>> SearchClassesAsync(string searchTerm, int limit = 10);
        Task<IEnumerable<Class>> GetClassesByCapacityAsync(int minCapacity, int maxCapacity, int limit = 10);
    }
}
