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
using System.Net;
using System.Text;

namespace TrackerSync.Sources.GitHub
{
    /// <summary>
    /// Identifies the base URL string which should be generated for the REST command
    /// </summary>
    enum BaseHttpReqUrlType
    {
        None,
        Repo
    }


    /// <summary>
    /// Base class for all HTTP REST requests made against GitHub Web Services API
    /// </summary>
    abstract class HttpRequest : TrackerSync.Sources.HttpRequest
    {
        #region ----------------------- Public Members ------------------------

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="settings">GitHub connection source settings</param>
        public HttpRequest( Sources.SourceSettings  settings ) : base( settings )
        {
        }

        /// <summary>
        /// Gets the GitHub connection source settings 
        /// </summary>
        public new SourceSettings SourceSettings
        {
            get { return (SourceSettings)base.SourceSettings; }
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
            SendRequest( BaseHttpReqUrlType.Repo, suffixFormat, args );
        }

        /// <summary>
        /// To be called by a deriving class when the request is to be executed
        /// </summary>
        /// <param name="baseUrlType">Identifies the base URL which is to be used for the request</param>
        /// <param name="suffixFormat">Suffix format string to be tacked onto the end of a request URL</param>
        /// <param name="args">Arguments for the format string</param>
        protected void SendRequest( BaseHttpReqUrlType  baseUrlType,
                                    string              suffixFormat,
                                    params object[]     args          )
        {
            string url = BuildApiPath( baseUrlType, suffixFormat, args );

            base.SendRequest( url, SourceSettings.Credentials );
        }

        /// <inheritdoc/>
        protected override void FillInHttpRequest( HttpWebRequest request )
        {
            base.FillInHttpRequest( request );

            // This flag is here to work around GitHub's bug where "POST /repos/.../issues" request
            // incorrectly returns "404 Not Found" instead of "401 Unauthorized" when the Authorization header
            // is not included with the request.
            request.PreAuthenticate = true;
        }

        #endregion

        #region ----------------------- Private Members -----------------------

        private string BuildApiPath( BaseHttpReqUrlType    baseUrlType,
                                     string                suffixFormat,
                                     params object[]       args          )
        {
            if( baseUrlType == BaseHttpReqUrlType.Repo )
            {
                return string.Format( "{0}/repos/{1}/{2}{3}",
                                      SourceSettings.ApiServerUrl, SourceSettings.UserName,
                                      SourceSettings.RepoName, string.Format( suffixFormat, args ) );
            }
            else if( baseUrlType == BaseHttpReqUrlType.None )
            {
                return SourceSettings.ApiServerUrl + string.Format( suffixFormat, args );
            }
            else
            {
                return string.Format( suffixFormat, args );
            }
        }

        #endregion
    }

}
