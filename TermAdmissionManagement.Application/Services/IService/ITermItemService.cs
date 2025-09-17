using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Infrastructure.Entities;

namespace TermAdmissionManagement.Application.Services.IService
{
    public interface ITermItemService
    {
        Task<List<TermItem>> GetTermItemsToProcessAsync(DateTime now, CancellationToken ct);
    }
}
