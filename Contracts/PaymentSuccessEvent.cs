using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class PaymentSuccessEvent
    {
        public int AdmissionFormId { get; set; }
        public List<int> ClassIds { get; set; } = new();
        public int Amount { get; set; }
        public string TxnRef { get; set; } = "";
        public DateTime? PayDate { get; set; }
    }

}
