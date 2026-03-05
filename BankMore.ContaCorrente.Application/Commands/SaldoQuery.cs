// SaldoQuery.cs
using MediatR;

namespace BankMore.ContaCorrente.Application.Commands
{
    public class SaldoQuery : IRequest<SaldoResponse>
    {
        public string IdContaCorrente { get; set; } = string.Empty;
    }

    public class SaldoResponse
    {
        public int NumeroConta { get; set; } 
        public string NomeTitular { get; set; } = string.Empty; 
        public string DataHoraConsulta { get; set; } = string.Empty; 
        public decimal SaldoAtual { get; set; } 
    }
}