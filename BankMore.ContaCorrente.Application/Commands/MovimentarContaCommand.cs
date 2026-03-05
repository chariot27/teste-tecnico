using MediatR;

namespace BankMore.ContaCorrente.Application.Commands
{
    public class MovimentarContaCommand : IRequest<bool>
    {
        public string ChaveIdempotencia { get; set; }
        public string? NumeroConta { get; set; }
        public decimal Valor { get; set; }
        public string TipoMovimento { get; set; }
        public string ContaIdDoToken { get; set; }
    }
}