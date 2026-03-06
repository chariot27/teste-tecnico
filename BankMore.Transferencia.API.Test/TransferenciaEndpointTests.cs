using BankMore.Transferencia.Application.Commands.EfetuarTransferencia;
using BankMore.Transferencia.Application.DTOs;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Xunit;

namespace BankMore.Transferencia.API.Test
{
    public class TransferenciaEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IMediator> _mediatorMock = new();

        public TransferenciaEndpointTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Substitui o MediatR real pelo Mock para isolar o teste de endpoint
                    services.AddScoped(_ => _mediatorMock.Object);

                    // Configura um esquema de autenticação Fake
                    services.AddAuthentication("TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
                });
            });
        }

        [Fact]
        public async Task Post_Transferencia_DeveRetornarNoContent_QuandoSucesso()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Adiciona o cabeçalho Bearer (o TestAuthHandler irá validar qualquer token aqui)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-fake-teste");

            // CORREÇÃO CS7036/CS0029: Passando Guid real e respeitando o construtor do DTO
            var request = new TransferenciaRequest(
                IdRequisicao: Guid.NewGuid(),
                ContaDestino: "9999",
                Valor: 100.00m
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<EfetuarTransferenciaCommand>(), default))
                         .ReturnsAsync(true);

            // Act
            var response = await client.PostAsJsonAsync("/api/transferencia", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Post_Transferencia_DeveRetornarBadRequest_QuandoFalhaNoProcessamento()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-fake-teste");

            var request = new TransferenciaRequest(
                IdRequisicao: Guid.NewGuid(),
                ContaDestino: "0000",
                Valor: 1
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<EfetuarTransferenciaCommand>(), default))
                         .ReturnsAsync(false);

            // Act
            var response = await client.PostAsJsonAsync("/api/transferencia", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    // --- Handler de Autenticação para Testes ---
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Simula os Claims que seu endpoint extrai do Token no Program.cs
            var claims = new[] {
                new Claim("idcontacorrente", Guid.NewGuid().ToString()),
                new Claim("numero_conta", "1001"),
                new Claim(ClaimTypes.Name, "Usuario Teste")
            };

            var identity = new ClaimsIdentity(claims, "TestScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}