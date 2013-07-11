#region Copyright (C) 2011-2013 MPExtended
// Copyright (C) 2011-2013 MPExtended Developers, http://www.mpextended.com/
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
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Applications.WebMediaPortal.Models;
using MPExtended.Libraries.Service;
using MPExtended.Services.Common.Interfaces;

namespace MPExtended.Applications.WebMediaPortal.Controllers
{
    [ServiceAuthorize]
    public class RecordingController : BaseController
    {
        //
        // GET: /Recording/
        public ActionResult Index()
        {
            // This action shouldn't be called
            return RedirectToActionPermanent("Categories");
        }

        public ActionResult Categories(RecordingCategoryType? type = null)
        {
            var recordings = Connections.Current.TAS.GetRecordings();
            return Data(new RecordingCategoriesViewModel(recordings, type));
        }

        public ActionResult Programs(string filter = null, string title = null)
        {
            var recordings = Connections.Current.TAS.GetRecordings(filter);
            return Data(new RecordingListViewModel(recordings, title));
        }

        public ActionResult Details(int id)
        {
            var recording = Connections.Current.TAS.GetRecordingById(id);
            if (recording == null)
                return HttpNotFound();

            return Data(new RecordingViewModel(recording));
        }

        public ActionResult PreviewImage(int id, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.Recording, id.ToString(), WebFileType.Content, width, height, "Images/default/recording.png");
        }

        public ActionResult ChannelLogo(int channelId, int width = 0, int height = 0)
        {
            return Images.ReturnFromService(WebMediaType.TV, channelId.ToString(), WebFileType.Logo, width, height, "Images/default/logo.png");
        }

        public ActionResult Watch(int id)
        {
            var recording = Connections.Current.TAS.GetRecordingById(id);
            if (recording == null)
                return HttpNotFound();

            return View(recording);
        }

        public ActionResult Delete(int id)
        {
            Log.Debug("Deleting recording id={0} on user request", id);
            Connections.Current.TAS.DeleteRecording(id);
            return RedirectToAction("Categories");
        }
    }
}
