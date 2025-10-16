using Auth.Services.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Services.Extensions
{
    public static class ServiceResponseExtensions
    {
        public static IActionResult ToApiResponse<T>(this ServiceResponse<T> serviceResponse)
        {
            if (serviceResponse.IsSuccess)
            {
                return new OkObjectResult(serviceResponse);
            }
            else
            {
                return new BadRequestObjectResult(serviceResponse);
            }
        }

        public static IActionResult ToApiResponse<T>(this ServiceResponse<T> serviceResponse, int statusCode)
        {
            if (serviceResponse.IsSuccess)
            {
                return new ObjectResult(serviceResponse) { StatusCode = statusCode };
            }
            else
            {
                return new ObjectResult(serviceResponse) { StatusCode = statusCode };
            }
        }
    }
}
