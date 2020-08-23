using System;
using System.Collections.Generic;
using Analogy.Interfaces;
using Analogy.Interfaces.Factories;

namespace Analogy.LogViewer.EventSource.Clef
{
    public class EventSourceFactory : IAnalogyFactory
    {
        internal static Guid Id = new Guid("508DFDE0-3BFC-4407-8A62-4965E472F497");
        public Guid FactoryId => Id;

        public string Title => "Clef EventSource Provider";

        public IEnumerable<IAnalogyChangeLog> ChangeLog { get; } = new List<AnalogyChangeLog>
        {
            new AnalogyChangeLog("First version", AnalogChangeLogType.None, "First version", new DateTime(2020, 08, 21))
        };
        public IEnumerable<string> Contributors { get; } = new List<string> { "Richard Webb" };
        public string About { get; } = "Clef EventSource Provider";
    }
}
