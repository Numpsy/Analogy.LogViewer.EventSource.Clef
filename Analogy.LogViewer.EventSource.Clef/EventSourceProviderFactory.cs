using System;
using System.Collections.Generic;
using Analogy.Interfaces;
using Analogy.Interfaces.Factories;

namespace Analogy.LogViewer.EventSource.Clef
{
    public class EventSourceProviderFactory : IAnalogyDataProvidersFactory
    {
        //serilog id to combine it:
        //public Guid FactoryId { get; set; } = new Guid("513A4393-425E-4054-92D4-6A816983E51F");

        public Guid FactoryId { get; set; } = EventSourceFactory.Id;
        public string Title { get; set; } = "Clef EventSource Provider";

        public IEnumerable<IAnalogyDataProvider> DataProviders => new List<IAnalogyDataProvider>
        {
            new EventSourceDataProvider(),
        };
    }
}
