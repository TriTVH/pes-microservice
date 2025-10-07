using SyllabusService.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Infrastructure.Repositories.IRepositories
{
    public interface IWeekRepository
    {
        Task<IEnumerable<Schedule>> GetSchedulesByClassIdAsync(int classId);
    }
}
