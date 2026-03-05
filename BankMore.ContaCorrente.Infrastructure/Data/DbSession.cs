using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace BankMore.ContaCorrente.Infrastructure.Data
{
    public class DbSessionContaCorrente : IDisposable
    {
        public IDbConnection Connection { get; }

        public DbSessionContaCorrente(IConfiguration configuration)
        {
            // Pega a string de conexão do appsettings.json
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            Connection = new SqliteConnection(connectionString);
            Connection.Open();
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}