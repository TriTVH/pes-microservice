using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermAdmissionManagement.Domain.DTOs
{
    public class ResponseObjectFromAnotherClient<T>
    {
        public string StatusResponseCode { get; set; } = "";
        public string Message { get; set; } = "";
        public T? Data { get; set; }

        // parameterless ctor required for deserialization
        public ResponseObjectFromAnotherClient() { }

        public ResponseObjectFromAnotherClient(string code, string message, T? data)
        {
            StatusResponseCode = code;
            Message = message;
            Data = data;
        }
    }
}
