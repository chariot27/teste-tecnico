using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data;
using BankMore.ContaCorrente.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÕES E STRINGS DE CONEXÃO ---
// Busca a string do appsettings.json configurada anteriormente
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("String de conexão 'DefaultConnection' não encontrada.");

// Busca a chave secreta do JWT configurada no appsettings.json
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? "ChaveSegurancaPadraoDe32CaracteresMinimo!";
var key = Encoding.ASCII.GetBytes(jwtSecret);

// --- 2. REGISTRO DE DEPENDÊNCIAS (DI) ---
// Resolve o erro de injeção de string passando a conexão para a Session
builder.Services.AddScoped<DbSessionContaCorrente>(provider =>
    new DbSessionContaCorrente(connectionString));

builder.Services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
builder.Services.AddScoped<IMovimentoRepository, MovimentoRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CadastrarContaHandler).Assembly);
});

// --- 3. SEGURANÇA E AUTENTICAÇÃO (JWT) ---
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// --- 4. CONFIGURAÇÃO DO SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token desta forma: Bearer {seu_token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --- 5. MIDDLEWARE PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// --- 6. ENDPOINTS (MINIMAL APIs) ---

// Cadastro de Conta
app.MapPost("/api/contacorrente/cadastro", async ([FromBody] CadastrarContaCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result;
})
.WithName("CadastrarConta").WithOpenApi();

// Login
app.MapPost("/api/contacorrente/login", async ([FromBody] LoginCommand command, IMediator mediator) =>
{
    // O 'result' já é um IResult vindo do Handler (com Ok, Unauthorized, etc.)
    var result = await mediator.Send(command);

    // Retorne diretamente para que o ASP.NET execute o resultado, 
    // em vez de serializá-lo como um objeto.
    return result;
})
.WithName("Login").WithOpenApi();

// Inativação de Conta
app.MapPatch("/api/contacorrente/inativar", async ([FromBody] InativarContaCommand command, IMediator mediator, ClaimsPrincipal user) =>
{
    var idConta = user.FindFirst("idcontacorrente")?.Value;
    if (string.IsNullOrEmpty(idConta))
        return Results.Json(new { message = "Não autorizado", type = "USER_UNAUTHORIZED" }, statusCode: 401);

    command.IdContaCorrente = idConta;
    var result = await mediator.Send(command);
    return Results.Ok(result);
})
.WithName("InativarConta").WithOpenApi().RequireAuthorization();

// Movimentação (Crédito/Débito)
app.MapPost("/api/contacorrente/movimentacao", async ([FromBody] MovimentarContaCommand command, IMediator mediator, ClaimsPrincipal user) =>
{
    var idContaToken = user.FindFirst("idcontacorrente")?.Value;
    if (string.IsNullOrEmpty(idContaToken))
        return Results.Json(new { message = "Acesso negado", type = "USER_UNAUTHORIZED" }, statusCode: 403);

    command.ContaIdDoToken = idContaToken;

    try
    {
        var sucesso = await mediator.Send(command);
        return sucesso ? Results.NoContent() : Results.BadRequest(new { message = "Erro ao processar" });
    }
    catch (Exception ex)
    {
        return Results.Json(new { message = ex.Message, type = "BUSINESS_ERROR" }, statusCode: 400);
    }
})
.WithName("MovimentarConta").WithOpenApi().RequireAuthorization();

// Consulta de Saldo
app.MapGet("/api/contacorrente/saldo", async (IMediator mediator, ClaimsPrincipal user) =>
{
    var idConta = user.FindFirst("idcontacorrente")?.Value;
    if (string.IsNullOrEmpty(idConta))
        return Results.Json(new { message = "Não autorizado", type = "USER_UNAUTHORIZED" }, statusCode: 403);

    try
    {
        var result = await mediator.Send(new SaldoQuery { IdContaCorrente = idConta });
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Json(new { message = ex.Message, type = "QUERY_ERROR" }, statusCode: 400);
    }
})
.WithName("ConsultarSaldo").WithOpenApi().RequireAuthorization();

app.Run();

public partial class Program { }