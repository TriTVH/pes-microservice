using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class ClassProcessResultEvent
    {
        public int AdmissionFormId { get; set; }
        public List<int> SuccessfulClassIds { get; set; } = new();
        public List<int> FailedClassIds { get; set; } = new();
        public string TxnRef { get; set; } = "";
        public int Amount { get; set; }
        public string Reason { get; set; } = "";
    }
}
