using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TermAdmissionManagement.Domain.DTOs;

namespace TermAdmissionManagement.Domain.IClients
{
    public interface IAuthClient
    {
        Task<ResponseObjectFromAnotherClient<ParentAccountDto>> GetParentAccountInfoAsync(int? id);
    }
}
