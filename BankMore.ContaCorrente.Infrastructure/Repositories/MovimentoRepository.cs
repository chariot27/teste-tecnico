using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data;
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

            // Dapper lida melhor com objetos nativos. 
            // Se houver uma transação aberta na Session, ela DEVE ser passada aqui.
            await _session.Connection.ExecuteAsync(sql, new
            {
                movimento.IdMovimento,
                movimento.IdContaCorrente,
                // Deixe o driver cuidar da data. Converter para string pode quebrar o filtro de SELECT depois.
                movimento.DataMovimento,
                movimento.TipoMovimento,
                movimento.Valor
            }, transaction: _session.Transaction);
        }

        public async Task<decimal> ObterSaldoPorContaAsync(string idContaCorrente)
        {
            // Nota: Se for SQLite, a função é SUM(). TOTAL() é específica e retorna 0.0 em vez de NULL.
            // Usei SUM e COALESCE para garantir compatibilidade e evitar retornos nulos.
            const string sql = @"
                SELECT 
                    SUM(CASE WHEN tipomovimento = 'C' THEN valor ELSE 0 END) - 
                    SUM(CASE WHEN tipomovimento = 'D' THEN valor ELSE 0 END)
                FROM movimento
                WHERE idcontacorrente = @idContaCorrente";

            var saldo = await _session.Connection.ExecuteScalarAsync<decimal?>(sql,
                new { idContaCorrente },
                transaction: _session.Transaction);

            return saldo ?? 0;
        }
    }
}