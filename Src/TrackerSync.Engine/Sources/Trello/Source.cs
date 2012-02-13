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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using TrackerSync.Data;

namespace TrackerSync.Sources.Trello
{
    public class Source : Sources.Source
    {
        public Source( SourceSettings settings )
                : base( new RuntimeSourceSettings( settings ) )
        {
        }

        public override void Connect()
        {
            new ResolveBoardIdRequest( Settings ).Execute();

            new ResolveListIdsRequest( Settings ).Execute();
        }
    }
}
