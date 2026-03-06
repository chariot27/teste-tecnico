using BankMore.Transferencia.Application.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BankMore.Transferencia.Infrastructure.ExternalServices;

public class ContaCorrenteService : IContaCorrenteService
{
    private readonly HttpClient _httpClient;
    public ContaCorrenteService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<bool> DebitarAsync(Guid idRequisicao, double valor, string token, string numeroConta)
    {
        Console.WriteLine($"\n[CONTA CORRENTE SERVICE] Iniciando DÉBITO...");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Ajustado: Enviando "D" como string, conforme o banco de dados espera
        var request = new
        {
            identificacaoRequisicao = idRequisicao.ToString(), // Guid convertido para string
            numeroConta = numeroConta,
            valor = valor,
            tipoMovimento = "D"
        };

        // Log para conferirmos se as aspas estão corretas no console
        Console.WriteLine($"[OUTBOUND JSON]: {System.Text.Json.JsonSerializer.Serialize(request)}");

        var response = await _httpClient.PostAsJsonAsync("api/contacorrente/movimentacao", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine($"[ERRO 400 - API CONTA CORRENTE REJEITOU DÉBITO]:");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Mensagem de Validação: {errorDetail}");
            Console.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n");
        }
        else
        {
            Console.WriteLine("[SUCESSO] API de Conta Corrente processou o débito.");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreditarAsync(Guid idRequisicao, string contaDestino, double valor, string token)
    {
        Console.WriteLine($"\n[CONTA CORRENTE SERVICE] Iniciando CRÉDITO...");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Ajustado: Enviando "C" como string
        var request = new
        {
            identificacaoRequisicao = idRequisicao.ToString(),
            numeroConta = contaDestino,
            valor = valor,
            tipoMovimento = "C"
        };

        Console.WriteLine($"[OUTBOUND JSON]: {System.Text.Json.JsonSerializer.Serialize(request)}");

        var response = await _httpClient.PostAsJsonAsync("api/contacorrente/movimentacao", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine($"[ERRO 400 - API CONTA CORRENTE REJEITOU CRÉDITO]:");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Mensagem de Validação: {errorDetail}");
            Console.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n");
        }
        else
        {
            Console.WriteLine("[SUCESSO] API de Conta Corrente processou o crédito.");
        }

        return response.IsSuccessStatusCode;
    }
}