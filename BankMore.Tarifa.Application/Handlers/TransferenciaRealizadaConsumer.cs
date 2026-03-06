using KafkaFlow;
using MediatR;
using BankMore.Tarifa.Application.Handlers;

namespace BankMore.Tarifa.Application.Handlers
{
    // Esta classe é responsável por receber a mensagem do Kafka
    public class TransferenciaRealizadaConsumer : IMessageHandler<TransferenciaMessage>
    {
        private readonly IMediator _mediator;

        public TransferenciaRealizadaConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(IMessageContext context, TransferenciaMessage message)
        {
            // Aqui o 'message' já chega como objeto TransferenciaMessage
            await _mediator.Send(new ProcessarTarifaCommand(message.IdContaCorrenteOrigem));
        }
    }


    public class TransferenciaMessage
    {
        public string IdRequisicao { get; set; }
        public int IdContaCorrenteOrigem { get; set; }
        public int IdContaCorrenteDestino { get; set; }
        public decimal Valor { get; set; }
    }
}