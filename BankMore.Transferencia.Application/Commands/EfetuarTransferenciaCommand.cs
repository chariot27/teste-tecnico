using MediatR;

namespace BankMore.Transferencia.Application.Commands.EfetuarTransferencia;

public record EfetuarTransferenciaCommand(
    Guid IdRequisicao,
    string ContaDestino,
    double Valor,
    string Token,
    string IdContaOrigem, // O GUID
    string NumeroContaOrigem // O número (ex: 1001) para a API externa
) : IRequest<bool>;