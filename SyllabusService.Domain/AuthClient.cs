using SyllabusService.Domain.DTOs;
using SyllabusService.Domain.DTOs.Response;
using SyllabusService.Domain.IClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SyllabusService.Domain
{
    public class AuthClient : IAuthClient
    {
        private readonly HttpClient _httpClient;

        public AuthClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<AccountDto?> GetTeacherProfileDtoById(int id)
        {
            var response = await _httpClient.GetAsync($"api/Hr/teacher/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            var result = System.Text.Json.JsonSerializer.Deserialize<AccountDto>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }

        public async Task<AccountDto?> GetParentProfileDto(int? id)
        {
            var response = await _httpClient.GetAsync($"api/auth/getAllAccount/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            var result = System.Text.Json.JsonSerializer.Deserialize<AccountDto>(content, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result;
        }

       
    }
}
