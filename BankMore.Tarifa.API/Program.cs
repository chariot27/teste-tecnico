using BankMore.Tarifa.Application.Handlers;
using BankMore.Tarifa.Domain.Interfaces;
using BankMore.Tarifa.Infrastructure.Repositories;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Data.Sqlite;
using System.Data;
using MediatR;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Infraestrutura ---
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITarifaRepository, TarifaRepository>();

// --- 2. Mediator ---
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProcessarTarifaCommand).Assembly));

// --- 3. KafkaFlow (Consolidado v3.1.0) ---
builder.Services.AddKafka(kafka => kafka
    .AddCluster(cluster => cluster
        .WithBrokers(new[] { "localhost:9092" })
        .AddConsumer(consumer => consumer
            .Topic("transferencias-realizadas")
            .WithGroupId("tarifa-service")
            .WithBufferSize(100)
            .WithWorkersCount(2)
            .AddMiddlewares(m => m
                .AddDeserializer<JsonCoreDeserializer>()
                .AddTypedHandlers(h => h.AddHandler<TransferenciaRealizadaHandler>())
            )
        )
        .AddProducer<ProcessarTarifaCommandHandler>(producer => producer
            .DefaultTopic("tarifacoes-realizadas")
            .AddMiddlewares(m => m
                .AddSerializer<JsonCoreSerializer>()
            )
        )
    )
);

// --- 4. Swagger & API Explorer (Essencial para aparecer no Swagger) ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore Tarifa API", Version = "v1" });
});

var app = builder.Build();

// --- 5. Inicialização do Kafka ---
// Se o Kafka estiver offline, o StartAsync pode demorar ou logar erros, mas a API deve subir.
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

// --- 6. Pipeline HTTP ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BankMore Tarifa API v1");
    c.RoutePrefix = string.Empty; // Faz o Swagger abrir direto na URL principal
});

app.UseHttpsRedirection();
app.UseAuthorization();

// --- 7. Endpoint de Consulta (Minimal API) ---
app.MapGet("/api/tarifas/{idConta}", async (int idConta, ITarifaRepository repo) =>
{
    var resultado = await repo.ObterPorContaAsync(idConta);
    return resultado != null && resultado.Any() ? Results.Ok(resultado) : Results.NotFound();
})
.WithName("GetTarifasPorConta")
.WithOpenApi(); // 👈 Sem isso, não aparece no Swagger!

app.MapControllers();
app.Run();

// ========== HANDLERS ==========

public class TransferenciaRealizadaHandler : IMessageHandler<TransferenciaMessage>
{
    private readonly IMediator _mediator;
    public TransferenciaRealizadaHandler(IMediator mediator) => _mediator = mediator;

    public async Task Handle(IMessageContext context, TransferenciaMessage message)
    {
        // Certifique-se que IdRequisicao é o campo correto da sua TransferenciaMessage
        await _mediator.Send(new ProcessarTarifaCommand(Convert.ToInt32(message.IdRequisicao)));
    }
}

namespace BankMore.Tarifa.Application.Handlers
{
    public record ProcessarTarifaCommand(int IdContaCorrente) : IRequest<bool>;

    public class ProcessarTarifaCommandHandler : IRequestHandler<ProcessarTarifaCommand, bool>
    {
        private readonly ITarifaRepository _repository;
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
            var valorTarifa = _config.GetValue<decimal>("TarifaConfig:Valor");

            var novaTarifa = new BankMore.Tarifa.Domain.Models.Tarifa
            {
                IdContaCorrente = request.IdContaCorrente,
                Valor = valorTarifa,
                DataMovimento = DateTime.Now
            };

            await _repository.Salvar(novaTarifa);

            await _producer.ProduceAsync(
                novaTarifa.IdContaCorrente.ToString(),
                new
                {
                    novaTarifa.IdContaCorrente,
                    ValorTarifado = novaTarifa.Valor,
                    DataProcessamento = DateTime.Now
                });

            return true;
        }
    }
}