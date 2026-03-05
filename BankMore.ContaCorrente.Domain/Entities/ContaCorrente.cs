using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Domain.Entities
{
    public class ContaCorrente
    {
        public string IdContaCorrente { get; set; } // TEXT(37)
        public int Numero { get; set; } // INTEGER(10)
        public string Nome { get; set; } // TEXT(100)
        public int Ativo { get; set; } // INTEGER(1) - 0 ou 1
        public string Senha { get; set; } // TEXT(100)
        public string Salt { get; set; } // TEXT(100)
    }
}
