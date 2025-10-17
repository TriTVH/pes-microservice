using System;
using System.Collections.Generic;

namespace Auth.Services.DTOs.AI
{
    public record ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? SuggestedActions { get; set; }
        public List<string> RelatedTopics { get; set; } = new();
    }
}
