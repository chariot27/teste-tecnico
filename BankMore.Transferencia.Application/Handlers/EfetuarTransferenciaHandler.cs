using MediatR;
using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Application.Interfaces;
using BankMore.Transferencia.Application.Commands.EfetuarTransferencia;

namespace BankMore.Transferencia.Application.Handlers;

public class EfetuarTransferenciaHandler : IRequestHandler<EfetuarTransferenciaCommand, bool>
{
    private readonly ITransferenciaRepository _transferenciaRepo;
    private readonly IIdempotenciaRepository _idempotenciaRepo;
    private readonly IContaCorrenteService _contaCorrenteService;

    public EfetuarTransferenciaHandler(
        ITransferenciaRepository transferenciaRepo,
        IIdempotenciaRepository idempotenciaRepo,
        IContaCorrenteService contaCorrenteService)
    {
        _transferenciaRepo = transferenciaRepo;
        _idempotenciaRepo = idempotenciaRepo;
        _contaCorrenteService = contaCorrenteService;
    }

    public async Task<bool> Handle(EfetuarTransferenciaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var jaProcessado = await _idempotenciaRepo.ObterPorChaveAsync(request.IdRequisicao.ToString());
            if (jaProcessado != null) return true;

            var debitoOk = await _contaCorrenteService.DebitarAsync(
                request.IdRequisicao,
                request.Valor,
                request.Token,
                request.NumeroContaOrigem);

            if (!debitoOk) return false;

            var creditoOk = await _contaCorrenteService.CreditarAsync(
                request.IdRequisicao,
                request.ContaDestino,
                request.Valor,
                request.Token);

            if (!creditoOk)
            {
                await _contaCorrenteService.DebitarAsync(
                    Guid.NewGuid(),
                    request.Valor * -1,
                    request.Token,
                    request.NumeroContaOrigem);

                return false;
            }

            var transferencia = new Movimento
            {
                IdTransferencia = Guid.NewGuid().ToString(),
                IdContaCorrente_Origem = request.IdContaOrigem,
                IdContaCorrente_Destino = request.ContaDestino,
                DataMovimento = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Valor = request.Valor
            };

            await _transferenciaRepo.AdicionarAsync(transferencia);

            await _idempotenciaRepo.SalvarChaveAsync(new Idempotencia
            {
                Chave_Idempotencia = request.IdRequisicao.ToString(),
                Resultado = "Sucesso"
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}