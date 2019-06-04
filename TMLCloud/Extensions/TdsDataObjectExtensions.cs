using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using GoogleVisualizationDataExtensions;
using AzureStorageExtensions;
using TMLCloud.Models;

namespace TdsDataObjectExtensions
{

    public class TdsDataObject : Object
    {
        public static readonly int ENTITE_NUMS = 250;
        private TdsTableManager tableManager;
        public enum TDS7130_FORMAT
        {
            // TMLDataType format
            kDefDblFmtFlg = -10001,
            kUseFmtFlg = -10000,
            kDefFmtFlg = -9999,
            kFloat_01_00_FmtFlg,

            kFmtp0 = 0,
            kFmtp1,
            kFmtp2,
            kFmtp3,
            kFmtp4,
            kFmtp5,
            kFmtp6,
            kFmtp7,
            kFmtp8,
            kFmtp9,
            kFmtp10,
            kFmtp11,
            kFmtp12,
            kFmtp13,
            kFmtp14,
            kFmtp15,

            kFmtE0 = 50,
            kFmtE1,
            kFmtE2,
            kFmtE3,
            kFmtE4,
            kFmtE5,
            kFmtE6,
            kFmtE7,

            // long format
            kFmtDate_YMD_HMS = 100,
            kFmtDate_MDY_HMS,
            kFmtDate_HMS,
            kFmtDate_YMD,
            kFmtDate_MD,
            kFmtDate_MD_H,
            kFmtDate_HM,

            kFmtDate_YYMD_HMS,
            kFmtDate_YYMDHM_,
            kFmtDate_MDYY_HMS,
            kFmtDate_MDY,

            kFmtDate_YMD_HMS_MS,
            kFmtDate_MDY_HMS_MS,
            kFmtDate_YYMD_HMS_MS,
            kFmtDate_MDYY_HMS_MS,

            kFmtDate_MS,
            kFmtDate_S_MS,
            kFmtDate_UTC,

            kFmtStr = 200,
        };

