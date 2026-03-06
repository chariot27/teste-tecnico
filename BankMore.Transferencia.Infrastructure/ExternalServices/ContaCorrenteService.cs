using BankMore.Transferencia.Application.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BankMore.Transferencia.Infrastructure.ExternalServices;

public class ContaCorrenteService : IContaCorrenteService
{
    private readonly HttpClient _httpClient;

    public ContaCorrenteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> DebitarAsync(Guid idRequisicao, double valor, string token, string numeroConta)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            identificacaoRequisicao = idRequisicao.ToString(),
            numeroConta = numeroConta,
            valor = valor,
            tipoMovimento = "D"
        };

        var response = await _httpClient.PostAsJsonAsync("api/contacorrente/movimentacao", request);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreditarAsync(Guid idRequisicao, string contaDestino, double valor, string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            identificacaoRequisicao = idRequisicao.ToString(),
            numeroConta = contaDestino,
            valor = valor,
            tipoMovimento = "C"
        };

        var response = await _httpClient.PostAsJsonAsync("api/contacorrente/movimentacao", request);

        return response.IsSuccessStatusCode;
    }
}