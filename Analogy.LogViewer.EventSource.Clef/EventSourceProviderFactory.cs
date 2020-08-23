using System;
using System.Collections.Generic;
using Analogy.Interfaces;
using Analogy.Interfaces.Factories;

namespace Analogy.LogViewer.EventSource.Clef
{
    public class EventSourceProviderFactory : IAnalogyDataProvidersFactory
    {
        public Guid FactoryId { get; } = EventSourceFactory.Id;
        public string Title => "Clef EventSource Provider";

        public IEnumerable<IAnalogyDataProvider> DataProviders => new List<IAnalogyDataProvider>
        {
            new EventSourceDataProvider(),
        };
    }
}
