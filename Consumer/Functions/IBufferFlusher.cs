namespace Consumer.Functions
{
    public interface IBufferFlusher
    {
        void Start(int bufferFlushIntervalInSeconds);
        void Flush();
    }
}