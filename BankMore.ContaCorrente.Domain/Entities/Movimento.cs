using System;

namespace BankMore.ContaCorrente.Domain.Entities
{
    public class Movimento
    {
        public string IdMovimento { get; private set; } // TEXT(37)
        public string IdContaCorrente { get; private set; } // TEXT(37)
        public DateTime DataMovimento { get; private set; } // TEXT(25)
        public string TipoMovimento { get; private set; } // TEXT(1) - 'C' ou 'D'
        public decimal Valor { get; private set; } // REAL

        // Construtor para Dapper/Persistência
        protected Movimento() { }

        public Movimento(string idContaCorrente, string tipoMovimento, decimal valor)
        {
            IdMovimento = Guid.NewGuid().ToString().ToUpper();
            IdContaCorrente = idContaCorrente;
            DataMovimento = DateTime.Now;
            TipoMovimento = tipoMovimento.ToUpper();
            Valor = valor;

            Validar();
        }

        private void Validar()
        {
            if (Valor <= 0)
                throw new ArgumentException("O valor deve ser positivo.", "INVALID_VALUE");

            if (TipoMovimento != "C" && TipoMovimento != "D")
                throw new ArgumentException("Tipo de movimento inválido. Use 'C' ou 'D'.", "INVALID_TYPE");

            if (string.IsNullOrEmpty(IdContaCorrente))
                throw new ArgumentException("Conta corrente é obrigatória.", "INVALID_ACCOUNT");
        }
    }
}