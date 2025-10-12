using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.DTOs.Request
{
    public class CreateFormRequest
    {

        public int StudentId { get; set; }
        public int AdmissionTermId { get; set; }
        public List<int> ClassIds { get; set; } = new();

    }
}
