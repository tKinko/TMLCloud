using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AzureStorageExtensions;

namespace SoracomEventsHub
{
    class SoracomEventProcessor : IEventProcessor
    {
        private const string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=soracomstrage;AccountKey=zeO+OMvvahK5m3kCb8o/efzUVjSexl8KwQOwixLWDH4InQ4YpDy7/TplhgKIzvy5XlKBGteMu6tCOYmzIP3D7g==;EndpointSuffix=core.windows.net";
        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                var body = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                dynamic bodybject = JsonConvert.DeserializeObject(body);
                dynamic dataobject;

                string soracom;
                if (bodybject.payloads == null)
                {
                    dataobject = bodybject;
                    soracom = "Beam";
                }
                else
                {
                    dataobject = bodybject.payloads;
                    soracom = "Funnel";
                }

                AzureStorageConfig _storageConfig = new AzureStorageConfig()
                {
                    ConnectionString = AzureStorageConnectionString,
                    Container = soracom
                };

                Console.WriteLine($"{soracom} received. date: '{dataobject.date}', Data: '{dataobject.data}'");

                //Soracom soracom = Soracom.ParseJSON(data);
            }
            return context.CheckpointAsync();
        }
    }
}
