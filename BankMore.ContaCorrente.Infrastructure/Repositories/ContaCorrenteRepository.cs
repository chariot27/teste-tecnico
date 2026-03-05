using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data;
using Dapper;
using System.Data;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Infrastructure.Repositories
{
    public class ContaCorrenteRepository : IContaCorrenteRepository
    {
        // 1. Campo privado renomeado para evitar conflito com nomes de parâmetros (Resolvendo CS0229)
        private readonly DbSessionContaCorrente _dbSession;

        public ContaCorrenteRepository(DbSessionContaCorrente dbSession)
        {
            _dbSession = dbSession;
        }

        public async Task<BankMore.ContaCorrente.Domain.Entities.ContaCorrente> ObterPorCpfAsync(string cpf)
        {
            using var conn = _dbSession.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<BankMore.ContaCorrente.Domain.Entities.ContaCorrente>(
                "SELECT * FROM contacorrente WHERE cpf = @cpf", new { cpf });
        }

        public async Task<BankMore.ContaCorrente.Domain.Entities.ContaCorrente> ObterPorNumeroAsync(int numero)
        {
            using var conn = _dbSession.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<BankMore.ContaCorrente.Domain.Entities.ContaCorrente>(
                "SELECT * FROM contacorrente WHERE numero = @numero", new { numero });
        }

        public async Task<BankMore.ContaCorrente.Domain.Entities.ContaCorrente> ObterPorIdAsync(string id)
        {
            using var conn = _dbSession.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<BankMore.ContaCorrente.Domain.Entities.ContaCorrente>(
                "SELECT * FROM contacorrente WHERE idcontacorrente = @id", new { id });
        }

        public async Task AdicionarAsync(BankMore.ContaCorrente.Domain.Entities.ContaCorrente conta)
        {
            using var conn = _dbSession.CreateConnection();
            // Verifique se a sua Entidade possui a propriedade 'Cpf' para o Dapper mapear corretamente
            var sql = @"INSERT INTO contacorrente (idcontacorrente, numero, nome, ativo, senha, salt, cpf) 
                        VALUES (@IdContaCorrente, @Numero, @Nome, @Ativo, @Senha, @Salt, @Cpf)";

            await conn.ExecuteAsync(sql, conta);
        }

        public async Task AtualizarStatusAsync(string idContaCorrente, int ativo)
        {
            using var conn = _dbSession.CreateConnection();
            await conn.ExecuteAsync(
                "UPDATE contacorrente SET ativo = @ativo WHERE idcontacorrente = @idContaCorrente",
                new { ativo, idContaCorrente });
        }

        public async Task<int> ObterProximoNumeroContaAsync()
        {
            using var conn = _dbSession.CreateConnection();
            // Inicia em 1001 se a tabela estiver vazia
            return await conn.ExecuteScalarAsync<int>("SELECT IFNULL(MAX(numero), 1000) + 1 FROM contacorrente");
        }
    }
}