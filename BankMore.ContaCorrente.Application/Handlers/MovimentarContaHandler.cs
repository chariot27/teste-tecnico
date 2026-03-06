using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using MediatR;
using System.Text.Json;

using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Data;
using MediatR;
using System.Text.Json;

namespace BankMore.ContaCorrente.Application.Handlers
{
    public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand, bool>
    {
        private readonly IMovimentoRepository _movimentoRepository;
        private readonly IIdempotenciaRepository _idempotenciaRepository;
        private readonly IContaCorrenteRepository _contaRepository;
        private readonly DbSessionContaCorrente _session;

        public MovimentarContaHandler(
            IMovimentoRepository movimentoRepository,
            IIdempotenciaRepository idempotenciaRepository,
            IContaCorrenteRepository contaRepository,
            DbSessionContaCorrente session)
        {
            _movimentoRepository = movimentoRepository;
            _idempotenciaRepository = idempotenciaRepository;
            _contaRepository = contaRepository;
            _session = session;
        }

        public async Task<bool> Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificação de Idempotência (Não precisa de transação para leitura)
            var jaProcessado = await _idempotenciaRepository.ObterPorChaveAsync(request.ChaveIdempotencia);
            if (jaProcessado != null) return true;

            // 2. Identificação da Conta
            var contaId = string.IsNullOrEmpty(request.NumeroConta) ? request.ContaIdDoToken : request.NumeroConta;

            // 3. Validações de Negócio
            var conta = await _contaRepository.ObterPorIdAsync(contaId);

            if (conta == null)
                throw new Exception("INVALID_ACCOUNT");

            if (conta.Ativo == 0)
                throw new Exception("INACTIVE_ACCOUNT");

            // Regra: Apenas crédito ('C') permitido para contas de terceiros
            if (request.TipoMovimento.ToUpper() == "D" && contaId != request.ContaIdDoToken)
                throw new Exception("INVALID_TYPE");

            // 4. Instância do Movimento
            var movimento = new Movimento(contaId, request.TipoMovimento.ToUpper(), request.Valor);

            // 5. Persistência com Controle de Transação
            _session.BeginTransaction();

            try
            {
                // Adiciona o movimento ao repositório (Passando pela transação da session interna)
                await _movimentoRepository.AdicionarAsync(movimento);

                // Cria o registro de idempotência
                var idempotencia = new Idempotencia
                {
                    Chave_Idempotencia = request.ChaveIdempotencia,
                    Requisicao = JsonSerializer.Serialize(request),
                    Resultado = "Sucesso - Movimento Registrado"
                };

                await _idempotenciaRepository.SalvarChaveAsync(idempotencia);

                // CONFIRMAÇÃO: Aqui o dado é gravado definitivamente no arquivo .db
                _session.Commit();

                return true;
            }
            catch (Exception ex)
            {
                // Se algo falhar, desfazemos tudo para não deixar o banco inconsistente
                _session.Rollback();
                throw new Exception("Erro ao persistir dados no banco: " + ex.Message);
            }
        }
    }
}