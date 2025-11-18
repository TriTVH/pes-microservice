using Auth.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Repositories
{
    public interface IActivityRepository
    {
        // AI Support Methods
        Task<IEnumerable<Activity>> GetActivitiesWithDetailsAsync(int limit = 10);
        Task<int> GetTotalActivitiesCountAsync();
    }
}
