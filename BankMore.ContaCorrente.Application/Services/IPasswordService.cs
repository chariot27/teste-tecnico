using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.ContaCorrente.Application.Services
{
    public interface IPasswordService
    {
        string GerarHash(string senha, out string salt);
        bool Verificar(string senha, string hash, string salt);
    }
}
