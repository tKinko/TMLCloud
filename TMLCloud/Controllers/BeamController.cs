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
    public class BeamController : _BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        public JsonResult GetCurrentUpdate()
        {
            BeamListModel model = new BeamListModel();
            model.UpdateStatus();

            return Json(model);
        }
        public IActionResult UpdateDataTable()
        {
            BeamDataListModel model = new BeamDataListModel();
            model.Url = "Beam/DataTable";

            return PartialView("_DataTablePartial", model);
        }
        public IActionResult UpdateErrTable()
        {
            BeamDataListModel model = new BeamDataListModel();
            model.Url = "Beam/ErrorTable";

            return PartialView("_ErrorTablePartial", model);
        }
        public JsonResult DataTable()
        {
            BeamDataListModel listModel = new BeamDataListModel();
            BeamListModel model = new BeamListModel();
            //model.ListTop = listModel.TdsObject.LastStep();

            return Json(listModel.DataTable(model));
        }
        public JsonResult ErrorTable()
        {
            BeamDataListModel listModel = new BeamDataListModel();

            return Json(listModel.ErrorTable());
        }

        public IActionResult DataClear()
        {
            BeamListModel model = new BeamListModel();
            model.DataClear();

            return Ok();
        }
    }
}
