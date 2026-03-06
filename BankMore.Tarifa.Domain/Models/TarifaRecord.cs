namespace BankMore.Tarifa.Domain.Models
{
    public class TarifaRecord
    {
        public int IdTarifa { get; set; }
        public int IdContaCorrente { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataMovimento { get; set; }
    }
}