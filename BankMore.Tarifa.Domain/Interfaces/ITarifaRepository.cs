using BankMore.Tarifa.Domain.Models;

namespace BankMore.Tarifa.Domain.Interfaces
{
    public interface ITarifaRepository
    {
        Task Salvar(Models.Tarifa tarifa);
        Task<IEnumerable<Models.Tarifa>> ObterPorContaAsync(int idContaCorrente);
    }
}