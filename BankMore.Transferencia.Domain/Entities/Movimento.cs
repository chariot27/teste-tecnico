namespace BankMore.Transferencia.Domain.Entities;

public class Movimento
{
    public string IdTransferencia { get; set; }
    public string IdContaCorrente_Origem { get; set; }
    public string IdContaCorrente_Destino { get; set; }
    public string DataMovimento { get; set; }
    public double Valor { get; set; }
}