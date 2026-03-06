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
            // 1. Verificação de Idempotência
            var jaProcessado = await _idempotenciaRepository.ObterPorChaveAsync(request.ChaveIdempotencia);
            if (jaProcessado != null) return true;

            // 2. Identificação do Número da Conta (ex: "1001")
            // Se o NumeroConta vier vazio no comando, usa o que está no Token
            var numeroContaFiltro = Convert.ToInt32(request.NumeroConta);
            var numeroConta = request.NumeroConta;

            // 3. Validações de Negócio buscando pelo NÚMERO
            var conta = await _contaRepository.ObterPorNumeroAsync(numeroContaFiltro);

            if (conta == null)
                throw new Exception("INVALID_ACCOUNT");

            if (conta.Ativo == 0)
                throw new Exception("INACTIVE_ACCOUNT");



            // 4. Instância do Movimento 
            // IMPORTANTE: Aqui passamos o conta.IdContaCorrente (O ID real/GUID do banco)
            var movimento = new Movimento(
                conta.IdContaCorrente,
                request.TipoMovimento.ToUpper(),
                request.Valor
            );

            // 5. Persistência com Controle de Transação
            _session.BeginTransaction();

            try
            {
                // Adiciona o movimento vinculado ao ID da conta
                await _movimentoRepository.AdicionarAsync(movimento);

                // Cria o registro de idempotência
                var idempotencia = new Idempotencia
                {
                    Chave_Idempotencia = request.ChaveIdempotencia,
                    Requisicao = JsonSerializer.Serialize(request),
                    Resultado = "Sucesso - Movimento Registrado"
                };

                await _idempotenciaRepository.SalvarChaveAsync(idempotencia);

                _session.Commit();
                return true;
            }
            catch (Exception ex)
            {
                _session.Rollback();
                throw new Exception("Erro ao persistir dados no banco: " + ex.Message);
            }
        }
    }
}