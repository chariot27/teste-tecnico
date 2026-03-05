using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Application.Handlers
{
    public class ConsultarSaldoHandler : IRequestHandler<SaldoQuery, SaldoResponse>
    {
        private readonly IContaCorrenteRepository _contaRepository;
        private readonly IMovimentoRepository _movimentoRepository;

        public ConsultarSaldoHandler(IContaCorrenteRepository contaRepository, IMovimentoRepository movimentoRepository)
        {
            _contaRepository = contaRepository;
            _movimentoRepository = movimentoRepository;
        }

        public async Task<SaldoResponse> Handle(SaldoQuery request, CancellationToken cancellationToken)
        {

            var conta = await _contaRepository.ObterPorIdAsync(request.IdContaCorrente);
            if (conta == null) throw new Exception("INVALID_ACCOUNT");


            if (conta.Ativo == 0) throw new Exception("INACTIVE_ACCOUNT");

            // 3. Obter a soma do repositório
            var saldo = await _movimentoRepository.ObterSaldoPorContaAsync(request.IdContaCorrente);

            return new SaldoResponse
            {
                NumeroConta = conta.Numero,
                NomeTitular = conta.Nome,
                DataHoraConsulta = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                SaldoAtual = saldo
            };
        }
    }
}
