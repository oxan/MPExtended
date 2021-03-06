﻿#region Copyright (C) 2011-2013 MPExtended
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Reflection;

namespace MPExtended.Libraries.SQLitePlugin
{
    public class AutoFiller<T> where T : new()
    {
        private IEnumerable<SQLFieldMapping> mapping;
        private Dictionary<SQLFieldMapping, int> autofillMapping;
        private Dictionary<string, PropertyInfo> autofillProperties;

        public AutoFiller(IEnumerable<SQLFieldMapping> mapping)
        {
            this.mapping = mapping;
        }

        private Dictionary<SQLFieldMapping, int> GenerateResultingMapping(SQLiteDataReader reader)
        {
            // this generates the field nr => (property name, property reader) mappings for the autofiller
            var ret = new Dictionary<SQLFieldMapping, int>();
            autofillProperties = typeof(T).GetProperties().ToDictionary(x => x.Name, x => x);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string fieldname = reader.GetName(i);
                IEnumerable<SQLFieldMapping> matchedMappings = mapping.Where(x => x.Field == fieldname);
                if (matchedMappings.Count() == 0)
                    continue;
                foreach (SQLFieldMapping thisMapping in matchedMappings)
                {
                    string propertyName = thisMapping.PropertyName;
                    if (!autofillProperties.ContainsKey(propertyName))
                        continue;

                    ret[thisMapping] = i;
                }
            }

            return ret;
        }

        public T AutoCreate(SQLiteDataReader reader)
        {
            // automatically fill objects based upon the sql mappings provided
            if (autofillMapping == null)
                autofillMapping = GenerateResultingMapping(reader);

            // return object
            T obj = new T();

            // loop through all properties and get the value for it
            foreach (KeyValuePair<SQLFieldMapping, int> item in autofillMapping)
            {
                // invoke
                object res;
                MethodInfo datareader;
                if (item.Key.ParameterizedReader != null)
                {
                    datareader = item.Key.ParameterizedReader.Method;
                    res = item.Key.ParameterizedReader.Invoke(reader, item.Value, item.Key.Parameter);
                }
                else
                {
                    datareader = item.Key.Reader.Method;
                    res = item.Key.Reader.Invoke(reader, item.Value);
                }

                // set value
                if (Attribute.IsDefined(datareader, typeof(MergeListReaderAttribute)))
                {
                    if (res != null && res is IEnumerable)
                    {
                        foreach (object row in (IEnumerable)res)
                        {
                            (autofillProperties[item.Key.PropertyName].GetValue(obj, null) as IList).Add(row);
                        }
                    }
                    else if (res != null)
                    {
                        (autofillProperties[item.Key.PropertyName].GetValue(obj, null) as IList).Add(res);
                    }
                }
                else
                {
                    autofillProperties[item.Key.PropertyName].SetValue(obj, res, null);
                }
            }

            return obj;
        }
    }
}
