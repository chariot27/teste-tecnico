using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Tarifa.Infrastructure.Data
{
    public class DbSessionTarifa
    {
        private readonly string _connectionString;

        public DbSessionTarifa(IConfiguration configuration)
        {
            // Busca a string de conexão configurada no appsettings
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Método para criar a conexão que o Dapper utilizará

    }
}
