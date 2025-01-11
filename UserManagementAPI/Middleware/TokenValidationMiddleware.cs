using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;
    private readonly string _secretKey;

    public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger, string secretKey)
    {
        _next = next;
        _logger = logger;
        _secretKey = secretKey;

        GenerateKey(secretKey);
    }

    private void GenerateKey(string secretKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);
        var signingKey = new SymmetricSecurityKey(key)
        {
            KeyId = "poop"
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("test", "value") }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        Console.WriteLine(tokenHandler.WriteToken(token));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Unauthorized - Missing Token");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized - Missing Token");
            return;
        }

        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring("Bearer ".Length).Trim();
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey))
            {
                KeyId = "poop"
            };
            var validation = new TokenValidationParameters
            {
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                IssuerSigningKey = signingKey,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                RequireAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validation, out SecurityToken validatedToken);

            await _next(context);
        }
        catch (SecurityTokenSignatureKeyNotFoundException ex)
        {
            _logger.LogError(ex, "Unauthorized - Token validation failed: Signature key not found");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized - Token validation failed: Signature key not found");
        }
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
// public static class TokenValidationMiddlewareExtensions
// {
//     public static IApplicationBuilder UseTokenValidationMiddleware(this IApplicationBuilder builder, string secretKey)
//     {
//         return builder.UseMiddleware<TokenValidationMiddleware>(secretKey);
//     }
// }