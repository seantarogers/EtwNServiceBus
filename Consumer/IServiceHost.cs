using Topshelf;

namespace Consumer
{
    public interface IServiceHost
    {
        bool Start(HostControl hostControl);
        bool Stop();
    }
}