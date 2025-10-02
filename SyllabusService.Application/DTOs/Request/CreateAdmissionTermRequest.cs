using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Application.DTOs.Request
{
   public record CreateAdmissionTermRequest(DateTime startDate, DateTime endDate, List<int> classIds);
}
