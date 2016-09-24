namespace Consumer.Dapper
{
    using System;
    using System.Data;
    using System.Data.SqlClient;

    using global::Dapper;

    public class DapperConnection : IDapperConnection
    {
        private bool disposed;

        private readonly IDbConnection connection;

        public DapperConnection(string connectionString)
        {
            connection = new SqlConnection(connectionString);
        }

        public void Open()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        public int Execute(string sql, object param = null, CommandType? commandType = default(CommandType?))
        {
            return connection.Execute(sql, param, null, default(int?), commandType);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Dispose();
                }
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}