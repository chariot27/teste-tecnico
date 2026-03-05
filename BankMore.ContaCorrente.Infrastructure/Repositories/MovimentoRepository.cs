using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data; // Import necessário para o DbSession
using Dapper;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Infrastructure.Repositories
{
    public class MovimentoRepository : IMovimentoRepository
    {
        private readonly DbSessionContaCorrente _session;

        public MovimentoRepository(DbSessionContaCorrente session)
        {
            _session = session;
        }

        public async Task AdicionarAsync(Movimento movimento)
        {
            const string sql = @"
                INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor)
                VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)";

            await _session.Connection.ExecuteAsync(sql, new
            {
                movimento.IdMovimento,
                movimento.IdContaCorrente,
                DataMovimento = movimento.DataMovimento.ToString("yyyy-MM-dd HH:mm:ss"),
                movimento.TipoMovimento,
                movimento.Valor
            });
        }

        public async Task<decimal> ObterSaldoPorContaAsync(string idContaCorrente)
        {
            
            const string sql = @"
                SELECT 
                    TOTAL(CASE WHEN tipomovimento = 'C' THEN valor ELSE 0 END) - 
                    TOTAL(CASE WHEN tipomovimento = 'D' THEN valor ELSE 0 END) as Saldo
                FROM movimento
                WHERE idcontacorrente = @idContaCorrente";

            return await _session.Connection.QueryFirstOrDefaultAsync<decimal>(sql, new { idContaCorrente });
        }
    }
}