        public class TdsStatus
        {
            public long status { get; set; }
            public DateTimeOffset Timestamp { get; set; }
        }
        public void InitTables(AzureStorageConfig azureStorageConfig)
        {
            tableManager = new TdsTableManager(azureStorageConfig);
        }
        public void DataClear()
        {
            tableManager.DataClear();
        }
        public long LastDate()
        {
            TdsStatus status = tableManager.GetTdsStatus("Date");
            if (status == null)
                return 0;

            return status.status;
        }
        protected void UpdateLastDate()
        {
            tableManager.SetTdsStatus("Date", DateTime.UtcNow.Ticks);
        }
        public void UpdateData(List<dynamic> datas)
        {
            if (datas.Count() > 0)
            {
                foreach (dynamic data in datas)
                {
                    SetEntityData(data);
                }
                UpdateLastDate();
            }
        }
        private void SetEntityData(dynamic data)
        {
            Console.WriteLine("SetEntityData start");
            tableManager.DataEntityManager.ResetEntity();
            tableManager.NameEntityManager.ResetEntity();

            DateTimeOffset dateTime = DateTimeOffset.Now;
            foreach (dynamic item in data)
            {
                if (item.Name == "date")
                {
                    dateTime = item.Value;
                    break;
                }
            }
            foreach (dynamic item in data)
            {
                if (item.Name == "date")
                    continue;
                tableManager.SetEntityItem(dateTime, item);
            }

            tableManager.InsertOrMargeDataTableEntity();
            Console.WriteLine("SetEntityData end\n");
        }
        public GoogleVisualizationDataTable CreatTdsDataTable(int lastStep = -1, int count = 50)
        {
            GoogleVisualizationDataTable dataTable;
            tableManager.CreatDataTable(out dataTable);

            return dataTable;
        }
        public void GetStatus(_BaseModel baseModel)
        {
            baseModel.LastDate = LastDate();
        }
        static public string GetFormatTds2Google(TDS7130_FORMAT format)
        {
            string fmt = "";
            switch (format)
            {
                case TDS7130_FORMAT.kDefDblFmtFlg:
                case TDS7130_FORMAT.kUseFmtFlg:
                case TDS7130_FORMAT.kDefFmtFlg:
                case TDS7130_FORMAT.kFloat_01_00_FmtFlg:
                    break;
                case TDS7130_FORMAT.kFmtp0:
                case TDS7130_FORMAT.kFmtp1:
                case TDS7130_FORMAT.kFmtp2:
                case TDS7130_FORMAT.kFmtp3:
                case TDS7130_FORMAT.kFmtp4:
                case TDS7130_FORMAT.kFmtp5:
                case TDS7130_FORMAT.kFmtp6:
                case TDS7130_FORMAT.kFmtp7:
                case TDS7130_FORMAT.kFmtp8:
                case TDS7130_FORMAT.kFmtp9:
                case TDS7130_FORMAT.kFmtp10:
                case TDS7130_FORMAT.kFmtp11:
                case TDS7130_FORMAT.kFmtp12:
                case TDS7130_FORMAT.kFmtp13:
                case TDS7130_FORMAT.kFmtp14:
                case TDS7130_FORMAT.kFmtp15:
                    fmt = "0." + new String('0', (int)format);
                    break;
                case TDS7130_FORMAT.kFmtE0:
                case TDS7130_FORMAT.kFmtE1:
                case TDS7130_FORMAT.kFmtE2:
                case TDS7130_FORMAT.kFmtE3:
                case TDS7130_FORMAT.kFmtE4:
                case TDS7130_FORMAT.kFmtE5:
                case TDS7130_FORMAT.kFmtE6:
                case TDS7130_FORMAT.kFmtE7:
                    break;

                // long format
                case TDS7130_FORMAT.kFmtDate_YMD_HMS:
                    fmt = "yy/MM/dd HH:mm:ss";
                    break;
                case TDS7130_FORMAT.kFmtDate_MDY_HMS:
                    fmt = "MM/dd/yy HH:mm:ss";
                    break;
                case TDS7130_FORMAT.kFmtDate_HMS:
                    fmt = "HH:mm:ss";
                    break;
                case TDS7130_FORMAT.kFmtDate_YMD:
                    fmt = "yy/MM/dd";
                    break;
                case TDS7130_FORMAT.kFmtDate_MD:
                    fmt = "MM/dd";
                    break;
                case TDS7130_FORMAT.kFmtDate_MD_H:
                    fmt = "MM/dd HH";
                    break;
                case TDS7130_FORMAT.kFmtDate_HM:
                    fmt = "HH:MM";
                    break;

                case TDS7130_FORMAT.kFmtDate_YYMD_HMS:
                    fmt = "yyyy/MM/dd HH:mm:ss";
                    break;
                case TDS7130_FORMAT.kFmtDate_YYMDHM_:
                    fmt = "yyyyMMddHH";
                    break;
                case TDS7130_FORMAT.kFmtDate_MDYY_HMS:
                    fmt = "MM/dd/yyyy HH:mm:ss";
                    break;
                case TDS7130_FORMAT.kFmtDate_MDY:
                    fmt = "MM/dd/yy";
                    break;

                case TDS7130_FORMAT.kFmtDate_YMD_HMS_MS:
                    fmt = "yy/MM/dd HH:mm:ss.ff";
                    break;
                case TDS7130_FORMAT.kFmtDate_MDY_HMS_MS:
                    fmt = "MM/dd/yy HH:mm:ss.ff";
                    break;
                case TDS7130_FORMAT.kFmtDate_YYMD_HMS_MS:
                    fmt = "yyyy/MM/dd HH:mm:ss.ff";
                    break;
                case TDS7130_FORMAT.kFmtDate_MDYY_HMS_MS:
                    fmt = "MM/dd/yyyy HH:mm:ss.ff";
                    break;

                case TDS7130_FORMAT.kFmtDate_MS:
                    fmt = "FF";
                    break;
                case TDS7130_FORMAT.kFmtDate_S_MS:
                    fmt = "ss.ff";
                    break;
                case TDS7130_FORMAT.kFmtDate_UTC:
                    fmt = "yyyy-MM-ddTHH:mm:ss.ffzzz";
                    break;
            }
            return fmt;
        }
        static public bool IsDateTimeFormat(TDS7130_FORMAT format)
        {
            bool fmt = false;
            switch (format)
            {
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_YMD_HMS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_MDY_HMS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_HMS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_YMD:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_MD:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_MD_H:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_HM:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_YYMD_HMS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_YYMDHM_:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_MDYY_HMS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_MDY:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_YMD_HMS_MS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_MDY_HMS_MS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_YYMD_HMS_MS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_MDYY_HMS_MS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_MS:
                case TdsDataObject.TDS7130_FORMAT.kFmtDate_S_MS:
                    fmt = true;
                    break;
                default:
                    fmt = false;
                    break;
            }

            return fmt;
        }

