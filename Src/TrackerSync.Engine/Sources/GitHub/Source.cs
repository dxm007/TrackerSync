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
using System.IO;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TrackerSync.Data;



namespace TrackerSync.Sources.GitHub
{
    /// <summary>
    /// Source class for iteracting with GitHub web services API
    /// </summary>
    public class Source : Sources.Source
    {
        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">Settings object describing parameters for communicating with GitHub</param>
        public Source( SourceSettings settings ) : base( settings )
        {
        }

        /// <inheritdoc/>
        public override void Connect()
        {
            new VerifyInfoRequest( Settings ).Execute();
        }
    }
}
