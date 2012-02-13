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
    static class EnumExtensions
    {
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
