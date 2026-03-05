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
        // Campo privado para a sessão de banco de dados
        private readonly DbSessionContaCorrente _dbSession;

        public ContaCorrenteRepository(DbSessionContaCorrente dbSession)
        {
            _dbSession = dbSession;
        }

        public async Task<BankMore.ContaCorrente.Domain.Entities.ContaCorrente> ObterPorCpfAsync(string cpf)
        {
            // Ajustado: Acesso via propriedade .Connection (sem parênteses) e sem 'using' local
            return await _dbSession.Connection.QueryFirstOrDefaultAsync<BankMore.ContaCorrente.Domain.Entities.ContaCorrente>(
                "SELECT * FROM contacorrente WHERE cpf = @cpf", new { cpf });
        }

        public async Task<BankMore.ContaCorrente.Domain.Entities.ContaCorrente> ObterPorNumeroAsync(int numero)
        {
            return await _dbSession.Connection.QueryFirstOrDefaultAsync<BankMore.ContaCorrente.Domain.Entities.ContaCorrente>(
                "SELECT * FROM contacorrente WHERE numero = @numero", new { numero });
        }

        public async Task<BankMore.ContaCorrente.Domain.Entities.ContaCorrente> ObterPorIdAsync(string id)
        {
            
            return await _dbSession.Connection.QueryFirstOrDefaultAsync<BankMore.ContaCorrente.Domain.Entities.ContaCorrente>(
                "SELECT * FROM contacorrente WHERE idcontacorrente = @id", new { id });
        }

        public async Task AdicionarAsync(BankMore.ContaCorrente.Domain.Entities.ContaCorrente conta)
        {
            
            var sql = @"INSERT INTO contacorrente (idcontacorrente, numero, nome, ativo, senha, salt, cpf) 
                        VALUES (@IdContaCorrente, @Numero, @Nome, @Ativo, @Senha, @Salt, @Cpf)";

            await _dbSession.Connection.ExecuteAsync(sql, conta);
        }

        public async Task<bool> AtualizarStatusAsync(string id, int status)
        {
            
            const string sql = "UPDATE contacorrente SET ativo = @status WHERE idcontacorrente = @id";
            var rowsAffected = await _dbSession.Connection.ExecuteAsync(sql, new { id, status });
            return rowsAffected > 0;
        }

        public async Task<int> ObterProximoNumeroContaAsync()
        {
            // Utiliza a conexão da sessão para buscar o próximo número sequencial
            return await _dbSession.Connection.ExecuteScalarAsync<int>("SELECT IFNULL(MAX(numero), 1000) + 1 FROM contacorrente");
        }
    }
}