using System.ComponentModel.DataAnnotations;

namespace Auth.Services.DTOs.AI
{
    public record ChatRequestDto
    {
        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "SessionId cannot exceed 100 characters")]
        public string? SessionId { get; set; }

        [StringLength(50, ErrorMessage = "UserRole cannot exceed 50 characters")]
        public string? UserRole { get; set; }
    }
}
