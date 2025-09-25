using Auth.Application.DTOs;
using Auth.Application.DTOs.Common;
using Auth.Application.DTOs.Teacher;
using Auth.Application.Security;
using Auth.Domain.Entities;
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
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;
        public AuthService(IAccountRepository repo,
                              IPasswordHasher<Account> passwordHasher,
                              IMapper mapper,
                              IConfiguration config, IJwtTokenGenerator jwt, IEmailSender emailSender, IMemoryCache cache)
        {
            _repo = repo;
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

            acc.UpdateProfile(dto.Name, dto.Phone, dto.Address, dto.AvatarUrl, dto.Gender);

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


        // --- Profile of current user ---
        public async Task<ViewProfileDto> GetProfileAsync(int userId)
        {
            var acc = await _repo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Account not found");
            return new ViewProfileDto(acc.Id, acc.Email, acc.Name, acc.Phone, acc.Address, acc.AvatarUrl, acc.Gender, acc.CreatedAt);
        }

        public async Task UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var acc = await _repo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Account not found");
            acc.UpdateProfile(dto.Name ?? acc.Name, dto.Phone ?? acc.Phone, dto.Address ?? acc.Address, dto.AvatarUrl ?? acc.AvatarUrl, dto.Gender ?? acc.Gender);
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

        public async Task UpdateTeacherAsync(int id, UpdateTeacherDto dto)
        {
            var acc = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Teacher not found");
            if (acc.Role != "TEACHER") throw new InvalidOperationException("Not a teacher");
            acc.UpdateProfile(dto.Name ?? acc.Name, dto.Phone ?? acc.Phone, dto.Address ?? acc.Address, dto.AvatarUrl ?? acc.AvatarUrl, dto.Gender ?? acc.Gender);
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
    }
}
