using System.Data;
using Microsoft.Data.Sqlite;

namespace BankMore.ContaCorrente.Infrastructure.Data
{
    public class DbSessionContaCorrente : IDisposable
    {
        public IDbConnection Connection { get; }
        public IDbTransaction? Transaction { get; private set; }

        public DbSessionContaCorrente(string connectionString)
        {
            Connection = new SqliteConnection(connectionString);
            Connection.Open();
        }

        public void BeginTransaction() => Transaction = Connection.BeginTransaction();
        public void Commit() => Transaction?.Commit();
        public void Rollback() => Transaction?.Rollback();

        public void Dispose()
        {
            Transaction?.Dispose();
            Connection?.Dispose();
        }
    }
}