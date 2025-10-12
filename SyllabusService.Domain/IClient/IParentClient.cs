using SyllabusService.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Domain.IClient
{
    public interface IParentClient
    {
        Task<ResponseObjectFromAnotherClient> GetStudentDtoById(int? id);
    }
}
