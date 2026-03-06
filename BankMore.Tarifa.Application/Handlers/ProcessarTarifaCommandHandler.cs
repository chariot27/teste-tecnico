using MediatR;
using Microsoft.Extensions.Configuration;
using KafkaFlow;
using BankMore.Tarifa.Domain.Models;
using BankMore.Tarifa.Domain.Interfaces;

namespace BankMore.Tarifa.Application.Handlers
{
    // Recomendo que o Command receba a mensagem completa ou os dados necessários
    public record ProcessarTarifaCommand(int IdContaCorrente) : IRequest<bool>;

    public class ProcessarTarifaCommandHandler : IRequestHandler<ProcessarTarifaCommand, bool>
    {
        private readonly ITarifaRepository _repository;
        // ✅ AJUSTE: Usando a interface tipada para que o KafkaFlow saiba qual produtor injetar
        private readonly IMessageProducer<ProcessarTarifaCommandHandler> _producer;
        private readonly IConfiguration _config;

        public ProcessarTarifaCommandHandler(
            ITarifaRepository repository,
            IMessageProducer<ProcessarTarifaCommandHandler> producer,
            IConfiguration config)
        {
            _repository = repository;
            _producer = producer;
            _config = config;
        }

        public async Task<bool> Handle(ProcessarTarifaCommand request, CancellationToken cancellationToken)
        {
            // Busca o valor da tarifa no appsettings.json
            var valorTarifa = _config.GetValue<decimal>("TarifaConfig:Valor");

            var novaTarifa = new BankMore.Tarifa.Domain.Models.Tarifa
            {
                IdContaCorrente = request.IdContaCorrente,
                Valor = valorTarifa,
                DataMovimento = DateTime.Now
            };

            // Salva no banco (Dapper/SQLite)
            await _repository.Salvar(novaTarifa);

            // Publica o evento de tarifação realizada no Kafka
            await _producer.ProduceAsync(
                novaTarifa.IdContaCorrente.ToString(),
                new
                {
                    IdContaCorrente = novaTarifa.IdContaCorrente,
                    ValorTarifado = novaTarifa.Valor,
                    DataProcessamento = DateTime.Now
                });

            return true;
        }
    }
}