
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using TDS7130v2AC.Extensions;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Text;

namespace BeamFunction
{
    public static class Function1
    {
        private const string EventHubConnectionString = "Endpoint=sb://funnel.servicebus.windows.net/;SharedAccessKeyName=RootManagerSharedAccessKey;SharedAccessKey=8Lm0dp6ICRFzg0FEFOiZvbqz7gFguNRx8TDBUtNOdS4=;EntityPath=sensor0";
        private const string EventHubName = "sensor0";
        private static ulong count = 0;
        //private const string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=soracomstrage;AccountKey=zeO+OMvvahK5m3kCb8o/efzUVjSexl8KwQOwixLWDH4InQ4YpDy7/TplhgKIzvy5XlKBGteMu6tCOYmzIP3D7g==;EndpointSuffix=core.windows.net";
        //[FunctionName("SoracomBeam")]
        //public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        //{
        //    log.Info("C# HTTP trigger function processed a request.");

        //    string name = req.Query["data"];

        //    string requestBody = new StreamReader(req.Body).ReadToEnd();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    name = name ?? data?.data;

        //    AzureStorageConfig _storageConfig = new AzureStorageConfig()
        //    {
        //        ConnectionString = AzureStorageConnectionString,
        //        Container = "beamdata"
        //    };

        //    AzureStorageExtensions.SaveInfoLog(_storageConfig, requestBody);

        //    return (ActionResult)new OkObjectResult("0");
        //    //return (ActionResult)new OkObjectResult($"operatorId {requestBody}");
        //}
        static private ulong GetNextSequenceNumber()
        {
            if (++count >= ulong.MaxValue)
                count = 0;
            return count;
        }
        [FunctionName("SoracomBeam")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };
            var sequenceNumber = GetNextSequenceNumber();

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            var data = new EventData(Encoding.UTF8.GetBytes(requestBody));
            data.Properties.Add("SequenceNumber", sequenceNumber);
            eventHubClient.SendAsync(data).Wait();
            eventHubClient.CloseAsync().Wait();

            return (ActionResult)new OkObjectResult("200");
        }
    }
}
