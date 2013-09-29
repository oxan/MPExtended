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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MPExtended.Services.StreamingService.Interfaces;
using MPExtended.Services.ScraperService.Interfaces;

namespace MPExtended.Applications.ServiceConfigurator.Code
{
    internal static class ExtensionMethods
    {
        public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(stream);

                using (var tempBitmap = new Bitmap(stream))
                {
                    // According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
                    // So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
                    return new Bitmap(tempBitmap);
                }
            }
        }

        public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        public static void UpdateList<TWpf, TOriginal>(this ObservableCollection<TWpf> wpfCollection, Func<TOriginal, string> idExtractor, IList<TOriginal> list)
            where TWpf : IWpfListItem<TOriginal>, new()
        {
            foreach (TOriginal item in list)
            {
                foreach (TWpf old in wpfCollection)
                {
                    if (old.Identifier.Equals(idExtractor(item)))
                    {
                        old.UpdateFrom(item);
                        goto contLoop1;
                    }
                }

                var forWpf = new TWpf();
                forWpf.UpdateFrom(item);
                wpfCollection.Add(forWpf);

            contLoop1: { }
            }

            for (int i = wpfCollection.Count - 1; i >= 0; i--)
            {
                foreach (TOriginal item in list)
                {
                    if (wpfCollection[i].Identifier == idExtractor(item))
                        goto contLoop2;
                }

                wpfCollection.RemoveAt(i);

            contLoop2: { }
            }
        }

        public static string ToJSON(this object obj)
        {
            if (obj == null) return string.Empty;
            using (var ms = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(obj.GetType());
                ser.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
