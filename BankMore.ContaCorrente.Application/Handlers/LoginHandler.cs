using MediatR;
using Microsoft.AspNetCore.Http;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Application.Services;

namespace BankMore.ContaCorrente.Application.Handlers
{
    public class LoginCommand : IRequest<IResult>
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginHandler : IRequestHandler<LoginCommand, IResult>
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public LoginHandler(
            IContaCorrenteRepository repository,
            IPasswordService passwordService,
            ITokenService tokenService)
        {
            _repository = repository;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        public async Task<IResult> Handle(LoginCommand request, CancellationToken ct)
        {
            // Busca a conta pelo CPF
            var conta = await _repository.ObterPorCpfAsync(request.Cpf);

            // Validação inicial
            if (conta == null)
            {
                return Results.Unauthorized();
            }

            // Verifica se a conta está ativa
            if (conta.Ativo == 0)
            {
                return Results.Json(new { mensagem = "Conta inativa." }, statusCode: 403);
            }

            // Valida a senha (Hash + Salt)
            bool senhaValida = _passwordService.Verificar(request.Senha, conta.Senha, conta.Salt);

            if (!senhaValida)
            {
                return Results.Unauthorized();
            }

            // Gera o Token JWT contendo a claim "idcontacorrente"
            var token = _tokenService.GerarToken(conta);

            return Results.Ok(new
            {
                mensagem = "Login realizado com sucesso",
                token = token,
                usuario = conta.Nome,
                numeroConta = conta.Numero
            });
        }
    }
}