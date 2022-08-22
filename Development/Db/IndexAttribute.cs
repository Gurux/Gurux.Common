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

using System;

namespace Gurux.Common.Db
{
    /// <summary>
    /// Is table column indexed to DB.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// Is index unique.
        /// </summary>
        public bool Unique
        {
            get;
            set;
        }

        /// <summary>
        /// Data is sorted in descending order.
        /// </summary>
        public bool Descend
        {
            get;
            set;
        }

        /// <summary>
        /// In default index is unique.
        /// </summary>
        public IndexAttribute()
        {
            Unique = true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="unique">Is index unique.</param>
        public IndexAttribute(bool unique)
        {
            Unique = unique;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="unique">Is index unique.</param>
        /// <param name="descend">Data is sorted in descending order.</param>
        public IndexAttribute(bool unique, bool descend)
        {
            Unique = unique;
            Descend = descend;
        }
    }
}
