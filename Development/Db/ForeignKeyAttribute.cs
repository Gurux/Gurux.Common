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
    /// Foreign key attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
    {
        public Type Type
        {
            get;
            private set;
        }

        public Type MapTable
        {
            get;
            private set;
        }

        /// <summary>
        /// Can foreign key be null. 
        /// </summary
        /// <remarks>
        /// Foreign key can't be null as default.
        /// </remarks>
        public bool AllowNull
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Foreign key type.</param>
        public ForeignKeyAttribute()
        {
            OnDelete = ForeignKeyDelete.None;
            OnUpdate = ForeignKeyUpdate.None;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Foreign key type.</param>
        public ForeignKeyAttribute(Type type)
        {
            Type = type;
            OnDelete = ForeignKeyDelete.None;
            OnUpdate = ForeignKeyUpdate.None;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Foreign key type.</param>
        /// <param name="mapTable">Map type.</param>
        public ForeignKeyAttribute(Type type, Type mapTable)
        {
            OnDelete = ForeignKeyDelete.None;
            OnUpdate = ForeignKeyUpdate.None;
            Type = type;
            MapTable = mapTable;
        }

        /// <summary>
        /// Define what happens to the items in the child table when the items on the parent table are deleted.
        /// </summary>
        [DefaultValue(ForeignKeyDelete.None)]
        public ForeignKeyDelete OnDelete
        {
            get;
            set;
        }

        /// <summary>
        /// Define what happens to the rows in the child table when rows in the parent table are updated.
        /// </summary>
        [DefaultValue(ForeignKeyDelete.None)]
        public ForeignKeyUpdate OnUpdate
        {
            get;
            set;
        }
    }
}
