using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Sentaur.Leaderboard.Api;

public class JwtTokenHolder
{
    public string Token { get; }
    
    public JwtTokenHolder(WebApplicationBuilder builder)
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Failed to get 'Jwt:Key'")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(issuer: issuer, signingCredentials: credentials);
        Token = new JwtSecurityTokenHandler().WriteToken(token);
    }
}