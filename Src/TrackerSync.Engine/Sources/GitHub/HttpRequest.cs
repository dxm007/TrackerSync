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
    enum BaseHttpReqUrlType
    {
        None,
        Repo,
        X
    }


    abstract class HttpRequest : TrackerSync.Sources.HttpRequest
    {
        public HttpRequest( Sources.SourceSettings  settings ) : base( settings )
        {
        }

        public new SourceSettings SourceSettings
        {
            get { return (SourceSettings)base.SourceSettings; }
        }

        protected void SendRequest( string              suffixFormat,
                                    params object[]     args          )
        {
            SendRequest( BaseHttpReqUrlType.Repo, suffixFormat, args );
        }

        protected void SendRequest( BaseHttpReqUrlType  baseUrlType,
                                    string              suffixFormat,
                                    params object[]     args          )
        {
            string url = BuildApiPath( baseUrlType, suffixFormat, args );

            base.SendRequest( url, SourceSettings.Credentials );
        }

        protected override void FillInHttpRequest( HttpWebRequest request )
        {
            base.FillInHttpRequest( request );

            // This flag is here to work around GitHub's bug where "POST /repos/.../issues" request
            // incorrectly returns "404 Not Found" instead of "401 Unauthorized" when the Authorization header
            // is not included with the request.
            request.PreAuthenticate = true;
        }

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

    }

}
