using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using MediatR;
using System.Text.Json;

namespace BankMore.ContaCorrente.Application.Handlers
{
    public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand, bool>
    {
        private readonly IMovimentoRepository _movimentoRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IContaCorrenteRepository _contaRepository;

        public MovimentarContaHandler(
            IMovimentoRepository movimentoRepository,
            IIdempotenciaRepository idempotenciaRepository,
            IContaCorrenteRepository contaRepository)
        {
            _movimentoRepository = movimentoRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _contaRepository = contaRepository;
        }

        public async Task<bool> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
        {

            var jaProcessado = await _idempotenciaRepository.ObterPorChaveAsync(request.ChaveIdempotencia);
            if (jaProcessado != null) return true; // Retorna sucesso sem reprocessar

            
            var contaId = string.IsNullOrEmpty(request.NumeroConta) ? request.ContaIdDoToken : request.NumeroConta;

            // 3. Validações de Negócio
            var conta = await _contaRepository.ObterPorIdAsync(contaId);

            if (conta == null)
                throw new Exception("INVALID_ACCOUNT"); // [cite: 54]

            if (conta.Ativo == 0)
                throw new Exception("INACTIVE_ACCOUNT"); // [cite: 55]

            
            if (request.TipoMovimento.ToUpper() == "D" && contaId != request.ContaIdDoToken)
                throw new Exception("INVALID_TYPE"); // Apenas crédito permitido para terceiros

            // 4. Criar e Persistir Movimento
            var movimento = new Movimento(contaId, request.TipoMovimento, request.Valor);
            await _movimentoRepository.AdicionarAsync(movimento); // [cite: 59]

            var idempotencia = new Idempotencia
            {
                Chave_Idempotencia = request.ChaveIdempotencia,
                Requisicao = JsonSerializer.Serialize(request),
                Resultado = "Sucesso - HTTP 204"
            };
            await _idempotenciaRepository.SalvarChaveAsync(idempotencia);

            return true;
        }
    }
}