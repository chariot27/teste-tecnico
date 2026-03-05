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

// --- 1. REGISTRO DE DEPENDÊNCIAS ---
builder.Services.AddScoped<DbSessionContaCorrente>();
builder.Services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
builder.Services.AddScoped<IMovimentoRepository, MovimentoRepository>();
builder.Services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CadastrarContaHandler).Assembly);
});

// --- 2. SEGURANÇA (JWT) ---
var secretKey = "SuaChaveSuperSecretaComPeloMenos32Caracteres!!";
var key = Encoding.ASCII.GetBytes(secretKey);

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

// --- 3. CONFIGURAÇÃO DO SWAGGER ---
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

// --- 4. MIDDLEWARE PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// --- 5. ENDPOINTS ---

app.MapPost("/api/contacorrente/cadastro", async ([FromBody] CadastrarContaCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("CadastrarConta")
.WithOpenApi();

app.MapPost("/api/contacorrente/login", async ([FromBody] LoginCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("Login")
.WithOpenApi();

app.MapPatch("/api/contacorrente/inativar", async ([FromBody] InativarContaCommand command, IMediator mediator, ClaimsPrincipal user) =>
{
    var idConta = user.FindFirst("idcontacorrente")?.Value;

    if (string.IsNullOrEmpty(idConta))
        return Results.Json(new { message = "Token inválido ou sem identificação de conta", type = "USER_UNAUTHORIZED" }, statusCode: 401);

    command.IdContaCorrente = idConta;
    return await mediator.Send(command);
})
.WithName("InativarConta")
.WithOpenApi()
.RequireAuthorization();

app.MapPost("/api/contacorrente/movimentacao", async ([FromBody] MovimentarContaCommand command, IMediator mediator, ClaimsPrincipal user) =>
{
    var idContaToken = user.FindFirst("idcontacorrente")?.Value;

    if (string.IsNullOrEmpty(idContaToken))
        return Results.Json(new { message = "Token inválido ou expirado", type = "USER_UNAUTHORIZED" }, statusCode: 403);

    command.ContaIdDoToken = idContaToken;

    try
    {
        var sucesso = await mediator.Send(command);
        return sucesso ? Results.NoContent() : Results.BadRequest();
    }
    catch (Exception ex)
    {
        return Results.Json(new { message = ex.Message, type = ex.Message }, statusCode: 400);
    }
})
.WithName("MovimentarConta")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/contacorrente/saldo", async (IMediator mediator, ClaimsPrincipal user) =>
{

    var idConta = user.FindFirst("idcontacorrente")?.Value;

    if (string.IsNullOrEmpty(idConta))
        return Results.Json(new { message = "Token inválido", type = "USER_UNAUTHORIZED" }, statusCode: 403);

    try
    {
        var result = await mediator.Send(new SaldoQuery { IdContaCorrente = idConta });
        return Results.Ok(result); // Retorna 200 com o body [cite: 69]
    }
    catch (Exception ex)
    {

        return Results.Json(new { message = ex.Message, type = ex.Message }, statusCode: 400);
    }
})
.WithName("ConsultarSaldo")
.WithOpenApi()
.RequireAuthorization(); // Garante autenticação via JWT [cite: 14]

app.Run();