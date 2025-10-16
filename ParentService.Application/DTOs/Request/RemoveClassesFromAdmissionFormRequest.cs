using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.DTOs.Request
{
    public class RemoveClassesFromAdmissionFormRequest
    {
        public int AdmissionFormId { get; set; }
        public List<int> ClassIds { get; set; }
    }
}
