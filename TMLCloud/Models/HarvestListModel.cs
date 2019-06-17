using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using TdsDataObjectExtensions;
using System.Text.RegularExpressions;
using SoracomEventsHub;

namespace TMLCloud.Models
{
    public class HarvestListModel : ListModel
    {
        public HarvestListModel() : base("harvest")
        {
           
        }
        public bool GetHarvestData()
        {
            IList<Subscriber> subscribers = new List<Subscriber>();

            SoracomHarvest soracom = new SoracomHarvest();
            soracom.TdsObject = TdsObject;
            bool ret = soracom.GetHarvestData(subscribers);

            foreach (Subscriber subscriber in subscribers)
            {
                TdsObject.UpdateData(subscriber.data);
            }

            return ret;
        }
    }
    public class Subscriber
    {
        public Subscriber()
        {
            data = new List<dynamic>();
        }
        public string imsi { get; set; }
        public long modifiedTime { get; set; }
        public List<dynamic> data { get; protected set; }
        public string url { get; set; }
    }
    public class SoracomHarvest
    {
        public TdsDataObject TdsObject { get; set; }
        protected SoracomDirectData directData;
        protected string apiKey { get; set; }
        protected string apiToken { get; set; }
        //APIキーとトークンの取得
        protected bool InitHarvest()
        {
            string authData = @"{""email"":""ezawa@tml.jp"",""password"":""TML-ezawa2018""}";
            HttpResponce data = Post("https://api.soracom.io/v1/auth", null, authData);
            if (data == null)
                return false;

            apiKey = data.data.apiKey;
            apiToken = data.data.token;
            return apiKey != null && apiToken != null;
        }
        // Subscriber の取得。　複数回に分かれての受信には対応していない。
        protected void InitSubscribers(IList<Subscriber> subscribers)
        {
            string[] headers = { $"X-Soracom-API-Key:{apiKey}", $"X-Soracom-Token:{apiToken}" };
            HttpResponce data = Get("https://api.soracom.io/v1/subscribers", headers);

            if (data == null)
                return;

            foreach (var item in data.data)
            {
                subscribers.Add(new Subscriber() {
                    imsi = item.imsi,
                    modifiedTime = item.lastModifiedAt,
                    url = $"https://api.soracom.io/v1/subscribers/{item.imsi}/data?sort=asc"
                });
            }
        }
        public bool GetHarvestData(IList<Subscriber> subscribers)
        {
            if (InitHarvest() == false)
                return false;

            if (subscribers.Count == 0)
            {
                InitSubscribers(subscribers);
                if (subscribers.Count == 0)
                    return false;
            }

            foreach (var item in subscribers)
            {
                directData = new SoracomDirectData();

                do
                {
                    GetSubScriverData(item);
                } while (item.url != "");
            }
            return true;
        }
        public bool GetSubScriverData(Subscriber subscriber)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.DateTimeOffset };
            string[] headers = { $"X-Soracom-API-Key:{apiKey}", $"X-Soracom-Token:{apiToken}" };

            HttpResponce data = Get(subscriber.url, headers);

            if (data == null)
                return false;

            subscriber.url = "";
            if (data.Headers.Get("link") != null)
            {
                string[] l = data.Headers.Get("link").Split(",");
                foreach (var s in l)
                {
                    if (s.IndexOf("next") > -1)
                    {
                        Match m = Regex.Match(s, @"(\<)(?<url>.+?)(\>)");
                        if (m.Success)
                            subscriber.url = "https://api.soracom.io" + m.Groups["url"].Value;
                        break;
                    }
                }
            }

            foreach (var item in data.data)
            {
                var bodybject = JsonConvert.DeserializeObject(item.content.Value, serializerSettings);

                try
                {
                    if (bodybject.payload != null)
                    {
                        List<dynamic> dataobjects = directData.ConvertData(bodybject.payload.Value);

                        if (dataobjects.Count > 0)
                            subscriber.data.AddRange(dataobjects);

                    }
                    else
                    {
                        if (bodybject.date == null)
                        {
                            var baseDt = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                            bodybject.date = new DateTimeOffset(item.time.Value * 10000 + baseDt.Ticks, TimeSpan.Zero);
                        }
                        subscriber.data.Add(bodybject);
                    }
                }
                catch (Exception e)
                {
                    TdsObject.SetErrerMessage(e.Data["org"].ToString(), e.Message);
                }
            }

            return true;
        }
        protected dynamic Post(string url, string[] headers, string contents)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Accept = "application/json";
            if (headers != null)
            {
                foreach (string head in headers)
                    request.Headers.Add(head);
            }
            request.ContentType = "application/json";


            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {

                streamWriter.Write(contents);
                streamWriter.Flush();
                streamWriter.Close();
            }

            HttpResponce bodybject = new HttpResponce();
            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    bodybject.data = JsonConvert.DeserializeObject(result);
                }
                bodybject.Headers = httpResponse.Headers;
            }
            catch
            {
                bodybject = null;
            }

            return bodybject;
        }

        protected HttpResponce Get(string url, string[] headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "application/json";
            if (headers != null)
            {
                foreach (string head in headers)
                    request.Headers.Add(head);
            }
            request.ContentType = "application/json";

            HttpResponce bodybject = new HttpResponce();
            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    bodybject.data = JsonConvert.DeserializeObject(result);
                }
                bodybject.Headers = httpResponse.Headers;
            }
            catch
            {
                bodybject = null;
            }

            return bodybject;
        }
        protected class HttpResponce
        {
            public WebHeaderCollection Headers {get;set;}
            public dynamic data { get; set; }
        }
    }
}
