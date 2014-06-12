//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Net;
using System.Runtime.Serialization;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Reflection.Emit;
using System.Collections;

namespace Gurux.Common
{
    public interface IGXReturn
    {
    }

    public interface IGXReturn<T> : IGXReturn
    {

    }

    /// <summary>
    /// JSON parser want's to create 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public delegate object CreateObjectEventhandler(object sender, Type type, Dictionary<string, object> data);

    /// <summary>
    /// This class is used to handle JSON serialization and deserialization.
    /// </summary>
    public class GXJsonParser
    {
        /// <summary>
        /// Serialized and deserialization objects are saved to cache to make serialization faster.
        /// </summary>
        private Dictionary<Type, SortedDictionary<string, GXSerializedItem>> SerializedObjects = new Dictionary<Type, SortedDictionary<string, GXSerializedItem>>();

        private CreateObjectEventhandler m_CreateObject;

        /// <summary>
        /// Server address.
        /// </summary>
        public string Address
        {
            get;
            set;
        }

        /// <summary>
        ///  Indent elements.
        /// </summary>
        public bool Indent
        {
            get;
            set;
        }

        /// <summary>
        ///  Gets or sets an object that contains data to associate with the parser.
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Notifies that abstract class is created.
        /// </summary>
        public event CreateObjectEventhandler OnCreateObject
        {
            add
            {
                m_CreateObject += value;
            }
            remove
            {
                m_CreateObject -= value;
            }
        }

