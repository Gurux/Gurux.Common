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

#if !NETCOREAPP2_0 && !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NETCOREAPP2_1 && !NETCOREAPP3_1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Gurux.Common;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;

namespace Gurux.Common
{
    /// <summary>
    /// Because MTA threading model is used clipboard can't access directly.
    /// </summary>
    public class ClipboardCopy
    {
        /// <summary>
        /// Copies data from items and sub items in the list view to the clipboard.
        /// </summary>
        /// <param name="data">Data to copy.</param>
        static public void CopyDataToClipboard(object data)
        {
            string str = string.Empty;
            if (data is string)
            {
                str = data as string;
            }
            else if (data != null && data.GetType().IsArray)
            {
                foreach (object it in (Array)data)
                {
                    str += it.ToString();
                }
            }
            else
            {
                ListView view = data as ListView;
                if (view != null)
                {
                    if (view.VirtualMode)
                    {
                        StringBuilder sb = new StringBuilder(view.SelectedIndices.Count * 50);
                        foreach (int pos in view.SelectedIndices)
                        {
                            ListViewItem it = view.Items[pos];
                            foreach (ListViewItem.ListViewSubItem sub in it.SubItems)
                            {
                                sb.Append(sub.Text);
                                sb.Append('\t');
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(Environment.NewLine);
                        }
                        str = sb.ToString();
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder(view.SelectedIndices.Count * 50);
                        foreach (ListViewItem it in view.SelectedItems)
                        {
                            foreach (ListViewItem.ListViewSubItem sub in it.SubItems)
                            {
                                sb.Append(sub.Text);
                                sb.Append('\t');
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(Environment.NewLine);
                        }
                        str = sb.ToString();
                    }
                }
            }
            if (!string.IsNullOrEmpty(str))
            {
                ClipboardCopy threadHelper = new ClipboardCopy(str);
                ThreadStart ts = new ThreadStart(threadHelper.CopyToClipboard);
                Thread STAThread1 = new Thread(ts);
                STAThread1.SetApartmentState(ApartmentState.STA);
                STAThread1.Start();
                STAThread1.Join();
            }
        }

        private string Data = string.Empty;

        ClipboardCopy(string data)
        {
            Data = data;
        }

        void CopyToClipboard()
        {
            Clipboard.SetDataObject(Data, true);
        }
    }
}
#endif //!NETCOREAPP2_0 && !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NETCOREAPP2_1 && !NETCOREAPP3_1