using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace SoracomEventsHub
{
    class Program
    {
        private const string EventHubConnectionString = "Endpoint=sb://funnel.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=rIPrxxcrG1KdLouSrk3d4Ev68ZeMLPi0dVE27TzQoJA=";
        private const string EventHubName = "sensor0";
        private const string StorageContainerName = "Events";
        private const string StorageAccountName = "soracomstrage";
        private const string StorageAccountKey = "zeO+OMvvahK5m3kCb8o/efzUVjSexl8KwQOwixLWDH4InQ4YpDy7/TplhgKIzvy5XlKBGteMu6tCOYmzIP3D7g==";
        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Registering EventProcessor...");

            var eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SoracomEventProcessor>();

            Console.WriteLine("Receiving. Press ENTER to stop worker.");
            Console.ReadLine();

            // Disposes of the Event Processor Host
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}
