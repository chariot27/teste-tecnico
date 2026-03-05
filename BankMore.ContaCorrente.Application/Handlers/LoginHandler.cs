using MediatR;
using Microsoft.AspNetCore.Http;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Application.Services;
using System.Threading;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Application.Handlers
{
    // 1. O Command de Login
    public class LoginCommand : IRequest<IResult>
    {
        public string Cpf { get; set; }
        public string Senha { get; set; }
    }

    // 2. O Handler de Login
    public class LoginHandler : IRequestHandler<LoginCommand, IResult>
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly IPasswordService _passwordService;

        public LoginHandler(IContaCorrenteRepository repository, IPasswordService passwordService)
        {
            _repository = repository;
            _passwordService = passwordService;
        }

        public async Task<IResult> Handle(LoginCommand request, CancellationToken ct)
        {
            // A. Buscar o usuário pelo CPF
            var conta = await _repository.ObterPorCpfAsync(request.Cpf);

            // B. Validação de Segurança: Se não achar, não diga "CPF não encontrado" 
            // Diga "Credenciais inválidas" para evitar enumeração de usuários.
            if (conta == null)
            {
                return Microsoft.AspNetCore.Http.Results.Unauthorized();
            }

            // C. Verificar se a conta está ativa
            if (conta.Ativo == 0)
            {
                return Microsoft.AspNetCore.Http.Results.Json(new { mensagem = "Conta inativa." }, statusCode: 403);
            }

            // D. Validar a Senha usando o Hash e o Salt do banco
            // Presumindo que seu IPasswordService tenha um método 'Verificar'
            bool senhaValida = _passwordService.Verificar(request.Senha, conta.Senha, conta.Salt);

            if (!senhaValida)
            {
                return Microsoft.AspNetCore.Http.Results.Unauthorized();
            }

            // E. Retorno de Sucesso
            // Aqui você poderia retornar um Token JWT. Por enquanto, retornaremos os dados básicos.
            return Microsoft.AspNetCore.Http.Results.Ok(new
            {
                mensagem = "Login realizado com sucesso",
                usuario = conta.Nome,
                numeroConta = conta.Numero
            });
        }
    }
}