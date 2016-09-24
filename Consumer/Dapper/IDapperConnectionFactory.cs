namespace Consumer.Dapper
{
    public interface IDapperConnectionFactory
    {
        IDapperConnection CreateConnection(string connectionString);
    }
}