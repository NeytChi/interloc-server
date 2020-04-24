using Serilog.Core;

using System;
using System.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace common
{
    public class AdminModule
    {
        private Logger log;
        private Context context;
        public AdminModule(Logger log, Context context)
        {
            this.log = log;
            this.context = context;
        }
        public string AuthToken(AdminCache cache, ref Admin admin, ref string message)
        {
            if ((admin = GetActivate(cache.admin_email, ref message)) != null) {
                if (VerifyHashedPassword(admin.admin_password, cache.admin_password)) {
                    log.Information("Authentication token for admin -> " + admin.admin_id);
                    return Token(admin);
                }
                else
                    message = "Wrong password.";
            }
            return string.Empty;
        }
        public Admin GetActivate(string adminEmail, ref string message)
        {
            Admin admin = context.admins.Where(a 
                => a.admin_email == adminEmail 
                && a.activate_hash == ""
                && a.deleted == false).FirstOrDefault();
            if (admin == null)
                message = "Unknow admin email.";
            return admin;
        }
        public string Token(Admin admin)
        {
            ClaimsIdentity identity = GetIdentity(admin);
            DateTime now = DateTime.UtcNow;
            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), 
                SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        private ClaimsIdentity GetIdentity(Admin admin)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimsIdentity.DefaultNameClaimType, admin.admin_id.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, admin.admin_email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, admin.admin_role.ToString())
            };
            ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Bearer Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
        public bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] hashedBuffer, buffer;
            if (hashedPassword == null || password == null)
                return false;
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
                return false;
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            hashedBuffer = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, hashedBuffer, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
                buffer = bytes.GetBytes(0x20);
            return ByteArraysEqual(ref hashedBuffer, ref buffer);
        }
        private bool ByteArraysEqual(ref byte[] b1,ref byte[] b2)
        {
            if (b1 == b2)
                return true;
            if (b1 == null || b2 == null)
                return false; 
            if (b1.Length != b2.Length)
                return false;
            for (int i = 0; i < b1.Length; i++) {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
    }
    public class AuthOptions
    {
        IConfigurationRoot config;
        public AuthOptions()
        {
            config = Program.serverConfiguration();
            ISSUER = config.GetValue<string>("issuer");
            AUDIENCE = config.GetValue<string>("audience");
            KEY = config.GetValue<string>("auth_key");
            LIFETIME = config.GetValue<int>("auth_lifetime");
        }
        public static string ISSUER;
        public static string AUDIENCE;
        private static string KEY;
        public static int LIFETIME = 1;
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}