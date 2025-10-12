using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Domain.DTOs
{
    public class ResponseObjectFromAnotherClient
    {
        public string StatusResponseCode { get; set; } = "";
        public string Message { get; set; } = "";
        public object? Data { get; set; }

        public ResponseObjectFromAnotherClient() { }


        public ResponseObjectFromAnotherClient(string code, string message, object data)
        {
            StatusResponseCode = code;
            Message = message;
            Data = data;
        }
    }
}