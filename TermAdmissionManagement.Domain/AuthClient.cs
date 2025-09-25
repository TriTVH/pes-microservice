using System.Net.Http.Json;
using System.Text.Json;
using TermAdmissionManagement.Domain.DTOs;
using TermAdmissionManagement.Domain.IClients;

public class AuthClient : IAuthClient
{
    private readonly HttpClient _http;
    public AuthClient(HttpClient http) => _http = http;

    public async Task<ResponseObjectFromAnotherClient<ParentAccountDto>?> GetParentAccountInfoAsync(int? id)
    {
        var url = $"api/auth/getAllAccount/{id}";
        var resp = await _http.GetAsync(url);
        var body = await resp.Content.ReadAsStringAsync();

        Console.WriteLine($"[AuthClient] GET {url} -> Status: {resp.StatusCode}");
        Console.WriteLine("[AuthClient] Response body:");
        Console.WriteLine(body);

        if (!resp.IsSuccessStatusCode) return null;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // 1) Try wrapper {statusResponseCode,message,data}
        try
        {
            var wrapper = JsonSerializer.Deserialize<ResponseObjectFromAnotherClient<JsonElement>>(body, options);
            if (wrapper != null && wrapper.Data.ValueKind == JsonValueKind.Object)
            {
                var dto = wrapper.Data.Deserialize<ParentAccountDto>(options);
                if (dto != null)
                {
                    return new ResponseObjectFromAnotherClient<ParentAccountDto>
                    {
                        StatusResponseCode = wrapper.StatusResponseCode,
                        Message = wrapper.Message,
                        Data = dto
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[AuthClient] wrapper deserialize failed: " + ex.Message);
        }

        // 2) Fallback: plain Account object
        try
        {
            var plain = JsonSerializer.Deserialize<ParentAccountDto>(body, options);
            if (plain != null)
            {
                return new ResponseObjectFromAnotherClient<ParentAccountDto>
                {
                    StatusResponseCode = "ok",
                    Message = "ok (plain response)",
                    Data = plain
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[AuthClient] direct deserialize to ParentAccountDto failed: " + ex.Message);
        }

        return null;
    }
}