        class TdsTableManager
        {
            public TdsTableManager(AzureStorageConfig azureStorageConfig)
            {
                AzureStorageConfig = azureStorageConfig;
                NameEntityManager = new TdsTableEntityManager<string>(this, "Setting");
                DataEntityManager = new TdsTableEntityManager<double>(this, "Data");
            }
            public void DataClear()
            {
                CloudTableClient tableClient = AzureStorageExtension.GetCloudTableClient(AzureStorageConfig);

                TableContinuationToken token = new TableContinuationToken();
                do
                {
                    var orphanedTables = tableClient.ListTablesSegmentedAsync(AzureStorageConfig.Container, token).Result;
                    token = orphanedTables.ContinuationToken;
                    foreach (CloudTable orphanedTableName in orphanedTables.Results)
                    {
                        orphanedTableName.DeleteAsync();
                        while (orphanedTableName.ExistsAsync().Result == true) { }
                    }
                }
                while (token != null);
                Thread.Sleep(50000);
            }
            public string DataKey { get; set; }
            public TdsTableEntityManager<string> NameEntityManager { get; protected set; }
            public TdsTableEntityManager<double> DataEntityManager { get; protected set; }
            protected AzureStorageConfig AzureStorageConfig { get; set; }

            public CloudTable GetTable(int index)
            {
                CloudTableClient tableClient = AzureStorageExtension.GetCloudTableClient(AzureStorageConfig);

                string str = string.Format("{0}{1}", AzureStorageConfig.Container, index);
                CloudTable table = tableClient.GetTableReference(str);

                table.CreateIfNotExistsAsync().Wait();

                while (table.ExistsAsync().Result != true) { }

                return table;
            }
            private const string DATETIMEOFFSET_FORMAT = "yyyyMMddHHmmssfffffffzzz";
            public void SetEntityItem(DateTimeOffset dateTime, dynamic item)
            {
                char[] nums = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                string name = (string)item.Name;
                int pos = name.IndexOfAny(nums);
                int ch = int.Parse(name.Substring(pos));
                int index = ch / ENTITE_NUMS + 1;
                var prop = string.Format("C{0:D4}", ch);

                DataKey = dateTime.UtcDateTime.ToString("DyyyyMMddHHmmssfff");
                DynamicTableEntity nameTableEntity = NameEntityManager.GetTableEntity(index, "Name");
                DynamicTableEntity dataTableEntity = DataEntityManager.GetTableEntity(index, DataKey);
                SetEntityValue(nameTableEntity, "ADATE", EntityProperty.GeneratePropertyForString("Date Time"));
                SetEntityValue(dataTableEntity, "ADATE", EntityProperty.GeneratePropertyForString(dateTime.ToString(DATETIMEOFFSET_FORMAT)));

                SetEntityValue(nameTableEntity, prop, EntityProperty.GeneratePropertyForString(item.Name));
                SetEntityValue(dataTableEntity, prop, EntityProperty.GeneratePropertyForDouble((double)item.Value));
            }
            private void SetEntityValue(DynamicTableEntity tdsTableEntity, string key, EntityProperty item)
            {
                IDictionary<string, EntityProperty> nameInfos = tdsTableEntity.Properties;
                if (nameInfos.ContainsKey(key) == false)
                {
                    nameInfos.Add(key, item);
                }
                else
                {
                    nameInfos[key] = item;
                }
            }

