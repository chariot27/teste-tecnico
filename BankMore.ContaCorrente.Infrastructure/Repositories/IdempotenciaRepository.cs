using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data;
using Dapper;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly DbSessionContaCorrente _session;

        public IdempotenciaRepository(DbSessionContaCorrente session)
        {
            _session = session;
        }

        public async Task<Idempotencia> ObterPorChaveAsync(string chaveIdempotencia)
        {
            const string sql = "SELECT * FROM idempotencia WHERE chave_idempotencia = @chaveIdempotencia";
            return await _session.Connection.QueryFirstOrDefaultAsync<Idempotencia>(sql, new { chaveIdempotencia });
        }

        public async Task SalvarChaveAsync(Idempotencia idempotencia)
        {
            const string sql = @"
                INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado)
                VALUES (@Chave_Idempotencia, @Requisicao, @Resultado)";

            await _session.Connection.ExecuteAsync(sql, idempotencia);
        }
    }
}