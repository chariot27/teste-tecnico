using System.Threading.Tasks;
using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.Interfaces
{
    public interface IContaCorrenteRepository
    {
        // Necessário para validar o CPF no cadastro (Regra: source 30)
        Task<Entities.ContaCorrente> ObterPorCpfAsync(string cpf);

        // Necessário para o Login (Regra: source 35)
        Task<Entities.ContaCorrente> ObterPorNumeroAsync(int numero);

        // Necessário para identificar a conta via Token JWT (Regra: source 37, 52)
        Task<Entities.ContaCorrente> ObterPorIdAsync(string id);

        // Persistência baseada no modelo ER (Regra: source 31)
        Task AdicionarAsync(Entities.ContaCorrente conta);

        // Regra de Inativação (Regra: source 46)
        Task AtualizarStatusAsync(string idContaCorrente, int ativo);

        // Regra de retorno do número da conta após cadastro (Regra: source 32)
        Task<int> ObterProximoNumeroContaAsync();
    }
}