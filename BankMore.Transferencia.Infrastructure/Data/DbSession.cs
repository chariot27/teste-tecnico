using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankMore.Transferencia.Infrastructure.Data
{
    public class DbSessionTransferencia
    {
        private readonly string _connectionString;

        public DbSessionTransferencia(IConfiguration configuration)
        {
            // Busca a string de conexão configurada no appsettings
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Método para criar a conexão que o Dapper utilizará
        public IDbConnection CreateConnection() => new SqliteConnection(_connectionString);
    }
}
