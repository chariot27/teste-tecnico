using System.Data;
using Dapper;
using BankMore.Tarifa.Domain.Models;
using BankMore.Tarifa.Domain.Interfaces;

namespace BankMore.Tarifa.Infrastructure.Repositories
{
    public class TarifaRepository : ITarifaRepository
    {
        private readonly IDbConnection _db;
        public TarifaRepository(IDbConnection db) => _db = db;

        
        public async Task Salvar(BankMore.Tarifa.Domain.Models.Tarifa tarifa)
        {
            const string sql = @"INSERT INTO tarifa (idcontacorrente, datamovimento, valor) 
                                 VALUES (@IdContaCorrente, @DataMovimento, @Valor)";
            
            await _db.ExecuteAsync(sql, tarifa);
        }

 
        public async Task<IEnumerable<BankMore.Tarifa.Domain.Models.Tarifa>> ObterPorContaAsync(int idContaCorrente)
        {
            // Alterado de <TarifaRecord> para <Tarifa> para resolver o erro de conversão
            return await _db.QueryAsync<BankMore.Tarifa.Domain.Models.Tarifa>(
                "SELECT * FROM tarifa WHERE idcontacorrente = @id", 
                new { id = idContaCorrente });
        }
    }
}