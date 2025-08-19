using blog_personal.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace blog_personal.Servicios
{
    public class AuthService : IAuthService
    {
        private readonly string _jwtKey;
        private readonly string _passwordSalt;
        private readonly string _adminFilePath;

        private readonly Task _initializationTask;
        
        public AuthService(IConfiguration configuration, string adminFilePath = null)
        {
            _jwtKey = configuration["JwtKey"] ?? throw new InvalidOperationException("JwtKey no configurado");
            _passwordSalt = configuration["PasswordSalt"] ?? throw new InvalidOperationException("PasswordSalt no configurado");
            _adminFilePath = adminFilePath ?? "wwwroot/data/admin.json";
        }
        public async Task InitializeAsync()
        {
            await InitializeAdminAsync();
        }
        private async Task InitializeAdminAsync()
        {
            if (!File.Exists(_adminFilePath))
            {
                var defaultAdmin = new AdminUser
                {
                    Username = "admin",
                    PasswordHash = HashPassword("admin123")
                };
                
                await SaveAdminAsync(defaultAdmin);
            }
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var admin = await LoadAdminAsync();
            
            if (admin == null) 
                return null;
    
            if (admin.PasswordHash != null && (admin.Username != username || !VerifyPassword(password, admin.PasswordHash)))
                return null;
    
            return GenerateJwtToken(admin);
        }
        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            var admin = await LoadAdminAsync();
            
            if (admin == null)
            {
                return false;
            }
    
            if (admin.PasswordHash != null && (!VerifyPassword(currentPassword, admin.PasswordHash)))
                return false;
    
            admin.PasswordHash = HashPassword(newPassword);
            await SaveAdminAsync(admin);
    
            return true;
        }
        private string GenerateJwtToken(AdminUser admin)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, admin.Username),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("isOwner", "true")
            };
        
            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddHours(4), // 4hs
                claims: claims,
                signingCredentials: credentials
            );
        
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<AdminUser?> LoadAdminAsync()
        {
            Console.WriteLine($"🔍 Buscando admin.json en: {_adminFilePath}");
            Console.WriteLine($"🔍 Archivo existe: {File.Exists(_adminFilePath)}");
    
            if (!File.Exists(_adminFilePath))
            {
                Console.WriteLine("❌ admin.json no encontrado");
                return null;
            }
    
            try
            {
                var json = await File.ReadAllTextAsync(_adminFilePath);
                Console.WriteLine($"✅ admin.json contenido: {json}");
                return JsonSerializer.Deserialize<AdminUser>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error leyendo admin.json: {ex.Message}");
                return null;
            }
        }
        private async Task SaveAdminAsync(AdminUser admin)
        {
            
            var json = JsonSerializer.Serialize(admin, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_adminFilePath, json);
        }
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + _passwordSalt));
            return Convert.ToBase64String(hashedBytes);
        }
        private bool VerifyPassword(string password, string storedHash)
        {
            var hashedInput = HashPassword(password);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hashedInput),
                Encoding.UTF8.GetBytes(storedHash)
            );
        }
    }
}

public class AuthServiceOptions
{
    public string AdminFilePath { get; set; } = "wwwroot/data/admin.json";
}