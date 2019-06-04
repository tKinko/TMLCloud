using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GoogleVisualizationDataExtensions;

namespace TMLCloud.Models
{
    public class ListModel : _BaseModel
    {
        public ListModel(string container) : base(container)
        {
            ListTop = -1;
        }
        public int ListTop { get; set; }
    }
    public class FunnelListModel : ListModel
    {
        public FunnelListModel() : base("funnel")
        {
        }
    }
    public class BeamListModel : ListModel
    {
        public BeamListModel() : base("beam")
        {
        }
    }
  public class DataTableModel : _BaseModel
    {
        public DataTableModel(string container) : base(container)
        {
        }
        public string Url { get; set; }
        public GoogleVisualizationDataTable DataTable(ListModel model)
        {
            return TdsObject.CreatTdsDataTable(model.ListTop);
        }
    }
    public class FunnelDataListModel : DataTableModel
    {
        public FunnelDataListModel() : base("funnel")
        {
        }
    }
    public class BeamDataListModel : DataTableModel
    {
        public BeamDataListModel() : base("beam")
        {
        }
    }
    public class HarvestDataListModel : DataTableModel
    {
        public HarvestDataListModel() : base("harvest")
        {
        }
    }
}
