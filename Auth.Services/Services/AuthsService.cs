using Auth.Application.DTOs;
using Auth.Application.Security;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Services.Services.IServices;
using AuthService.API.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        public AuthService(IAccountRepository repo,
                              IPasswordHasher<Account> passwordHasher,
                              IMapper mapper,
                              IConfiguration config, IJwtTokenGenerator jwt)
        {
            _repo = repo;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _config = config;
            _jwt = jwt;
        }

        //public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        //{
        //    var acc = await _repo.GetByEmailAsync(request.Email);
        //    if (acc == null) throw new UnauthorizedAccessException("Invalid credentials");

        //    var result = _passwordHasher.VerifyHashedPassword(acc, acc.PasswordHash, request.Password);
        //    if (result == PasswordVerificationResult.Failed)
        //        throw new UnauthorizedAccessException("Invalid credentials");

        //    var token = GenerateJwtToken(acc);
        //    return new LoginResponseDto(token, acc.Role);
        //}
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // Tìm account theo email
            var acc = await _repo.GetByEmailAsync(request.Email);
            if (acc == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            // Kiểm tra mật khẩu bằng IPasswordHasher<Account>
            var result = _passwordHasher.VerifyHashedPassword(acc, acc.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Invalid credentials");

            // Sinh JWT token
            var token = _jwt.GenerateToken(acc);

            return new LoginResponseDto(token, acc.Role);
        }

        public async Task<AccountDto> RegisterAsync(RegisterRequestDto request)
        {
            var exists = await _repo.GetByEmailAsync(request.Email);
            if (exists != null) throw new InvalidOperationException("Email already registered");

            var domain = new Account(request.Email, request.Name, request.Role);
            domain.SetPasswordHash(_passwordHasher.HashPassword(domain, request.Password));

            await _repo.AddAsync(domain); 

            var dto = _mapper.Map<AccountDto>(domain);
            return dto;
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

            // Optionally allow changing role/status
            // using reflection or methods; for simplicity:
            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                // no direct setter, use reflection or design method; here assume internal ability:
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
    }
}
