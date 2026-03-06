using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Infrastructure.Data;
using BankMore.Transferencia.Infrastructure.Repositories;
using BankMore.Transferencia.Application.Interfaces;
using BankMore.Transferencia.Infrastructure.ExternalServices;
using BankMore.Transferencia.Application.Handlers;
using BankMore.Transferencia.Application.Commands.EfetuarTransferencia;
using BankMore.Transferencia.Application.DTOs;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÕES SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore Transferencia API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            }, Array.Empty<string>()
        }
    });
});

// --- 2. INJEÇÃO DE DEPENDÊNCIA ---
builder.Services.AddScoped<DbSessionTransferencia>();
builder.Services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();

// AJUSTE CRÍTICO: Configuração de HTTPS e Bypass de SSL para Localhost
builder.Services.AddHttpClient<IContaCorrenteService, ContaCorrenteService>(client =>
{
    // Forçamos a porta 7197 em HTTPS conforme seus logs
    var url = builder.Configuration["ExternalApis:ContaCorrenteUrl"] ?? "https://localhost:7197/";
    client.BaseAddress = new Uri(url);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Permite certificados auto-assinados do .NET em localhost
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EfetuarTransferenciaHandler).Assembly));

// --- 3. SEGURANÇA ---
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "ChaveMestraBankMore2024Segura!";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// --- 4. ENDPOINT ---
app.MapPost("/api/transferencia", async (
    [FromBody] TransferenciaRequest request,
    IMediator mediator,
    ClaimsPrincipal user,
    HttpContext context) =>
{
    var idContaOrigem = user.FindFirst("idcontacorrente")?.Value;
    var numContaOrigem = user.FindFirst("numero_conta")?.Value; // Pega o número (ex: 1001) do JWT

    var token = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "").Trim();

    var command = new EfetuarTransferenciaCommand(
        request.IdRequisicao,
        request.ContaDestino,
        (double)request.Valor,
        token,
        idContaOrigem,
        numContaOrigem // <--- Passando o novo parâmetro
    );

    var sucesso = await mediator.Send(command);
    return sucesso ? Results.NoContent() : Results.BadRequest();
});

app.Run();
// No final do Program.cs da API
public partial class Program { }