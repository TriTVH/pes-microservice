using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermAdmissionManagement.Application.DTOs.Request
{
    public record UpdateAdmissionTermRequest(int id, DateTime startDateTime, DateTime endDateTime, List<UpdateTermItemRequest> termItems);
    public record UpdateTermItemRequest(string grade,int expectedClasses, int DefaultFee);
}
