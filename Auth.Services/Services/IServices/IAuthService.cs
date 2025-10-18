using Auth.Application.DTOs;
using Auth.Application.DTOs.Common;
using Auth.Application.DTOs.Teacher;
using Auth.Application.DTOs.Parent;
using AuthService.API.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Services.Services.IServices
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<AccountDto> RegisterAsync(RegisterRequestDto request);
        Task<IEnumerable<AccountDto>> GetAllAsync();
        Task<AccountDto?> GetByIdAsync(int id);
        Task UpdateAsync(int id, UpdateAccountDto dto);
        Task DeleteAsync(int id); // sets Status = ACCOUNT_BAN
        Task UnBanAsync(int id);

        // --- Forgot password / Reset password ---
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(string email, string token, string newPassword);
        
        // --- Simple forgot password (no email) ---
        Task<ForgotPasswordSimpleResponseDto> ForgotPasswordSimpleAsync(ForgotPasswordSimpleRequestDto request);
        
        // --- Change password for logged in user ---
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);

        // --- Profile ---
        Task<ViewProfileDto> GetProfileAsync(int userId);
        Task UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task MarkFirstLoginCompletedAsync(int userId);

        // --- Teacher management ---
        Task<ProfileDto> CreateTeacherAsync(CreateTeacherDto dto);
        Task<CreateTeacherResponseDto> CreateTeacherEmailOnlyAsync(CreateTeacherEmailOnlyDto dto);
        Task UpdateTeacherAsync(int id, UpdateTeacherDto dto);
        Task<IEnumerable<ProfileDto>> GetTeachersAsync();

        // --- Parent list ---
        Task<IEnumerable<ProfileDto>> GetParentsAsync();

        // --- Export teachers/parents ---
        Task<ExportResult> ExportTeachersAsync();
        Task<ExportResult> ExportParentsAsync();

        // --- Parent specific registration ---
        Task<ParentDto> RegisterParentAsync(RegisterParentRequestDto request);
    }
}
