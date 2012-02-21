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

namespace TrackerSync.Sources.Trello
{
    /// <summary>
    /// Base class for all HTTP REST requests made against Trello Web Services API
    /// </summary>
    abstract class HttpRequest : TrackerSync.Sources.HttpRequest
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">Trello connection source settings</param>
        public HttpRequest( Sources.SourceSettings settings ) : base( settings )
        {
        }

        /// <inheritdoc/>
        public new RuntimeSourceSettings SourceSettings
        {
            get { return (RuntimeSourceSettings)base.SourceSettings; }
        }

        #endregion

        #region ----------------------- Protected Members ---------------------

        /// <summary>
        /// To be called by a deriving class when the request is to be executed
        /// </summary>
        /// <param name="suffixFormat">Suffix format string to be tacked onto the end of a request URL</param>
        /// <param name="args">Arguments for the format string</param>
        protected void SendRequest( string              suffixFormat,
                                    params object[]     args          )
        {
            string  url = string.Format( "{0}/1{1}&key={2}&token={3}",
                                         SourceSettings.ApiServerUrl,
                                         string.Format( suffixFormat, args ),
                                         SourceSettings.DevKey,
                                         SourceSettings.MemberToken           );

            base.SendRequest( url, null );
        }

        #endregion
    }
}
