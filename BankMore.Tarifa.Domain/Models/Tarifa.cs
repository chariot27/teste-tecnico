using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Tarifa.Domain.Models
{
    public class Tarifa
    {
        public int IdTarifa { get; set; } //
        public int IdContaCorrente { get; set; } 
        public decimal Valor { get; set; } 
        public DateTime DataMovimento { get; set; } 
    }
}
