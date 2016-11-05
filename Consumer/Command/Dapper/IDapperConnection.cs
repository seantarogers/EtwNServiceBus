namespace Consumer.Command.Dapper
{
    using System;
    using System.Data;

    public interface IDapperConnection : IDisposable
    {
        void Open();

        int Execute(string sql, object param = null, CommandType? commandType = default(CommandType?));        
    }
}