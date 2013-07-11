#region Copyright (C) 2013 MPExtended
// Copyright (C) 2013 MPExtended Developers, http://www.mpextended.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MPExtended.Applications.WebMediaPortal.Mvc
{
    public class DataResult : ActionResult
    {
        public object Model { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            var actionResult = CreateActionResult(context);
            actionResult.ExecuteResult(context);
        }

        private ActionResult CreateActionResult(ControllerContext context)
        {
            var request = context.HttpContext.Request;
            bool preferJson = request.AcceptTypes.Contains("application/json") ||
                              request.AcceptTypes.Contains("text/javascript") ||
                             (request.Params["format"] != null && request["format"] == "json");

            if (preferJson)
                return CreateJsonResult(context);
            return CreateViewResult(context);
        }

        private JsonResult CreateJsonResult(ControllerContext context)
        {
            return new JsonResult
            {
                Data = Model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private ViewResult CreateViewResult(ControllerContext context)
        {
            var viewData = context.Controller.ViewData;
            viewData.Model = Model;
            return new ViewResult()
            {
                ViewData = viewData,
                TempData = context.Controller.TempData,
                ViewEngineCollection = ViewEngines.Engines
            };
        }
    }
}