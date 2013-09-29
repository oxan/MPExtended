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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using MPExtended.Services.StreamingService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal class WpfStreamingSession : IWpfListItem<WebStreamingSession>, INotifyPropertyChanged
    {
        public string Identifier { get; private set; }
        public string ClientDescription { get; private set; }
        public string ClientIP { get; private set; }
        public string Profile { get; private set; }
        public string File { get; private set; }
        public string Progress { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public WpfStreamingSession()
        {
        }

        public void UpdateFrom(WebStreamingSession newSession)
        {
            Identifier = newSession.Identifier;
            ClientDescription = newSession.ClientDescription;
            ClientIP = newSession.ClientIPAddress;

            if (Profile == null || Profile != newSession.Profile)
            {
                Profile = newSession.Profile;
                NotifyPropertyChanged("Profile");
            }

            if (File == null || File != newSession.DisplayName)
            {
                File = newSession.DisplayName;
                NotifyPropertyChanged("File");
            }

            if (newSession.PlayerPosition > 0)
            {
                TimeSpan span = TimeSpan.FromMilliseconds(newSession.PlayerPosition);
                string progress = String.Format("{0:h\\:mm\\:ss} ({0}%)", span, newSession.PercentageProgress);
                if (Progress == null || Progress != progress)
                {
                    Progress = progress;
                    NotifyPropertyChanged("Progress");
                }
            }
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}
