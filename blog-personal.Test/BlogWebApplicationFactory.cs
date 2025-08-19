using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text;
using blog_personal.Servicios;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace blog_personal.Test;

public class BlogWebApplicationFactory : WebApplicationFactory<global::Program>
{
    private readonly string _testDataPath;

    public BlogWebApplicationFactory()
    {
        _testDataPath = Path.Combine(
            Path.GetTempPath(), 
            "blog_tests",
            Guid.NewGuid().ToString()
        );

        Directory.CreateDirectory(Path.Combine(_testDataPath, "wwwroot", "data", "posts"));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Configuración correcta del ContentRoot
        var projectDir = Directory.GetCurrentDirectory();
        var solutionDir = Path.Combine(projectDir, "..", "blog-personal");
        if (Directory.Exists(solutionDir))
        {
            builder.UseContentRoot(solutionDir);
        }

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtKey"] = "SuperSecretTestingKey1234567890123456789012",
                ["PasswordSalt"] = "SuperSecretTestingSalt1234567890"
            });
        });

        builder.ConfigureServices(services =>
        {
            var authServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthService));
            if (authServiceDescriptor != null)
            {
                services.Remove(authServiceDescriptor);
            }

            var postServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPostService));
            if (postServiceDescriptor != null)
            {
                services.Remove(postServiceDescriptor);
            }

            string adminFilePath = Path.Combine(_testDataPath, "wwwroot", "data", "admin.json");
            var adminDir = Path.GetDirectoryName(adminFilePath);
            Directory.CreateDirectory(adminDir!);

            var passwordSalt = "SuperSecretTestingSalt1234567890";
            var defaultAdmin = new
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123", passwordSalt)
            };

            var json = JsonSerializer.Serialize(defaultAdmin, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(adminFilePath, json);

            Console.WriteLine($"admin.json creado en: {adminFilePath}");

            
            services.AddSingleton<IAuthService>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                return new AuthService(config, adminFilePath);
            });

            services.AddSingleton<IPostService>(provider =>
            {
                var testBasePath = Path.Combine(_testDataPath, "wwwroot");
                return new PostService(testBasePath);
            });
        });
    }
    private static string HashPassword(string password, string salt)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToBase64String(hashedBytes);
    }
    
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            System.Text.Encodings.Web.UrlEncoder encoder, 
            ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] 
            { 
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("isOwner", "true")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Directory.Exists(_testDataPath))
        {
            try { Directory.Delete(_testDataPath, true); } catch { }
        }
        base.Dispose(disposing);
    }
}
