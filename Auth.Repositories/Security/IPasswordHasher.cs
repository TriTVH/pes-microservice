using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
namespace Auth.Infrastructure.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string plain, string hash);
    }

    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        public bool Verify(string plain, string hash) => BCrypt.Net.BCrypt.Verify(plain, hash);
    }
}
