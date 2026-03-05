using MediatR;
using Microsoft.AspNetCore.Http;

namespace BankMore.ContaCorrente.Application.Commands
{
    public class InativarContaCommand : IRequest<IResult>
    {
        public string Senha { get; set; } = string.Empty;
        // Preenchido internamente via Token
        public string? IdContaCorrente { get; set; }
    }
}