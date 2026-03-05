using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Domain.Entities
{
    public class Idempotencia
    {
        public string Chave_Idempotencia { get; set; } // TEXT(37)
        public string Requisicao { get; set; } // TEXT(1000)
        public string Resultado { get; set; } // TEXT(1000)
    }
}
