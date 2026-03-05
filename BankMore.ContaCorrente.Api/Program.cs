using BankMore.ContaCorrente.Application.Handlers;
using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data;
using BankMore.ContaCorrente.Infrastructure.Repositories;
using Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REGISTRO DE DEPENDÊNCIAS (Serviços e Infra) ---
builder.Services.AddScoped<DbSessionContaCorrente>();
builder.Services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Registro do MediatR (Versão 12+)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CadastrarContaHandler).Assembly);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. CONSTRUÇÃO DO APP (O BUILD deve vir antes de usar 'app') ---
var app = builder.Build();


// --- 4. MIDDLEWARE PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- 5. DEFINIÇÃO DOS ENDPOINTS (Minimal APIs) ---

// Endpoint de Cadastro
app.MapPost("/api/contacorrente/cadastro", async ([FromBody] CadastrarContaCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("CadastrarConta")
.WithOpenApi();

// Endpoint de Login
app.MapPost("/api/contacorrente/login", async ([FromBody] LoginCommand command, IMediator mediator) =>
{
    return await mediator.Send(command);
})
.WithName("Login")
.WithOpenApi();

app.Run();