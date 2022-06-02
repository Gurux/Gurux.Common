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

namespace Gurux.Common.Db
{
    /// <summary>
    /// This class is used add external functionality to the table object.
    /// </summary>
    public abstract class GXTableBase
    {
        /// <summary>
        /// This method is called before Item is added to database.
        /// </summary>
        public virtual void BeforeAdd() { }

        /// <summary>
        /// This method is called after Item is added to database.
        /// </summary>
        public virtual void AfterAdd() { }

        /// <summary>
        /// This method is called before Item is updated to database.
        /// </summary>
        public virtual void BeforeUpdate() { }

        /// <summary>
        /// This method is called after Item is updated to database.
        /// </summary>
        public virtual void AfterUpdate() { }

        /// <summary>
        /// This method is called before Item is removed from database.
        /// </summary>
        public virtual void BeforeRemove() { }

        /// <summary>
        /// This method is called after Item is removed from database.
        /// </summary>
        public virtual void AfterRemove() { }
    }
}
