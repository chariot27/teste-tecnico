using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BankMore.ContaCorrente.Infrastructure.Data
{
    public class DbSessionContaCorrente
    {
        private readonly string _connectionString;

        public DbSessionContaCorrente(IConfiguration configuration)
        {
            // Busca a string de conexão configurada no appsettings
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Método para criar a conexão que o Dapper utilizará
        public IDbConnection CreateConnection() => new SqliteConnection(_connectionString);
    }
}
