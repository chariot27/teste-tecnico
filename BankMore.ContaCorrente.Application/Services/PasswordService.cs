using BankMore.ContaCorrente.Domain.Interfaces; // Se a interface estiver aqui
using BCrypt.Net;

namespace BankMore.ContaCorrente.Application.Services
{
    public class PasswordService : IPasswordService
    {
        public string GerarHash(string senha, out string salt)
        {
            salt = BCrypt.Net.BCrypt.GenerateSalt();
            return BCrypt.Net.BCrypt.HashPassword(senha, salt);
        }

        public bool Verificar(string senha, string hash, string salt)
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
    }
}