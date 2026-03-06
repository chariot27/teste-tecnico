using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Application.DTOs
{

    public record TransferenciaRequest(
        Guid IdRequisicao,
        string ContaDestino,
        decimal Valor
    );
}
