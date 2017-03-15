namespace Consumer.Functions
{
    using log4net.Appender;

    public interface IBufferingForwardingAppenderBuilder
    {
        BufferingForwardingAppender Build();
    }
}