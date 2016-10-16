﻿namespace Consumer.Dapper
{
    public class DapperConnectionFactory : IDapperConnectionFactory
    {
        public IDapperConnection CreateConnection(string connectionString)
        {
            return new DapperConnection(connectionString);
        }
    }
}