        public virtual TResponse Get<TResponse>(IGXReturn<TResponse> request)
        {
            StringBuilder sb = new StringBuilder();
            Serialize(request, sb, true, false);
            string cmd = request.GetType().Name + "/" + sb.ToString();
            cmd = "?" + Uri.EscapeDataString(cmd);
            HttpWebRequest req = WebRequest.Create(Address + cmd) as HttpWebRequest;
            req.Accept = "application/json";
            req.Headers.Add("Accept-Encoding", "gzip,deflate,gzip, deflate");
            req.Method = "GET";
            using (HttpWebResponse response = req.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(String.Format
                        ("Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                }
            }
            return default(TResponse);
        }

        public virtual TResponse Post<TResponse>(IGXReturn<TResponse> request)
        {
            StringBuilder sb = new StringBuilder();
            Serialize(request, sb, true, false);
            string cmd = request.GetType().Name + "/" + sb.ToString();
            cmd = "?" + Uri.EscapeDataString(cmd);
            HttpWebRequest req = WebRequest.Create(Address + cmd) as HttpWebRequest;
            req.Accept = "application/json";
            req.Headers.Add("Accept-Encoding", "gzip,deflate,gzip, deflate");
            req.Method = "POST";
            using (HttpWebResponse response = req.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(String.Format
                        ("Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                }
            }      
            return default(TResponse);
        }

        /// <summary>
        /// Parse JSON Objects.
        /// </summary>
        /// <param name="data">data string.</param>
        /// <returns>Name/value pair of found objects.</returns>
        public static Dictionary<string, object> ParseObjects(string data)
        {
            int index = 0;
            return ParseObjects(data, ref index, null, false);
        }

        /// <summary>
        /// Parse JSON Objects from File.
        /// </summary>
        /// <param name="data">data string.</param>
        /// <returns>Name/value pair of found objects.</returns>
        public static Dictionary<string, object> ParseObjectsFromFile(string path)
        {
            using (TextReader reader = File.OpenText(path))
            {
                int index = 0;
                return ParseObjects(reader.ReadToEnd(), ref index, null, false);
            }
        }


        /// <summary>
        /// Convert JSON epoch time to DateTime.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static DateTime GetDateTime(string str)
        {
            if (str != null && str.StartsWith("\\/Date("))
            {
                if (str[20] == '-' || str[20] == '+')
                {
                    int hours = int.Parse(str.Substring(21, 2));
                    int minutes = int.Parse(str.Substring(23, 2));
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    long seconds = long.Parse(str.Substring(7, 13));
                    if (str[20] == '-')
                    {
                        epoch.AddHours(hours);
                        epoch.AddMinutes(minutes);
                    }
                    else
                    {
                        epoch.AddHours(-hours);
                        epoch.AddMinutes(-minutes);
                    }
                    return epoch.AddSeconds(seconds / 1000).ToLocalTime();
                }
                else
                {
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    long seconds = long.Parse(str.Substring(8, 13));
                    return epoch.AddSeconds(seconds / 1000).ToLocalTime();
                }
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// Convert date time to epoch string.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        static string ToString(DateTime dt)
        {
            long value = (long)(dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, dt.Kind)).TotalMilliseconds;
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(dt);
            if (offset.TotalMinutes != 0)
            {
                string str = "\"\\/Date(" + value.ToString();
                if (offset.TotalMinutes > 0)
                {
                    str += "+";                  
                }
                else
                {
                    str += "-";                   
                }
                str += TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours.ToString("00") + 
                    TimeZone.CurrentTimeZone.GetUtcOffset(dt).Minutes.ToString("00");
                return str + ")\\/\"";
            }            
            return "\"\\/Date(" + value.ToString() + ")\\/\"";
        }

        /// <summary>
        /// Set object value.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pd"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        static void SetValue(object target, PropertyInfo pi, FieldInfo fi, string value, Type type)
        {
            object val;
            if (type == typeof(byte[]))
            {
                val = Convert.FromBase64String(value);
            }
            else if (type == typeof(DateTime))
            {
                val = GetDateTime(value);
            }
            else if (type == typeof(Guid))
            {
                val = new Guid(value);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                SetValue(target, pi, fi, value, Nullable.GetUnderlyingType(type));
                return;
            }
            else if (type == typeof(double))
            {
                val = Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (type == typeof(float))
            {
                val = (float)Convert.ToDouble(value, CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (type == typeof(TimeSpan))
            {
                val = System.Xml.XmlConvert.ToTimeSpan(value);
            }
            else if (type.IsEnum)
            {
                val = Enum.Parse(type, value);
            }
            else if (type == typeof(object))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    int pos = value.IndexOf(':');
                    if (pos != -1)
                    {
                        string tmp = value.Substring(0, pos);
                        Type tp = Type.GetType(tmp);
                        tmp = value.Substring(pos + 1);
                        val = Convert.ChangeType(tmp, tp);
                    }
                    else
                    {
                        val = null;
                    }
                }
                else
                {
                    val = null;
                }                
            }
            else
            {
                val = Convert.ChangeType(value, type);
            }
            if (pi != null)
            {
                pi.SetValue(target, val, null);
            }
            else
            {
                fi.SetValue(target, val);
            }
        }

        /// <summary>
        /// Deserialize JSON data to objects.
        /// </summary>
        /// <typeparam name="T">Deserialized type.</typeparam>
        /// <param name="data">JSON data.</param>
        /// <returns>Deserialized object.</returns>
        public T Deserialize<T>(string data)
        {
            return (T) Deserialize(data, typeof(T));
        }

        /// <summary>
        /// Deserialize JSON data to objects.
        /// </summary>
        /// <param name="type">Data type.</param>
        /// <param name="data">JSON data as a string.</param>
        /// <returns>JSON data.</returns>
        public object Deserialize(string data, Type type)
        {
            int index = 0;
            Dictionary<string, object> list = ParseObjects(data, ref index, null, false);
            return Deserialize(list, type);
        }

        /// <summary>
        /// Save object as JSON object.
        /// </summary>
        /// <param name="target">object to save.</param>
        /// <param name="path">File path.</param>
        public static void Save(object target, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                GXFileSystemSecurity.UpdateDirectorySecurity(dir);
            }
            GXJsonParser parser = new GXJsonParser();
            parser.Indent = true;
            string data = parser.Serialize(target);
            using (TextWriter writer = File.CreateText(path))
            {
                writer.Write(data);
            }
            GXFileSystemSecurity.UpdateFileSecurity(path);
        }

        /// <summary>
        /// Get JSON value by name.
        /// </summary>
        /// <param name="data">JSON data.</param>
        /// <param name="name">object name.</param>
        /// <returns>Found value or null if name not found.</returns>
        public object GetValue(string data, string name)
        {
            int index = 0;
            Dictionary<string, object> list = ParseObjects(data, ref index, name, false);
            if (list.ContainsKey(name))
            {
                return list[name];
            }
            return null;
        }

        /// <summary>
        /// Load JSON object.
        /// </summary>
        /// <typeparam name="T">Onject type to load.</typeparam>
        /// <param name="path">File path.</param>
        /// <returns>Loaded object.</returns>
        public static T Load<T>(string path)
        {
            if (File.Exists(path))
            {
                GXJsonParser parser = new GXJsonParser();
                using (TextReader reader = File.OpenText(path))
                {
                    return parser.Deserialize<T>(reader.ReadToEnd());
                }
            }
            return default(T);
        }

        /// <summary>
        /// Is file JSON file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsJSONFile(string path)
        {
            using (TextReader reader = File.OpenText(path))
            {
                if (reader.Peek() == '{')
                {                    
                    return true;                                                               
                }
            }
            return false;
        }

        /// <summary>
        /// Try load JSON object.
        /// </summary>
        /// <typeparam name="T">Onject type to load.</typeparam>
        /// <param name="path">File path.</param>
        /// <returns>Loaded object.</returns>
        public static bool TryLoad<T>(string path, out T result)
        {            
            if (File.Exists(path))
            {
                GXJsonParser parser = new GXJsonParser();
                using (TextReader reader = File.OpenText(path))
                {
                    if (reader.Peek() == '{')
                    {
                        try
                        {
                            result = parser.Deserialize<T>(reader.ReadToEnd());
                            return true;
                        }
                        catch (Exception)
                        {
                            result = default(T);
                            return false;
                        }
                    }                    
                }
            }
            result = default(T);
            return false;
        }

        /// <summary>
        /// Load JSON object.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="type">Object type</param>
        public static object Load(string path, Type type)
        {
            GXJsonParser parser = new GXJsonParser();
            using (TextReader reader = File.OpenText(path))
            {
                return parser.Deserialize(reader.ReadToEnd(), type);
            }
        }

        /// <summary>
        /// Load JSON object.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="type">Object type</param>
        public object LoadFile(string path, Type type)
        {
            using (TextReader reader = File.OpenText(path))
            {
                return Deserialize(reader.ReadToEnd(), type);
            }
        }

        /// <summary>
        /// Serialize object to JSON string.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public string Serialize(object target)
        {
            StringBuilder sb = new StringBuilder();
            Serialize(target, sb, false, Indent);
            return sb.ToString();
        }

        static object ShouldSerializeValue(object value)
        {            
            //If value is nullable.
            if (value == null)
            {
                return null;
            }
            if (value is sbyte)
            {
                if ((sbyte)value == 0)
                {
                    return null;
                }
            }
            else if (value is Int16)
            {
                if ((Int16)value == 0)
                {
                    return null;
                }
            }
            else if (value is Int32)
            {
                if ((Int32)value == 0)
                {
                    return null;
                }
            }
            else if (value is Int64)
            {
                if ((Int64)value == 0)
                {
                    return null;
                }
            }
            if (value is byte)
            {
                if ((byte)value == 0)
                {
                    return null;
                }
            }
            else if (value is UInt16)
            {
                if ((UInt16)value == 0)
                {
                    return null;
                }
            }
            else if (value is UInt32)
            {
                if ((UInt32)value == 0)
                {
                    return null;
                }
            }
            else if (value is UInt64)
            {
                if ((UInt64)value == 0)
                {
                    return null;
                }
            }
            else if (value is double)
            {
                if ((double)value == 0)
                {
                    return null;
                }
            }
            else if (value is float)
            {
                if ((float)value == 0)
                {
                    return null;
                }
            }
            else if (value is bool)
            {
                if (!(bool)value)
                {
                    return null;
                }
            }
            else if (value is string)
            {
                if (string.IsNullOrEmpty((string)value))
                {
                    return null;
                }
            }
            else if (value is DateTime)
            {
                if ((DateTime)value == DateTime.MinValue)
                {
                    return null;
                }
            }
            else if (value is TimeSpan)
            {
                if ((TimeSpan)value == TimeSpan.Zero)
                {
                    return null;
                }
            }
            else if (value is Guid)
            {
                if ((Guid)value == Guid.Empty)
                {
                    return null;
                }
            }
            return value;
        }

        private class GXSerializedItem
        {
            public object Target;
            public object DefaultValue;
        }

        /// <summary>
        /// Get serialized property and field values.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static SortedDictionary<string, GXSerializedItem> GetValues(Type type)
        {
            SortedDictionary<string, GXSerializedItem> list = new SortedDictionary<string, GXSerializedItem>();            
            bool all = type.GetCustomAttributes(typeof(DataContractAttribute), false).Length == 0;
            BindingFlags flags;
            if (all)
            {
                flags = BindingFlags.Instance | BindingFlags.Public;
            }
            else
            {
                flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            }
            //Save properties.
            foreach (PropertyInfo it in type.GetProperties(flags))
            {
                DataMemberAttribute[] attr = (DataMemberAttribute[])it.GetCustomAttributes(typeof(DataMemberAttribute), true);
                //If value is not marked as ignored.
                if (((all && it.CanWrite) || attr.Length != 0) &&
                        it.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), true).Length == 0)
                {
                    if (!list.ContainsKey(it.Name))
                    {
                        GXSerializedItem s = new GXSerializedItem();
                        s.Target = it;
                        string name;
                        if (attr.Length == 0 || string.IsNullOrEmpty(attr[0].Name))
                        {
                            name = it.Name;
                        }
                        else
                        {
                            name = attr[0].Name;
                        }                        
                        DefaultValueAttribute[] def = (DefaultValueAttribute[]) it.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                        if (def.Length != 0)
                        {
                            s.DefaultValue = def[0].Value;
                        }
                        list.Add(name, s);
                    }
                }
            }
            if (!all)
            {
                //Save data members.
                foreach (FieldInfo it in type.GetFields(flags))
                {
                    DataMemberAttribute[] attr = (DataMemberAttribute[])it.GetCustomAttributes(typeof(DataMemberAttribute), true);
                    if (attr.Length != 0)
                    {
                        if (!list.ContainsKey(it.Name))
                        {
                            GXSerializedItem s = new GXSerializedItem();
                            s.Target = it;
                            string name;
                            if (attr.Length == 0 || string.IsNullOrEmpty(attr[0].Name))
                            {
                                name = it.Name;
                            }
                            else
                            {
                                name = attr[0].Name;
                            }
                            DefaultValueAttribute[] def = (DefaultValueAttribute[])it.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                            if (def.Length != 0)
                            {
                                s.DefaultValue = def[0].Value;
                            }
                            list.Add(name, s);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Serialize object to JSON string.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="get"></param>
        /// <returns></returns>
        void Serialize(object target, StringBuilder sb, bool get, bool indent)
        {
            SortedDictionary<string, GXSerializedItem> list;
            Type type = target.GetType();
            if (SerializedObjects.ContainsKey(type))
            {
                list = SerializedObjects[type];
            }
            else
            {
                list = GetValues(type);
                SerializedObjects.Add(type, list);
            }            
            if (target is System.Collections.IEnumerable)
            {
                System.Collections.IEnumerator coll = (target as System.Collections.IEnumerable).GetEnumerator();
                sb.Append("[");
                bool first = true;
                while (coll.MoveNext())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }
                    Serialize(coll.Current, sb, get, false);
                }
                sb.Append("]");                
            }
            else
            {
                if (!get)
                {
                    sb.Append("{");
                }
                int start = sb.Length;
                foreach (var it in list)
                {                    
                    object value;                    
                    Type dataType;
                    if (it.Value.Target is PropertyInfo)
                    {
                        value = ShouldSerializeValue(((it.Value.Target) as PropertyInfo).GetValue(target, null));
                        dataType = ((it.Value.Target) as PropertyInfo).PropertyType;
                    }
                    else
                    {
                        value = ShouldSerializeValue((it.Value.Target as FieldInfo).GetValue(target));
                        dataType = (it.Value.Target as FieldInfo).FieldType;
                    }
                    if (value != null && !value.Equals(it.Value.DefaultValue))
                    {                        
                        string str = null;
                        if (value is byte[])
                        {
                            str = Convert.ToBase64String((byte[])value);
                        }                        
                        else if (value is DateTime)
                        {                            
                            str = ToString((DateTime)value);
                        }
                        else if (value is Guid)
                        {                            
                            str = "\"" + value.ToString().Replace("-", "") + "\"";
                        }
                        else if (value is string)
                        {
                            str = "\"" + value.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                        }
                        else if (value.GetType().IsClass)
                        {
                            StringBuilder sb2 = new StringBuilder();
                            Serialize(value, sb2, get, false);
                            str = sb2.ToString();
                        }
                        else if (value is float)
                        {
                            str = ((float)value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                        }
                        else if (value is double)
                        {
                            str = ((double)value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                        }
                        else if (value is TimeSpan)
                        {
                            str = "\"" + System.Xml.XmlConvert.ToString((TimeSpan)value) + "\"";
                        }
                        else if (value.GetType().IsEnum)
                        {
                            str = "\"" + value.ToString() + "\"";
                        }
                        else if (dataType == typeof(object))
                        {
                            str = "\"" + value.GetType().FullName + ":" + value.ToString() + "\"";
                        }
                        else
                        {
                            str = value.ToString();
                        }
                        if (sb.Length != start)
                        {                            
                            sb.Append(",");
                            if (indent)
                            {
                                sb.Append(Environment.NewLine);
                            }
                        }
                        if (!get)
                        {
                            sb.Append("\"");
                        }
                        sb.Append(it.Key);
                        if (!get)
                        {
                            sb.Append("\":");
                        }
                        else
                        {
                            sb.Append("=");
                        }
                        if (str != null)
                        {
                            sb.Append(str);
                        }
                    }
                }
                if (!get)
                {
                    sb.Append("}");
                }
            }                      
        }

        static Type GetPropertyType(Type target)
        {
            Type[] types = target.GetGenericArguments();
            if (types.Length == 0)
            {
                return GetPropertyType(target.BaseType);
            }
            return types[0];
        }

        /// <summary>
        /// Deserialize JSON data to objects.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>        
        /// <returns></returns>
        public object Deserialize(Dictionary<string, object> data, Type type)
        {
            return Deserialize(data, type, null);
        }

        static Hashtable Objects = new Hashtable();
        public static object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
            /*
            Func<object> tmp = (Func<object>)Objects[type];
            if (tmp != null)
            {
                return tmp();
            }
            var dynMethod = new DynamicMethod("factory_" + type.Name, type, null, type);
            ILGenerator ilGen = dynMethod.GetILGenerator();
            ilGen.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));
            ilGen.Emit(OpCodes.Ret);
            tmp = (Func<object>)dynMethod.CreateDelegate(typeof(Func<object>));
            Objects.Add(type, tmp);
            return tmp();
             * */
        }

        /// <summary>
        /// Deserialize JSON data to objects.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>        
        /// <returns></returns>
        private object Deserialize(Dictionary<string, object> data, Type type, object tmp)
        {            
            if (tmp == null && !type.IsArray)
            {
                if (type.IsAbstract)
                {
                    if (m_CreateObject == null)
                    {
                        throw new Exception("Can't create abstact class: " + type.FullName);
                    }
                    tmp = m_CreateObject(this, type, data);
                    type = tmp.GetType();
                }
                else
                {
                    if (m_CreateObject != null)
                    {
                        tmp = m_CreateObject(this, type, data);                        
                    }
                    if (tmp != null)
                    {
                        type = tmp.GetType();
                    }
                    else
                    {
                        tmp = CreateInstance(type);
                    }
                }
            }
            SortedDictionary<string, GXSerializedItem> list;            
            if (SerializedObjects.ContainsKey(type))
            {
                list = SerializedObjects[type];
            }
            else
            {
                list = GetValues(type);
                SerializedObjects.Add(type, list);                
            }
            Dictionary<string, object>.Enumerator serializedItem = data.GetEnumerator();
            SortedDictionary<string, GXSerializedItem>.Enumerator item = list.GetEnumerator();
            while (serializedItem.MoveNext())
            {
                if (tmp is System.Collections.IList && string.Compare(serializedItem.Current.Key, "Items", true) == 0)
                {
                    Type tp = GetPropertyType(type);
                    if (serializedItem.Current.Value != null)
                    {
                        System.Collections.IList list2 = tmp as System.Collections.IList;
                        foreach (object it in (System.Collections.IEnumerable)serializedItem.Current.Value)
                        {
                            list2.Add(Deserialize((Dictionary<string, object>)it, tp));
                        }
                    }
                }
                else if (type.IsArray)
                {
                    Type itemType = type.GetElementType();
                    if (serializedItem.Current.Value != null)
                    {
                        System.Collections.IList sItems = (System.Collections.IList)serializedItem.Current.Value;
                        Array items = Array.CreateInstance(itemType, sItems.Count);
                        tmp = items;
                        if (sItems.Count != 0)
                        {
                            int pos = -1;
                            foreach (object it in sItems)
                            {
                                items.SetValue(Deserialize((Dictionary<string, object>)it, itemType), ++pos);
                            }
                        }
                    }
                    else//If array is empty.
                    {
                        tmp = Array.CreateInstance(itemType, 0);
                    }
                }
                else
                {
                    if (!item.MoveNext())
                    {
                        break;
                    }
                    if (!UpdateValue(tmp, item.Current, serializedItem.Current))
                    {
                        bool found = false;
                        while (item.MoveNext())
                        {
                            if (UpdateValue(tmp, item.Current, serializedItem.Current))
                            {
                                found = true;                                
                                break;
                            }                            
                        }
                        if (!found)
                        {
                            throw new Exception("Unknown tag: " + serializedItem.Current.Key);
                        }
                    }
                }
            }
            //If array is empty.
            if (type.IsArray && tmp == null)
            {
                Type itemType = type.GetElementType();
                tmp = Array.CreateInstance(itemType, 0);
            }
            return tmp;
        }

        /// <summary>
        /// Update serialized value.
        /// </summary>
        /// <param name="target">Component where value is updated.</param>
        /// <param name="item">Property where data is updated.</param>
        /// <param name="serializedItem">Serialized data</param>
        /// <returns>True, if value is updated.</returns>
        private bool UpdateValue(object target, KeyValuePair<string, GXSerializedItem> item, KeyValuePair<string, object> serializedItem)
        {
            bool ret = string.Compare(item.Key, serializedItem.Key, true) == 0;
            if (ret)
            {
                Type tp;
                PropertyInfo pi = null;
                FieldInfo fi = null;
                if (item.Value.Target is PropertyInfo)
                {
                    pi = item.Value.Target as PropertyInfo;
                    tp = pi.PropertyType;
                }
                else
                {
                    fi = item.Value.Target as FieldInfo;
                    tp = fi.FieldType;
                }
                if (tp != typeof(object) && tp != typeof(string) && tp.IsClass && !tp.IsArray)
                {
                    if (serializedItem.Value is List<object>)
                    {
                        System.Collections.IList list2 = (System.Collections.IList)CreateInstance(tp);
                        Type itemType = GetPropertyType(tp);
                        List<object> items = (List<object>)serializedItem.Value;
                        foreach (object it in (List<object>)serializedItem.Value)
                        {
                            object value2 = Deserialize((Dictionary<string, object>)it, itemType);
                            list2.Add(value2);
                        }
                        if (pi != null)
                        {
                            pi.SetValue(target, list2, null);
                        }
                        else
                        {
                            fi.SetValue(target, list2);
                        }
                    }
                    else
                    {
                        Dictionary<string, object> value = (Dictionary<string, object>)serializedItem.Value;
                        object val = null;
                        if (pi != null)
                        {
                            val = pi.GetValue(target, null);
                        }
                        else
                        {
                            val = fi.GetValue(target);
                        }
                        //If class is already made.
                        if (val != null)
                        {
                            Deserialize(value, val.GetType(), val);
                            return true;
                        }                        
                        if (pi != null)
                        {
                            pi.SetValue(target, Deserialize(value, tp, val), null);
                        }
                        else
                        {
                            fi.SetValue(target, Deserialize(value, tp, val));
                        }                        
                    }
                }
                else if (tp.IsArray)
                {
                    Type itemType = tp.GetElementType();
                    List<object> items = (List<object>)serializedItem.Value;
                    Array list2 = Array.CreateInstance(itemType, items.Count);
                    int pos = -1;
                    foreach (object it in (List<object>)serializedItem.Value)
                    {
                        object value2 = Deserialize((Dictionary<string, object>)it, itemType);
                        list2.SetValue(value2, ++pos);
                    }
                    if (pi != null)
                    {
                        pi.SetValue(target, list2, null);
                    }
                    else
                    {
                        fi.SetValue(target, list2);
                    }
                }
                else
                {
                    SetValue(target, pi, fi, (string)serializedItem.Value, tp);                    
                }                
            }
            return ret;
        }

        static bool IsControlChar(char ch)
        {
            return ch == ',' || ch == '{' || ch == '}' || ch == '[' || ch == ']' || ch == ':' || ch == '\"';
        }

        /// <summary>
        /// Parse JSON Objects.
        /// </summary>
        /// <param name="data">data string.</param>
        /// <param name="index">index of string.</param>
        /// <param name="tagName">Tag name to find. When tag is found parse is breaked.</param>
        /// <returns>Name/value pair of found objects.</returns>
        static Dictionary<string, object> ParseObjects(string data, ref int index, string tagName, bool collection)
        {
            SortedDictionary<string, object> list = new SortedDictionary<string, object>();
            List<object> array = null;
            if (collection)
            {
                array = array = new List<object>();
            }
            string key = null;
            object value = null;
            bool insideString = false;
            StringBuilder sb = new StringBuilder();
            for (; index < data.Length; ++index)
            {
                char ch = data[index];
                //If control char and not inside of a string.
                if (!insideString && IsControlChar(ch))
                {
                    if (ch == '\"')
                    {
                        insideString = true;
                    }
                    else if (ch == ':')
                    {
                        key = sb.ToString();
                        sb.Length = 0;
                    }
                    else if (ch == ',')
                    {
                        if (sb.Length != 0)
                        {
                            value = sb.ToString().Replace("\\\"", "\"").Replace("\\\\", "\\");
                            sb.Length = 0;
                        }
                        if (key != null)
                        {
                            list.Add(key, value);
                            if (tagName != null && key == tagName)
                            {
                                break;
                            }
                            key = null;
                            value = null;
                        }
                    }
                    //Object starts.
                    else if (ch == '{')
                    {
                        ++index;
                        value = ParseObjects(data, ref index, tagName, false);
                        if (array != null)
                        {
                            array.Add(value);
                        }
                        else
                        {
                            if (key != null)
                            {
                                list.Add(key, value);
                                key = null;
                            }
                            else
                            {
                                foreach (var it in (Dictionary<string, object>)value)
                                {
                                    list.Add(it.Key, it.Value);
                                }
                            }
                            if (tagName != null && key == tagName)
                            {
                                break;
                            }
                        }
                    }
                    //Object ends.
                    else if (ch == '}')
                    {
                        if (sb.Length != 0)
                        {
                            value = sb.ToString().Replace("\\\"", "\"").Replace("\\\\", "\\");
                            sb.Length = 0;
                        }
                        if (key != null)
                        {
                            list.Add(key, value);
                            if (tagName != null && key == tagName)
                            {
                                break;
                            }
                            key = null;
                            value = null;
                        }
                        return new Dictionary<string, object>(list);
                    }
                    //Collection starts
                    else if (ch == '[')
                    {
                        array = new List<object>();                        
                        ++index;
                        value = ParseObjects(data, ref index, tagName, true);
                        foreach(var it in (Dictionary<string, object>)value)
                        {
                            if (key == null)
                            {
                                list.Add(it.Key, it.Value);
                            }
                            else
                            {
                                list.Add(key, it.Value);
                            }
                        }
                        //If property of objects.
                        if (key == null)
                        {
                            return (Dictionary<string, object>)value;
                        }
                        //If array is property array.                                           
                        sb.Length = 0;
                        value = null;
                        key = null;
                    }
                    //Collection ends
                    else if (ch == ']')
                    {
                        list.Add("Items", array);
                        break;                       
                    }                    
                    else if (key != null && array == null)
                    {
                        value = sb.ToString().Replace("\\\"", "\"").Replace("\\\\", "\\");
                        list.Add(key, value);
                        sb.Length = 0;
                        key = null;
                        value = null;
                    }
                }
                else if (insideString || (ch != '\r' && ch != '\n'))
                {
                    if (ch == '\"' && data[index - 1] != '\\')
                    {
                        insideString = false;                        
                    }
                    else if (ch != '\"' || insideString)
                    {
                        sb.Append(ch);
                    }                   
                }
            }
            return new Dictionary<string, object>(list);            
        }
    }
}
