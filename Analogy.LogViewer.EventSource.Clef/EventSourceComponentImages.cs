using System;
using System.Drawing;
using Analogy.DataProviders.Extensions;
using Analogy.LogViewer.EventSource.Clef.Properties;

namespace Analogy.LogViewer.EventSource.Clef
{
    public class EventSourceComponentImages : IAnalogyComponentImages
    {
        public Image GetLargeImage(Guid analogyComponentId)
        {
            if (analogyComponentId == EventSourceFactory.Id)
                return Resources.Serilog_icon32x32;
            return null;
        }

        public Image GetSmallImage(Guid analogyComponentId)
        {
            if (analogyComponentId == EventSourceFactory.Id)
                return Resources.Serilog_icon;
            return null;
        }

        public Image GetOnlineConnectedLargeImage(Guid analogyComponentId) => null;

        public Image GetOnlineConnectedSmallImage(Guid analogyComponentId) => null;

        public Image GetOnlineDisconnectedLargeImage(Guid analogyComponentId) => null;

        public Image GetOnlineDisconnectedSmallImage(Guid analogyComponentId) => null;
    }
}

