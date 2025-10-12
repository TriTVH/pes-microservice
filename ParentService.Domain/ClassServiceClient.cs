using ParentService.Application.DTOs;
using ParentService.Domain.IClient;
using SyllabusService.Application.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ParentService.Domain
{
    public class ClassServiceClient : IClassServiceClient
    {
        private readonly HttpClient _httpClient;

        public ClassServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResponseObjectFromAnotherClient> GetAdmissionTermById(int id)
        {
            var response = await _httpClient.GetAsync($"api/term/common/{id}");
            var content = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ResponseObjectFromAnotherClient>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }

        public async Task<ResponseObjectFromAnotherClient> CheckClassesAvailabilityAsync(CheckClassRequest request)
        {

            var response = await _httpClient.PutAsJsonAsync($"api/classes/public/check/availability", request);
            var content = await response.Content.ReadAsStringAsync();

            var result = System.Text.Json.JsonSerializer.Deserialize<ResponseObjectFromAnotherClient>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }

        public async Task<ResponseObjectFromAnotherClient> GetByClassId(int id)
        {
            var response = await _httpClient.GetAsync($"api/classes/public/{id}");
            var content = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ResponseObjectFromAnotherClient>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }

        public async Task<ResponseObjectFromAnotherClient> GetClassesByIds(List<int> ids)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/classes/public/by-ids", ids);
            var content = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<ResponseObjectFromAnotherClient>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }

    }
}
