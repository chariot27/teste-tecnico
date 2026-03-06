using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Domain.Entities
{
    public class Transferencia
    {
        public string IdMovimento { get; set; } // TEXT(37)
        public string IdContaCorrente { get; set; } // TEXT(37)
        public string DataMovimento { get; set; } // TEXT(25) - Formato DD/MM/YYYY
        public char TipoMovimento { get; set; } // TEXT(1) - 'C' ou 'D'
        public double Valor { get; set; } // REAL
    }
}
