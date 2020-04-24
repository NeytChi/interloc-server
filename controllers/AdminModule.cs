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
        private ProfileCondition condition;
        private Logger log;
        private Context context;
        public AdminModule(Logger log, Context context)
        {
            this.log = log;
            this.context = context;
            this.condition = new ProfileCondition(log);
        }
        public Admin CreateAdmin(string adminEmail, string adminPassword, int adminRole, ref string message)
        {
            if (condition.EmailIsTrue(adminEmail, ref message)
                && condition.PasswordIsTrue(adminPassword, ref message)) {
                if (GetNonDelete(adminEmail, ref message) == null) {
                    Admin admin = new Admin() {
                        admin_email = adminEmail,
                        admin_password = condition.HashPassword(adminPassword),
                        admin_role = adminRole,
                        activate_hash = "",
                        created_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        last_login_at = 0,
                    };
                    context.admins.Add(admin);
                    context.SaveChanges();
                    log.Information("Add new admin, id ->" + admin.admin_id);
                    return admin;
                }
                else 
                    message = "Admin with this email is exist";
            }            
            return null;
        }        
        public string AuthToken(AdminCache cache, ref Admin admin, ref string message)
        {
            if ((admin = GetActivate(cache.admin_email, ref message)) != null) {
                if (condition.VerifyHashedPassword(admin.admin_password, cache.admin_password)) {
                    log.Information("Authentication token for admin -> " + admin.admin_id);
                    return Token(admin);
                }
                else
                    message = "Wrong password.";
            }
            return string.Empty;
        }
        public Admin GetNonDelete(string adminEmail, ref string message)
        {
            Admin admin = context.admins.Where(a 
                => a.admin_email == adminEmail 
                && a.deleted == false).FirstOrDefault();
            if (admin == null)
                message = "Unknow admin email.";
            return admin;
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