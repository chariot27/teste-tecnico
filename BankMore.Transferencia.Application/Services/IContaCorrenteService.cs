namespace BankMore.Transferencia.Application.Interfaces;

public interface IContaCorrenteService
{
    Task<bool> DebitarAsync(Guid idRequisicao, double valor, string token, string numeroConta);
    Task<bool> CreditarAsync(Guid idRequisicao, string contaDestino, double valor, string token);
}