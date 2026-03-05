using Dapper; // Esta linha resolve o erro CS1061
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Infrastructure.Data;
using System.Data; // Necessário para IDbConnection
using System.Threading.Tasks;

namespace BankMore.Transferencia.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly DbSessionTransferencia _session;
        public IdempotenciaRepository(DbSessionTransferencia session) => _session = session;

        public async Task<Idempotencia> ObterPorChaveAsync(string chaveIdempotencia)
        {
            using var conn = _session.CreateConnection();
            // Agora o Dapper estenderá a IDbConnection com este método
            return await conn.QueryFirstOrDefaultAsync<Idempotencia>(
                "SELECT * FROM idempotencia WHERE chave_idempotencia = @chaveIdempotencia",
                new { chaveIdempotencia });
        }

        public async Task SalvarChaveAsync(Idempotencia idempotencia)
        {
            using var conn = _session.CreateConnection();
            var sql = @"INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado) 
                        VALUES (@chave_idempotencia, @requisicao, @resultado)";

            await conn.ExecuteAsync(sql, idempotencia);
        }
    }
}