//=================================================================================================
//=================================================================================================
//
// Copyright (c) 2012 Dennis Mnuskin
//
// This file is part of TrackerSync application.
//
// This source code is distributed under the MIT license.  For full text, see
// http://www.opensource.org/licenses/mit-license.php Same text is found in LICENSE.txt file which
// is located in root directory of the project.
//
//=================================================================================================
//=================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackerSync
{
    /// <summary>
    /// Extension methods for Enum types
    /// </summary>
    static class EnumExtensions
    {
        /// <summary>
        /// Designed to work with enumeration types that use [Flags] attribute to return a
        /// list of flag values which are current set.
        /// </summary>
        /// <param name="value">Enumeration flag value</param>
        /// <returns>Enumerable object which iterates over individual flag values</returns>
        public static IEnumerable< Enum > Flags( this Enum value )
        {
            foreach( var x in Enum.GetValues( value.GetType() ).Cast<Enum>() )
            {
                if( value.HasFlag( x ) )
                {
                    yield return x;
                }
            }
        }
    }
}
