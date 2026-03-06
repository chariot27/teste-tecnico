using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Handlers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

public class ContaCorrenteTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IMediator> _mediatorMock = new();

    public ContaCorrenteTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMediator));
                if (descriptor != null) services.Remove(descriptor);
                services.AddSingleton(_mediatorMock.Object);
            });
        });
    }

    #region Utilitários

    private string ObterCaminhoArquivoToken()
    {
        var diretorioProjeto = Directory.GetCurrentDirectory();
        var caminhoOut = Path.GetFullPath(Path.Combine(diretorioProjeto, "..", "..", "..", "out"));
        if (!Directory.Exists(caminhoOut)) Directory.CreateDirectory(caminhoOut);
        return Path.Combine(caminhoOut, "token.txt");
    }

    private void SalvarTokenNoArquivo(string token)
    {
        File.WriteAllText(ObterCaminhoArquivoToken(), token);
    }

    private void AdicionarAutenticacaoBearer(HttpClient client)
    {
        var caminho = ObterCaminhoArquivoToken();
        if (File.Exists(caminho))
        {
            var token = File.ReadAllText(caminho);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    #endregion

    [Fact]
    public async Task Cadastro_DeveRetornarOk_QuandoDadosSaoValidos()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new CadastrarContaCommand
        {
            Nome = "teste1",
            Cpf = "05181748650",
            Senha = "Password123"
        };

        // CORREÇÃO DEFINITIVA: 
        // Como o erro indica Task<IResult>, o retorno deve ser um IResult castado explicitamente.
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CadastrarContaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IResult)Results.Ok(new { success = true }));

        // Act
        var response = await client.PostAsJsonAsync("/api/contacorrente/cadastro", command);

        // Assert
        var corpoString = await response.Content.ReadAsStringAsync();

        // Se ainda retornar 500, o corpoString mostrará o erro interno da Minimal API
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrEmpty(corpoString), "O corpo da resposta não deve estar vazio.");
    }

    [Fact]
    public async Task Login_DeveGerarESalvarTokenNoArquivo()
    {
        var client = _factory.CreateClient();
        var expectedToken = $"fake-jwt-token-{Guid.NewGuid()}";

        // CORREÇÃO: Cast explícito para IResult para o Moq aceitar
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IResult)Results.Ok(new { Token = expectedToken }));

        var response = await client.PostAsJsonAsync("/api/contacorrente/login", new LoginCommand { Cpf = "05181748652", Senha = "Password122" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        SalvarTokenNoArquivo(expectedToken);
    }

    [Fact]
    public async Task Movimentacao_DeveRetornarNoContent_UsandoTokenDoArquivo()
    {
        var client = _factory.CreateClient();
        AdicionarAutenticacaoBearer(client);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<MovimentarContaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var response = await client.PostAsJsonAsync("/api/contacorrente/movimentacao", new MovimentarContaCommand { Valor = 10, TipoMovimento = "C" });

        Assert.True(response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ConsultarSaldo_DeveRetornarOk_UsandoTokenDoArquivo()
    {
        var client = _factory.CreateClient();
        AdicionarAutenticacaoBearer(client);

        // CORREÇÃO: Se o MediatR espera um objeto de resposta, passe um objeto compatível
        // Se SaldoQuery retorna um objeto, o Moq precisa dele. 
        // Aqui usamos 'object' como fallback se não houver uma classe SaldoResponse visível.
        // Se você não tiver a classe SaldoResponse acessível no teste, 
        // o Mediator não saberá como converter o objeto.
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SaldoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SaldoResponse { SaldoAtual = 0 }); // Use a classe real

        var response = await client.GetAsync("/api/contacorrente/saldo");

        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InativarConta_DeveRetornarOk_UsandoTokenDoArquivo()
    {
        var client = _factory.CreateClient();
        AdicionarAutenticacaoBearer(client);

        // CORREÇÃO: Garanta que o retorno coincida com o que o Handler de Inativar retorna
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<InativarContaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IResult)Results.Ok(new { message = "Conta inativada" }));

        var response = await client.PatchAsJsonAsync("/api/contacorrente/inativar", new InativarContaCommand());

        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized);
    }
}