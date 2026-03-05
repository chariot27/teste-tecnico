using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Services;
using BankMore.ContaCorrente.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BankMore.ContaCorrente.Application.Handlers
{
    public class InativarContaHandler : IRequestHandler<InativarContaCommand, IResult>
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly IPasswordService _passwordService;

        public InativarContaHandler(IContaCorrenteRepository repository, IPasswordService passwordService)
        {
            _repository = repository;
            _passwordService = passwordService;
        }

        public async Task<IResult> Handle(InativarContaCommand request, CancellationToken ct)
        {
            var conta = await _repository.ObterPorIdAsync(request.IdContaCorrente!);

            if (conta == null)
                return Results.Json(new { message = "Conta não encontrada", type = "INVALID_ACCOUNT" }, statusCode: 400);

            
            if (!_passwordService.Verificar(request.Senha, conta.Senha, conta.Salt))
                return Results.Json(new { message = "Senha inválida", type = "USER_UNAUTHORIZED" }, statusCode: 401);

            await _repository.AtualizarStatusAsync(conta.IdContaCorrente, 0);

            return Results.NoContent(); // HTTP 204 em caso de sucesso [cite: 47]
        }
    }
}