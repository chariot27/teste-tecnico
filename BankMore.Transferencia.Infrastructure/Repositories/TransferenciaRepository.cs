using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Infrastructure.Data;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Infrastructure.Repositories
{
    public class TransferenciaRepository : ITransferenciaRepository
    {
        private readonly DbSessionTransferencia _session;

        public TransferenciaRepository(DbSessionTransferencia session)
        {
            _session = session;
        }

        public async Task AdicionarAsync(Domain.Entities.Movimento transferencia)
        {
            using var conn = _session.CreateConnection();

            // Query baseada estritamente no seu arquivo transferencia.sql
            var sql = @"INSERT INTO transferencia (
                            idtransferencia, 
                            idcontacorrente_origem, 
                            idcontacorrente_destino, 
                            datamovimento, 
                            valor) 
                        VALUES (
                            @IdTransferencia, 
                            @IdContaCorrente_Origem, 
                            @IdContaCorrente_Destino, 
                            @DataMovimento, 
                            @Valor)";

            await conn.ExecuteAsync(sql, transferencia);
        }

        public async Task<IEnumerable<Domain.Entities.Movimento>> ObterPorContaOrigemAsync(string idContaOrigem)
        {
            using var conn = _session.CreateConnection();
            var sql = "SELECT * FROM transferencia WHERE idcontacorrente_origem = @idContaOrigem";
            return await conn.QueryAsync<Domain.Entities.Movimento>(sql, new { idContaOrigem });
        }
    }
}