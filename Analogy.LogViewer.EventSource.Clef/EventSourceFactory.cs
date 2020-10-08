using Analogy.Interfaces;
using Analogy.Interfaces.Factories;
using System;
using System.Collections.Generic;
using System.Drawing;
using Analogy.LogViewer.EventSource.Clef.Properties;

namespace Analogy.LogViewer.EventSource.Clef
{
    public class EventSourceFactory : IAnalogyFactory
    {
        internal static Guid Id = new Guid("508DFDE0-3BFC-4407-8A62-4965E472F497");
        public Guid FactoryId { get; set; } = Id;

        public string Title { get; set; } = "Clef EventSource Provider";

        public IEnumerable<IAnalogyChangeLog> ChangeLog { get; set; } = new List<AnalogyChangeLog>
        {
            new AnalogyChangeLog("First version", AnalogChangeLogType.None, "First version", new DateTime(2020, 08, 21))
        };

        public Image LargeImage { get; set; } = Resources.Serilog_icon32x32;
        public Image SmallImage { get; set; } = Resources.Serilog_icon;
        public IEnumerable<string> Contributors { get; set; } = new List<string> { "Richard Webb" };
        public string About { get; set; } = "Clef EventSource Provider";
    }
}
