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

namespace Gurux.Common.Db
{
    /// <summary>
    /// AllowNull attribute can be used tell is null value allowed for the database column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AllowNullAttribute : Attribute
    {
        /// <summary>
        /// Is null allowed.
        /// </summary>
        public bool AllowNull
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AllowNullAttribute()
        {

        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="allowNull">Is null allowed.</param>
        public AllowNullAttribute(bool allowNull)
        {
            AllowNull = allowNull;
        }
    }
}
