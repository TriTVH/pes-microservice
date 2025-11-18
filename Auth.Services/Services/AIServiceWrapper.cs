using Auth.Services.DTOs.AI;
using Auth.Services.DTOs.Common;
using Auth.Services.Services.IServices;
using System;
using System.Threading.Tasks;

namespace Auth.Services.Services
{
    public class AIServiceWrapper
    {
        private readonly IAIService _aiService;

        public AIServiceWrapper(IAIService aiService)
        {
            _aiService = aiService;
        }

        public async Task<ServiceResponse<ChatResponseDto>> ChatWithResponseAsync(ChatRequestDto request)
        {
            try
            {
                var result = await _aiService.ChatAsync(request);
                return ServiceResponse<ChatResponseDto>.Success("AI response generated successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<ChatResponseDto>.Error($"AI chat failed: {ex.Message}", null, "AI_CHAT_ERROR");
            }
        }

        public async Task<ServiceResponse<string>> GetSystemPromptWithResponseAsync()
        {
            try
            {
                var result = await _aiService.GetSystemPromptAsync();
                return ServiceResponse<string>.Success("System prompt retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Error($"Failed to get system prompt: {ex.Message}", null, "GET_SYSTEM_PROMPT_ERROR");
            }
        }

        public async Task<ServiceResponse<string>> TestDatabaseConnectionAsync()
        {
            try
            {
                var result = await _aiService.TestDatabaseConnectionAsync();
                return ServiceResponse<string>.Success("Database connection test completed", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<string>.Error($"Database connection test failed: {ex.Message}", null, "DATABASE_TEST_ERROR");
            }
        }
    }
}
