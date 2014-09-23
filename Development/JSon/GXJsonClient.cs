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
using System.Net;
using System.IO;
using Gurux.Common.Internal;

namespace Gurux.Common.JSon
{
    /// <summary>
    /// Progress event handler.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="current"></param>
    /// <param name="max"></param>
    public delegate void ProgressEventHandler(object sender, int current, int max);

    /// <summary>
    /// Gurux JSON Client.
    /// </summary>
    public class GXJsonClient
    {
        private bool CancelOperation;
        ProgressEventHandler Progress;

        /// <summary>
        /// Server address.
        /// </summary>
        public string Address
        {
            get;
            set;
        }

        /// <summary>
        /// Authentication user name.
        /// </summary>
        public string UserName
        {
            get;
            private set;
        }

        /// <summary>
        /// Authentication password.
        /// </summary>
        public string Password
        {
            get;
            private set;
        }

        public bool AlwaysSendBasicAuthHeader
        {
            get;
            set;
        }

        public void SetCredentials(string name, string password)
        {
            this.UserName = name;
            Password = password;
        }

        /// <summary>
        /// Progress event handler.
        /// </summary>        
        public event ProgressEventHandler OnProgress
        {
            add
            {
                Progress += value;
            }
            remove
            {
                Progress -= value;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GXJsonClient()
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">Server address.</param>
        public GXJsonClient(string address)
        {
            Address = address;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">Server address.</param>
        public GXJsonClient(string address, string userName, string password)
        {
            Address = address;
            UserName = userName;
            Password = password;
        }

        public TimeSpan Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// JSON parser.
        /// </summary>
        GXJsonParser Parser = new GXJsonParser();

        /// <summary>
        /// Send JSON data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual T Get<T>(IGXRequest<T> request)
        {
            HttpWebRequest req = Send("GET", request);           
            return GetResponse<T>(req);
        }

        /// <summary>
        /// Send JSON data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual T Put<T>(IGXRequest<T> request)
        {
            HttpWebRequest req = Send("PUT", request);
            return GetResponse<T>(req);
        }

        /// <summary>
        /// Send JSON data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual T Delete<T>(IGXRequest<T> request)
        {
            HttpWebRequest req = Send("DELETE", request);
            return GetResponse<T>(req);
        }

        /// <summary>
        /// Post JSON data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual T Post<T>(IGXRequest<T> request)
        {
            HttpWebRequest req = Send("POST", request);         
            return GetResponse<T>(req);
        }

        /// <summary>
        /// Cancel current operation.
        /// </summary>
        public void Cancel()
        {
            CancelOperation = true;
        }

        /// <summary>
        /// Parse to JSON and send to the server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private HttpWebRequest Send<T>(string method, IGXRequest<T> request)
        {
            CancelOperation = false;
            string cmd;
            bool content = method == "POST" || method == "PUT";
            using (TextWriter writer = new StringWriter())
            {
                Parser.Serialize(request, writer, true, !content, false);
                cmd = writer.ToString();
            }
            HttpWebRequest req;
            if (content)//If POST or PUT.
            {
                req = WebRequest.Create(Address + "json/reply/" + request.GetType().Name) as HttpWebRequest;
            }
            else //If GET or DELETE.
            {
                req = WebRequest.Create(Address + "json/reply/" + request.GetType().Name + "?" + cmd) as HttpWebRequest;
            }
            if (this.Timeout.TotalMilliseconds != 0)
            {
                req.Timeout = (int)this.Timeout.TotalMilliseconds;
            }
            req.ContentType = "application/json; charset=utf-8";
            req.Accept = "text/json";
            req.Method = method;
            //Add basic authentication if it is used.
            if (!string.IsNullOrEmpty(UserName))
            {
                req.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(UserName + ":" + Password)));
            }
            if (content)
            {
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    List<string> packets = GXInternal.SplitInParts(cmd, 1024);
                    foreach (string it in packets)
                    {
                        streamWriter.Write(it);
                        //If user has canceled transaction.
                        if (CancelOperation)
                        {
                            return null;
                        }
                    }
                }
            }
            return req;
        }
        
        /// <summary>
        /// De-serialize response to REST object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        private T GetResponse<T>(WebResponse response)
        {
            int length = 0;
            var d = response.Headers["Content-Length"];
            if (d != null)
            {
                length = int.Parse(d.ToString());
            }
            try
            {
                if (length != 0 && Progress != null)
                {
                    Progress(this, 0, length);
                }
                MemoryStream ms = new MemoryStream(length);
                Stream stream = response.GetResponseStream();
                byte[] buffer = new byte[length == 0 || length > 1024 ? 1024 : length];
                IAsyncResult read = stream.BeginRead(buffer, 0, buffer.Length, null, null);
                while (!CancelOperation)
                {
                    // wait for the read operation to complete
                    read.AsyncWaitHandle.WaitOne();
                    int count = stream.EndRead(read);
                    ms.Write(buffer, 0, count);
                    // If read is done.
                    if (ms.Position == length || count == 0)
                    {
                        break;
                    }
                    if (length != 0 && Progress != null)
                    {
                        Progress(this, (int)ms.Position, length);
                    }
                    read = stream.BeginRead(buffer, 0, buffer.Length, null, null);
                }
                ms.Position = 0;
                return Parser.Deserialize<T>(ms);
            }
            finally
            {
                if (length != 0 && Progress != null)
                {
                    Progress(this, (int)0, 0);
                }
            }
        }
        
        /// <summary>
        /// Read data fro the server.
        /// </summary>
        private T GetResponse<T>(HttpWebRequest req)
        {
            if (CancelOperation)
            {
                return default(T);
            }
            try
            {
                using (HttpWebResponse response = req.GetResponse() as HttpWebResponse)                
                {                   
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format
                            ("Server error (HTTP {0}: {1}).",
                            response.StatusCode,
                            response.StatusDescription));
                    }       
                    return GetResponse<T>(response);
                }
            }        
            catch (WebException ex)
            {
                GXErrorWrapper err = GetResponse<GXErrorWrapper>(ex.Response);
                throw err.GetException();
            }
        }
    }
}
