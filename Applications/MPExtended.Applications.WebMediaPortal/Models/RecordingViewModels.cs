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
using System.Web.Routing;
using MPExtended.Applications.WebMediaPortal.Code;
using MPExtended.Services.TVAccessService.Interfaces;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.Common.Interfaces;
using MoreLinq;

namespace MPExtended.Applications.WebMediaPortal.Models
{
    public enum RecordingCategoryType
    {
        Program,
        Channel,
        Genre
    }

    public class RecordingChannelViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RecordingCategoryViewModel
    {
        public string Name { get; set; }
        public string Filter { get; set; }
        public string ArtworkAction { get; set; }
        public RouteValueDictionary ArtworkParameters { get; set; }
    }

    public class RecordingCategoriesViewModel
    {
        private IList<WebRecordingBasic> _recordings;

        public RecordingCategoryType? SelectedType { get; private set; }

        public IEnumerable<RecordingCategoryViewModel> Programs
        {
            get
            {
                return _recordings.GroupBy(x => x.Title)
                                  .Select(x => x.OrderByDescending(y => y.EndTime).First())
                                  .Select(x => new RecordingCategoryViewModel()
                                  {
                                      Name = x.Title,
                                      Filter = String.Format("Title='{0}'", x.Title),
                                      ArtworkAction = "PreviewImage",
                                      ArtworkParameters = new RouteValueDictionary() { { "id", x.Id } }
                                  })
                                  .OrderBy(x => x.Name);
            }
        }

        public IEnumerable<RecordingCategoryViewModel> Channels
        {
            get
            {
                return _recordings.DistinctBy(x => x.ChannelId)
                                  .Select(x => new RecordingCategoryViewModel()
                                  {
                                      Name = x.ChannelName,
                                      Filter = String.Format("ChannelId={0}", x.ChannelId),
                                      ArtworkAction = "ChannelLogo",
                                      ArtworkParameters = new RouteValueDictionary() { { "channelId", x.ChannelId } }
                                  })
                                  .OrderBy(x => x.Name);
            }
        }

        public IEnumerable<RecordingCategoryViewModel> Genres
        {
            get
            {
                return _recordings.DistinctBy(x => x.Genre)
                                  .Select(x => new RecordingCategoryViewModel()
                                  {
                                      Name = x.Genre,
                                      Filter = String.Format("Genre='{0}'", x.Genre)
                                  })
                                  .OrderBy(x => x.Name);
            }
        }

        public RecordingCategoriesViewModel(IList<WebRecordingBasic> recordings, RecordingCategoryType? selectedType)
        {
            _recordings = recordings;
            // TODO: Ideally the skin should be able to pick this
            SelectedType = selectedType ?? RecordingCategoryType.Program;
        }

        public IEnumerable<RecordingCategoryViewModel> ForCurrentType()
        {
            return SelectedType == RecordingCategoryType.Genre ? Genres :
                   SelectedType == RecordingCategoryType.Channel ? Channels :
                   SelectedType == RecordingCategoryType.Program ? Programs :
                   null;
        }
    }

    public class RecordingViewModel
    {
        public WebRecordingBasic Recording { get; private set; }
        public int Id { get { return Recording.Id; } }

        // This is a variant of MediaItemModel, which uses TAS instead of MAS. It can probably be made into a proper parent class, but 
        // since we only have one media type for TAS in this way, probably not worth it.
        private WebRecordingFileInfo fileInfo;
        private WebMediaInfo mediaInfo;

        public WebRecordingFileInfo FileInfo
        {
            get
            {
                if (fileInfo == null)
                    fileInfo = Connections.Current.TAS.GetRecordingFileInfo(Recording.Id);

                return fileInfo;
            }
        }

        public WebMediaInfo MediaInfo
        {
            get
            {
                if (mediaInfo == null)
                    mediaInfo = Connections.Current.TASStreamControl.GetMediaInfo(WebMediaType.Recording, null, Recording.Id.ToString(), 0);

                return mediaInfo;
            }
        }

        public bool Accessible
        {
            get
            {
                return Connections.Current.TASStreamControl.GetItemSupportStatus(WebMediaType.Recording, null, Recording.Id.ToString(), 0).Supported;
            }
        }

        public string Quality
        {
            get
            {
                return MediaInfoFormatter.GetShortQualityName(Accessible, MediaInfo);
            }
        }

        public string FullQuality
        {
            get
            {
                return MediaInfoFormatter.GetFullInfoString(Accessible, MediaInfo, FileInfo);
            }
        }

        public RecordingViewModel(WebRecordingBasic recording)
        {
            Recording = recording;
        }
    }

    public class RecordingListViewModel
    {
        public string Title { get; private set; }
        public IEnumerable<RecordingViewModel> Recordings { get; private set; }

        public RecordingListViewModel(IEnumerable<WebRecordingBasic> recordings, string title)
        {
            Recordings = recordings.Select(x => new RecordingViewModel(x));
            Title = title;
        }
    }
}