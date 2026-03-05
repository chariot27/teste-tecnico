using BankMore.Transferencia.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Domain.Interfaces
{
    public interface IIdempotenciaRepository
    {
        Task<Idempotencia> ObterPorChaveAsync(string chaveIdempotencia);
        Task SalvarChaveAsync(Idempotencia idempotencia);
    }
}
