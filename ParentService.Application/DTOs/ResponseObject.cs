using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Application.DTOs
{
    public class ResponseObject
    {
        public string StatusResponseCode { get; set; } = "";
        public string Message { get; set; } = "";
        public object? Data { get; set; }

        public ResponseObject(string code, string message, object data)
        {
            StatusResponseCode = code;
            Message = message;
            Data = data;
        }
    }
}
