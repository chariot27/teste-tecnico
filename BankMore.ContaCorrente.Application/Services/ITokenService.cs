using BankMore.ContaCorrente.Domain.Entities;

public interface ITokenService
{
    string GerarToken(ContaCorrente conta);
}