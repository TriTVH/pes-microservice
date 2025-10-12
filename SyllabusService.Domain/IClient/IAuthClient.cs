using SyllabusService.Domain.DTOs;
using SyllabusService.Domain.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Domain.IClient
{
    public interface IAuthClient
    {
        Task<AccountDto?> GetTeacherProfileDtoById(int id);
        Task<AccountDto?> GetParentProfileDto(int? id);
    }
}
