﻿//
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
#if !NETCOREAPP2_0 && !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NETCOREAPP2_1 && !NETCOREAPP3_1 && !NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.IO;

namespace Gurux.Common
{
    /// <summary>
    /// This class is used to handle file elevation.
    /// </summary>
    public static class GXFileSystemSecurity
    {
        /// <summary>
        /// Opens up directory access for Everyone at FullAccess.
        /// </summary>
        /// <param name="path">Directory to updated.</param>
        public static void UpdateDirectorySecurity(string path)
        {
            DirectoryInfo dInfo = new DirectoryInfo(path);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        /// <summary>
        /// Opens up file access for Everyone at FullAccess.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public static void UpdateFileSecurity(string filePath)
        {
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            FileInfo fInfo = new FileInfo(filePath);
            FileSecurity fSecurity = File.GetAccessControl(filePath);
            fSecurity.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.FullControl,
                        InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            fInfo.SetAccessControl(fSecurity);
        }
    }
}
#endif //!NETCOREAPP2_0 && !NETSTANDARD2_0 && !NETSTANDARD2_1 && !NETCOREAPP2_1 && !NETCOREAPP3_1 && !NET6_0