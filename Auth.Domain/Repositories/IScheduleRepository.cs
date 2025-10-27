using Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Repositories
{
    public interface IScheduleRepository
    {
        // AI Support Methods
        Task<IEnumerable<Schedule>> GetSchedulesWithDetailsAsync(int limit = 10);
        Task<int> GetTotalSchedulesCountAsync();
    }
}