            public void InsertOrMargeDataTableEntity()
            {
                NameEntityManager.InsertOrMargeDataTableEntity();
                DataEntityManager.InsertOrMargeDataTableEntity();
                SetTdsDataStatus();
            }
            public TdsStatus GetTdsStatus(string key)
            {
                CloudTable _table0 = GetTable(0);
                TdsStatus ret = null;
                TableOperation tableOperation = TableOperation.Retrieve<DynamicTableEntity>("Prop", key);
                try
                {
                    TableResult tableResult = _table0.ExecuteAsync(tableOperation).Result;
                    if (tableResult.Result != null)
                    {
                        ret = new TdsStatus();
                        ret.status = (long)((DynamicTableEntity)tableResult.Result).Properties["N001"].Int64Value;
                        ret.Timestamp = ((DynamicTableEntity)tableResult.Result).Timestamp;
                    }
                }
                catch
                {
                    ret = null;
                }
                return ret;
            }
            public void SetTdsStatus(string key, long value)
            {
                CloudTable _table0 = GetTable(0);

                DynamicTableEntity tdsPropEntity = new DynamicTableEntity("Prop", key);
                tdsPropEntity.Properties.Add("N001", EntityProperty.GeneratePropertyForLong( value));
                _table0.ExecuteAsync(TableOperation.InsertOrMerge(tdsPropEntity)).Wait();
            }
            public void SetTdsDataStatus()
            {
                CloudTable _table0 = GetTable(0);
                IList<int> useTabels = DataEntityManager.UseTabels();
                DynamicTableEntity tdsPropEntity = new DynamicTableEntity("Data", DataKey);
                foreach (int i in useTabels)
                {
                    tdsPropEntity.Properties.Add(string.Format("N{0,3:D3}", i), EntityProperty.GeneratePropertyForInt(i));
                }
                _table0.ExecuteAsync(TableOperation.InsertOrMerge(tdsPropEntity)).Wait();
            }
            public void CreatDataTable(out GoogleVisualizationDataTable dataTable)
            {
                dataTable = new GoogleVisualizationDataTable();
                Dictionary<string, int> tableIndex = new Dictionary<string, int>();
                GetNames(dataTable, tableIndex);
                GetDatas(dataTable, tableIndex);
            }
            protected void GetNames(GoogleVisualizationDataTable dataTable, Dictionary<string, int> tableIndex)
            {
                int index = 1;
                bool addDate = true;
                TableOperation tableOperation = TableOperation.Retrieve<DynamicTableEntity>("Setting", "Name");
                CloudTableClient tableClient = AzureStorageExtension.GetCloudTableClient(AzureStorageConfig);
                string table0 = string.Format("{0}0", AzureStorageConfig.Container);

                TableContinuationToken token = new TableContinuationToken();
                do
                {
                    var orphanedTables = tableClient.ListTablesSegmentedAsync(AzureStorageConfig.Container, token).Result;
                    token = orphanedTables.ContinuationToken;
                    foreach (CloudTable cloudTable in orphanedTables.Results)
                    {
                        if (cloudTable.Name == table0)
                            continue;

                        DynamicTableEntity tdsTableEntity = (DynamicTableEntity)cloudTable.ExecuteAsync(tableOperation).Result.Result;
                        IDictionary<string, EntityProperty> nameInfos = tdsTableEntity.Properties;
                        string name = "";
                        foreach (KeyValuePair<string, EntityProperty> pInfo in nameInfos)
                        {
                            if (pInfo.Key != "PartitionKey" && pInfo.Key != "RowKey" && pInfo.Key != "Timestamp")
                            {
                                name = pInfo.Value.StringValue; //.GetValue(tdsTableEntity);
                                if (name == null)
                                    break;

                                if (pInfo.Key == "ADATE")
                                {
                                    if (addDate)
                                    {
                                        dataTable.AddColumn(name, "datetime");
                                        tableIndex.Add(pInfo.Key, 0);
                                        addDate = false;
                                    }
                                }
                                else
                                {
                                    dataTable.AddColumn(name, "number");
                                    tableIndex.Add(pInfo.Key, index++);
                                }
                            }
                        }
                    }
                }
                while (token != null);
            }
            protected void GetDatas(GoogleVisualizationDataTable dataTable, Dictionary<string, int> tableIndex)
            {
                CloudTable _table0 = GetTable(0);
                TableQuery<DynamicTableEntity> tableQuery = new TableQuery<DynamicTableEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Data"));

                var maxCols = dataTable.cols.Count;
                TableContinuationToken continuationToken = null;
                TableQuerySegment<DynamicTableEntity> tableQueryResult = _table0.ExecuteQuerySegmentedAsync(tableQuery, continuationToken).Result;



                foreach (DynamicTableEntity tableEntity in tableQueryResult.Results)
                {
                    IDictionary<string, EntityProperty> nameInfos = tableEntity.Properties;
                    List<int> list = new List<int>();
                    foreach (KeyValuePair<string, EntityProperty> pInfo in nameInfos)
                    {
                        list.Add((int)pInfo.Value.Int32Value);
                    }
                    list.Sort();

                    var values = new List<GoogleVisualizationDataTable.Row.RowValue>();
                    foreach(var item in tableIndex)
                        values.Add(new GoogleVisualizationDataTable.Row.RowValue(null, TDS7130_FORMAT.kDefDblFmtFlg));

                    bool addDate = true;
                    foreach (int index in list)
                    {
                        CloudTable _table = GetTable(index);
                        TableResult tableResult = _table.ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>("Data", tableEntity.RowKey)).Result;
                        AddDataTableRow(maxCols, values, (DynamicTableEntity)tableResult.Result, ref addDate, tableIndex);
                    }
                    dataTable.AddRow(values);
                }
            }
            private void AddDataTableRow(int maxCols, List<GoogleVisualizationDataTable.Row.RowValue> values, DynamicTableEntity tableEntity, ref bool addDate, Dictionary<string, int> tableIndex)
            {
                IDictionary<string, EntityProperty> nameInfos = tableEntity.Properties;
                foreach (KeyValuePair<string, EntityProperty> pInfo in nameInfos)
                {
                    if (pInfo.Key != "PartitionKey" && pInfo.Key != "RowKey" && pInfo.Key != "Timestamp")
                    {
                        var value = pInfo.Value;
                        if (value == null)
                            continue;

                         if (tableIndex.ContainsKey(pInfo.Key) == false)
                            continue;

                        int index = tableIndex[pInfo.Key];
                        if (pInfo.Key == "ADATE")
                        {
                            if (!addDate)
                                continue;
                            addDate = false;

                            values[index].format = TdsDataObject.TDS7130_FORMAT.kFmtDate_YYMD_HMS_MS;
                            values[index].objValue = DateTimeOffset.ParseExact(value.StringValue, DATETIMEOFFSET_FORMAT, CultureInfo.DefaultThreadCurrentUICulture);
                        }
                        else
                        {
                            values[index].format = TDS7130_FORMAT.kDefDblFmtFlg;
                            values[index].objValue = value.DoubleValue;
                        }
                    }
                }
            }
        }
        class TdsTableEntityManager<T>
        {
            public TdsTableManager TdsTableManager { get; protected set; }
            public string PartitionKey { get; protected set; }

