using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) => _config = config;

    public string GerarToken(ContaCorrente conta)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Tenta ler do appsettings.json, se não houver, usa a chave padrão de 32 caracteres
        var secretKey = _config["Jwt:Key"] ?? "SuaChaveSuperSecretaComPeloMenos32Caracteres!!";
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                // PADRONIZAÇÃO: Usando "idcontacorrente" (minúsculo) para evitar erro 401 no Program.cs
                new Claim("idcontacorrente", conta.IdContaCorrente),
                new Claim(ClaimTypes.Name, conta.Nome),
                new Claim("numero_conta", conta.Numero.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}