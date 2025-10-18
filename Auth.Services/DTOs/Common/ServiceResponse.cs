using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Services.DTOs.Common
{
    public class ServiceResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string ErrorCode { get; set; }
        public DateTime Timestamp { get; set; }

        public ServiceResponse(bool isSuccess, string message, T data, string errorCode = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            ErrorCode = errorCode;
            Timestamp = DateTime.UtcNow;
        }

        public static ServiceResponse<T> Success(string message, T data)
        {
            return new ServiceResponse<T>(true, message, data);
        }

        public static ServiceResponse<T> Error(string message, T data = default(T), string errorCode = null)
        {
            return new ServiceResponse<T>(false, message, data, errorCode);
        }

        public static ServiceResponse<T> Warning(string message, T data)
        {
            return new ServiceResponse<T>(true, message, data);
        }
    }
}