            protected DynamicTableEntity[] tableEntitys = { };

            public TdsTableEntityManager(TdsTableManager tdsTableManager, string partitionKey)
            {
                TdsTableManager = tdsTableManager;
                PartitionKey = partitionKey;
            }
            public DynamicTableEntity GetTableEntity(int index, string rowKey)
            {
                if (index >= tableEntitys.Count())
                {
                    Array.Resize(ref tableEntitys, index + 1);
                }

                DynamicTableEntity tableEntity = tableEntitys[index];
                if (tableEntity == null)
                {
                        tableEntitys[index] = tableEntity = new DynamicTableEntity(PartitionKey, rowKey);
                }
                return tableEntity;
            }
            public void InsertOrMargeDataTableEntity()
            {
                for (int index = 1; index < tableEntitys.Count(); index++)
                {
                    DynamicTableEntity tableEntity = tableEntitys[index];
                    if (tableEntity != null)
                    {
                        CloudTable cloudTable = TdsTableManager.GetTable(index);
                        cloudTable.ExecuteAsync(TableOperation.InsertOrMerge(tableEntity)).Wait();
                    }
                }
            }
            public void ResetEntity()
            {
                for(int i = 0; i < tableEntitys.Count();i++)
                    tableEntitys[i] = null;
            }
            public IList<int> UseTabels()
            {
                IList<int> list = new List<int>();
                for (int index = 1; index < tableEntitys.Count(); index++)
                {
                    DynamicTableEntity tableEntity = tableEntitys[index];
                    if (tableEntitys[index] != null)
                    {
                        list.Add(index);
                    }
                }

                return list;
            }
        }
    }

}