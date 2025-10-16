using Auth.Services.DTOs.Common;
using Auth.Services.Services.IServices;
using Auth.Application.DTOs;
using Auth.Application.DTOs.Teacher;
using Auth.Application.DTOs.Parent;
using Auth.Application.DTOs.Common;
using AuthService.API.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Services.Services
{
    public class AuthServiceWrapper
    {
        private readonly IAuthService _authService;

        public AuthServiceWrapper(IAuthService authService)
        {
            _authService = authService;
        }

        // === AUTHENTICATION & AUTHORIZATION ===
        public async Task<ServiceResponse<LoginResponseDto>> LoginWithResponseAsync(LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return ServiceResponse<LoginResponseDto>.Success("Login successful", result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ServiceResponse<LoginResponseDto>.Error("Invalid credentials", null, "INVALID_CREDENTIALS");
            }
            catch (Exception ex)
            {
                return ServiceResponse<LoginResponseDto>.Error($"Login failed: {ex.Message}", null, "LOGIN_ERROR");
            }
        }

        public async Task<ServiceResponse<AccountDto>> RegisterWithResponseAsync(RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return ServiceResponse<AccountDto>.Success("Registration successful", result);
            }
            catch (InvalidOperationException ex)
            {
                return ServiceResponse<AccountDto>.Error($"Registration failed: {ex.Message}", null, "REGISTRATION_ERROR");
            }
            catch (Exception ex)
            {
                return ServiceResponse<AccountDto>.Error($"Registration failed: {ex.Message}", null, "REGISTRATION_ERROR");
            }
        }

        // === PASSWORD MANAGEMENT ===
        public async Task<ServiceResponse<ForgotPasswordSimpleResponseDto>> ForgotPasswordSimpleWithResponseAsync(ForgotPasswordSimpleRequestDto request)
        {
            try
            {
                var result = await _authService.ForgotPasswordSimpleAsync(request);
                return ServiceResponse<ForgotPasswordSimpleResponseDto>.Success("Reset token generated successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<ForgotPasswordSimpleResponseDto>.Error($"Failed to generate reset token: {ex.Message}", null, "FORGOT_PASSWORD_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> ForgotPasswordWithResponseAsync(string email)
        {
            try
            {
                await _authService.ForgotPasswordAsync(email);
                return ServiceResponse<object>.Success("Reset password email sent successfully", null);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Failed to send reset password email: {ex.Message}", null, "FORGOT_PASSWORD_EMAIL_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> ResetPasswordWithResponseAsync(string email, string token, string newPassword)
        {
            try
            {
                await _authService.ResetPasswordAsync(email, token, newPassword);
                return ServiceResponse<object>.Success("Password reset successfully", null);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Password reset failed: {ex.Message}", null, "RESET_PASSWORD_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> ChangePasswordWithResponseAsync(int userId, ChangePasswordRequestDto request)
        {
            try
            {
                await _authService.ChangePasswordAsync(userId, request);
                return ServiceResponse<object>.Success("Password changed successfully", null);
            }
            catch (InvalidOperationException ex)
            {
                return ServiceResponse<object>.Error($"Password change failed: {ex.Message}", null, "PASSWORD_CHANGE_ERROR");
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Password change failed: {ex.Message}", null, "PASSWORD_CHANGE_ERROR");
            }
        }

        // === PROFILE MANAGEMENT ===
        public async Task<ServiceResponse<ViewProfileDto>> GetProfileWithResponseAsync(int userId)
        {
            try
            {
                var result = await _authService.GetProfileAsync(userId);
                return ServiceResponse<ViewProfileDto>.Success("Profile retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<ViewProfileDto>.Error($"Failed to get profile: {ex.Message}", null, "PROFILE_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> UpdateProfileWithResponseAsync(int userId, UpdateProfileDto dto)
        {
            try
            {
                await _authService.UpdateProfileAsync(userId, dto);
                return ServiceResponse<object>.Success("Profile updated successfully", null);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Profile update failed: {ex.Message}", null, "PROFILE_UPDATE_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> MarkFirstLoginCompletedWithResponseAsync(int userId)
        {
            try
            {
                await _authService.MarkFirstLoginCompletedAsync(userId);
                return ServiceResponse<object>.Success("First login marked as completed", null);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Failed to mark first login completed: {ex.Message}", null, "FIRST_LOGIN_ERROR");
            }
        }

        // === TEACHER MANAGEMENT ===
        public async Task<ServiceResponse<ProfileDto>> CreateTeacherWithResponseAsync(CreateTeacherDto dto)
        {
            try
            {
                var result = await _authService.CreateTeacherAsync(dto);
                return ServiceResponse<ProfileDto>.Success("Teacher created successfully", result);
            }
            catch (InvalidOperationException ex)
            {
                return ServiceResponse<ProfileDto>.Error($"Teacher creation failed: {ex.Message}", null, "TEACHER_CREATION_ERROR");
            }
            catch (Exception ex)
            {
                return ServiceResponse<ProfileDto>.Error($"Teacher creation failed: {ex.Message}", null, "TEACHER_CREATION_ERROR");
            }
        }

        public async Task<ServiceResponse<CreateTeacherResponseDto>> CreateTeacherEmailOnlyWithResponseAsync(CreateTeacherEmailOnlyDto dto)
        {
            try
            {
                var result = await _authService.CreateTeacherEmailOnlyAsync(dto);
                return ServiceResponse<CreateTeacherResponseDto>.Success("Teacher created successfully", result);
            }
            catch (InvalidOperationException ex)
            {
                return ServiceResponse<CreateTeacherResponseDto>.Error($"Teacher creation failed: {ex.Message}", null, "TEACHER_CREATION_ERROR");
            }
            catch (Exception ex)
            {
                return ServiceResponse<CreateTeacherResponseDto>.Error($"Teacher creation failed: {ex.Message}", null, "TEACHER_CREATION_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> UpdateTeacherWithResponseAsync(int id, UpdateTeacherDto dto)
        {
            try
            {
                await _authService.UpdateTeacherAsync(id, dto);
                return ServiceResponse<object>.Success("Teacher updated successfully", null);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Teacher update failed: {ex.Message}", null, "TEACHER_UPDATE_ERROR");
            }
        }

        public async Task<ServiceResponse<IEnumerable<ProfileDto>>> GetTeachersWithResponseAsync()
        {
            try
            {
                var result = await _authService.GetTeachersAsync();
                return ServiceResponse<IEnumerable<ProfileDto>>.Success("Teachers retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<ProfileDto>>.Error($"Failed to get teachers: {ex.Message}", null, "GET_TEACHERS_ERROR");
            }
        }

        // === PARENT MANAGEMENT ===
        public async Task<ServiceResponse<ParentDto>> RegisterParentWithResponseAsync(RegisterParentRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterParentAsync(request);
                return ServiceResponse<ParentDto>.Success("Parent registration successful", result);
            }
            catch (InvalidOperationException ex)
            {
                return ServiceResponse<ParentDto>.Error($"Parent registration failed: {ex.Message}", null, "PARENT_REGISTRATION_ERROR");
            }
            catch (Exception ex)
            {
                return ServiceResponse<ParentDto>.Error($"Parent registration failed: {ex.Message}", null, "PARENT_REGISTRATION_ERROR");
            }
        }

        public async Task<ServiceResponse<IEnumerable<ProfileDto>>> GetParentsWithResponseAsync()
        {
            try
            {
                var result = await _authService.GetParentsAsync();
                return ServiceResponse<IEnumerable<ProfileDto>>.Success("Parents retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<ProfileDto>>.Error($"Failed to get parents: {ex.Message}", null, "GET_PARENTS_ERROR");
            }
        }

        // === ACCOUNT MANAGEMENT ===
        public async Task<ServiceResponse<IEnumerable<AccountDto>>> GetAllAccountsWithResponseAsync()
        {
            try
            {
                var result = await _authService.GetAllAsync();
                return ServiceResponse<IEnumerable<AccountDto>>.Success("Accounts retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<AccountDto>>.Error($"Failed to get accounts: {ex.Message}", null, "GET_ACCOUNTS_ERROR");
            }
        }

        public async Task<ServiceResponse<AccountDto>> GetAccountByIdWithResponseAsync(int id)
        {
            try
            {
                var result = await _authService.GetByIdAsync(id);
                if (result == null)
                    return ServiceResponse<AccountDto>.Error("Account not found", null, "ACCOUNT_NOT_FOUND");
                
                return ServiceResponse<AccountDto>.Success("Account retrieved successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<AccountDto>.Error($"Failed to get account: {ex.Message}", null, "GET_ACCOUNT_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> UpdateAccountWithResponseAsync(int id, UpdateAccountDto dto)
        {
            try
            {
                await _authService.UpdateAsync(id, dto);
                return ServiceResponse<object>.Success("Account updated successfully", null);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Account update failed: {ex.Message}", null, "ACCOUNT_UPDATE_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> DeleteAccountWithResponseAsync(int id)
        {
            try
            {
                await _authService.DeleteAsync(id);
                return ServiceResponse<object>.Success("Account banned successfully", null);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Account ban failed: {ex.Message}", null, "ACCOUNT_BAN_ERROR");
            }
        }

        public async Task<ServiceResponse<object>> UnbanAccountWithResponseAsync(int id)
        {
            try
            {
                await _authService.UnBanAsync(id);
                return ServiceResponse<object>.Success("Account unbanned successfully", null);
            }
            catch (Exception ex)
            {
                return ServiceResponse<object>.Error($"Account unban failed: {ex.Message}", null, "ACCOUNT_UNBAN_ERROR");
            }
        }

        // === EXPORT FUNCTIONS ===
        public async Task<ServiceResponse<ExportResult>> ExportTeachersWithResponseAsync()
        {
            try
            {
                var result = await _authService.ExportTeachersAsync();
                return ServiceResponse<ExportResult>.Success("Teachers exported successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<ExportResult>.Error($"Teacher export failed: {ex.Message}", null, "EXPORT_TEACHERS_ERROR");
            }
        }

        public async Task<ServiceResponse<ExportResult>> ExportParentsWithResponseAsync()
        {
            try
            {
                var result = await _authService.ExportParentsAsync();
                return ServiceResponse<ExportResult>.Success("Parents exported successfully", result);
            }
            catch (Exception ex)
            {
                return ServiceResponse<ExportResult>.Error($"Parent export failed: {ex.Message}", null, "EXPORT_PARENTS_ERROR");
            }
        }
    }
}
