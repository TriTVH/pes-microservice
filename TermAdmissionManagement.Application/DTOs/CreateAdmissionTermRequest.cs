using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermAdmissionManagement.Application.DTOs
{
    public record CreateAdmissionTermRequest(DateTime startDateTime, DateTime endDateTime, List<CreateTermItemRequest> termItems);
    public record CreateTermItemRequest( int expectedClasses, string grade );

}
