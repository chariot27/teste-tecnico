using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data;
using Dapper; // <--- ESTA LINHA É OBRIGATÓRIA
using System.Threading.Tasks; // <--- PARA O TASK FUNCIONAR

namespace BankMore.ContaCorrente.Infrastructure.Repositories
{
    public class IdempotenciaRepository : IIdempotenciaRepository
    {
        private readonly DbSessionContaCorrente _session;
        public IdempotenciaRepository(DbSessionContaCorrente session) => _session = session;

        public async Task<Idempotencia?> ObterPorChaveAsync(string chaveIdempotencia)
        {
            using var conn = _session.CreateConnection();
            // O Dapper estende a IDbConnection aqui
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