using SyllabusService.Domain.DTOs;
using SyllabusService.Domain.IClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SyllabusService.Domain
{
    public class AuthClient : IAuthClient
    {
        private readonly HttpClient _http;
        public AuthClient(HttpClient http) => _http = http;

        public async Task<ResponseObjectFromAnotherClient<TeacherProfileDto>> GetTeacherProfile(int? id)
        {
            var url = $"api/Hr/teacher/{id}";
            var resp = await _http.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                return new ResponseObjectFromAnotherClient<TeacherProfileDto>
                {
                    StatusResponseCode = (int)resp.StatusCode,
                    Message = resp.ReasonPhrase,
                    Data = null
                };
            } 
                

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                var plain = JsonSerializer.Deserialize<TeacherProfileDto>(body, options);
                if (plain != null)
                {
                    return new ResponseObjectFromAnotherClient<TeacherProfileDto>
                    {
                        StatusResponseCode = (int)resp.StatusCode,
                        Message = resp.ReasonPhrase,
                        Data = plain
                    };
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
            }
            return null;
        }
    }
}
