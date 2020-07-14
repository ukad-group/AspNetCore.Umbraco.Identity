using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Umbraco.Identity
{
    public class UmbracoPasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class, IUser
    {
        public string HashPassword(TUser user, string password)
        {
            if (password == null)
            {
                return string.Empty;
            }

            return Convert.ToBase64String(new HMACSHA1
            {
                Key = Encoding.Unicode.GetBytes(password)
            }
            .ComputeHash(Encoding.Unicode.GetBytes(password)));

        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            var providedPasswordHash = HashPassword(user, providedPassword);
            if (providedPasswordHash == hashedPassword)
            {
                return PasswordVerificationResult.Success;
            }
            return PasswordVerificationResult.Failed;
        }
    }
}
