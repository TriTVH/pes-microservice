using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.Entities
{
    public class Account
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string? Name { get; private set; }
        public string? Phone { get; private set; }
        public string? Address { get; private set; }
        public string? AvatarUrl { get; private set; }
        public string? Gender { get; private set; }
        public string Role { get; private set; }
        public string Status { get; private set; }
        public string? PasswordHash { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Account() { } // EF Core

        public Account(string email, string name, string role)
        {
            Email = email;
            Name = name;
            Role = role;
            Status = "ACCOUNT_ACTIVE";
            CreatedAt = DateTime.UtcNow;
        }

        public void SetPasswordHash(string hash)
        {
            PasswordHash = hash;
        }
        public bool VerifyPassword(string plainPassword, Func<string, string, bool> verifyFunc)
        {
            if (string.IsNullOrWhiteSpace(PasswordHash))
                return false;

            return verifyFunc(plainPassword, PasswordHash);
        }
        public void UpdateProfile(string name, string phone, string address, string avatarUrl, string gender)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            if (!string.IsNullOrWhiteSpace(phone))
                Phone = phone;

            if (!string.IsNullOrWhiteSpace(address))
                Address = address;

            if (!string.IsNullOrWhiteSpace(avatarUrl))
                AvatarUrl = avatarUrl;

            if (!string.IsNullOrWhiteSpace(gender))
                Gender = gender;
        }

        public void ChangeRole(string role)
        {
            if (!string.IsNullOrWhiteSpace(role))
                Role = role;
        }

        public void ChangeStatus(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
                Status = status;
        }

        public void Ban()
        {
            Status = "ACCOUNT_BAN";
        }
    }
}
