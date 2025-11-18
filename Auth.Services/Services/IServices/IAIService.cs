using Auth.Services.DTOs.AI;

namespace Auth.Services.Services.IServices
{
    public interface IAIService
    {
        Task<ChatResponseDto> ChatAsync(ChatRequestDto request);
        Task<string> GetSystemPromptAsync();
        Task<string> TestDatabaseConnectionAsync();
    }
}
