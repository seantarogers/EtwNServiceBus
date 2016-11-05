namespace Consumer.Command.Commands
{
    using System;

    public class CreateErrorLogCommand : Command
    {
        public DateTime LogDate { get; set; }
        public string Logger { get; set; }
        public string LogMessage { get; set; }
        public string ApplicationName { get; set; }        
    }
}