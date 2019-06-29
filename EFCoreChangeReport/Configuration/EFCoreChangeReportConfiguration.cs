using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EFCoreChangeReport.Configuration
{

    public interface IEFCoreChangeReportConfiguration
    {
        Sinks.Sink.SinkDelegate SinkReports { get; set; }
        JsonSerializerSettings JsonSerializerSettings { get; set; }
    }

    public class EFCoreChangeReportConfiguration : IEFCoreChangeReportConfiguration
    {
        public EFCoreChangeReportConfiguration()
        {
        }

        public Sinks.Sink.SinkDelegate SinkReports { get; set; }
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}
