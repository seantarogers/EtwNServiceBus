using System;
using System.Timers;
using log4net;
using log4net.Appender;
using static Consumer.Constants.ConsumerConstants;

namespace Consumer.Functions
{
    public class BufferFlusher : IBufferFlusher
    {
        private static Timer timer;
        private readonly ILog logger;

        public BufferFlusher(ILog logger)
        {
            this.logger = logger;
        }

        public void Start(int bufferFlushIntervalInSeconds)
        {
            timer = new Timer { Interval = TimeSpan.FromSeconds(bufferFlushIntervalInSeconds).TotalMilliseconds };
            timer.Elapsed += TimerElapsed;
            timer.Enabled = true;
        }

        public void Flush()
        {
            logger.Debug(FlushingAllBufferedAppenders);

            try
            {
                var loggerRepository = LogManager.GetRepository();
                var bufferedAppenderCount = 0;
                foreach (var appender in loggerRepository.GetAppenders())
                {
                    var bufferingAppenderSkeleton = appender as BufferingAppenderSkeleton;
                    if (bufferingAppenderSkeleton != null)
                    {
                        bufferingAppenderSkeleton.Flush(true);
                        bufferedAppenderCount++;
                    }
                }

                logger.DebugFormat(FlushedAllBufferedAppenders, bufferedAppenderCount);
            }
            catch (Exception exception)
            {
                logger.ErrorFormat(FlushingException, exception);
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            Flush();
            timer.Enabled = true;
        }
    }
}