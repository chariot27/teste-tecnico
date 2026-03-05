using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankMore.Transferencia.Domain.Entities;

namespace BankMore.Transferencia.Domain.Interfaces
{
    public interface ITransferenciaRepository
    {
        // Alterado de 'Transferencia' para 'Movimento'
        Task AdicionarAsync(Movimento movimento);

        // Alterado para retornar uma lista de Movimento
        Task<IEnumerable<Movimento>> ObterPorContaOrigemAsync(string idContaOrigem);
    }
}