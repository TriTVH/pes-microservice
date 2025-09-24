using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermAdmissionManagement.Application.DTOs.Request
{
    public record UpdateAdmissionTermStatusRequest(int id, string action);
}
