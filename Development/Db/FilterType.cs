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
using System.Runtime.Serialization;

namespace Gurux.Common.Db
{
    /// <summary>
    /// Filter Type defines how filter is used with the property.
    /// </summary>
    public enum FilterType : int
    {
        /// <summary>
        /// Filtered value must be exact the same.
        /// </summary>
        /// <remarks>
        /// For string values compare is case sensitive.
        /// </remarks>
        [EnumMember(Value = "0")]
        Exact = 0,
        /// <summary>
        /// Value must be equal.
        /// </summary>
        /// <remarks>
        /// For string values compare is case-insensitive.
        /// </remarks>
        [EnumMember(Value = "1")]
        Equals = 1,
        /// <summary>
        /// Value must be greater than.
        /// </summary>
        [EnumMember(Value = "2")]
        Greater = 2,
        /// <summary>
        /// Value must be less than.
        /// </summary>
        [EnumMember(Value = "3")]
        Less = 3,
        /// <summary>
        /// Value must be greater or equal.
        /// </summary>
        [EnumMember(Value = "4")]
        GreaterOrEqual = 4,
        /// <summary>
        /// Value must be less or equal.
        /// </summary>
        [EnumMember(Value = "5")]
        LessOrEqual = 5,
        /// <summary>
        /// String value starts with the filtered value.
        /// </summary>
        [EnumMember(Value = "6")]
        StartsWith = 6,
        /// <summary>
        /// String value ends with the filtered value.
        /// </summary>
        [EnumMember(Value = "7")]
        EndsWith = 7,
        /// <summary>
        /// String value contains the filtered value.
        /// </summary>
        [EnumMember(Value = "8")]
        Contains = 8,
    }
}
