using SyllabusService.Domain.DTOs;
using SyllabusService.Domain.DTOs.Response;
using SyllabusService.Domain.IClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Domain
{
    public class ParentClient : IParentClient
    {
        private readonly HttpClient _httpClient;

        public ParentClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResponseObjectFromAnotherClient> GetStudentDtoById(int? id)
        {
            var response = await _httpClient.GetAsync($"api/student/public/{id}");

            var content = await response.Content.ReadAsStringAsync();

            var result = System.Text.Json.JsonSerializer.Deserialize<ResponseObjectFromAnotherClient>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
    }
}
