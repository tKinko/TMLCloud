using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TdsDataObjectExtensions;

namespace GoogleVisualizationDataExtensions
{
    public class GVDOption
    {
        public long TopStep { get; set; }
        public int PageNums { get; set; }
        public string culture { get; set; }
    }
    public class GoogleVisualizationDataTable
    {
        public IList<Col> cols { get; } = new List<Col>();
        public IList<Row> rows { get; } = new List<Row>();

        public void AddColumn(string label, string type)
        {
            cols.Add(new Col() { label = label, type = type });
        }

        public void AddRow(IList<Row.RowValue> values)
        {
            rows.Add(new Row() { c = values.Select(x => new Row.RowValue( x.objValue,x.format )) });
        }

        public class Col
        {
            public string label { get; set; }
            public string type { get; set; }
        }

        public class Row
        {
            public IEnumerable<RowValue> c { get; set; }
            public class RowValue
            {
                protected RowValue() { }
                public RowValue(object v, TdsDataObject.TDS7130_FORMAT fmt)
                {
                    objValue = v;
                    format = fmt;
                }

                [IgnoreDataMember]
                public TdsDataObject.TDS7130_FORMAT format = TdsDataObject.TDS7130_FORMAT.kDefFmtFlg;
                [IgnoreDataMember]
                public object objValue = null;
                public object v
                {
                    set { objValue = value;}
                    get
                    {
                        if (objValue == null)
                            return objValue;

                        if (format == TdsDataObject.TDS7130_FORMAT.kDefFmtFlg)
                        {
                            return objValue;
                        }

                        if (TdsDataObject.IsDateTimeFormat(format) == true)
                        {
                            //return objValue;
                            return ((DateTimeOffset)objValue).ToString(TdsDataObject.GetFormatTds2Google(TdsDataObject.TDS7130_FORMAT.kFmtDate_UTC));
                        }

                        Double val = Convert.ToDouble(objValue);
                        if (Double.IsInfinity(val))
                            return null;

                        return objValue;
                    }
                }
                public object f
                {
                    get
                    {
                        if (objValue == null)
                            return "-------";

                        if (format == TdsDataObject.TDS7130_FORMAT.kDefFmtFlg)
                        {
                            return objValue;
                        }

                        if (TdsDataObject.IsDateTimeFormat(format) == true)
                        {
                            return ((DateTimeOffset)objValue).ToString(TdsDataObject.GetFormatTds2Google(format));
                        }

                        Double val = Convert.ToDouble(objValue);
                        if (Double.IsInfinity(val))
                            return "*******";

                        return string.Format(string.Format("{{0:{0}}}", TdsDataObject.GetFormatTds2Google(format)),val);
                    }
                }
            }
        }

        public string JsonDataTable()
        {
            // DateTime => new Date(value) convert
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
        }
    }
 
}
