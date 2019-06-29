using EFCoreChangeReport.Models;
using System.Collections.Generic;

namespace EFCoreChangeReport.Sinks
{
    public class Sink
    {
        public delegate void SinkDelegate(List<Report> reports);
    }
}
