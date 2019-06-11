using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;
using AzureStorageExtensions;
using TdsDataObjectExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SoracomEventsHub
{
    class SoracomEvent : IDisposable
    {
        private const string EventHubConnectionString = "Endpoint=sb://funnel.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=rIPrxxcrG1KdLouSrk3d4Ev68ZeMLPi0dVE27TzQoJA=";
        private const string EventHubName = "sensor0";
        private const string StorageContainerName = "funnel";
        private const string StorageAccountName = "soracomstrage";
        private const string StorageAccountKey = "zeO+OMvvahK5m3kCb8o/efzUVjSexl8KwQOwixLWDH4InQ4YpDy7/TplhgKIzvy5XlKBGteMu6tCOYmzIP3D7g==";

        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);
        private EventProcessorHost eventProcessorHost;

        static private TdsDataObject funnelDataObject;
        static private TdsDataObject beamDataObject;

        public async void StartEventHub()
        {
            Console.WriteLine("Registering EventProcessor...");

            funnelDataObject = createTdsDataObject("funnel");
            beamDataObject = createTdsDataObject("beam");



            eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SoracomEventProcessor>();
        }
        private TdsDataObject createTdsDataObject(string container)
        {
            TdsDataObject tdsDataObject = new TdsDataObject();
            AzureStorageConfig _storageConfig = new AzureStorageConfig()
            {
                ConnectionString = StorageConnectionString,
                Container = container

            };
            tdsDataObject.InitTables(_storageConfig);
            return tdsDataObject;
        }
        //public async void StopEventHub()
        //{
        //    // Disposes of the Event Processor Host
        //    await eventProcessorHost.UnregisterEventProcessorAsync();
        //}

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)。
                }

                if (eventProcessorHost != null)
                {
                    // Disposes of the Event Processor Host
                    Console.WriteLine("Unregistering EventProcessor...");
                    eventProcessorHost.UnregisterEventProcessorAsync();
                }
                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~SoracomEvent() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

        class SoracomEventProcessor : IEventProcessor
        {
            static SoracomDirectData beamDirect = new SoracomDirectData();
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
                JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.DateTimeOffset };

                List<dynamic> funneldatas = new List<dynamic>();
                List<dynamic> beamdatas = new List<dynamic>();
                List<dynamic> curdatas;
                List<dynamic> dataobjects = new List<dynamic>();
                foreach (var eventData in messages)
                {
                    //Console.WriteLine($"SequenceNumber {eventData.Properties["SequenceNumber"]}");

                    dataobjects.Clear();

                    var body = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    dynamic bodyobject = JsonConvert.DeserializeObject(body, serializerSettings);

                    string container = "";
                    try
                    {
                        if (bodyobject.payload != null)
                        {
                            container = "Beam";
                            lock (beamDirect)
                            {
                                dataobjects = beamDirect.ConvertData(bodyobject.payload.Value);
                            }
                            curdatas = beamdatas;
                        }
                        else if (bodyobject.payloads == null)
                        {
                            dataobjects.Add(bodyobject);
                            container = "Beam";
                            curdatas = beamdatas;
                        }
                        else
                        {
                            container = "Funnel";
                            dataobjects.Add(bodyobject.payloads);
                            curdatas = funneldatas;
                        }
                        foreach (var dataobject in dataobjects)
                        {
                            if (dataobject.comb == null)
                            {
                                curdatas.Add(dataobject);
                            }
                            else
                            {
                                foreach (var comb in dataobject.comb)
                                {
                                    curdatas.Add(comb);
                                }
                            }
                        }
                        Console.WriteLine($"{container} received. ");


                        //foreach (dynamic item in dataobject)
                        //{
                        //    Console.WriteLine($"{item.Name}: {item.Value}");
                        //}
                    }catch(Exception e)
                    {
                        if (container == "Beam")
                            beamDataObject.SetErrerMessage(e.Data["org"].ToString(), e.Message);
                        if (container == "Funnel")
                            funnelDataObject.SetErrerMessage(e.Data["org"].ToString(), e.Message);
                    }
                }

                funnelDataObject.UpdateData(funneldatas);
                beamDataObject.UpdateData(beamdatas);

                return context.CheckpointAsync();
            }
        }
    }
    public class SoracomDirectData
    {
        public SoracomDirectData() { CombData = ""; }
        protected string CombData { get; set; }
        protected Encoding enc = Encoding.GetEncoding("UTF-8");
        protected JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.DateTimeOffset };
        protected bool ArrayData { get; set; }
        protected int startIndex;
        public IList<dynamic> ConvertData(string payloadValue)
        {

            int pos;
            IList<dynamic> bodyobjects = new List<dynamic>();

            try
            {

            string s = enc.GetString(Convert.FromBase64String(payloadValue));

            if (CombData == "")
            {
                pos = s.IndexOf('{');
                if (pos == -1)
                    return bodyobjects;

                startIndex = 0;
                CombData = s.Substring(pos);
                pos = s.IndexOf('[');
                if (pos > -1)
                    ArrayData = true;
            }
            else
            {
                CombData += s;
            }

            do
            {
                if(ArrayData == true)
                {
                    pos = CombData.IndexOf(']');
                    if (pos == -1)
                        break;
                    startIndex = pos;
                }
                pos = CombData.IndexOf('}', startIndex) + 1;
                if (pos == 0)
                    break;

                dynamic bodyobject = JsonConvert.DeserializeObject(CombData.Substring(0, pos), serializerSettings);
                if(ArrayData == true && bodyobject.comb != null)
                {
                    bodyobjects.Add(bodyobject);
                    ArrayData = false;
                    startIndex = 0;
                }
                else if (bodyobject.date != null)
                {
                    bodyobjects.Add(bodyobject);
                }

                if (CombData.Length > pos)
                {
                    pos = CombData.IndexOf('{', pos - 1);
                    if (pos == -1)
                    {
                        CombData = "";
                        break;
                    }
                    else
                    {
                        CombData = CombData.Substring(pos);
                        pos = s.IndexOf('[');
                        if (pos > -1)
                            ArrayData = true;
                    }
                }
                else
                {
                    CombData = "";
                    break;
                }
            } while (true);
            }
            catch(Exception e)
            {
                e.Data["org"] = CombData;
                CombData = "";
                ArrayData = false;
                startIndex = 0;
                throw;
            }

            return bodyobjects;
        }
    }
}
