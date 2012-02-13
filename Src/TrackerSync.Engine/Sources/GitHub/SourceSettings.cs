﻿//=================================================================================================
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
    public class SourceSettings : TrackerSync.Sources.SourceSettings
    {
        public SourceSettings()
        {
            base.GetAllIncludesClosedIssues = false;
            base.IsPrimary = true;
        }

        public NetworkCredential Credentials { get; private set; }

        public string RepoName { get; private set; }

        public string UserName { get {
            return this.Credentials.GetCredential( 
                        new Uri( this.ApiServerUrl ), "Basic" ).UserName; } }

        public string ApiServerUrl { get { return "https://api.github.com"; } }

        public override void Load( System.Xml.Linq.XElement elem )
        {
            base.Load( elem );

            foreach( var prop in elem.Elements() )
            {
                switch( prop.Name.ToString().ToLower() )
                {
                case "credentials":
                    this.Credentials = new NetworkCredential( 
                                            (string)prop.Attribute( "UserName" ),
                                            (string)prop.Attribute( "Password" )  );
                    break;
                case "repo":
                    this.RepoName = (string)prop;
                    break;
                default:
                    break;
                }
            }

            Validate();
        }

        private void Validate()
        {
            if( string.IsNullOrEmpty( this.Credentials.UserName ) )
            {
                throw new ApplicationException( "Missing credential information" );
            }
            else if( string.IsNullOrEmpty( this.RepoName ) )
            {
                throw new ApplicationException( "Missing repo name" );
            }
        }
   }
}
