using MediatR;
using Microsoft.AspNetCore.Http; // Necessário para IResult e Results
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Application.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Application.Handlers
{
    // O Command representa a intenção de cadastrar
    // IResult pertence ao namespace Microsoft.AspNetCore.Http
    public class CadastrarContaCommand : IRequest<IResult>
    {
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Senha { get; set; }
    }

    public class CadastrarContaHandler : IRequestHandler<CadastrarContaCommand, IResult>
    {
        private readonly IContaCorrenteRepository _repository;
        private readonly IPasswordService _password;

        public CadastrarContaHandler(IContaCorrenteRepository repository, IPasswordService password)
        {
            _repository = repository;
            _password = password;
        }

        public async Task<IResult> Handle(CadastrarContaCommand request, CancellationToken ct)
        {
            // 1. Validar CPF - Regra da Ana: Retornar HTTP 400 e tipo INVALID_DOCUMENT
            if (!CpfValidador.Validar(request.Cpf))
            {
                return Microsoft.AspNetCore.Http.Results.BadRequest(new
                {
                    mensagem = "O CPF informado é inválido.",
                    tipo = "INVALID_DOCUMENT"
                });
            }

            // 2. Verificar se já existe conta para este CPF (Evitar duplicidade)
            var contaExistente = await _repository.ObterPorCpfAsync(request.Cpf);
            if (contaExistente != null)
            {
                return Microsoft.AspNetCore.Http.Results.BadRequest(new
                {
                    mensagem = "Já existe uma conta cadastrada para este documento.",
                    tipo = "INVALID_DOCUMENT"
                });
            }

            // 3. Criptografia da Senha (Hash + Salt) - Exigência de Segurança
            var hash = _password.GerarHash(request.Senha, out var salt);

            // 4. Criação da Entidade de Domínio
            var novaConta = new BankMore.ContaCorrente.Domain.Entities.ContaCorrente
            {
                IdContaCorrente = Guid.NewGuid().ToString(),
                Numero = await _repository.ObterProximoNumeroContaAsync(),
                Nome = request.Nome,
                Cpf = request.Cpf,
                Ativo = 1, // Conta inicia como ativa (1)
                Senha = hash,
                Salt = salt
            };

            // 5. Persistência no Banco de Dados via Repositório (Dapper)
            await _repository.AdicionarAsync(novaConta);

            // 6. Retorno de Sucesso - Regra: Retornar apenas o número da conta gerado
            return Microsoft.AspNetCore.Http.Results.Ok(new { numeroConta = novaConta.Numero });
        }
    }

    // Validador auxiliar
    public static class CpfValidador
    {
        public static bool Validar(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;
            var apenasNumeros = new string(cpf.Where(char.IsDigit).ToArray());
            return apenasNumeros.Length == 11;
        }
    }
}