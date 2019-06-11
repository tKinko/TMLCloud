using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using TdsDataObjectExtensions;
using AzureStorageExtensions;

namespace TMLCloud.Models
{
    public class _BaseModel
    {
        public _BaseModel(string container)
        {
            Container = container;
        }

        public long LastDate { get;  set; }
        public long ErrorDate { get;  set; }

        public void UpdateStatus()
        {
            TdsObject.GetStatus(this);
        }

        [IgnoreDataMember]
        protected string Container { get; set; }
        [IgnoreDataMember]
        private const string StorageAccountName = "soracomstrage";
        [IgnoreDataMember]
        private const string StorageAccountKey = "zeO+OMvvahK5m3kCb8o/efzUVjSexl8KwQOwixLWDH4InQ4YpDy7/TplhgKIzvy5XlKBGteMu6tCOYmzIP3D7g==";
        [IgnoreDataMember]
        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        [IgnoreDataMember]
        protected TdsDataObject _tdsDataObject;
        public TdsDataObject TdsObject
        {
            get
            {
                if (_tdsDataObject == null)
                {
                    TdsObject = createTdsDataObject();
                }
                return _tdsDataObject;
            }
            protected set { _tdsDataObject = value; }
        }
        private TdsDataObject createTdsDataObject()
        {
            TdsDataObject tdsDataObject = new TdsDataObject();
            AzureStorageConfig _storageConfig = new AzureStorageConfig()
            {
                ConnectionString = StorageConnectionString,
                Container = Container

            };
            tdsDataObject.InitTables(_storageConfig);
            return tdsDataObject;
        }

        public void DataClear()
        {
            TdsObject.DataClear();
        }

    }
}
