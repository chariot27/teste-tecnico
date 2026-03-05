namespace BankMore.ContaCorrente.Domain.Entities
{
    public class ContaCorrente
    {
        public string IdContaCorrente { get; set; } // TEXT(37) - UUID único [cite: 26]
        public int Numero { get; set; }             // INTEGER(10) - Gerado no cadastro [cite: 32]
        public string Nome { get; set; }             // TEXT(100)
        public string Cpf { get; set; }              // Adicionado para validar Cadastro/Login [cite: 29, 35]
        public int Ativo { get; set; }               // 1 para Ativa, 0 para Inativa [cite: 46]
        public string Senha { get; set; }            // Hash da senha [cite: 31]
        public string Salt { get; set; }             // Salt da criptografia [cite: 31]
    }
}