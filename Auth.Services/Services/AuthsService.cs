using Auth.Application.DTOs;
using Auth.Application.DTOs.Common;
using Auth.Application.DTOs.Teacher;
using Auth.Application.DTOs.Parent;
using Auth.Application.Security;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Repositories;
using Auth.Services.Services.IServices;
using AuthService.API.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _repo;
        private readonly IParentRepository _parentRepo;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;
        public AuthService(IAccountRepository repo,
                              IParentRepository parentRepo,
                              IPasswordHasher<Account> passwordHasher,
                              IMapper mapper,
                              IConfiguration config, IJwtTokenGenerator jwt, IEmailSender emailSender, IMemoryCache cache)
        {
            _repo = repo;
            _parentRepo = parentRepo;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _config = config;
            _jwt = jwt;
            _emailSender = emailSender;
            _cache = cache;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {

            var acc = await _repo.GetByEmailAsync(request.Email);
            if (acc == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            var result = _passwordHasher.VerifyHashedPassword(acc, acc.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid credentials");
          
            var token = _jwt.GenerateToken(acc);

            return new LoginResponseDto(token, acc.Role);
        }

        public async Task<AccountDto> RegisterAsync(RegisterRequestDto request)
        {
            var exists = await _repo.GetByEmailAsync(request.Email);
            if (exists != null) throw new InvalidOperationException("Email already registered");

            var domain = new Account(request.Email, request.Name, "PARENT");
            domain.SetPasswordHash(_passwordHasher.HashPassword(domain, request.Password));

            await _repo.AddAsync(domain);

            return _mapper.Map<AccountDto>(domain);
        }
        public async Task<IEnumerable<AccountDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(a => _mapper.Map<AccountDto>(a));
        }

        public async Task<AccountDto?> GetByIdAsync(int id)
        {
            var a = await _repo.GetByIdAsync(id);
            return a == null ? null : _mapper.Map<AccountDto>(a);
        }

        public async Task UpdateAsync(int id, UpdateAccountDto dto)
        {
            var acc = await _repo.GetByIdAsync(id);
            if (acc == null) throw new KeyNotFoundException("Account not found");

            acc.UpdateProfile(dto.Name, dto.Phone, dto.Address, dto.AvatarUrl, dto.Gender, dto.IdentityNumber);

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
               
                typeof(Account).GetProperty("Role")?.SetValue(acc, dto.Role);
            }
            if (!string.IsNullOrWhiteSpace(dto.Status))
                typeof(Account).GetProperty("Status")?.SetValue(acc, dto.Status);

            await _repo.UpdateAsync(acc);
        }

        public async Task DeleteAsync(int id)
        {
            var acc = await _repo.GetByIdAsync(id);
            if (acc == null) throw new KeyNotFoundException("Account not found");

            acc.Ban();
            await _repo.UpdateAsync(acc);
        }
        public async Task UnBanAsync(int id)
        {
            var acc = await _repo.GetByIdAsync(id);
            if (acc == null) throw new KeyNotFoundException("Account not found");

            acc.UnBan();
            await _repo.UpdateAsync(acc);
        }
        private string GenerateJwtToken(Account user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // --- Forgot password: tạo token và gửi email ---
        public async Task ForgotPasswordAsync(string email)
        {
            var account = await _repo.GetByEmailAsync(email);
            if (account == null)
                throw new InvalidOperationException("Email not found");

            var resetToken = Guid.NewGuid().ToString();
            _cache.Set(email, resetToken, TimeSpan.FromMinutes(15));

            var resetLink = $"https://yourdomain.com/reset-password?token={resetToken}&email={email}";
            var subject = "Reset your password";
            var body = $@"<h3>Password Reset</h3>
                          <p>Click <a href='{resetLink}'>here</a> to reset your password.</p>";

            await _emailSender.SendEmailAsync(email, subject, body);
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (!_cache.TryGetValue(email, out string? savedToken) || savedToken != token)
                throw new InvalidOperationException("Invalid or expired token");

            var account = await _repo.GetByEmailAsync(email);
            if (account == null)
                throw new InvalidOperationException("Account not found");

            
            var newHash = _passwordHasher.HashPassword(account, newPassword);
            account.SetPasswordHash(newHash);

            await _repo.UpdateAsync(account);

            _cache.Remove(email);
        }

       
        public async Task<ForgotPasswordSimpleResponseDto> ForgotPasswordSimpleAsync(ForgotPasswordSimpleRequestDto request)
        {
            var account = await _repo.GetByEmailAsync(request.Email);
            if (account == null)
                throw new InvalidOperationException("Email not found");

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString();
            var expiresAt = DateTime.UtcNow.AddMinutes(15);
            
            // Store token in cache
            _cache.Set(request.Email, resetToken, TimeSpan.FromMinutes(15));

            return new ForgotPasswordSimpleResponseDto(
                resetToken,
                "Reset token generated successfully. Use this token to reset your password.",
                expiresAt
            );
        }


       
        public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            // Validate confirm password
            if (request.NewPassword != request.ConfirmPassword)
                throw new ArgumentException("Confirm password does not match new password");

            // Get the account
            var account = await _repo.GetByIdAsync(userId);
            if (account == null)
                throw new KeyNotFoundException("Account not found");

            // Verify current password
            var currentPasswordResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.CurrentPassword);
            if (currentPasswordResult == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Current password is incorrect");

            // Check if new password is different from current password
            var newPasswordResult = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, request.NewPassword);
            if (newPasswordResult == PasswordVerificationResult.Success)
                throw new InvalidOperationException("New password must be different from current password");

            // Hash and set new password
            var newHash = _passwordHasher.HashPassword(account, request.NewPassword);
            account.SetPasswordHash(newHash);

            // Update account
            await _repo.UpdateAsync(account);
        }


        // --- Profile of current user ---
        public async Task<ViewProfileDto> GetProfileAsync(int userId)
        {
            var acc = await _repo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Account not found");
            return new ViewProfileDto(acc.Id, acc.Email, acc.Name, acc.Phone, acc.Address, acc.AvatarUrl, acc.Gender, acc.IdentityNumber, acc.Role, acc.Status, acc.CreatedAt, acc.FirstLogin);
        }

        public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var acc = await _repo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Account not found");
            acc.UpdateProfile(dto.Name ?? acc.Name, dto.Phone ?? acc.Phone, dto.Address ?? acc.Address, dto.AvatarUrl ?? acc.AvatarUrl, dto.Gender ?? acc.Gender, dto.IdentityNumber ?? acc.IdentityNumber);
            await _repo.UpdateAsync(acc);
        }

        public async Task MarkFirstLoginCompletedAsync(int userId)
        {
            var acc = await _repo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Account not found");
            acc.SetFirstLoginCompleted();
            await _repo.UpdateAsync(acc);
        }

        // --- Teacher management ---
        public async Task<ProfileDto> CreateTeacherAsync(CreateTeacherDto dto)
        {
            var exists = await _repo.GetByEmailAsync(dto.Email);
            if (exists != null) throw new InvalidOperationException("Email already registered");

            var acc = new Account(dto.Email, dto.Name, "TEACHER");
            acc.SetPasswordHash(_passwordHasher.HashPassword(acc, dto.Password));
            await _repo.AddAsync(acc);
            return new ProfileDto(acc.Id, acc.Email, acc.Name, acc.Phone, acc.Address, acc.AvatarUrl, acc.Gender, acc.Role);
        }


        public async Task<CreateTeacherResponseDto> CreateTeacherEmailOnlyAsync(CreateTeacherEmailOnlyDto dto)
        {
            var exists = await _repo.GetByEmailAsync(dto.Email);
            if (exists != null) throw new InvalidOperationException("Email already registered");

            // Extract name from email (before @)
            var name = dto.Email.Split('@')[0];
            
            // Generate random password
            var generatedPassword = GenerateRandomPassword();
            
            // Create account with role TEACHER
            var acc = new Account(dto.Email, name, "TEACHER");
            acc.SetPasswordHash(_passwordHasher.HashPassword(acc, generatedPassword));
            await _repo.AddAsync(acc);

            // Send email with credentials
            var subject = "Teacher Account Created - PES System";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                    <h2 style='color: #2c3e50; text-align: center; margin-bottom: 30px;'>Welcome to PES System</h2>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 6px; margin-bottom: 20px;'>
                        <h3 style='color: #495057; margin-top: 0;'>Your Teacher Account Has Been Created</h3>
                        <p style='margin-bottom: 10px;'><strong>Username (Email):</strong> {dto.Email}</p>
                        <p style='margin-bottom: 10px;'><strong>Password:</strong> <span style='background-color: #e9ecef; padding: 4px 8px; border-radius: 4px; font-family: monospace; font-weight: bold;'>{generatedPassword}</span></p>
                        <p style='margin-bottom: 10px;'><strong>Name:</strong> {name}</p>
                        <p style='margin-bottom: 0;'><strong>Role:</strong> Teacher</p>
                    </div>
                    
                    <div style='background-color: #d4edda; padding: 15px; border-radius: 6px; margin-bottom: 20px;'>
                        <h4 style='color: #155724; margin-top: 0;'>Important Security Information:</h4>
                        <ul style='margin-bottom: 0; color: #155724;'>
                            <li>Please login and change your password immediately</li>
                            <li>Keep your credentials secure and confidential</li>
                            <li>Contact HR if you have any issues accessing your account</li>
                        </ul>
                    </div>
                    
                    <div style='background-color: #e3f2fd; padding: 15px; border-radius: 6px; margin-bottom: 20px;'>
                        <h4 style='color: #1976d2; margin-top: 0;'>Next Steps:</h4>
                        <ol style='margin-bottom: 0; color: #1976d2;'>
                            <li>Login to the system using the credentials above</li>
                            <li>Change your password to something secure</li>
                            <li>Complete your profile information</li>
                        </ol>
                    </div>
                    
                    <div style='text-align: center; margin-top: 30px;'>
                        <p style='color: #6c757d; font-size: 14px;'>This is an automated message from the PES System</p>
                        <p style='color: #6c757d; font-size: 14px;'>Please do not reply to this email</p>
                    </div>
                    
                    <p style='margin-top: 30px;'>Best regards,<br><strong>HR Team - PES System</strong></p>
                </div>
            ";

            try
            {
                await _emailSender.SendEmailAsync(dto.Email, subject, body);
                
                return new CreateTeacherResponseDto(
                    acc.Id,
                    acc.Email,
                    name,
                    generatedPassword,
                    "Teacher created successfully. Login credentials sent via email."
                );
            }
            catch (Exception ex)
            {
                // If email fails, still return success but with manual credential info
                return new CreateTeacherResponseDto(
                    acc.Id,
                    acc.Email,
                    name,
                    generatedPassword,
                    $"Teacher created successfully. Email sending failed - please provide credentials manually:\n" +
                    $"Username: {dto.Email}\n" +
                    $"Password: {generatedPassword}\n" +
                    $"Error: {ex.Message}"
                );
            }
        }


        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task UpdateTeacherAsync(int id, UpdateTeacherDto dto)
        {
            var acc = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Teacher not found");
            if (acc.Role != "TEACHER") throw new InvalidOperationException("Not a teacher");
            acc.UpdateProfile(dto.Name ?? acc.Name, dto.Phone ?? acc.Phone, dto.Address ?? acc.Address, dto.AvatarUrl ?? acc.AvatarUrl, dto.Gender ?? acc.Gender, dto.IdentityNumber ?? acc.IdentityNumber);
            await _repo.UpdateAsync(acc);
        }

        public async Task<IEnumerable<ProfileDto>> GetTeachersAsync()
        {
            var all = await _repo.GetAllAsync();
            return all.Where(a => a.Role == "TEACHER").Select(a => new ProfileDto(a.Id, a.Email, a.Name, a.Phone, a.Address, a.AvatarUrl, a.Gender, a.Role));
        }

        // --- Parent list ---
        public async Task<IEnumerable<ProfileDto>> GetParentsAsync()
        {
            var all = await _repo.GetAllAsync();
            return all.Where(a => a.Role == "PARENT").Select(a => new ProfileDto(a.Id, a.Email, a.Name, a.Phone, a.Address, a.AvatarUrl, a.Gender, a.Role));
        }

        // --- Export teachers/parents as Excel ---
        public async Task<ExportResult> ExportTeachersAsync()
        {
            var teachers = (await GetTeachersAsync()).ToList();
            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("Teachers");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Email";
            ws.Cell(1, 3).Value = "Name";
            ws.Cell(1, 4).Value = "Phone";
            ws.Cell(1, 5).Value = "Address";
            ws.Cell(1, 6).Value = "Gender";

            for (int i = 0; i < teachers.Count; i++)
            {
                var r = i + 2;
                var t = teachers[i];
                ws.Cell(r, 1).Value = t.Id;
                ws.Cell(r, 2).Value = t.Email;
                ws.Cell(r, 3).Value = t.Name;
                ws.Cell(r, 4).Value = t.Phone;
                ws.Cell(r, 5).Value = t.Address;
                ws.Cell(r, 6).Value = t.Gender;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return new ExportResult(ms.ToArray(), $"teachers_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
        }

        public async Task<ExportResult> ExportParentsAsync()
        {
            var parents = (await GetParentsAsync()).ToList();
            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("Parents");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Email";
            ws.Cell(1, 3).Value = "Name";
            ws.Cell(1, 4).Value = "Phone";
            ws.Cell(1, 5).Value = "Address";
            ws.Cell(1, 6).Value = "Gender";

            for (int i = 0; i < parents.Count; i++)
            {
                var r = i + 2;
                var p = parents[i];
                ws.Cell(r, 1).Value = p.Id;
                ws.Cell(r, 2).Value = p.Email;
                ws.Cell(r, 3).Value = p.Name;
                ws.Cell(r, 4).Value = p.Phone;
                ws.Cell(r, 5).Value = p.Address;
                ws.Cell(r, 6).Value = p.Gender;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return new ExportResult(ms.ToArray(), $"parents_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx");
        }

        // --- Parent specific registration ---
        public async Task<ParentDto> RegisterParentAsync(RegisterParentRequestDto request)
        {
            // Validate relationship to child
            if (!RelationshipTypeExtensions.IsValid(request.RelationshipToChild))
            {
                throw new ArgumentException("Relationship to child must be either 'Cha' or 'Mẹ'");
            }

            // Check if email already exists
            var exists = await _repo.GetByEmailAsync(request.Email);
            if (exists != null) 
                throw new InvalidOperationException("Email already registered");

            // Create Account with PARENT role, ACCOUNT_ACTIVE status
            var account = new Account(request.Email, request.Name, "PARENT");
            account.SetPasswordHash(_passwordHasher.HashPassword(account, request.Password));
            
            // Save Account first to get the ID
            await _repo.AddAsync(account);

            // Create Parent entity with additional information
            var parent = new Parent
            {
                Job = request.Job,
                RelationshipToChild = request.RelationshipToChild,
                AccountId = account.Id
            };

            // Save Parent entity
            await _parentRepo.AddAsync(parent);

            // Return ParentDto with all information
            return new ParentDto(
                parent.Id,
                account.Email,
                account.Name,
                parent.Job,
                parent.RelationshipToChild,
                account.Id
            );
        }
    }
}
