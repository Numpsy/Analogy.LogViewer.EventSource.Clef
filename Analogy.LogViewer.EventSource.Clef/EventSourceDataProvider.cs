using Analogy.Interfaces;
using Analogy.LogViewer.Serilog;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;
using Newtonsoft.Json;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analogy.LogViewer.Serilog.DataTypes;
using Newtonsoft.Json.Linq;
using Serilog.Events;

namespace Analogy.LogViewer.EventSource.Clef
{
    class EventSourceDataProvider : IAnalogyRealTimeDataProvider
    {
        // No files to handle?
        IAnalogyOfflineDataProvider? IAnalogyRealTimeDataProvider.FileOperationsHandler { get; }
        public Image? ConnectedLargeImage { get; set; }
        public Image? ConnectedSmallImage { get; set; }
        public Image? DisconnectedLargeImage { get; set; }
        public Image? DisconnectedSmallImage { get; set; }

        public Guid Id { get; set; } = new Guid("82E677CB-6C2C-4ED9-AE6F-BD68D465D0B2");

        // simple fixed display name string.
        string IAnalogyDataProvider.OptionalTitle { get; set; }= "Clef EventSource";

        // We don't use any custom colours.
        bool IAnalogyDataProvider.UseCustomColors { get; set; }

        // Events - these are managed by the host app.
        public event EventHandler<AnalogyDataSourceDisconnectedArgs>? OnDisconnected;
        public event EventHandler<AnalogyLogMessageArgs>? OnMessageReady;
        public event EventHandler<AnalogyLogMessagesArgs>? OnManyMessagesReady;

        // We could possibly return false here if we aren't running elevated, but rather than doing that
        // we'll start up and then report an error from StartReceiving()
        Task<bool> IAnalogyRealTimeDataProvider.CanStartReceiving()
        {
            return Task.FromResult(true);
        }

        // We don't currently do anything with the colours.
        (Color backgroundColor, Color foregroundColor) IAnalogyDataProvider.GetColorForMessage(IAnalogyLogMessage logMessage) => (Color.Empty, Color.Empty);

        // We don't currently do anything special with the headers.
        IEnumerable<(string originalHeader, string replacementHeader)> IAnalogyDataProvider.GetReplacementHeaders() => Enumerable.Empty<(string originalHeader, string replacementHeader)>();

        // For logging
        private IAnalogyLogger? Logger { get; set; }

        // Create and reuse a single deserializer.
        private readonly Lazy<JsonSerializer> serializer = new Lazy<JsonSerializer>(() => CreateSerializer(), LazyThreadSafetyMode.None);

        // set some things up.
        Task IAnalogyDataProvider.InitializeDataProviderAsync(IAnalogyLogger logger)
        {
            this.Logger = logger;
            return Task.CompletedTask;
        }

        // Nothing to do here.
        void IAnalogyDataProvider.MessageOpened(AnalogyLogMessage message)
        {
        }

        private TraceEventSession? traceEventSession;
        private ITextFormatter? textFormatter;

        Task IAnalogyRealTimeDataProvider.StartReceiving()
        {
            if (!(TraceEventSession.IsElevated() ?? false))
            {
                // log an error and put a message in the viewer window.
                this.Logger?.LogError("Clef EventSource Provider", "Process must be run as admin in order to view EventSource events");

                AnalogyLogMessage m = new AnalogyLogMessage
                {
                    Class = AnalogyLogClass.General,
                    Date = DateTime.Now,
                    Module = "Analogy.LogViewer.EventSource.Clef",
                    Level = AnalogyLogLevel.Error,
                    Text = "Process must be run as admin in order to view EventSource events",
                };

                OnMessageReady?.Invoke(this, new AnalogyLogMessageArgs(m, Environment.MachineName, string.Empty, Id));

            }
            else
            {
                // Create text formatter
                // @@TODO@@ Make the template configurable
                this.textFormatter = new MessageTemplateTextFormatter("{Message}{NewLine}{Exception}");

                // Create trace session.
                // @@TODO@@ Does the session name need to be configurable or per-instance?
                this.traceEventSession = new TraceEventSession("AnalofyClefSourceSession");

                // Set up the provider. This could be configurable to allow for alternate providers.
                traceEventSession.EnableProvider("Serilog-Sinks-EventSource-Clef");

                traceEventSession.Source.Dynamic.AddCallbackForProviderEvent("Serilog-Sinks-EventSource-Clef", "Message", delegate (TraceEvent data)
                {
                    // get the JSON payload
                    var clefPayload = (string)data.PayloadByName("clefPayload");
                    // parse it (TODO:: Error handling!)
                    var logEvent = ReadFromString(clefPayload, serializer.Value);
                    var logMessage = CommonParser.ParseLogEventProperties(logEvent);

                    // format the message text
                    using (var sr = new StringWriter())
                    {
                        this.textFormatter.Format(logEvent, sr);
                        logMessage.Text = sr.ToString().Trim();
                    }

                    // patch up various properties with the ones from the TraceEvent
                    // @@TBD@@ Which data should take precedence if available in both the TraceEvent and CLEF ?
                    if (logMessage.ThreadId == 0)
                        logMessage.ThreadId = data.ThreadID;

                    if (logMessage.ProcessId == 0)
                        logMessage.ProcessId = data.ProcessID;

                    if (string.IsNullOrEmpty(logMessage.Module))
                        logMessage.Module = data.ProcessName;

                    OnMessageReady?.Invoke(this, new AnalogyLogMessageArgs(logMessage, string.Empty, "Clef EventSource", Id));
                });

                // @@TBD@@ Store this in case we need extra clean-up on it later?
                Task.Run(() => traceEventSession.Source.Process());
            }
            return Task.CompletedTask;
        }

        // Tidy up the TraceEventSession on shutdown, or it can be left around after the viewer has gone away.
        Task IAnalogyRealTimeDataProvider.StopReceiving()
        {
            traceEventSession?.Source?.StopProcessing();
            traceEventSession?.Dispose();
            traceEventSession = null;

            OnDisconnected?.Invoke(this, new AnalogyDataSourceDisconnectedArgs("user disconnected", Environment.MachineName, Id));
            return Task.CompletedTask;
        }

        static JsonSerializer CreateSerializer()
        {
            return JsonSerializer.Create(new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None,
                Culture = CultureInfo.InvariantCulture
            });
        }

        static LogEvent ReadFromString(string document, JsonSerializer serializer)
        {
            using var jsonReader = new JsonTextReader(new StringReader(document));
            var jObject = serializer.Deserialize<JObject>(jsonReader);

            return LogEventReader.ReadFromJObject(jObject, new CompactJsonFormatMessageFields());
        }
    }
}
