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
using System.ComponentModel;

namespace Gurux.Common.Db
{
    /// <summary>
    /// Foreign key delete actions.
    /// </summary>
    public enum ForeignKeyDelete
    {
        /// <summary>
        /// Foreign key delete action is not used. This is a default.
        /// </summary>
        None,
        /// <summary>
        /// Items are removed from child table when parent item is removed.
        /// </summary>
        Cascade,
        /// <summary>
        /// If parent item is try to remove and there are items on the child table deletion is rejected.
        /// </summary>
        Empty,
        /// <summary>
        /// Deletion is restrict.
        /// </summary>
        Restrict
    }
}
