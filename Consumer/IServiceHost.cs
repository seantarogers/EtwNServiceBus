namespace Consumer
{
    using Topshelf;

    public interface IServiceHost
    {
        bool Start(HostControl hostControl);
        bool Stop();
    }
}