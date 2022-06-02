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
    /// Collection of indexed table column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class IndexCollectionAttribute
        : Attribute
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
        /// Name of the index.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Names of composite index.
        /// </summary>
        public string[] Columns
        {
            get;
            private set;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param column="column">Name of composite index.</param>
        public IndexCollectionAttribute(string column)
        {
            Columns = new string[] { column };
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param column="unique">Is index unique.</param>
        /// <param column="column">Name of composite index.</param>
        public IndexCollectionAttribute(bool unique, string column)
        {
            Unique = unique;
            Columns = new string[] { column };
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param column="unique">Is index unique.</param>
        /// <param column="column">Name of composite index</param>
        public IndexCollectionAttribute(bool unique, string column1, string colum2)
        {
            Unique = unique;
            Columns = new string[] { column1, colum2 };
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param column="unique">Is index unique.</param>
        /// <param column="column">Name of composite index</param>
        public IndexCollectionAttribute(bool unique, string column1, string colum2, string column3)
        {
            Unique = unique;
            Columns = new string[] { column1, colum2, column3 };
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param column="unique">Is index unique.</param>
        /// <param column="column">Name of composite index</param>
        public IndexCollectionAttribute(bool unique, string column1, string colum2, string column3, string name4)
        {
            Unique = unique;
            Columns = new string[] { column1, colum2, column3, name4 };
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param column="unique">Is index unique.</param>
        /// <param column="column">Name of composite index</param>
        public IndexCollectionAttribute(bool unique, string column1, string colum2, string column3, string name4, string column5)
        {
            Unique = unique;
            Columns = new string[] { column1, colum2, column3, name4, column5 };
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param column="unique">Is index unique.</param>
        /// <param column="column">Name of composite index</param>
        public IndexCollectionAttribute(bool unique, string column1, string colum2, string column3, string name4, string column5, string column6)
        {
            Unique = unique;
            Columns = new string[] { column1, colum2, column3, name4, column5, column6 };
        }
    }
}