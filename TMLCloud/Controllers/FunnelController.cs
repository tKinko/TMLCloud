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
    public class FunnelController : _BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        public JsonResult GetCurrentUpdate()
        {
            FunnelListModel model = new FunnelListModel();
            model.UpdateStatus();

            return Json(model);
        }
        public IActionResult UpdateDataTable()
        {
            FunnelDataListModel model = new FunnelDataListModel();
            model.Url = "Funnel/DataTable";
            
            return PartialView("_DataTablePartial", model);
        }
        public JsonResult DataTable()
        {
            FunnelDataListModel listModel = new FunnelDataListModel();
            FunnelListModel model = new FunnelListModel();
            //model.ListTop = listModel.TdsObject.LastStep();

            return Json(listModel.DataTable(model));
        }
        public IActionResult DataClear()
        {
            FunnelListModel model = new FunnelListModel();
            model.DataClear();

            return Ok();
        }
    }
}
