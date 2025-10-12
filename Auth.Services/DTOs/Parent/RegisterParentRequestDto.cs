using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auth.Services.Validation;

namespace Auth.Application.DTOs.Parent
{
    public record RegisterParentRequestDto(
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        string Email, 
        
        [Required(ErrorMessage = "Password is required")]
        string Password, 
        
        [Required(ErrorMessage = "Name is required")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        string Name,
        
        [Required(ErrorMessage = "Job is required")]
        string Job,
        
        [Required(ErrorMessage = "Relationship to child is required")]
        [RelationshipValidation(ErrorMessage = "Relationship must be either 'Cha' or 'Mẹ'")]
        string RelationshipToChild
    );
}
