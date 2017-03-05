using System;
using System.Linq;
using System.Text;
using Consumer.Adapters;

namespace Consumer.Functions
{
    public class EventPayloadBuilder : IEventPayloadBuilder
    {
        private const string TraceSourceItemName = "source";

        public EventPayload Build(ITraceEventAdapter traceEventAdapter)
        {
            if (PayloadIsEmpty(traceEventAdapter))
            {
                return new EventPayload();
            }

            string traceSource;
            if (!TryGetTraceSource(traceEventAdapter, out traceSource))
            {
                return new EventPayload();
            }

            return new EventPayload
            {
                TraceDate = traceEventAdapter.TimeStamp,
                TraceSource = traceSource,
                IsValid = true,
                ProviderName = traceEventAdapter.ProviderName,
                Payload = ExtractPayload(traceEventAdapter)
            };
        }

        private static string ExtractPayload(ITraceEventAdapter traceEventAdapter)
        {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < traceEventAdapter.PayloadNames.Length; i++)
            {
                if (IsSourceItem(traceEventAdapter, i))
                {
                    continue;
                }

                stringBuilder.AppendFormat("{0}: {1}; ", traceEventAdapter.PayloadNames[i],
                    Convert.ToString(traceEventAdapter.PayloadString(i)));
            }

            return stringBuilder.ToString().Trim();
        }

        private static bool IsSourceItem(ITraceEventAdapter traceEventAdapter, int index) 
            => traceEventAdapter.PayloadNames[index].Equals(TraceSourceItemName, StringComparison.OrdinalIgnoreCase);

        private static bool TryGetTraceSource(ITraceEventAdapter traceEventAdapter, out string traceSource)
        {
            var traceSourcePayload = traceEventAdapter.PayloadByName(TraceSourceItemName);
            if (traceSourcePayload == null)
            {
                traceSource = string.Empty;
                return false;
            }

            traceSource = Convert.ToString(traceSourcePayload);
            return !string.IsNullOrWhiteSpace(traceSource);
        }

        private static bool PayloadIsEmpty(ITraceEventAdapter traceEventAdapter) => traceEventAdapter.PayloadNames == null || !traceEventAdapter.PayloadNames.Any();
    }
}