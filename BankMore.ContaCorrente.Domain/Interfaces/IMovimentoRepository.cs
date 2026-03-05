using BankMore.ContaCorrente.Domain.Entities;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Domain.Interfaces
{
    public interface IMovimentoRepository
    {
        Task AdicionarAsync(Movimento movimento);

        
        Task<decimal> ObterSaldoPorContaAsync(string idContaCorrente);
    }
}