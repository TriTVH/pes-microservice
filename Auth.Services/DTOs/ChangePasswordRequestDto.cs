using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.DTOs
{
    public record ChangePasswordRequestDto(
        [Required(ErrorMessage = "Current password is required")]
        string CurrentPassword,
        
        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        string NewPassword,
        
        [Required(ErrorMessage = "Confirm password is required")]
        string ConfirmPassword
    );
}
