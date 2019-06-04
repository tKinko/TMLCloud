using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TMLCloud.Models;
using TdsDataObjectExtensions;

namespace TMLCloud.Controllers
{
    public class HarvestController : _BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetHarvest()
        {
            HarvestListModel model = new HarvestListModel();

            model.GetHarvestData();

            return Ok();
        }
        public JsonResult GetCurrentUpdate()
        {
            HarvestListModel model = new HarvestListModel();
            model.UpdateStatus();

            return Json(model);
        }
        public IActionResult UpdateDataTable()
        {
            HarvestDataListModel model = new HarvestDataListModel();
            model.Url = "Harvest/DataTable";

            return PartialView("_DataTablePartial", model);
        }
        public JsonResult DataTable()
        {
            HarvestDataListModel listModel = new HarvestDataListModel();
            HarvestListModel model = new HarvestListModel();

            return Json(listModel.DataTable(model));
        }
        public IActionResult DataClear()
        {
            HarvestListModel model = new HarvestListModel();
            model.DataClear();

            return Ok();
        }
    }
}
