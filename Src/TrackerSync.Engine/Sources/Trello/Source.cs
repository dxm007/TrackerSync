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
    /// <summary>
    /// Source class for interacting with Trello web services API
    /// </summary>
    public class Source : Sources.Source
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">Settings object describing parameters for communicating with Trello</param>
        public Source( SourceSettings settings )
                : base( new RuntimeSourceSettings( settings ) )
        {
        }

        /// <inheritdoc/>
        public override void Connect()
        {
            new ResolveBoardIdRequest( Settings ).Execute();

            new ResolveListIdsRequest( Settings ).Execute();
        }
    }
}
