namespace Consumer.Command.Dapper
{
    public interface IDapperConnectionFactory
    {
        IDapperConnection CreateConnection(string connectionString);
    }
}