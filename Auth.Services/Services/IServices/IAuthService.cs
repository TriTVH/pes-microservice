using Auth.Application.DTOs;
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

    }
}
